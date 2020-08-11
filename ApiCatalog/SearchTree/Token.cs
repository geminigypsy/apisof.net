using System;

namespace ApiCatalog.SearchTree
{
    public struct Token
    {
        public static readonly Token Empty = new Token(string.Empty);

        public Token(string text)
            : this (text, 0, text.Length)
        {
        }

        public Token(string text, int offset)
            : this(text, 0, text.Length - offset)
        {
        }

        public Token(string text, int offset, int length)
        {
            Text = text;
            Offset = offset;
            Length = length;
        }

        public string Text { get; }

        public int Offset { get; }

        public int Length { get; }

        public int End => Offset + Length;

        public char this[int index] => Text[Offset + index];

        public static int Compare(Token left, Token right, StringComparison comparisonType)
        {
            var length = Math.Min(left.Length, right.Length);
            var result = string.Compare(left.Text, left.Offset, right.Text, right.Offset, length, comparisonType);
            if (result == 0)
                return left.Length.CompareTo(right.Length);

            return result;
        }

        public static int CommonPrefixLength(Token token, Token word2)
        {
            var length = Math.Min(token.Length, word2.Length);
            var result = 0;
            while (result < length && char.ToLowerInvariant(token[result]) == char.ToLowerInvariant(word2[result]))
                result++;
            return result;
        }

        public static implicit operator Token(string text)
        {
            return new Token(text);
        }

        public override string ToString()
        {
            return Text.Substring(Offset, Length);
        }
    }
}