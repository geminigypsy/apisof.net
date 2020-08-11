using System.Collections.Generic;

namespace ApiCatalog.SearchTree
{
    internal sealed class TokenNodeBuilder<T> : TokenNode<T>
    {
        public TokenNodeBuilder(TokenNodeBuilder<T> parent, string text)
        {
            Parent = parent;
            Text = text;
        }

        public TokenNodeBuilder<T> Parent { get; }

        public override string Text { get; }

        public override IReadOnlyList<TokenValue<T>> Values => MutableValues;

        public override IReadOnlyList<TokenNode<T>> Children => MutableChildren;

        public List<TokenValue<T>> MutableValues { get; } = new List<TokenValue<T>>(0);

        public List<TokenNodeBuilder<T>> MutableChildren { get; } = new List<TokenNodeBuilder<T>>(0);

        public override int Offset => Parent?.Offset + Parent?.Text.Length ?? 0;
    }
}