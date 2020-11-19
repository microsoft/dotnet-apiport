// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability
{
    /// <summary>
    /// This interface provides a set of functionality that implements the analysis. The separation of concerns here
    /// is not terribly clear. This "engine" doesn't have access to the AnalysisRequest or AnalysisResponse, but is
    /// responsible for generating much of the latter from the former by careful coordination by an IRequestAnalyzer.
    /// It's possible that this interface was separated from IRequestAnalyzer to provide 2 implementations.
    /// I suspect that collapsing this back would result in a fair amount of simplification.
    /// </summary>
    public interface IAnalysisEngine
    {
        DateTimeOffset CatalogLastUpdated { get; }

        /// <summary>
        /// This method is poorly-named. "Unreferenced" seems to indicate assemblies that were in the set, but not referenced.
        /// However, in reality, this is the set of assemblies that are referenced from assemblies in the analysis set,
        /// are not in the set themselves. This method is intended to filter such a set so that it no longer includes
        /// "well-known" assemblies such as framework assemblies, or assemblies that are in the specified user assemblies
        /// TODO: Consider changing this name.
        /// </summary>
        /// <param name="unreferencedAssemblies">This is intended to be the "unresolved assemblies" provided from the original analysis request.</param>
        /// <param name="specifiedUserAssemblies">This is the user assemblies provided from the original analysis request.</param>
        /// <returns>A filtered list of "unreferenced" assemblies.</returns>
        IEnumerable<string> FindUnreferencedAssemblies(
            IEnumerable<string> unreferencedAssemblies,
            IEnumerable<AssemblyInfo> specifiedUserAssemblies);

        // TODO: document the intended semantics
        IList<MemberInfo> FindMembersNotInTargets(
            IEnumerable<FrameworkName> targets,
            ICollection<string> userAssemblies,
            IDictionary<MemberInfo, ICollection<AssemblyInfo>> dependencies);

        // TODO: document the intended semantics
        IEnumerable<AssemblyInfo> FindBreakingChangeSkippedAssemblies(
            IEnumerable<FrameworkName> targets,
            IEnumerable<AssemblyInfo> userAssemblies,
            IEnumerable<IgnoreAssemblyInfo> assembliesToIgnore);

        // TODO: document the intended semantics
        public IList<ExceptionInfo> FindMembersMayThrow(IEnumerable<FrameworkName> targets,
            ICollection<string> submittedAssemblies,
            IDictionary<MemberInfo,
            ICollection<AssemblyInfo>> dependencies);

        // TODO: document the intended semantics
        IEnumerable<BreakingChangeDependency> FindBreakingChanges(
            IEnumerable<FrameworkName> targets,
            IDictionary<MemberInfo, ICollection<AssemblyInfo>> dependencies,
            IEnumerable<AssemblyInfo> assembliesToIgnore,
            IEnumerable<string> breakingChangesToSuppress,
            ICollection<string> userAssemblies,
            bool showRetargettingIssues = false);

        // TODO: document the intended semantics
        IEnumerable<NuGetPackageInfo> GetNuGetPackagesInfoFromAssembly(IEnumerable<AssemblyInfo> assemblies, IEnumerable<FrameworkName> targets);

        // TODO: document the intended semantics
        IEnumerable<NuGetPackageInfo> GetNuGetPackagesInfo(IEnumerable<string> referencedNuGetPackages, IEnumerable<FrameworkName> targets);

        // TODO: document the intended semantics
        IEnumerable<string> ComputeAssembliesToRemove(IEnumerable<AssemblyInfo> userAssemblies, IEnumerable<FrameworkName> targets, IEnumerable<NuGetPackageInfo> nugetPackagesForUserAssemblies);

        // TODO: document the intended semantics
        IDictionary<MemberInfo, ICollection<AssemblyInfo>> FilterDependencies(IDictionary<MemberInfo, ICollection<AssemblyInfo>> dependencies, IEnumerable<string> assembliesToRemove);
    }
}
