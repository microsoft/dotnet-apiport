// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
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
            var targets = _targetNameParser.MapTargetsToExplicitVersions(request.Targets.SelectMany(_targetMapper.GetNames)).ToList();

            var notInAnyTarget = _analysisEngine.FindMembersNotInTargets(targets, request.Dependencies);
            var unresolvedAssemblies = request.UnresolvedAssembliesDictionary != null
                ? request.UnresolvedAssembliesDictionary.Keys
                : request.UnresolvedAssemblies;

            var missingUserAssemblies = _analysisEngine.FindUnreferencedAssemblies(unresolvedAssemblies, request.UserAssemblies).ToList();

            var breakingChanges = _analysisEngine.FindBreakingChanges(request.Dependencies).ToList();

            var reportingResult = _reportGenerator.ComputeReport(
                targets,
                submissionId,
                request.Dependencies,
                notInAnyTarget,
                request.UnresolvedAssembliesDictionary,
                missingUserAssemblies,
                request.AssembliesWithErrors);

            return new AnalyzeResponse
            {
                MissingDependencies = notInAnyTarget,
                UnresolvedUserAssemblies = missingUserAssemblies,
                Targets = targets,
                ReportingResult = reportingResult,
                SubmissionId = submissionId,
                BreakingChanges = breakingChanges
            };
        }
    }
}