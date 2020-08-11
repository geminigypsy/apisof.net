namespace ApiCatalog.SearchTree
{
    internal sealed partial class TokenTreeBuilder<T>
    {
        private readonly StringTable _table = new StringTable();

        public TokenNodeBuilder<T> Root { get; } = new TokenNodeBuilder<T>(null, string.Empty);

        public void Add(string text, Tokenizer tokenizer, T data)
        {
            var tokens = tokenizer(text);

            for (var i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Length == 1 && !char.IsLetter(tokens[i].Text[0]))
                    continue;

                var offset = tokens[i].Offset;
                var current = Root;

                for (var j = i; j < tokens.Count; j++)
                {
                    var word = _table.Intern(tokens[j].Text);
                    var index = current.BinarySearch(word);
                    if (index >= 0)
                    {
                        current = current.MutableChildren[index];
                    }
                    else
                    {
                        var node = new TokenNodeBuilder<T>(current, word);
                        current.MutableChildren.Insert(~index, node);
                        current = node;
                    }
                }

                current.MutableValues.Add(new TokenValue<T>(data, offset));
            }
        }

        public TokenTree<T> ToTree()
        {
            var root = TokenNode<T>.Create(Root);
            return new TokenTree<T>(root);
        }
    }
}