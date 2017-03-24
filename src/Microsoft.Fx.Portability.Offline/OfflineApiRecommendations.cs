// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability
{
    public class OfflineApiRecommendations : AncestorApiRecommendations
    {
        private IEnumerable<BreakingChange> _breakingChanges;
        private IApiCatalogLookup _lookup;

        public OfflineApiRecommendations(IApiCatalogLookup lookup, IEnumerable<BreakingChange> breakingChanges)
            : base(lookup)
        {
            _lookup = lookup;
            _breakingChanges = breakingChanges;
        }

        protected override IEnumerable<BreakingChange> GetBreakingChanges(string docId)
        {
            return _breakingChanges.Where(b => b.ApplicableApis != null && b.ApplicableApis.Contains(docId, StringComparer.Ordinal));
        }
    }
}
