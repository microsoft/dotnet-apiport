// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using Microsoft.Fx.Portability.Resources;
using System;
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

        public async Task<IEnumerable<byte[]>> GetAnalysisReportAsync(IApiPortOptions options)
        {
            IDependencyInfo dependencyFinderEngine = _dependencyFinder.FindDependencies(options.InputAssemblies, _progressReport);

            if (dependencyFinderEngine.UserAssemblies.Any())
            {
                AnalyzeRequest request = GenerateRequest(options, dependencyFinderEngine);

                var tasks = options.OutputFormats
                    .Select(f => GetResultFromService(request, f))
                    .ToList();

                await Task.WhenAll(tasks);

                return tasks.Select(t => t.Result).ToList();
            }
            else
            {
                _progressReport.ReportIssue(LocalizedStrings.NoFilesAvailableToUpload);

                return Enumerable.Empty<byte[]>();
            }
        }

        public async Task<string> GetExtensionForFormat(string format)
        {
            var outputFormats = await _apiPortService.GetResultFormatsAsync();
            var outputFormat = outputFormats.Response.FirstOrDefault(f => string.Equals(format, f.DisplayName, StringComparison.OrdinalIgnoreCase));

            if (outputFormat == null)
            {
                throw new UnknownReportFormatException(format);
            }

            return outputFormat.FileExtension;
        }

        public async Task<IEnumerable<string>> ListResultFormatsAsync()
        {
            using (var progressTask = _progressReport.StartTask(LocalizedStrings.RetrievingOutputFormats))
            {
                try
                {
                    var outputFormats = await _apiPortService.GetResultFormatsAsync();

                    CheckEndpointStatus(outputFormats.Headers.Status);

                    return outputFormats.Response.Select(r => r.DisplayName).ToList();
                }
                catch (Exception)
                {
                    progressTask.Abort();
                    throw;
                }
            }
        }

        private AnalyzeRequest GenerateRequest(IApiPortOptions options, IDependencyInfo dependencyFinder)
        {
            return new AnalyzeRequest
            {
                Targets = options.Targets.SelectMany(_targetMapper.GetNames).ToList(),
                Dependencies = dependencyFinder.Dependencies,
                UnresolvedAssemblies = dependencyFinder.UnresolvedAssemblies.Keys.ToList(),
                UnresolvedAssembliesDictionary = dependencyFinder.UnresolvedAssemblies,
                UserAssemblies = dependencyFinder.UserAssemblies.ToList(),
                AssembliesWithErrors = dependencyFinder.AssembliesWithErrors.ToList(),
                ApplicationName = options.Description,
                Version = AnalyzeRequest.CurrentVersion,
                RequestFlags = options.RequestFlags
            };
        }

        /// <summary>
        /// Gets the Portability of an application as a ReportingResult.
        /// </summary>
        /// <returns>Set of APIs/assemblies that are not portable/missing.</returns>
        private async Task<ReportingResult> GetResultFromService(AnalyzeRequest request, IDependencyInfo dependencyFinder)
        {
            var fullResponse = await RetrieveResultAsync(request);

            CheckEndpointStatus(fullResponse.Headers.Status);

            using (var progressTask = _progressReport.StartTask(LocalizedStrings.ComputingReport))
            {
                try
                {
                    var response = fullResponse.Response;
                    var hasDependencyFinder = dependencyFinder != null;

                    return _reportGenerator.ComputeReport(
                        response.Targets,
                        response.SubmissionId,
                        request.RequestFlags,
                        hasDependencyFinder ? dependencyFinder.Dependencies : null, //allDependencies
                        response.MissingDependencies,
                        hasDependencyFinder ? dependencyFinder.UnresolvedAssemblies : null, //unresolvedAssemblies
                        response.UnresolvedUserAssemblies,
                        hasDependencyFinder ? dependencyFinder.AssembliesWithErrors : null //assembliesWithErrors
                    );
                }
                catch (Exception)
                {
                    progressTask.Abort();
                    throw;
                }
            }
        }

        private async Task<ServiceResponse<AnalyzeResponse>> RetrieveResultAsync(AnalyzeRequest request)
        {
            using (var progressTask = _progressReport.StartTask(LocalizedStrings.SendingDataToService))
            {
                try
                {
                    return await _apiPortService.SendAnalysisAsync(request);
                }
                catch (Exception)
                {
                    progressTask.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets the Portability report for the request.
        /// </summary>
        /// <returns>An array of bytes corresponding to the report.</returns>
        private async Task<byte[]> GetResultFromService(AnalyzeRequest request, string format)
        {
            using (var progressTask = _progressReport.StartTask(LocalizedStrings.SendingDataToService))
            {
                try
                {
                    var response = await _apiPortService.SendAnalysisAsync(request, format);

                    CheckEndpointStatus(response.Headers.Status);

                    return response.Response;
                }
                catch (Exception)
                {
                    progressTask.Abort();
                    throw;
                }
            }
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
            using (var progressTask = _progressReport.StartTask(LocalizedStrings.RetrievingTargets))
            {
                try
                {
                    var targets = await _apiPortService.GetTargetsAsync();

                    CheckEndpointStatus(targets.Headers.Status);

                    return targets.Response;
                }
                catch (Exception)
                {
                    progressTask.Abort();
                    throw;
                }
            }
        }
    }
}
