using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace ApiCatalog
{
    public sealed class SuffixTreeBaseline
    {
        internal const ushort Version = 1;
        internal static readonly byte[] MagicNumbers = Encoding.UTF8.GetBytes("STFB");

        private readonly byte[] _buffer;
        private readonly int _stringsStart;
        private readonly int _stringsLength;
        private readonly int _nodesStart;
        private readonly int _nodesLength;
        private readonly int _rootOffset;

        public SuffixTreeBaseline(byte[] buffer,
                                  int stringsStart,
                                  int stringsLength,
                                  int nodesStart,
                                  int nodesLength,
                                  int rootOffset)
        {
            _buffer = buffer;
            _stringsStart = stringsStart;
            _stringsLength = stringsLength;
            _nodesStart = nodesStart;
            _nodesLength = nodesLength;
            _rootOffset = rootOffset;
        }

        private ReadOnlySpan<byte> StringTable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return new ReadOnlySpan<byte>(_buffer, _stringsStart, _stringsLength);
            }
        }

        private ReadOnlySpan<byte> NodeTable
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return new ReadOnlySpan<byte>(_buffer, _nodesStart, _nodesLength);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlySpan<byte> GetString(int stringOffset)
        {
            var lengthSpan = StringTable.Slice(stringOffset, 4);
            var length = BinaryPrimitives.ReadInt32LittleEndian(lengthSpan);
            return StringTable.Slice(stringOffset + 4, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlySpan<byte> GetNodeText(int nodeOffset)
        {
            var stringOffsetSpan = NodeTable.Slice(nodeOffset, 4);
            var stringOffset = BinaryPrimitives.ReadInt32LittleEndian(stringOffsetSpan);
            return GetString(stringOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlySpan<int> GetNodeChildren(int nodeOffset)
        {
            var childCountSpan = NodeTable.Slice(nodeOffset + 4, 4);
            var childCount = BinaryPrimitives.ReadInt32LittleEndian(childCountSpan);
            var childrenBytes = NodeTable.Slice(nodeOffset + 4 + 4, childCount * 4);
            return MemoryMarshal.Cast<byte, int>(childrenBytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlySpan<int> GetNodeValues(int nodeOffset)
        {
            var childCountSpan = NodeTable.Slice(nodeOffset + 4, 4);
            var childCount = BinaryPrimitives.ReadInt32LittleEndian(childCountSpan);
            var valueCountSpan = NodeTable.Slice(nodeOffset + 4 + 4 + childCount * 4, 4);
            var valueCount = BinaryPrimitives.ReadInt32LittleEndian(valueCountSpan);
            var valueBytes = NodeTable.Slice(nodeOffset + 4 + childCount * 4 + 4 + 4, valueCount * 4);
            return MemoryMarshal.Cast<byte, int>(valueBytes);
        }

        private int GetChildIndex(int nodeOffset, ReadOnlySpan<byte> text)
        {
            var children = GetNodeChildren(nodeOffset);

            var lo = 0;
            var hi = children.Length - 1;

            while (lo <= hi)
            {
                var i = (lo + hi) / 2;

                var childText = GetNodeText(children[i]);
                var c = childText.SequenceCompareTo(text);
                if (c == 0)
                    return children[i];
                if (c < 0)
                    lo = i + 1;
                else
                    hi = i - 1;
            }

            return -1;
        }

        public ReadOnlySpan<int> Lookup(string key)
        {
            var currentNodeOffset = _rootOffset;

            foreach (var token in Tokenizer.Tokenize(key))
            {
                var tokenBytes = Encoding.UTF8.GetBytes(token);
                var childOffset = GetChildIndex(currentNodeOffset, tokenBytes);

                if (childOffset == -1)
                    return null;

                currentNodeOffset = childOffset;
            }

            return GetNodeValues(currentNodeOffset);
        }

        public void WriteDot(TextWriter writer)
        {
            var idByNode = new Dictionary<int, string>();

            var stack = new Stack<int>();
            stack.Push(_rootOffset);

            while (stack.Count > 0)
            {
                var offset = stack.Pop();
                idByNode.Add(offset, $"N{idByNode.Count}");

                var children = GetNodeChildren(offset);

                for (var i = children.Length - 1; i >= 0; i--)
                {
                    var childOffset = children[i];
                    stack.Push(childOffset);
                }
            }

            writer.WriteLine("digraph {");

            stack.Push(_rootOffset);

            while (stack.Count > 0)
            {
                var from = stack.Pop();
                var fromId = idByNode[from];
                foreach (var to in GetNodeChildren(from))
                {
                    var toId = idByNode[to];
                    var textBytes = GetNodeText(to);
                    var text = Encoding.UTF8.GetString(textBytes);
                    writer.WriteLine($"    {fromId} -> {toId} [label = \"{text}\"]");
                    stack.Push(to);
                }
            }

            writer.WriteLine("}");
        }

        public static SuffixTreeBaseline Load(byte[] buffer)
        {
            // header
            //   magic value = byte[4]
            //   version = short
            //   strings = start: int, length: int
            //   nodes = start: int, length: int
            // strings
            //   string...
            //     length: int, byte...
            // nodes
            //   node...
            //     text = int
            //     children = count:int, int...
            //     values = count:int, int...

            if (buffer.Length < 26 || !buffer.AsSpan(0, MagicNumbers.Length).SequenceEqual(MagicNumbers))
                throw new InvalidDataException();

            var versionSpan = buffer.AsSpan().Slice(4, 2);
            var version = BinaryPrimitives.ReadInt16LittleEndian(versionSpan);
            if (version != Version)
                throw new InvalidDataException();

            var stringsStartSpan = buffer.AsSpan().Slice(6, 4);
            var stringsLengthSpan = buffer.AsSpan().Slice(10, 4);
            var nodesStartSpan = buffer.AsSpan().Slice(14, 4);
            var nodesLengthSpan = buffer.AsSpan().Slice(18, 4);
            var rootOffsetSpan = buffer.AsSpan().Slice(22, 4);

            var stringsStart = BinaryPrimitives.ReadInt32LittleEndian(stringsStartSpan);
            var stringsLength = BinaryPrimitives.ReadInt32LittleEndian(stringsLengthSpan);
            var nodesStart = BinaryPrimitives.ReadInt32LittleEndian(nodesStartSpan);
            var nodesLength = BinaryPrimitives.ReadInt32LittleEndian(nodesLengthSpan);
            var rootOffset = BinaryPrimitives.ReadInt32LittleEndian(rootOffsetSpan);

            if (stringsStart != 26 || stringsLength < 0 || stringsLength > buffer.Length - stringsStart)
                throw new InvalidDataException();

            var expectedNodeStart = stringsStart + stringsLength;

            if (nodesStart != expectedNodeStart || nodesLength < 0 || nodesLength > buffer.Length - nodesStart)
                throw new InvalidDataException();

            if (rootOffset < nodesStart || rootOffset >= nodesStart + nodesLength)
                throw new InvalidDataException();

            return new SuffixTreeBaseline(buffer, stringsStart, stringsLength, nodesStart, nodesLength, rootOffset);
        }

        public Stats GetStats()
        {
            var result = new Stats();

            var stringOffset = 0;
            while (stringOffset < _stringsLength)
            {
                var text = GetString(stringOffset);
                stringOffset += 4 + text.Length;
                result.Strings++;
            }

            var stack = new Stack<int>();
            stack.Push(_rootOffset);

            while (stack.Count > 0)
            {
                var offset = stack.Pop();

                var children = GetNodeChildren(offset);
                var values = GetNodeValues(offset);

                result.Nodes++;

                if (children.Length == 0 && values.Length == 0)
                    result.Nodes_NoChildren_NoValues++;
                if (children.Length == 0 && values.Length == 1)
                    result.Nodes_NoChildren_SingleValue++;
                if (children.Length == 0 && values.Length > 1)
                    result.Nodes_NoChildren_MultipleValues++;
                if (children.Length == 1 && values.Length == 0)
                    result.Nodes_SingleChild_NoValues++;
                if (children.Length == 1 && values.Length == 1)
                    result.Nodes_SingleChild_SingleValue++;
                if (children.Length == 1 && values.Length > 1)
                    result.Nodes_SingleChild_MultipleValues++;
                if (children.Length > 1 && values.Length == 0)
                    result.Nodes_MultipleChildren_NoValues++;
                if (children.Length > 1 && values.Length == 1)
                    result.Nodes_MultipleChildren_SingleValue++;
                if (children.Length > 1 && values.Length > 1)
                    result.Nodes_MultipleChildren_MultipleValues++;

                foreach (var childOffset in GetNodeChildren(offset))
                    stack.Push(childOffset);
            }

            return result;
        }

        public sealed class Stats
        {
            public int Strings { get; set; }
            public int Nodes { get; set; }
            public int Nodes_NoChildren_NoValues { get; set; }
            public int Nodes_NoChildren_SingleValue { get; set; }
            public int Nodes_NoChildren_MultipleValues { get; set; }
            public int Nodes_SingleChild_NoValues { get; set; }
            public int Nodes_SingleChild_SingleValue { get; set; }
            public int Nodes_SingleChild_MultipleValues { get; set; }
            public int Nodes_MultipleChildren_NoValues { get; set; }
            public int Nodes_MultipleChildren_SingleValue { get; set; }
            public int Nodes_MultipleChildren_MultipleValues { get; set; }

            public void WriteTo(TextWriter writer)
            {
                writer.WriteLine("Strings............................. {0:N0}", Strings);
                writer.WriteLine("Nodes............................... {0:N0}", Nodes);
                writer.WriteLine("No children, no values.............. {0:N0}", Nodes_NoChildren_NoValues);
                writer.WriteLine("No children, single value........... {0:N0}", Nodes_NoChildren_SingleValue);
                writer.WriteLine("No children, multiple values........ {0:N0}", Nodes_NoChildren_MultipleValues);
                writer.WriteLine("Single child, no values............. {0:N0}", Nodes_SingleChild_NoValues);
                writer.WriteLine("Single child, single value.......... {0:N0}", Nodes_SingleChild_SingleValue);
                writer.WriteLine("Single child, multiple values....... {0:N0}", Nodes_SingleChild_MultipleValues);
                writer.WriteLine("Multiple children, no values........ {0:N0}", Nodes_MultipleChildren_NoValues);
                writer.WriteLine("Multiple children, single value..... {0:N0}", Nodes_MultipleChildren_SingleValue);
                writer.WriteLine("Multiple children, multiple values.. {0:N0}", Nodes_MultipleChildren_MultipleValues);
            }
        }
    }
}