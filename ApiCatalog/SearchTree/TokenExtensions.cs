namespace ApiCatalog.SearchTree
{
    public static class TokenExtensions
    {
        public static Token Subsegment(this string text, int offset)
        {
            return new Token(text, offset, text.Length - offset);
        }

        public static Token Subsegment(this string text, int offset, int length)
        {
            return new Token(text, offset, length);
        }

        public static Token Subsegment(this Token segment, int offset)
        {
            return new Token(segment.Text, segment.Offset + offset, segment.Length - offset);
        }

        public static Token Subsegment(this Token segment, int offset, int length)
        {
            return new Token(segment.Text, segment.Offset + offset, length);
        }
    }
}