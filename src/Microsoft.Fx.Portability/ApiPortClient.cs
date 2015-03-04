// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using Microsoft.Fx.Portability.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability
{
    public class ApiPortClient
    {
        private readonly IApiPortService _apiPortService;
        private readonly IProgressReporter _progressReport;
        private readonly ITargetMapper _targetMapper;
        private readonly IDependencyFinder _dependencyFinder;
        private readonly IReportGenerator _reportGenerator;

        public ITargetMapper TargetMapper { get { return _targetMapper; } }

        public ApiPortClient(IApiPortService apiPortService, IProgressReporter progressReport, ITargetMapper targetMapper, IDependencyFinder dependencyFinder, IReportGenerator reportGenerator)
        {
            _apiPortService = apiPortService;
            _progressReport = progressReport;
            _targetMapper = targetMapper;
            _dependencyFinder = dependencyFinder;
            _reportGenerator = reportGenerator;
        }

        public async Task<ReportingResult> AnalyzeAssemblies(IApiPortOptions options)
        {
            IDependencyInfo dependencyFinderEngine = _dependencyFinder.FindDependencies(options.InputAssemblies, _progressReport);

            if (dependencyFinderEngine.UserAssemblies.Any())
            {
                AnalyzeRequest request = GenerateRequest(options, dependencyFinderEngine);

                return await GetResultFromService(request, dependencyFinderEngine);
            }
            else
            {
                _progressReport.ReportIssue(LocalizedStrings.NoFilesAvailableToUpload);

                return null;
            }
        }

        public async Task<byte[]> GetAnalysisReportAsync(IApiPortOptions options)
        {
            IDependencyInfo dependencyFinderEngine = _dependencyFinder.FindDependencies(options.InputAssemblies, _progressReport);

            if (dependencyFinderEngine.UserAssemblies.Any())
            {
                AnalyzeRequest request = GenerateRequest(options, dependencyFinderEngine);

                return await GetResultFromService(request, options.OutputFormat);
            }
            else
            {
                _progressReport.ReportIssue(LocalizedStrings.NoFilesAvailableToUpload);

                return null;
            }
        }

        private AnalyzeRequest GenerateRequest(IApiPortOptions options, IDependencyInfo dependencyFinder)
        {
            AnalyzeRequest request = new AnalyzeRequest
            {
                Targets = options.Targets.SelectMany(_targetMapper.GetNames).ToList(),
                Dependencies = dependencyFinder.Dependencies,
                UnresolvedAssemblies = dependencyFinder.UnresolvedAssemblies.Keys.ToList(),
                UnresolvedAssembliesDictionary = dependencyFinder.UnresolvedAssemblies,
                UserAssemblies = dependencyFinder.UserAssemblies.ToList(),
                AssembliesWithErrors = dependencyFinder.AssembliesWithErrors.ToList(),
                ApplicationName = options.Description,
                Version = AnalyzeRequest.CurrentVersion,
            };

            if (options.NoTelemetry)
            {
                request.RequestFlags |= AnalyzeRequestFlags.NoTelemetry;
            }

            return request;
        }

        /// <summary>
        /// Gets the Portability of an application as a ReportingResult.
        /// </summary>
        /// <returns>Set of APIs/assemblies that are not portable/missing.</returns>
        private async Task<ReportingResult> GetResultFromService(AnalyzeRequest request, IDependencyInfo dependencyFinder)
        {
            _progressReport.StartTask(LocalizedStrings.SendingDataToService);
            var fullResponse = await _apiPortService.SendAnalysisAsync(request);
            _progressReport.FinishTask();

            CheckEndpointStatus(fullResponse.Headers.Status);

            _progressReport.StartParallelTask(LocalizedStrings.ComputingReport, LocalizedStrings.ProcessedItems);

            var response = fullResponse.Response;

            var result = _reportGenerator.ComputeReport(
                response.Targets,
                response.SubmissionId,
                 dependencyFinder?.Dependencies,
                response.MissingDependencies,
                dependencyFinder?.UnresolvedAssemblies,
                response.UnresolvedUserAssemblies,
                dependencyFinder?.AssembliesWithErrors
            );

            _progressReport.FinishTask();

            return result;
        }

        /// <summary>
        /// Gets the Portability report for the request.
        /// </summary>
        /// <returns>An array of bytes corresponding to the report.</returns>
        private async Task<byte[]> GetResultFromService(AnalyzeRequest request, ResultFormat format)
        {
            _progressReport.StartTask(LocalizedStrings.SendingDataToService);
            var response = await _apiPortService.SendAnalysisAsync(request, format);
            _progressReport.FinishTask();

            CheckEndpointStatus(response.Headers.Status);

            return response.Response;
        }

        /// <summary>
        /// Verifies that the service is alive.  If the service is not alive, then an issue is logged 
        /// that will be reported back to the user.
        /// </summary>
        private void CheckEndpointStatus(EndpointStatus status)
        {
            if (status == EndpointStatus.Deprecated)
            {
                _progressReport.ReportIssue(LocalizedStrings.ServerEndpointDeprecated);
            }
        }

        public async Task<IEnumerable<AvailableTarget>> ListTargets()
        {
            _progressReport.StartTask(LocalizedStrings.RetrievingTargets);

            var targets = await _apiPortService.GetTargetsAsync();

            _progressReport.FinishTask();

            CheckEndpointStatus(targets.Headers.Status);

            return targets.Response;
        }
    }
}
