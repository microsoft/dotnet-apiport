// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public enum UsageDataFilter
    {
        HideSupported,
        ShowAll
    }

    public class UsageDataCollection
    {
        public IList<FrameworkName> Targets { get; set; }

        public IEnumerable<UsageData> Usage { get; set; }

        public int SubmissionCount { get; set; }

        public int NextSkip { get; set; }

        public int PreviousSkip { get; set; }

        public int Skip { get; set; }

        public int Top { get; set; }

        public UsageDataFilter Filter { get; set; }

        public IEnumerable<FrameworkName> AllTargets { get; set; }
    }
}
