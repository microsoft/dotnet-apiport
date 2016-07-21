using ApiPortVS.Contracts;
using ApiPortVS.Resources;
using ApiPortVS.ViewModels;
using EnvDTE;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.Reporting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPortVS.Analyze
{
    public class ProjectAnalyzer : ApiPortVsAnalyzer
    {
        private readonly IReportViewer _reportViewer;
        private readonly IFileWriter _reportWriter;
        private readonly IServiceProvider _serviceProvider;
        private readonly IFileSystem _fileSystem;
        private readonly ISourceLineMapper _sourceLineMapper;
        private readonly Microsoft.VisualStudio.Shell.ErrorListProvider _errorList;

        public ProjectAnalyzer(ApiPortClient client, OptionsViewModel optionsViewModel, IFileWriter reportWriter, IReportViewer reportViewer, ISourceLineMapper sourceLineMapper, IServiceProvider serviceProvider, IFileSystem fileSystem, Microsoft.VisualStudio.Shell.ErrorListProvider errorList, TextWriter outputWindow, ITargetMapper targetMapper, IProgressReporter reporter, IDependencyFinder dependencyFinder, IApiPortService service, IReportGenerator reportGenerator)
            : base(client, optionsViewModel, outputWindow, targetMapper, reporter, dependencyFinder, service, reportGenerator)
        {
            _reportWriter = reportWriter;
            _reportViewer = reportViewer;
            _sourceLineMapper = sourceLineMapper;
            _serviceProvider = serviceProvider;
            _fileSystem = fileSystem;
            _errorList = errorList;
        }

        public async Task AnalyzeProjectAsync(Project project, string reportFileName)
        {
            var buildSucceeded = await BuildProjectAsync(project);
            if (!buildSucceeded)
            {
                var message = string.Format(LocalizedStrings.UnableToBuildProject, project.Name);
                throw new InvalidOperationException(message);
            }

            // TODO: Add this to the options model
            // Swap the lines below to include all assemblies in the output directory
            //var targetAssemblies = project.GetAssemblyPaths(GetAllAssembliesInDirectory);
            var targetAssemblies = project.GetAssemblyPaths();

            // This call writes the HTML portability report
            var reportPath = await WriteAnalysisReportAsync(targetAssemblies, _reportWriter, project.GetProjectFileDirectory(), reportFileName);

            if (!string.IsNullOrEmpty(reportPath))
            {
                _reportViewer.View(reportPath);
            }

            // This call sets the default targets and highlights any source lines.
            var analysis = await AnalyzeAssembliesAsync(targetAssemblies);
            var sourceItems = await Task.Run(() => _sourceLineMapper.GetSourceInfo(targetAssemblies, analysis));

            DisplaySourceItemsInErrorList(sourceItems, project);
        }

        private async Task<bool> BuildProjectAsync(Project project)
        {
            var builder = new ProjectBuilder(_serviceProvider);
            var completionSource = new TaskCompletionSource<bool>();
            builder.Build(project, completionSource);
            var buildSucceeded = await completionSource.Task; // lack of timeout trusts VS to somehow end the build

            return buildSucceeded;
        }

        protected virtual bool FileHasAnalyzableExtension(string fileName)
        {
            var extension = _fileSystem.GetFileExtension(fileName);

            bool analyzable = (string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase)
                               || string.Equals(extension, ".exe", StringComparison.OrdinalIgnoreCase))
                              && fileName.IndexOf("vshost", StringComparison.OrdinalIgnoreCase) == -1;

            return analyzable;
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
                var hierarchy = project.GetHierarchy(_serviceProvider);

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
