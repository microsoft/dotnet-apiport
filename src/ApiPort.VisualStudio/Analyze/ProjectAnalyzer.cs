// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using ApiPortVS.Resources;
using EnvDTE;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPortVS.Analyze
{
    public class ProjectAnalyzer
    {
        private readonly IFileWriter _reportWriter;
        private readonly IServiceProvider _serviceProvider;
        private readonly IFileSystem _fileSystem;
        private readonly ISourceLineMapper _sourceLineMapper;
        private readonly Microsoft.VisualStudio.Shell.ErrorListProvider _errorList;
        private readonly IVsApiPortAnalyzer _analyzer;
        private readonly ProjectBuilder _builder;

        public ProjectAnalyzer(
            IVsApiPortAnalyzer analyzer,
            Microsoft.VisualStudio.Shell.ErrorListProvider errorList,
            ISourceLineMapper sourceLineMapper,
            IFileWriter reportWriter,
            IFileSystem fileSystem,
            ProjectBuilder builder,
            ITargetMapper targetMapper)
        {
            _analyzer = analyzer;
            _sourceLineMapper = sourceLineMapper;
            _reportWriter = reportWriter;
            _fileSystem = fileSystem;
            _builder = builder;
            _errorList = errorList;
        }

        public async Task AnalyzeProjectAsync(Project project)
        {
            var buildSucceeded = await _builder.BuildAsync(project);
            if (!buildSucceeded)
            {
                var message = string.Format(LocalizedStrings.UnableToBuildProject, project.Name);
                throw new InvalidOperationException(message);
            }

            // TODO: Add this to the options model
            // Swap the lines below to include all assemblies in the output directory
            //var targetAssemblies = project.GetAssemblyPaths(GetAllAssembliesInDirectory);
            var targetAssemblies = project.GetAssemblyPaths();

            var result = await _analyzer.WriteAnalysisReportsAsync(targetAssemblies, _reportWriter, true);

            var sourceItems = await Task.Run(() => _sourceLineMapper.GetSourceInfo(targetAssemblies, result));

            DisplaySourceItemsInErrorList(sourceItems, project);
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

        private void DisplaySourceItemsInErrorList(IEnumerable<ISourceMappedItem> items, Project project)
        {
            _errorList.Tasks.Clear();
            _errorList.Refresh();
            _errorList.SuspendRefresh();

            try
            {
                var hierarchy = project.GetHierarchy();

                foreach (var item in items)
                {
                    if (!_fileSystem.FileExists(item.Path))
                    {
                        continue;
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
