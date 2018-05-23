// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability.Azure.ObjectModel
{
    public class FxApiUsageData
    {
        private IReadOnlyList<FxApiUsage> _usage;

        public IReadOnlyList<FxApiUsage> Usage
        {
            get { return _usage; }
            set
            {
                _usage = value
                    .OrderByDescending(o => o.Count)
                    .ToList()
                    .AsReadOnly();
            }
        }

        public int SubmissionCount { get; set; }
    }
}
