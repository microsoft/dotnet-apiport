// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using ApiPortVS.Resources;
using EnvDTE;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using VisualStudio = Microsoft.VisualStudio.Shell;

namespace ApiPortVS.Analyze
{
    public class ProjectAnalyzer
    {
        private readonly IFileWriter _reportWriter;
        private readonly IFileSystem _fileSystem;
        private readonly ISourceLineMapper _sourceLineMapper;
        private readonly Microsoft.VisualStudio.Shell.ErrorListProvider _errorList;
        private readonly IVsApiPortAnalyzer _analyzer;
        private readonly ProjectBuilder _builder;
        private readonly IVSThreadingService _threadingService;

        public ProjectAnalyzer(
            IVsApiPortAnalyzer analyzer,
            Microsoft.VisualStudio.Shell.ErrorListProvider errorList,
            ISourceLineMapper sourceLineMapper,
            IFileWriter reportWriter,
            IFileSystem fileSystem,
            ProjectBuilder builder,
            IVSThreadingService threadingService)
        {
            _analyzer = analyzer;
            _sourceLineMapper = sourceLineMapper;
            _reportWriter = reportWriter;
            _fileSystem = fileSystem;
            _builder = builder;
            _errorList = errorList;
            _threadingService = threadingService;
        }

        public async Task AnalyzeProjectAsync(ICollection<Project> projects, CancellationToken cancellationToken = default(CancellationToken))
        {
            var buildSucceeded = await _builder.BuildAsync(projects).ConfigureAwait(false);

            if (!buildSucceeded)
            {
                throw new PortabilityAnalyzerException(LocalizedStrings.UnableToBuildProject);
            }

            // TODO: Add option to include everything in output, not just build artifacts
            var targetAssemblies = new ConcurrentBag<string>();

            foreach (var project in projects)
            {
                var output = await project.GetBuildOutputFilesAsync().ConfigureAwait(false);

                // Could not find any output files for this. Skip it.
                if (output == null)
                {
                    continue;
                }

                foreach (var file in output)
                {
                    targetAssemblies.Add(file);
                }
            }

            if (!targetAssemblies.Any())
            {
                throw new PortabilityAnalyzerException(LocalizedStrings.FailedToLocateBuildOutputDir);
            }

            var result = await _analyzer.WriteAnalysisReportsAsync(targetAssemblies, _reportWriter, true).ConfigureAwait(false);

            var sourceItems = await Task.Run(() => _sourceLineMapper.GetSourceInfo(targetAssemblies, result)).ConfigureAwait(false);

            await DisplaySourceItemsInErrorList(sourceItems, projects).ConfigureAwait(false);
        }

        public bool FileHasAnalyzableExtension(string fileName)
        {
            var extension = _fileSystem.GetFileExtension(fileName);

            return (string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(extension, ".exe", StringComparison.OrdinalIgnoreCase))
                    && fileName.IndexOf("vshost", StringComparison.OrdinalIgnoreCase) == -1;
        }

        private IEnumerable<string> GetAllAssembliesInDirectory(string directory)
        {
            try
            {
                var files = _fileSystem.FilesInDirectory(directory);
                return files.Where(file => FileHasAnalyzableExtension(file));
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        private async Task DisplaySourceItemsInErrorList(IEnumerable<ISourceMappedItem> items, ICollection<Project> projects)
        {
            if (!items.Any())
            {
                return;
            }

            await _threadingService.SwitchToMainThreadAsync();

            _errorList.Tasks.Clear();
            _errorList.Refresh();
            _errorList.SuspendRefresh();

            var projectWithOutputMappings = new ConcurrentDictionary<string, IVsHierarchy>();

            foreach (var project in projects)
            {
                var outputs = await project.GetBuildOutputFilesAsync().ConfigureAwait(false);

                if (outputs == null)
                {
                    continue;
                }

                var hierarchy = project.GetHierarchy();

                foreach (var output in outputs)
                {
                    projectWithOutputMappings.AddOrUpdate(output, hierarchy, (existingKey, existingValue) => hierarchy);
                }
            }

            await _threadingService.SwitchToMainThreadAsync();

            try
            {
                var defaultHierarchy = projects.First().GetHierarchy();

                foreach (var item in items)
                {
                    if (!_fileSystem.FileExists(item.Path))
                    {
                        continue;
                    }

                    if (!projectWithOutputMappings.TryGetValue(item.Assembly, out IVsHierarchy hierarchy))
                    {
                        hierarchy = defaultHierarchy;
                    }

                    var errorWindowTask = item.GetErrorWindowTask(hierarchy);
                    var result = _errorList.Tasks.Add(errorWindowTask);
                }
            }
            finally
            {
                _errorList.ResumeRefresh();
            }

            // Outside the finally because it will obscure errors reported on the output window
            _errorList.BringToFront();
        }
    }
}
