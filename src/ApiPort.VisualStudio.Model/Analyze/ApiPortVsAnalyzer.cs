// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using ApiPortVS.Resources;
using ApiPortVS.ViewModels;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Collections.Generic;
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
            IFileWriter reportWriter,
            bool includeJson)
        {
            await _outputWindow.ShowWindowAsync().ConfigureAwait(false);

            await _optionsViewModel.UpdateAsync().ConfigureAwait(false);

            var reportDirectory = _optionsViewModel.OutputDirectory;
            var outputFormats = _optionsViewModel.Formats.Where(f => f.IsSelected).Select(f => f.DisplayName);
            var reportFileName = _optionsViewModel.DefaultOutputName;

            var analysisOptions = await GetApiPortOptions(assemblyPaths, outputFormats, Path.Combine(reportDirectory, reportFileName)).ConfigureAwait(false);
            var issuesBefore = _reporter.Issues.Count;

            var result = await _client.WriteAnalysisReportsAsync(analysisOptions, includeJson).ConfigureAwait(false);

            if (!result.Paths.Any())
            {
                var issues = _reporter.Issues.ToArray();

                await VisualStudio.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                for (int i = issuesBefore; i < issues.Length; i++)
                {
                    _outputWindow.WriteLine(LocalizedStrings.ListItem, issues[i]);
                }
            }

            await _viewer.ViewAsync(result.Paths).ConfigureAwait(false);

            return result.Result;
        }

        private async Task<IApiPortOptions> GetApiPortOptions(IEnumerable<string> assemblyPaths, IEnumerable<string> formats, string reportFileName)
        {
            await _optionsViewModel.UpdateAsync().ConfigureAwait(false);

            foreach (var invalidPlatform in _optionsViewModel.InvalidTargets)
            {
                if (invalidPlatform.Versions.Any(v => v.IsSelected))
                {
                    await VisualStudio.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

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
                await VisualStudio.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                _outputWindow.WriteLine(LocalizedStrings.UsingDefaultTargets);
                _outputWindow.WriteLine(LocalizedStrings.TargetSelectionGuidance);
            }

            // TODO: Allow setting description
            string description = null;

            return new AnalysisOptions(
                description,
                assemblyPaths,
                targets,
                formats,
                !_optionsViewModel.SaveMetadata,
                reportFileName);
        }
    }
}
