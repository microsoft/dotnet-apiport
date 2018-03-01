// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPort
{
    internal class ApiPortServiceSearcher : ISearcher<string>
    {
        private readonly IApiPortService _service;

        public ApiPortServiceSearcher(IApiPortService service)
        {
            _service = service;
        }

        public IEnumerable<string> Search(string query, int numberOfHits)
        {
            return SearchAsync(query, numberOfHits).Result;
        }

        public async Task<IEnumerable<string>> SearchAsync(string query, int numberOfHits)
        {
            var result = await _service.SearchFxApiAsync(query, numberOfHits);

            return result.Response
                .Select(r => r.DocId)
                .ToList();
        }
    }
}
