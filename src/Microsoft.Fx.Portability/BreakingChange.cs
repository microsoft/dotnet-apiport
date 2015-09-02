// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability
{
    public class BreakingChange : IComparable<BreakingChange>
    {
        public BreakingChange DeepCopy()
        {
            return new BreakingChange
            {
                Id = this.Id,
                Title = this.Title,
                Details = this.Details,
                Suggestion = this.Suggestion,
                Link = this.Link,
                Markdown = this.Markdown,
                ApplicableApis = this.ApplicableApis?.ToList(),
                Related = this.Related?.ToList(),
                VersionBroken = this.VersionBroken,
                VersionFixed = this.VersionFixed,
                IsBuildTime = this.IsBuildTime,
                IsQuirked = this.IsQuirked,
                SourceAnalyzerStatus = this.SourceAnalyzerStatus,
                BugLink = this.BugLink,
                Notes = this.Notes,
                ImpactScope = this.ImpactScope
            };
        }

        public string Id { get; set; }

        public string Title { get; set; }

        public string Details { get; set; }

        public string Suggestion { get; set; }

        public string Link { get; set; }

        public string Markdown { get; set; }

        public IEnumerable<string> ApplicableApis { get; set; }

        public IEnumerable<string> Related { get; set; }

        public Version VersionBroken { get; set; }

        public Version VersionFixed { get; set; }

        public bool IsBuildTime { get; set; }

        public bool IsQuirked { get; set; }

        public BreakingChangeAnalyzerStatus SourceAnalyzerStatus { get; set; }

        public string BugLink { get; set; }

        public string Notes { get; set; }

        public bool IsRetargeting
        {
            get
            {
                return IsBuildTime || IsQuirked;
            }
        }

        public BreakingChangeImpact ImpactScope { get; set; }

        public int CompareTo(BreakingChange other)
        {
            if (other == null)
            {
                return -1;
            }

            return string.CompareOrdinal(Title, other.Title);
        }
    }
}
