// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PortabilityService.AnalysisService
{
    public class ApiPortData
    {
        /// <summary>
        /// Coresponding ApiCatalog data types fetched from the Git repository.
        /// </summary>
        public static readonly string[] CatalogDataTypes = new[] {
            nameof(BreakingChange),
            nameof(RecommendedChange)
        };

        public IDictionary<string, string> Components { get; set; } = new Dictionary<string, string>();

        public IEnumerable<BreakingChange> BreakingChanges { get; set; } = Enumerable.Empty<BreakingChange>();

        public IDictionary<string, ICollection<BreakingChange>> BreakingChangesDictionary { get; set; } = new Dictionary<string, ICollection<BreakingChange>>();

        public IDictionary<string, string> RecommendedChanges { get; set; } = new Dictionary<string, string>();

        public static bool IsDataValidType(string dataType) => CatalogDataTypes.Any(x => x.Equals(dataType, StringComparison.Ordinal));
    }
}
