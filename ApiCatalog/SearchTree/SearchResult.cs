using System.Collections.Generic;
using System.Linq;

namespace ApiCatalog.SearchTree
{
    public struct SearchResult<T>
    {
        public SearchResult(T item, IEnumerable<SearchMatch> matches)
        {
            Item = item;
            Matches = matches.ToArray();
        }

        internal SearchResult(T item, SearchMatch[] matches)
        {
            Item = item;
            Matches = matches;
        }

        public SearchResult(T item, SearchMatch match)
        {
            Item = item;
            Matches = new [] {match};
        }

        public SearchResult(T item)
        {
            Item = item;
            Matches = SearchMatch.NoMatches;
        }

        public T Item { get; }

        public int Offset => Matches.DefaultIfEmpty().Min(m => m.Offset);

        public int End => Matches.DefaultIfEmpty().Max(m => m.Offset + m.Length);

        public int Length => End - Offset;

        public IReadOnlyCollection<SearchMatch> Matches { get; }

        public override string ToString()
        {
            return $"{Item}";
        }
    }
}