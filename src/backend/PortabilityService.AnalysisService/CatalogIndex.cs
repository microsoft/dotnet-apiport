// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using System;

namespace PortabilityService.AnalysisService
{
    public sealed class CatalogIndex : IDisposable
    {
        private IApiCatalogLookup _catalog;
        private ISearcher<string> _index;

        public CatalogIndex(IApiCatalogLookup catalog, ISearcher<string> index)
        {
            _catalog = catalog;
            _index = index;
        }

        public IApiCatalogLookup Catalog { get { return _catalog; } }

        public ISearcher<string> Index { get { return _index; } }

        public void Dispose()
        {
            var searcher = _index as IDisposable;

            if (searcher != null)
            {
                searcher.Dispose();
            }
        }
    }
}
