// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.Reports.Html
{
    public class CompatibilityResultsModel
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public IEnumerable<KeyValuePair<BreakingChange, IEnumerable<MemberInfo>>> Breaks { get; private set; }
        public int WarningThreshold { get; private set; }
        public int ErrorThreshold { get; private set; }

        public CompatibilityResultsModel(string name, string description, IEnumerable<KeyValuePair<BreakingChange, IEnumerable<MemberInfo>>> breaks, int warningThreshold, int errorThreshold)
        {
            Name = name;
            Description = description;
            Breaks = breaks;
            WarningThreshold = warningThreshold;
            ErrorThreshold = errorThreshold;
        }
    }
}
