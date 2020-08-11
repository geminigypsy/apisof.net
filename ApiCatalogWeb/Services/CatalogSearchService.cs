using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ApiCatalog.SearchTree;

using Microsoft.Extensions.Hosting;

namespace ApiCatalogWeb.Services
{
    public class CatalogSearchServiceWarmUp : IHostedService
    {
        private readonly CatalogSearchService _searchService;

        public CatalogSearchServiceWarmUp(CatalogSearchService searchService)
        {
            _searchService = searchService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _searchService.InitializeAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class CatalogSearchService
    {
        private readonly CatalogService _catalogService;
        private TokenTree<CatalogApi> _tree;

        public CatalogSearchService(CatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        public async Task InitializeAsync()
        {
            var apis = await _catalogService.GetAllApisWithFullNameAsync();
            _tree = TokenTree.Create(apis.Select(a => KeyValuePair.Create(a.Name, a)), ApiTokenization.Tokenizer);
        }

        public IEnumerable<SearchResult<CatalogApi>> Search(string text)
        {
            if (_tree == null)
                return Array.Empty<SearchResult<CatalogApi>>();

            return _tree.Search(text);
        }
    }
}
