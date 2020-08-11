using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiCatalog.SearchTree
{
    public abstract partial class TokenNode<T>
    {
        private static readonly IReadOnlyList<TokenValue<T>> NoValues = new TokenValue<T>[0];
        private static readonly IReadOnlyList<TokenNode<T>> NoChildren = new TokenNode<T>[0];

        public static readonly TokenNode<T> Empty = new LeafNode(0, string.Empty, NoValues);

        public abstract int Offset { get; }
        public abstract string Text { get; }
        public abstract IReadOnlyList<TokenValue<T>> Values { get; }
        public abstract IReadOnlyList<TokenNode<T>> Children { get; }

        internal TokenNode()
        {
        }

        internal static TokenNode<T> Create(TokenNodeBuilder<T> root)
        {
            var values = root.MutableValues.Count == 0 ? NoValues : root.MutableValues.ToArray();
            var children = root.MutableChildren.Count == 0 ? NoChildren : root.MutableChildren.Select(Create).ToArray();

            if (children.Count == 0)
                return new LeafNode(root.Offset, root.Text, values);

            if (values.Count == 0)
                return new InteriorNoValuesNode(root.Offset, root.Text, children);

            return new InteriorNode(root.Offset, root.Text, root.MutableValues, children);
        }

        public IEnumerable<TokenNode<T>> DescendantsAndSelf()
        {
            var queue = new Queue<TokenNode<T>>();
            queue.Enqueue(this);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                yield return node;

                foreach (var child in node.Children)
                    queue.Enqueue(child);
            }
        }

        internal int BinarySearch(Token word)
        {
            var lo = 0;
            var hi = Children.Count - 1;

            while (lo <= hi)
            {
                var i = (lo + hi) / 2;

                var c = Token.Compare(Children[i].Text, word, StringComparison.OrdinalIgnoreCase);
                if (c == 0)
                    return i;
                if (c < 0)
                    lo = i + 1;
                else
                    hi = i - 1;
            }

            return ~lo;
        }

        public override string ToString()
        {
            return $"{Text} (Values = {Values.Count:N0}, Children = {Children.Count:N0})";
        }
    }
}