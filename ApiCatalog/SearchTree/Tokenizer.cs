using System.Collections.Generic;

namespace ApiCatalog.SearchTree
{
    public delegate IReadOnlyList<Token> Tokenizer(string text);
}