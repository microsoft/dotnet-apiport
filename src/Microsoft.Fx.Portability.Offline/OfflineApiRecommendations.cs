// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability
{
    public class OfflineApiRecommendations : IApiRecommendations
    {
        private IEnumerable<BreakingChange> _breakingChanges;
        private IApiCatalogLookup _lookup;

        public OfflineApiRecommendations(IApiCatalogLookup lookup, IEnumerable<BreakingChange> breakingChanges)
        {
            _lookup = lookup;
            _breakingChanges = breakingChanges;
        }

        public IEnumerable<ApiNote> GetNotes(string docId)
        {
            return Enumerable.Empty<ApiNote>();
        }

        public IEnumerable<BreakingChange> GetBreakingChanges(string docId)
        {
            return _breakingChanges.Where(b => b.ApplicableApis.Contains(docId, StringComparer.Ordinal));
        }

        public string GetRecommendedChanges(string docId)
        {
            return _lookup.GetRecommendedChange(docId);
        }

        public string GetSourceCompatibleChanges(string docId)
        {
            return _lookup.GetSourceCompatibilityEquivalent(docId);
        }

        public string GetComponent(string docId)
        {
            return string.Empty;
        }
    }
}
