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
        private readonly IDependencyOrderer _orderer;

        public RequestAnalyzer(
            ITargetNameParser targetNameParser,
            IAnalysisEngine analysisEngine,
            ITargetMapper targetMapper,
            IReportGenerator reportGenerator,
            IDependencyOrderer orderer)
        {
            _targetNameParser = targetNameParser;
            _analysisEngine = analysisEngine;
            _targetMapper = targetMapper;
            _reportGenerator = reportGenerator;
            _orderer = orderer;
        }

        /// <summary>
        /// Analyzes a request, which has been put together from deep inspection of a program and produces a
        /// response containing the results of the analysis. The split between the inspection done to create
        /// the request and the analysis itself can be thought of as the gathering and aggregation of information
        /// and artifacts from a logical program, and the analysis of those artifacts. This separation is fairly fuzzy,
        /// but serves to minimize the data present in the request and prevent the transmission of code or other
        /// unneeded artifacts for analysis.
        /// </summary>
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

            // The intent here seems to be to filter the assemblies to only those with "real" data.
            // We need to pass full assembly info instead of just name so we have all the context
            // for doing trickier lookups like packages.
            var assemblyIdentities = request?.UserAssemblies.Where(x => x?.AssemblyIdentity is not null)
                ?? Enumerable.Empty<AssemblyInfo>();

            // If the dictionary is available, use its keys, otherwise, fall back to UnresolvedAssemblies.
            // Unresolved in this situation means that these were assemblies that were found as dependencies, but
            // they weren't able to be resolved back to one of the assemblies that was provided for analysis
            var unresolvedAssemblies = request.UnresolvedAssembliesDictionary?.Keys ?? request.UnresolvedAssemblies;

            // TODO: write down semantics of "unreferenced" and "missing"
            // TODO: this returns just assembly names. We need to either construct assemblyinfos from these later, or have this return assemblyinfos.
            var missingUserAssemblies = _analysisEngine.FindUnreferencedAssemblies(unresolvedAssemblies, request.UserAssemblies).ToList();

            // TODO: write down what "breaking change skipped assemblies" is
            var breakingChangeSkippedAssemblies = request.RequestFlags.HasFlag(AnalyzeRequestFlags.ShowBreakingChanges)
                ? _analysisEngine.FindBreakingChangeSkippedAssemblies(targets, request.UserAssemblies, request.AssembliesToIgnore).ToList()
                : new List<AssemblyInfo>();

            // TODO: write down what user assemblies is
            var userAssemblies = new HashSet<string>(assemblyIdentities.Select(x => x.AssemblyIdentity), StringComparer.OrdinalIgnoreCase);

            // TODO: doc
            // TODO: considering managing this list better
            var assembliesFoundInPackages = new HashSet<string>();

            // TODO: doc
            var nugetPackages = new List<NuGetPackageInfo>();

            // If the request contains the list of referenced NuGet packages (which it should if it comes from Visual Studio), find if there are supported versions for those packages.
            // If the request does not contain the list of referenced NuGet packages (request comes from command line version of the tool), get package info for user assemblies and for missing assemblies.
            // Also remove from analysis those user assemblies for which supported packages are found.
            if (request.ReferencedNuGetPackages != null)
            {
                // TODO: evaluate this. We can likely improve this with our source-based package analysis
                nugetPackages = _analysisEngine.GetNuGetPackagesInfo(request.ReferencedNuGetPackages, targets).ToList();
            }
            else
            {
                // TODO: this is roughly where we need to plug in.
                var nugetPackagesForUserAssemblies = _analysisEngine.GetNuGetPackagesInfoFromAssembly(assemblyIdentities, targets);

                // TODO: what are the semantics of ComputeAssembliesToRemove? Do we remove for reasons other than being in a package?
                // (if so, why is this limited to this case?)
                assembliesFoundInPackages = new HashSet<string>(_analysisEngine.ComputeAssembliesToRemove(request.UserAssemblies, targets, nugetPackagesForUserAssemblies), StringComparer.OrdinalIgnoreCase);

                // TODO: this call takes AssemblyInfo now. We don't know much about missing assemblies, so we can just generate them from name for now.
                // We need to unify the strategy here though.
                var nugetPackagesForMissingAssemblies = _analysisEngine.GetNuGetPackagesInfoFromAssembly(from a in missingUserAssemblies select new AssemblyInfo { AssemblyIdentity = a }, targets);
                nugetPackages = nugetPackagesForMissingAssemblies.Union(nugetPackagesForUserAssemblies).ToList();
            }

            // in-place sort of packages
            // TODO: why?
            nugetPackages.Sort(new NuGetPackageInfoComparer());

            // remove the assemblies we have marked for removal.
            // TODO: consider using set operations
            userAssemblies.RemoveWhere(assembliesFoundInPackages.Contains);

            var dependencies = _analysisEngine.FilterDependencies(request.Dependencies, assembliesFoundInPackages);

            // TODO: interpret intent here.
            var notInAnyTarget = request.RequestFlags.HasFlag(AnalyzeRequestFlags.ShowNonPortableApis)
                ? _analysisEngine.FindMembersNotInTargets(targets, userAssemblies, dependencies)
                : Array.Empty<MemberInfo>();

            var breakingChanges = request.RequestFlags.HasFlag(AnalyzeRequestFlags.ShowBreakingChanges)
                ? _analysisEngine.FindBreakingChanges(targets, request.Dependencies, breakingChangeSkippedAssemblies, request.BreakingChangesToSuppress, userAssemblies, request.RequestFlags.HasFlag(AnalyzeRequestFlags.ShowRetargettingIssues)).ToList()
                : new List<BreakingChangeDependency>();

            var apiPotentialExceptions = request.RequestFlags.HasFlag(AnalyzeRequestFlags.ShowExceptionApis)
                ? _analysisEngine.FindMembersMayThrow(targets, userAssemblies, dependencies)
                : Array.Empty<ExceptionInfo>();

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
                RecommendedOrder = _orderer.GetOrder(request.Entrypoints.FirstOrDefault(), request.UserAssemblies),
                SubmissionId = submissionId,
                BreakingChanges = breakingChanges,
                BreakingChangeSkippedAssemblies = breakingChangeSkippedAssemblies,
                NuGetPackages = nugetPackages,
                ThrowingMembers = apiPotentialExceptions,
                Request = request
            };
        }
    }
}
