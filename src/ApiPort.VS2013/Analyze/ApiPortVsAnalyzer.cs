using ApiPortVS.Resources;
using ApiPortVS.ViewModels;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPortVS.Analyze
{
    public class ApiPortVsAnalyzer
    {
        private readonly ApiPortClient _client;
        private readonly OptionsViewModel _optionsViewModel;
        private readonly TextWriter _outputWindow;
        private readonly ITargetMapper _targetMapper;
        private readonly IProgressReporter _reporter;
        private readonly IDependencyFinder _dependencyFinder;
        private readonly IApiPortService _apiPortService;
        private readonly IReportGenerator _reportGenerator;

        public ApiPortVsAnalyzer(ApiPortClient client, OptionsViewModel optionsViewModel, TextWriter outputWindow, ITargetMapper targetMapper, IProgressReporter reporter, IDependencyFinder dependencyFinder, IApiPortService service, IReportGenerator reportGenerator)
        {
            _client = client;
            _optionsViewModel = optionsViewModel;
            _outputWindow = outputWindow;
            _targetMapper = targetMapper;
            _reporter = reporter;
            _dependencyFinder = dependencyFinder;
            _apiPortService = service;
            _reportGenerator = reportGenerator;
        }

        protected async Task<ReportingResult> AnalyzeAssembliesAsync(IEnumerable<string> assemblyPaths)
        {
            var paths = assemblyPaths.Select(t => new AssemblyFile(t));
            var dependencyInfo = _dependencyFinder.FindDependencies(paths, _reporter);

            var analysisOptions = await GetApiPortOptions(assemblyPaths, "json");

            var request = GenerateRequest(analysisOptions, dependencyInfo);
            var result = await GetResultsAsync(request, dependencyInfo);

            // For consistency, if a user analyzes without selecting targets, select the service's defaults afterward
            if (!analysisOptions.Targets.Any())
            {
                SelectPlatformsFromReportingResult(result);
            }

            return result;
        }

        private AnalyzeRequest GenerateRequest(IApiPortOptions options, IDependencyInfo dependencyInfo)
        {
            // TODO: This will be a public method on ApiPortClient in Microsoft.Fx.Portability v1.0.1
            return new AnalyzeRequest
            {
                Targets = options.Targets.SelectMany(alias => _targetMapper.GetNames(alias)).ToList(),
                Dependencies = dependencyInfo.Dependencies,
                AssembliesToIgnore = Enumerable.Empty<IgnoreAssemblyInfo>(),
                UnresolvedAssemblies = dependencyInfo.UnresolvedAssemblies.Keys.ToList(),
                UnresolvedAssembliesDictionary = dependencyInfo.UnresolvedAssemblies,
                UserAssemblies = dependencyInfo.UserAssemblies.ToList(),
                AssembliesWithErrors = dependencyInfo.AssembliesWithErrors.ToList(),
                ApplicationName = options.Description,
                Version = AnalyzeRequest.CurrentVersion,
                RequestFlags = options.RequestFlags
            };
        }

        private async Task<ReportingResult> GetResultsAsync(AnalyzeRequest request, IDependencyInfo dependencyInfo)
        {
            using (var progressTask = _reporter.StartTask(Microsoft.Fx.Portability.Resources.LocalizedStrings.AnalyzingCompatibility))
            {
                try
                {
                    var fullResponse = await _apiPortService.SendAnalysisAsync(request);
                    var response = fullResponse.Response;

                    var dependencies = dependencyInfo == null ? null : dependencyInfo.Dependencies
                        .ToDictionary(
                            entry => response.MissingDependencies.FirstOrDefault(m => m == entry.Key) ?? entry.Key,
                            entry => entry.Value
                        );

                    return _reportGenerator.ComputeReport(
                       response.Targets,
                       response.SubmissionId,
                       request.RequestFlags,
                       dependencies,
                       response.MissingDependencies,
                       dependencyInfo == null ? null : dependencyInfo.UnresolvedAssemblies,
                       response.UnresolvedUserAssemblies,
                       dependencyInfo == null ? null : dependencyInfo.AssembliesWithErrors
                   );
                }
                catch (Exception)
                {
                    progressTask.Abort();
                    throw;
                }
            }
        }
        protected async Task<string> WriteAnalysisReportAsync(
            IEnumerable<string> assemblyPaths,
            IFileWriter reportWriter,
            string reportDirectory,
            string reportFileName)
        {
            // TODO: This is currently hard coded - we should expose a way to ask for more types of reports
            var outputFormat = "html";

            var analysisOptions = await GetApiPortOptions(assemblyPaths, outputFormat, Path.Combine(reportDirectory, reportFileName));

            var issuesBefore = _reporter.Issues.Count;

            var result = await _client.WriteAnalysisReportsAsync(analysisOptions);

            // TODO: Support multiple reports in VS extension
            var filename = result.FirstOrDefault();
            
            if (string.IsNullOrEmpty(filename))
            {
                var issues = _reporter.Issues.ToArray();

                for (int i = issuesBefore; i < issues.Length; i++)
                {
                    _outputWindow.WriteLine(LocalizedStrings.ListItem, issues[i]);
                }
            }
            else
            {
                _outputWindow.WriteLine(LocalizedStrings.ListItem, string.Format(LocalizedStrings.ReportLocation, filename));
            }

            return filename;
        }

        protected void SelectPlatformsFromReportingResult(ReportingResult analysis)
        {
            foreach (var frameworkName in analysis.Targets)
            {
                var platform = _optionsViewModel.Targets.FirstOrDefault(p => StringComparer.Ordinal.Equals(p.Name, frameworkName.Identifier));

                if (platform != null)
                {
                    var version = platform.Versions.FirstOrDefault(v => StringComparer.Ordinal.Equals(v.Version, frameworkName.Version.ToString()));

                    if (version != null)
                    {
                        version.IsSelected = true;
                    }
                }
            }
        }

        private async Task<IApiPortOptions> GetApiPortOptions(IEnumerable<string> assemblyPaths, string format, string reportFileName = AnalysisOptions.DefaultReportFilename)
        {
            var invalidPlatforms = await _optionsViewModel.UpdateTargets();

            foreach (var invalidPlatform in invalidPlatforms)
            {
                if (invalidPlatform.Versions.Any(v => v.IsSelected))
                {
                    var message = string.Format(LocalizedStrings.InvalidPlatformSelectedFormat, invalidPlatform.Name);
                    _outputWindow.WriteLine(message);
                }
            }

            var targets = _optionsViewModel.Targets
                .SelectMany(p => p.Versions.Where(v => v.IsSelected))
                .Select(p => p.ToString())
                .ToList();

            if (!targets.Any())
            {
                _outputWindow.WriteLine(LocalizedStrings.UsingDefaultTargets);
                _outputWindow.WriteLine(LocalizedStrings.TargetSelectionGuidance);
            }

            // TODO: Allow setting description
            string description = null;

            return new AnalysisOptions(
                description,
                assemblyPaths,
                targets.SelectMany(_targetMapper.GetNames),
                format,
                !_optionsViewModel.SaveMetadata,
                reportFileName);
        }
    }
}
