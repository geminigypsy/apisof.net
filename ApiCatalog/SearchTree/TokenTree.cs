using System.Collections.Generic;
using System.Linq;

namespace ApiCatalog.SearchTree
{
    public static class TokenTree
    {
        public static TokenTree<string> Create(IEnumerable<string> values, Tokenizer tokenizer)
        {
            return Create(values.Select(v => new KeyValuePair<string, string>(v, v)), tokenizer);
        }

        public static TokenTree<T> Create<T>(IEnumerable<KeyValuePair<string, T>> values, Tokenizer tokenizer)
        {
            var builder = new TokenTreeBuilder<T>();

            foreach (var pair in values)
                builder.Add(pair.Key, tokenizer, pair.Value);

            return builder.ToTree();
        }
    }
}
