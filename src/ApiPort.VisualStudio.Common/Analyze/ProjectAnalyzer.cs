// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using ApiPortVS.Models;
using ApiPortVS.Resources;
using EnvDTE;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Reporting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApiPortVS.Analyze
{
    public class ProjectAnalyzer
    {
        private readonly IFileWriter _reportWriter;
        private readonly IFileSystem _fileSystem;
        private readonly ISourceLineMapper _sourceLineMapper;
        private readonly IErrorListProvider _errorList;
        private readonly IVsApiPortAnalyzer _analyzer;
        private readonly IProjectBuilder _builder;
        private readonly IVSThreadingService _threadingService;
        private readonly IProjectMapper _projectMapper;

        public ProjectAnalyzer(
            IVsApiPortAnalyzer analyzer,
            IErrorListProvider errorList,
            ISourceLineMapper sourceLineMapper,
            IFileWriter reportWriter,
            IFileSystem fileSystem,
            IProjectBuilder builder,
            IProjectMapper projectMapper,
            IVSThreadingService threadingService)
        {
            _analyzer = analyzer;
            _sourceLineMapper = sourceLineMapper;
            _reportWriter = reportWriter;
            _fileSystem = fileSystem;
            _builder = builder;
            _errorList = errorList;
            _projectMapper = projectMapper;
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
                var output = await _builder.GetBuildOutputFilesAsync(project).ConfigureAwait(false);

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

            var dictionary = new ConcurrentBag<CalculatedProject>();

            foreach (var project in projects)
            {
                var outputFiles = await _builder.GetBuildOutputFilesAsync(project).ConfigureAwait(false);
                var hierarchy = await _projectMapper.GetVsHierarchyAsync(project).ConfigureAwait(false);

                dictionary.Add(new CalculatedProject(project, hierarchy, outputFiles ?? Enumerable.Empty<string>()));
            }

            await _errorList.DisplaySourceItemsAsync(sourceItems, dictionary.ToArray()).ConfigureAwait(false);
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
    }
}
