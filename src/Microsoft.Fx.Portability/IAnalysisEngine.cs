// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability
{
    public interface IAnalysisEngine
    {
        DateTimeOffset CatalogLastUpdated { get; }

        IEnumerable<string> FindUnreferencedAssemblies(
            IEnumerable<string> unreferencedAssemblies,
            IEnumerable<AssemblyInfo> specifiedUserAssemblies);

        IList<MemberInfo> FindMembersNotInTargets(
            IEnumerable<FrameworkName> targets,
            ICollection<string> userAssemblies,
            IDictionary<MemberInfo, ICollection<AssemblyInfo>> dependencies);

        IEnumerable<AssemblyInfo> FindBreakingChangeSkippedAssemblies(
            IEnumerable<FrameworkName> targets,
            IEnumerable<AssemblyInfo> userAssemblies,
            IEnumerable<IgnoreAssemblyInfo> assembliesToIgnore);

        IEnumerable<BreakingChangeDependency> FindBreakingChanges(
            IEnumerable<FrameworkName> targets,
            IDictionary<MemberInfo, ICollection<AssemblyInfo>> dependencies,
            IEnumerable<AssemblyInfo> assembliesToIgnore,
            IEnumerable<string> breakingChangesToSuppress,
            ICollection<string> userAssemblies,
            bool ShowRetargettingIssues = false);
    }
}
