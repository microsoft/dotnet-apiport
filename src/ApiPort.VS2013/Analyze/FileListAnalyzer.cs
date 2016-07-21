using ApiPortVS.Contracts;
using ApiPortVS.ViewModels;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.Reporting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPortVS.Analyze
{
    public class FileListAnalyzer : ApiPortVsAnalyzer
    {
        private readonly IFileSystem _fileSystem;
        private readonly IReportViewer _reportViewer;
        private readonly IFileWriter _reportWriter;

        public FileListAnalyzer(ApiPortClient client, OptionsViewModel optionsViewModel, IFileSystem fileSystem, IFileWriter reportWriter, IReportViewer reportViewer, TextWriter outputWindow, ITargetMapper targetMapper, IProgressReporter reporter, IDependencyFinder dependencyFinder, IApiPortService service, IReportGenerator reportGenerator)
            : base(client, optionsViewModel, outputWindow, targetMapper, reporter, dependencyFinder, service, reportGenerator)
        {
            _fileSystem = fileSystem;
            _reportWriter = reportWriter;
            _reportViewer = reportViewer;
        }

        public async Task AnalyzeProjectAsync(IEnumerable<string> inputAssemblyPaths, string reportFileName)
        {
            // write report to same directory as input
            var dirForReport = _fileSystem.GetDirectoryNameFromPath(inputAssemblyPaths.First());
            var reportPath = await WriteAnalysisReportAsync(inputAssemblyPaths, _reportWriter, dirForReport, reportFileName);

            if (!string.IsNullOrEmpty(reportPath))
            {
                _reportViewer.View(reportPath);
            }

            // This second call sets the default targets and highlights any source lines.
            await AnalyzeAssembliesAsync(inputAssemblyPaths);
        }
    }
}
