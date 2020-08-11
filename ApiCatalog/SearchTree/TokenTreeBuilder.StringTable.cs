using System.Collections.Generic;

namespace ApiCatalog.SearchTree
{
    partial class TokenTreeBuilder<T>
    {
        private sealed class StringTable
        {
            private readonly Dictionary<string, string> _table = new Dictionary<string, string>();

            public string Intern(string value)
            {
                string result;
                if (!_table.TryGetValue(value, out result))
                {
                    result = value;
                    _table.Add(value, result);
                }

                return result;
            }
        }
    }
}