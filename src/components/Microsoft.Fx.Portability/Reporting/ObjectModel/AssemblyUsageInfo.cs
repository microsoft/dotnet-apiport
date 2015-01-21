// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.Reporting.ObjectModel
{
    public class AssemblyUsageInfo
    {
        public List<TargetUsageInfo> UsageData { get; set; }

        public AssemblyInfo SourceAssembly { get; set; }

        public AssemblyUsageInfo(AssemblyInfo assembly, int targetsCount)
        {
            SourceAssembly = assembly;

            // Initialize the target information
            UsageData = new List<TargetUsageInfo>(targetsCount);
            for (int i = 0; i < targetsCount; i++)
            {
                UsageData.Add(new TargetUsageInfo());
            }
        }
    }
}
