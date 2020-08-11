using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ApiCatalog.SearchTree;

using ApiCatalogWeb.Services;
using Microsoft.AspNetCore.Components;

namespace ApiCatalogWeb.Pages
{
    public partial class Search
    {
        [Inject]
        public CatalogSearchService CatalogSearchService { get; set; }

        public IReadOnlyList<SearchResult<CatalogApi>> Results { get; set; }

        public async Task SearchAsync(string text)
        {
            Results = await Task.Run(() => CatalogSearchService.Search(text).Take(100).ToArray());
            //StateHasChanged();
        }
    }
}
