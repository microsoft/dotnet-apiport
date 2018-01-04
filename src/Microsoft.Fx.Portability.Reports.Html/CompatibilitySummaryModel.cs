// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.Reports.Html
{
    public class CompatibilitySummaryModel
    {
        public IDictionary<BreakingChange, IEnumerable<MemberInfo>> Breaks { get; }
        public AssemblyInfo Assembly { get; }
        public ReportingResult ReportingResult { get; }

        public CompatibilitySummaryModel(IDictionary<BreakingChange, IEnumerable<MemberInfo>> breaks, AssemblyInfo assembly, ReportingResult reportingResult)
        {
            Breaks = breaks;
            Assembly = assembly;
            ReportingResult = reportingResult;
        }
    }
}
