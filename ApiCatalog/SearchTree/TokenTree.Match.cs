using System.Collections.Generic;

namespace ApiCatalog.SearchTree
{
    partial class TokenTree<T>
    {
        private class Match
        {
            public Match(Match previous, TokenNode<T> node, Token term)
            {
                Term = term;
                Previous = previous;
                Node = node;
            }

            public Match Previous { get; }

            public TokenNode<T> Node { get; }

            public Token Term { get; }

            public IEnumerable<Match> AncestorsAndSelf()
            {
                var current = this;
                while (current != null)
                {
                    yield return current;
                    current = current.Previous;
                }
            }
        }
    }
}
