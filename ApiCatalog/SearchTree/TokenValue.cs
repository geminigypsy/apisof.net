namespace ApiCatalog.SearchTree
{
    public struct TokenValue<T>
    {
        public TokenValue(T item, int offset)
        {
            Item = item;
            Offset = offset;
        }

        public T Item { get; }

        public int Offset { get; }
    }
}