// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability.Analyzer
{
    public class RequestAnalyzer : IRequestAnalyzer
    {
        private readonly ITargetNameParser _targetNameParser;
        private readonly IAnalysisEngine _analysisEngine;
        private readonly ITargetMapper _targetMapper;
        private readonly IReportGenerator _reportGenerator;

        public RequestAnalyzer(ITargetNameParser targetNameParser, IAnalysisEngine analysisEngine, ITargetMapper targetMapper, IReportGenerator reportGenerator)
        {
            _targetNameParser = targetNameParser;
            _analysisEngine = analysisEngine;
            _targetMapper = targetMapper;
            _reportGenerator = reportGenerator;
        }

        public AnalyzeResponse AnalyzeRequest(AnalyzeRequest request, string submissionId)
        {
            // Get the list of targets we should consider in the analysis
            var targets = _targetNameParser
                .MapTargetsToExplicitVersions(request.Targets.SelectMany(_targetMapper.GetNames))
                .OrderBy(x => x.FullName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            // TODO: It's possible that an AssemblyInfo in UserAssemblies is null.
            // This appears to be coming from analysis in the VSIX, possibly
            // from CCI.  Figure out where this is coming from.
            var assemblyIdentities = request?.UserAssemblies.Where(x => x != null && x.AssemblyIdentity != null).Select(a => a.AssemblyIdentity)
                ?? Enumerable.Empty<string>();

            var unresolvedAssemblies = request.UnresolvedAssembliesDictionary != null
                ? request.UnresolvedAssembliesDictionary.Keys
                : request.UnresolvedAssemblies;

            var missingUserAssemblies = _analysisEngine.FindUnreferencedAssemblies(unresolvedAssemblies, request.UserAssemblies).ToList();

            var breakingChangeSkippedAssemblies = request.RequestFlags.HasFlag(AnalyzeRequestFlags.ShowBreakingChanges)
                ? _analysisEngine.FindBreakingChangeSkippedAssemblies(targets, request.UserAssemblies, request.AssembliesToIgnore).ToList()
                : new List<AssemblyInfo>();

            var userAssemblies = new HashSet<string>(assemblyIdentities, StringComparer.OrdinalIgnoreCase);
            var assembliesToRemove = new HashSet<string>();
            var nugetPackages = new List<NuGetPackageInfo>();

            // If the request contains the list of referenced NuGet packages (which it should if it comes from Visual Studio), find if there are supported versions for those packages.
            // If the request does not contain the list of referenced NuGet packages (request comes from command line version of the tool), get package info for user assemblies and for missing assemblies.
            // Also remove from analysis those user assemblies for which supported packages are found.
            if (request.ReferencedNuGetPackages != null)
            {
                nugetPackages = _analysisEngine.GetNuGetPackagesInfo(request.ReferencedNuGetPackages, targets).ToList();
            }
            else
            {
                var nugetPackagesForUserAssemblies = _analysisEngine.GetNuGetPackagesInfoFromAssembly(assemblyIdentities, targets);
                assembliesToRemove = new HashSet<string>(_analysisEngine.ComputeAssembliesToRemove(request.UserAssemblies, targets, nugetPackagesForUserAssemblies), StringComparer.OrdinalIgnoreCase);

                var nugetPackagesForMissingAssemblies = _analysisEngine.GetNuGetPackagesInfoFromAssembly(missingUserAssemblies, targets);
                nugetPackages = nugetPackagesForMissingAssemblies.Union(nugetPackagesForUserAssemblies).ToList();
            }

            nugetPackages.Sort(new NuGetPackageInfoComparer());

            userAssemblies.RemoveWhere(assembliesToRemove.Contains);

            var dependencies = _analysisEngine.FilterDependencies(request.Dependencies, assembliesToRemove);
            var notInAnyTarget = request.RequestFlags.HasFlag(AnalyzeRequestFlags.ShowNonPortableApis)
                ? _analysisEngine.FindMembersNotInTargets(targets, userAssemblies, dependencies)
                : Array.Empty<MemberInfo>();

            var breakingChanges = request.RequestFlags.HasFlag(AnalyzeRequestFlags.ShowBreakingChanges)
                ? _analysisEngine.FindBreakingChanges(targets, request.Dependencies, breakingChangeSkippedAssemblies, request.BreakingChangesToSuppress, userAssemblies, request.RequestFlags.HasFlag(AnalyzeRequestFlags.ShowRetargettingIssues)).ToList()
                : new List<BreakingChangeDependency>();

            var reportingResult = _reportGenerator.ComputeReport(
                targets,
                submissionId,
                request.RequestFlags,
                dependencies,
                notInAnyTarget,
                request.UnresolvedAssembliesDictionary,
                missingUserAssemblies,
                request.AssembliesWithErrors,
                nugetPackages);

            return new AnalyzeResponse
            {
                CatalogLastUpdated = _analysisEngine.CatalogLastUpdated,
                ApplicationName = request.ApplicationName,
                MissingDependencies = notInAnyTarget,
                UnresolvedUserAssemblies = missingUserAssemblies,
                Targets = targets,
                ReportingResult = reportingResult,
                SubmissionId = submissionId,
                BreakingChanges = breakingChanges,
                BreakingChangeSkippedAssemblies = breakingChangeSkippedAssemblies,
                NuGetPackages = nugetPackages
            };
        }
    }
}
