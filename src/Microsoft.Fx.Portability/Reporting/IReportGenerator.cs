// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.Reporting
{
    public interface IReportGenerator
    {
        ReportingResult ComputeReport(IList<FrameworkName> targets, string submissionId, IDictionary<MemberInfo, ICollection<AssemblyInfo>> allDependencies, IList<MemberInfo> missingDependencies, IDictionary<string, ICollection<string>> unresolvedAssemblies, IList<string> unresolvedUserAssemblies, IEnumerable<string> assembliesWithErrors);
    }
}
