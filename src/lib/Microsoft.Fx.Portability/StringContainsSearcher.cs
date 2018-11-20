// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability
{
    public class StringContainsSearcher : ISearcher<string>
    {
        private readonly IApiCatalogLookup _lookup;

        public StringContainsSearcher(IApiCatalogLookup lookup)
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
                .Take(numberOfHits)
                .ToList();
        }

        public Task<IEnumerable<string>> SearchAsync(string query, int numberOfHits)
        {
            return Task.FromResult(Search(query, numberOfHits));
        }
    }
}
