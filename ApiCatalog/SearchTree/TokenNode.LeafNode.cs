using System.Collections.Generic;

namespace ApiCatalog.SearchTree
{
    partial class TokenNode<T>
    {
        private sealed class LeafNode : TokenNode<T>
        {
            public LeafNode(int offset, string text, IReadOnlyList<TokenValue<T>> values)
            {
                Offset = offset;
                Text = text;
                Values = values;
            }

            public override int Offset { get; }

            public override string Text { get; }

            public override IReadOnlyList<TokenValue<T>> Values { get; }

            public override IReadOnlyList<TokenNode<T>> Children => NoChildren;
        }
    }
}