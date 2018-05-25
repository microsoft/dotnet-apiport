// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortabilityService.AnalysisService
{
    public class DummyRecommendations : IApiRecommendations
    {
        private static readonly List<BreakingChange> s_EmptyBreakingChanges = new List<BreakingChange>(0);
        private static readonly List<ApiNote> s_EmptyApiNotes = new List<ApiNote>(0);

        public IEnumerable<BreakingChange> GetBreakingChanges(string docId)
        {
            return s_EmptyBreakingChanges;
        }

        public string GetComponent(string docId)
        {
            return $"Dummy Component for {docId}";
        }

        public IEnumerable<ApiNote> GetNotes(string docId)
        {
            return s_EmptyApiNotes;
        }

        public string GetRecommendedChanges(string docId)
        {
            return $"Dummy Recommended Changes for {docId}";
        }

        public string GetSourceCompatibleChanges(string docId)
        {
            return $"Dummy Source Compatible Changes for {docId}";
        }
    }
}
