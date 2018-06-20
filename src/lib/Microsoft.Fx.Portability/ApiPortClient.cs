// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using Microsoft.Fx.Portability.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability
{
    public class ApiPortClient
    {
        /// <summary>
        /// Maximum number of targets that can be submitted to the service at
        /// a time.
        /// </summary>
        /// <remarks>OpenXML supports a maximum of 26 columns.</remarks>
        public const int MaxNumberOfTargets = 15;

        private const string Json = "json";
        private const string Excel = nameof(Excel);

        private readonly IApiPortService _apiPortService;
        private readonly IProgressReporter _progressReport;
        private readonly ITargetMapper _targetMapper;
        private readonly IDependencyFinder _dependencyFinder;
        private readonly IReportGenerator _reportGenerator;
        private readonly IEnumerable<IgnoreAssemblyInfo> _assembliesToIgnore;
        private readonly IFileWriter _writer;

        public ApiPortClient(IApiPortService apiPortService, IProgressReporter progressReport, ITargetMapper targetMapper, IDependencyFinder dependencyFinder, IReportGenerator reportGenerator, IEnumerable<IgnoreAssemblyInfo> assembliesToIgnore, IFileWriter writer)
        {
            _apiPortService = apiPortService;
            _progressReport = progressReport;
            _targetMapper = targetMapper;
            _dependencyFinder = dependencyFinder;
            _reportGenerator = reportGenerator;
            _assembliesToIgnore = assembliesToIgnore;
            _writer = writer;
        }

        /// <summary>
        /// Retrieve a list of targets available from the service
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<AvailableTarget>> GetTargetsAsync()
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

        /// <summary>
        /// Writes analysis reports to path supplied by options
        /// </summary>
        /// <param name="options"></param>
        /// <returns>Output paths to the reports that were successfully written.</returns>
        public async Task<IEnumerable<string>> WriteAnalysisReportsAsync(IApiPortOptions options)
        {
            var result = await WriteAnalysisReportsAsync(options, false);

            return result.Paths;
        }

        /// <summary>
        /// Writes analysis reports to path supplied by options
        /// </summary>
        /// <param name="options"></param>
        /// <param name="includeResponse"></param>
        /// <returns>Output paths to the reports that were successfully written.</returns>
        public async Task<ReportingResultPaths> WriteAnalysisReportsAsync(IApiPortOptions options, bool includeResponse)
        {
            ValidateOptions(options);

            var jsonAdded = includeResponse ? TryAddJsonToOptions(options, out options) : false;

            foreach (var errorInput in options.InvalidInputFiles)
            {
                _progressReport.ReportIssue(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.InvalidFileName, errorInput));
            }

            var results = await GetAnalysisResultAsync(options);
            var outputPaths = new List<string>();

            AnalyzeResponse response = null;

            foreach (var result in results.Results)
            {
                if (string.Equals(Json, result.Format, StringComparison.OrdinalIgnoreCase))
                {
                    response = result.Data?.Deserialize<AnalyzeResponse>();

                    if (jsonAdded)
                    {
                        continue;
                    }
                }

                var outputPath = await CreateReport(result.Data, options.OutputFileName, result.Format, options.OverwriteOutputFile);

                if (!string.IsNullOrEmpty(outputPath))
                {
                    outputPaths.Add(outputPath);
                }
            }

            return new ReportingResultPaths
            {
                Paths = outputPaths,
                Result = GetReportingResult(results.Request, response, results.Info)
            };
        }

        public async Task<IEnumerable<string>> GetResultFormatsAsync()
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

        /// <summary>
        /// Writes a report given the output format and filename.
        /// </summary>
        /// <returns>null if unable to write the report otherwise, will return the full path to the report.</returns>
        private async Task<string> CreateReport(byte[] result, string suppliedOutputFileName, string outputFormat, bool overwriteFile)
        {
            string filePath = null;

            using (var progressTask = _progressReport.StartTask(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.WritingReport, outputFormat)))
            {
                try
                {
                    filePath = Path.GetFullPath(suppliedOutputFileName);
                }
                catch (Exception ex)
                {
                    _progressReport.ReportIssue(string.Format(CultureInfo.InvariantCulture, ex.Message));
                    progressTask.Abort();

                    return null;
                }

                var outputDirectory = Path.GetDirectoryName(filePath);
                var outputFileName = Path.GetFileName(filePath);
                try
                {
                    var extension = await GetExtensionForFormat(outputFormat);

                    var filename = await _writer.WriteReportAsync(result, extension, outputDirectory, outputFileName, overwriteFile);

                    if (string.IsNullOrEmpty(filename))
                    {
                        _progressReport.ReportIssue(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.CouldNotWriteReport, outputDirectory, outputFileName, extension));
                        progressTask.Abort();

                        return null;
                    }
                    else
                    {
                        return filename;
                    }
                }
                catch (Exception)
                {
                    progressTask.Abort();
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets an analysis report based on the options supplied
        /// </summary>
        /// <param name="options">Options to generate report</param>
        /// <returns>A collection of reports</returns>
        private async Task<MultipleFormatAnalysis> GetAnalysisResultAsync(IApiPortOptions options)
        {
            var assemblies = options.InputAssemblies?.Keys ?? Array.Empty<IAssemblyFile>();
            var dependencyInfo = _dependencyFinder.FindDependencies(assemblies, _progressReport);

            if (dependencyInfo.UserAssemblies.Any())
            {
                AnalyzeRequest request = GenerateRequest(options, dependencyInfo);

                // Create the progress reporter here (instead of within GetResultFromServiceAsync) since the reporter does not work well when run in parallel
                using (var progressTask = _progressReport.StartTask(LocalizedStrings.AnalyzingCompatibility))
                {
                    try
                    {
                        var results = await _apiPortService.SendAnalysisAsync(request, options.OutputFormats);

                        CheckEndpointStatus(results.Headers.Status);

                        return new MultipleFormatAnalysis
                        {
                            Info = dependencyInfo,
                            Request = request,
                            Results = results.Response
                        };
                    }
                    catch (Exception)
                    {
                        progressTask.Abort();
                        throw;
                    }
                }
            }
            else
            {
                _progressReport.ReportIssue(LocalizedStrings.NoFilesToAnalyze);

                return new MultipleFormatAnalysis
                {
                    Results = Enumerable.Empty<ReportingResultWithFormat>()
                };
            }
        }

        private async Task<string> GetExtensionForFormat(string format)
        {
            var outputFormats = await _apiPortService.GetResultFormatsAsync();
            var outputFormat = outputFormats.Response.FirstOrDefault(f => string.Equals(format, f.DisplayName, StringComparison.OrdinalIgnoreCase));

            if (outputFormat == null)
            {
                throw new UnknownReportFormatException(format);
            }

            return outputFormat.FileExtension;
        }

        private AnalyzeRequest GenerateRequest(IApiPortOptions options, IDependencyInfo dependencyInfo)
        {
            // Match the dependencyInfo for each user assembly to the given
            // input assemblies to see whether or not the assembly was explicitly
            // specified.
            foreach (var assembly in dependencyInfo.UserAssemblies)
            {
                // Windows's file paths are case-insensitive
                var matchingAssembly = options.InputAssemblies.FirstOrDefault(x => x.Key.Name.Equals(assembly.Location, StringComparison.OrdinalIgnoreCase));

                // AssemblyInfo is explicitly specified if we found a matching
                // assembly location in the input dictionary AND the value is
                // true.
                assembly.IsExplicitlySpecified = matchingAssembly.Key != default(IAssemblyFile)
                    && matchingAssembly.Value;
            }

            return new AnalyzeRequest
            {
                Targets = options.Targets.SelectMany(_targetMapper.GetNames).ToList(),
                Dependencies = dependencyInfo.Dependencies,
                AssembliesToIgnore = _assembliesToIgnore,                 // We pass along assemblies to ignore instead of filtering them from Dependencies at this point
                                                                          // because breaking change analysis and portability analysis will likely want to filter dependencies
                                                                          // in different ways for ignored assemblies.
                                                                          // For breaking changes, we should show breaking changes for
                                                                          // an assembly if it is un-ignored on any of the user-specified targets and we should hide breaking changes
                                                                          // for an assembly if it ignored on all user-specified targets.
                                                                          // For portability analysis, on the other hand, we will want to show portability for precisely those targets
                                                                          // that a user specifies that are not on the ignore list. In this case, some of the assembly's dependency
                                                                          // information will be needed.
                UnresolvedAssemblies = dependencyInfo.UnresolvedAssemblies.Keys.ToList(),
                UnresolvedAssembliesDictionary = dependencyInfo.UnresolvedAssemblies,
                UserAssemblies = dependencyInfo.UserAssemblies.ToList(),
                AssembliesWithErrors = dependencyInfo.AssembliesWithErrors.ToList(),
                ApplicationName = options.Description,
                Version = AnalyzeRequest.CurrentVersion,
                RequestFlags = options.RequestFlags,
                BreakingChangesToSuppress = options.BreakingChangeSuppressions,
                ReferencedNuGetPackages = options.ReferencedNuGetPackages
            };
        }

        private ReportingResult GetReportingResult(AnalyzeRequest request, AnalyzeResponse response, IDependencyInfo dependencyInfo)
        {
            if (response == null)
            {
                return null;
            }

            using (var progressTask = _progressReport.StartTask(LocalizedStrings.ComputingReport))
            {
                try
                {
                    return _reportGenerator.ComputeReport(
                        response.Targets,
                        response.SubmissionId,
                        request.RequestFlags,
                        dependencyInfo?.Dependencies,
                        response.MissingDependencies,
                        dependencyInfo?.UnresolvedAssemblies,
                        response.UnresolvedUserAssemblies,
                        dependencyInfo?.AssembliesWithErrors,
                        response.NuGetPackages
                    );
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

        /// <summary>
        /// Add JSON to the options object if it is not there. This is used in cases where an analysis
        /// doesn't request the JSON result, but the result is needed for analysis (ie source line mapping)
        /// </summary>
        /// <param name="options"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        private bool TryAddJsonToOptions(IApiPortOptions options, out IApiPortOptions other)
        {
            var outputs = new HashSet<string>(options.OutputFormats, StringComparer.OrdinalIgnoreCase);

            if (outputs.Contains(Json))
            {
                other = options;
                return false;
            }
            else
            {
                outputs.Add(Json);

                other = new ReadWriteApiPortOptions(options)
                {
                    OutputFormats = outputs
                };

                return true;
            }
        }

        /// <summary>
        /// Ensures that the analysis options are valid.  If they are not,
        /// throws a <see cref="InvalidApiPortOptionsException"/>
        /// </summary>
        private static void ValidateOptions(IApiPortOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.Targets.Count() > MaxNumberOfTargets && options.OutputFormats.Contains(Excel, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidApiPortOptionsException(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.TooManyTargetsMessage, MaxNumberOfTargets));
            }
        }

        private struct MultipleFormatAnalysis
        {
            public AnalyzeRequest Request;
            public IDependencyInfo Info;
            public IEnumerable<ReportingResultWithFormat> Results;
        }
    }
}
