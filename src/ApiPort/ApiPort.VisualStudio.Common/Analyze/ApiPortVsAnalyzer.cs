// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using ApiPortVS.Resources;
using ApiPortVS.ViewModels;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPortVS.Analyze
{
    public class ApiPortVsAnalyzer : IVsApiPortAnalyzer
    {
        private readonly ApiPortClient _client;
        private readonly OptionsViewModel _optionsViewModel;
        private readonly IOutputWindowWriter _outputWindow;
        private readonly IProgressReporter _reporter;
        private readonly IReportViewer _viewer;

        public ApiPortVsAnalyzer(
            ApiPortClient client,
            OptionsViewModel optionsViewModel,
            IOutputWindowWriter outputWindow,
            IReportViewer viewer,
            IProgressReporter reporter)
        {
            _client = client;
            _optionsViewModel = optionsViewModel;
            _outputWindow = outputWindow;
            _viewer = viewer;
            _reporter = reporter;
        }

        public async Task<ReportingResult> WriteAnalysisReportsAsync(
            IEnumerable<string> assemblyPaths,
            IEnumerable<string> installedPackages,
            IFileWriter reportWriter,
            bool includeJson)
        {
            await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            await _outputWindow.ShowWindowAsync();

            await _optionsViewModel.UpdateAsync();

            var reportDirectory = _optionsViewModel.OutputDirectory;
            var outputFormats = _optionsViewModel.Formats.Where(f => f.IsSelected).Select(f => f.DisplayName);
            var reportFileName = _optionsViewModel.DefaultOutputName;

            var analysisOptions = await GetOptionsAsync(assemblyPaths, outputFormats, installedPackages, Path.Combine(reportDirectory, reportFileName));
            var issuesBefore = _reporter.Issues.Count;

            var result = await _client.WriteAnalysisReportsAsync(analysisOptions, includeJson);

            if (!result.Paths.Any())
            {
                var issues = _reporter.Issues.ToArray();

                for (var i = issuesBefore; i < issues.Length; i++)
                {
                    _outputWindow.WriteLine(LocalizedStrings.ListItem, issues[i]);
                }
            }

            await _viewer.ViewAsync(result.Paths);

            return result.Result;
        }

        private async Task<IApiPortOptions> GetOptionsAsync(IEnumerable<string> assemblyPaths, IEnumerable<string> formats, IEnumerable<string> referencedNugetPackages, string reportFileName)
        {
            await _optionsViewModel.UpdateAsync().ConfigureAwait(false);
            await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            foreach (var invalidPlatform in _optionsViewModel.InvalidTargets)
            {
                if (invalidPlatform.Versions.Any(v => v.IsSelected))
                {
                    var message = string.Format(CultureInfo.CurrentCulture, LocalizedStrings.InvalidPlatformSelectedFormat, invalidPlatform.Name);
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

            // NuGet packages referenced in the project system are not
            // explicitly passed in, so we'll not want to see their portability
            // statistics.
            return new AnalysisOptions(
                description,
                assemblyPaths,
                targets,
                formats,
                referencedNugetPackages,
                !_optionsViewModel.SaveMetadata,
                reportFileName,
                isAssemblySpecified: false);
        }
    }
}
