namespace ApiCatalog.SearchTree
{
    public struct SearchMatch
    {
        internal static readonly SearchMatch[] NoMatches = new SearchMatch[0];

        public SearchMatch(int offset, int length)
        {
            Offset = offset;
            Length = length;
        }

        public int Offset { get; }

        public int Length { get; }

        public override string ToString()
        {
            return $"[{Offset}; {Offset + Length})";
        }
    }
}