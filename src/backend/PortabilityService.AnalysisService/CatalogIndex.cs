// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using System;

namespace PortabilityService.AnalysisService
{
    public sealed class CatalogIndex : IDisposable
    {
        public CatalogIndex(IApiCatalogLookup catalog, ISearcher<string> index)
        {
            Catalog = catalog;
            Index = index;
        }

        public IApiCatalogLookup Catalog { get; }

        public ISearcher<string> Index { get; }

        public void Dispose()
        {
            if (Index is IDisposable searcher)
            {
                searcher.Dispose();
            }
        }
    }
}
