using System.Collections.Generic;

namespace ApiCatalog.SearchTree
{
    partial class TokenNode<T>
    {
        private sealed class InteriorNode : InteriorNoValuesNode
        {
            public InteriorNode(int offset, string text, IReadOnlyList<TokenValue<T>> values,  IReadOnlyList<TokenNode<T>> children)
                : base(offset, text, children)
            {
                Values = values;
            }

            public override IReadOnlyList<TokenValue<T>> Values { get; }
        }
    }
}