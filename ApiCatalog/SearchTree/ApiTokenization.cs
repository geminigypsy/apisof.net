using System.Collections.Generic;
using System.Linq;

namespace ApiCatalog.SearchTree
{
    public static class ApiTokenization
    {
        public static readonly Tokenizer Tokenizer = t => Tokenize(t).ToArray();

        public static IEnumerable<Token> Tokenize(string text)
        {
            var position = 0;
            var inWord = false;
            const char endOfString = '\0';

            while (position < text.Length)
            {
                var c = text[position];
                var l = position + 1 < text.Length ? text[position + 1] : endOfString;
                var start = position;

                if (inWord && c == '_')
                {
                    position++;
                }
                else if (c == '_' || char.IsLetter(c))
                {
                    ScanIdentifier(text, ref position);
                    inWord = true;
                }
                else if (char.IsDigit(c))
                {
                    ScanDigits(text, ref position);
                    inWord = false;
                }
                else if (c == '(' || c == '[')
                {
                    yield break;
                }
                else
                {
                    position++;
                    inWord = false;
                }

                var length = position - start;
                var tokenText = text.Substring(start, length);
                yield return tokenText.Subsegment(start, length);
            }
        }

        private static void ScanIdentifier(string text, ref int position)
        {
            if (text[position] == '_')
                position++;

            if (position < text.Length && char.IsUpper(text[position]))
                position++;

            if (position < text.Length)
            {
                if (char.IsUpper(text[position]))
                {
                    ScanUpperCaseOrDigit(text, ref position);
                }
                else
                {
                    ScanLowerCaseOrDigit(text, ref position);

                    if (position < text.Length && text[position] == '_')
                        ScanUnderscores(text, ref position);
                }
            }

            if (position < text.Length && text[position] == '<')
                ScanGenericName(text, ref position);
        }

        private static void ScanUpperCaseOrDigit(string text, ref int position)
        {
            while (IsUpperOrDigit(text, position) && !IsLower(text, position + 1))
                position++;
        }

        private static void ScanLowerCaseOrDigit(string text, ref int position)
        {
            while (IsLowerOrDigit(text, position))
                position++;
        }

        private static void ScanUnderscores(string text, ref int position)
        {
            while (position < text.Length && text[position] == '_')
                position++;
        }

        private static void ScanGenericName(string text, ref int position)
        {
            // Skip '<'
            position++;

            while (position < text.Length && text[position] != '>')
                position++;

            // Consume '>'
            if (position < text.Length)
                position++;
        }

        private static void ScanDigits(string text, ref int position)
        {
            while (position < text.Length && char.IsDigit(text[position]))
                position++;
        }

        private static bool IsUpperOrDigit(string text, int position)
        {
            if (position >= text.Length)
                return false;

            var c = text[position];
            return char.IsUpper(c) || char.IsDigit(c);
        }

        private static bool IsLowerOrDigit(string text, int position)
        {
            if (position >= text.Length)
                return false;

            var c = text[position];
            return char.IsLower(c) || char.IsDigit(c);
        }

        private static bool IsLower(string text, int position)
        {
            if (position >= text.Length)
                return false;

            var c = text[position];
            return char.IsLower(c);
        }
    }
}