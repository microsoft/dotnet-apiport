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

            var userAssemblies = new HashSet<string>(request.UserAssemblies.Select(a => a.AssemblyIdentity), StringComparer.OrdinalIgnoreCase);

            var notInAnyTarget = request.RequestFlags.HasFlag(AnalyzeRequestFlags.ShowNonPortableApis)
                ? _analysisEngine.FindMembersNotInTargets(targets, userAssemblies, request.Dependencies)
                : new List<MemberInfo>();

            var unresolvedAssemblies = request.UnresolvedAssembliesDictionary != null
                ? request.UnresolvedAssembliesDictionary.Keys
                : request.UnresolvedAssemblies;

            var missingUserAssemblies = _analysisEngine.FindUnreferencedAssemblies(unresolvedAssemblies, request.UserAssemblies).ToList();

            var breakingChangeSkippedAssemblies = request.RequestFlags.HasFlag(AnalyzeRequestFlags.ShowBreakingChanges)
                ? _analysisEngine.FindBreakingChangeSkippedAssemblies(targets, request.UserAssemblies, request.AssembliesToIgnore).ToList()
                : new List<AssemblyInfo>();

            var breakingChanges = request.RequestFlags.HasFlag(AnalyzeRequestFlags.ShowBreakingChanges)
                ? _analysisEngine.FindBreakingChanges(targets, request.Dependencies, breakingChangeSkippedAssemblies, request.BreakingChangesToSuppress, userAssemblies, request.RequestFlags.HasFlag(AnalyzeRequestFlags.ShowRetargettingIssues)).ToList()
                : new List<BreakingChangeDependency>();

            var reportingResult = _reportGenerator.ComputeReport(
                targets,
                submissionId,
                request.RequestFlags,
                request.Dependencies,
                notInAnyTarget,
                request.UnresolvedAssembliesDictionary,
                missingUserAssemblies,
                request.AssembliesWithErrors);

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
                BreakingChangeSkippedAssemblies = breakingChangeSkippedAssemblies
            };
        }
    }
}
