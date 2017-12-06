// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.Reports.Html
{
    public class CompatibilitySummaryModel
    {
        public IDictionary<BreakingChange, IEnumerable<MemberInfo>> Breaks { get; private set; }
        public AssemblyInfo Assembly { get; private set; }
        public ReportingResult ReportingResult { get; private set; }
        public int LoopCounter { get; private set; }

        public CompatibilitySummaryModel(IDictionary<BreakingChange, IEnumerable<MemberInfo>> breaks, AssemblyInfo assembly, ReportingResult reportingResult, int loopCounter)
        {
            Breaks = breaks;
            Assembly = assembly;
            ReportingResult = reportingResult;
            LoopCounter = loopCounter;
        }
    }
}
