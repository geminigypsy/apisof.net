using System.Collections.Generic;

namespace ApiCatalog.SearchTree
{
    partial class TokenNode<T>
    {
        private class InteriorNoValuesNode : TokenNode<T>
        {
            public InteriorNoValuesNode(int offset, string text, IReadOnlyList<TokenNode<T>> children)
            {
                Offset = offset;
                Text = text;
                Children = children;
            }

            public override int Offset { get; }

            public override string Text { get; }

            public override IReadOnlyList<TokenValue<T>> Values => NoValues;

            public override IReadOnlyList<TokenNode<T>> Children { get; }
        }
    }
}