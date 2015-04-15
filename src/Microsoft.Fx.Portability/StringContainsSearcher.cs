// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability
{
    public class StringContainsSearch : ISearcher<string>
    {
        private readonly IApiCatalogLookup _lookup;

        public StringContainsSearch(IApiCatalogLookup lookup)
        {
            _lookup = lookup;
        }

        public IEnumerable<string> Search(string query, int numberOfHits)
        {
            var queryItems = new HashSet<string>(query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);

            return _lookup.DocIds
                .Select(_lookup.GetApiDefinition)
                .Where(a => queryItems.All(q => a.FullName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
                .Select(a => a.DocId)
                .Take(numberOfHits);
        }
    }
}