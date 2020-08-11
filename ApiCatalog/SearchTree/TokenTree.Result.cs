namespace ApiCatalog.SearchTree
{
    partial class TokenTree<T>
    {
        private struct Result
        {
            public Result(TokenNode<T> node, Match[] matches)
            {
                Node = node;
                Matches = matches;
            }

            public TokenNode<T> Node { get; }

            public Match[] Matches { get; }
        }
    }
}
