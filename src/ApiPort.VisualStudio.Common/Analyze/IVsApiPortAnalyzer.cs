using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiPortVS.Analyze
{
    public interface IVsApiPortAnalyzer
    {
        Task<ReportingResult> WriteAnalysisReportsAsync(
            IEnumerable<string> inputAssemblyPaths,
            IEnumerable<string> installedPackages,
            IFileWriter reportWriter,
            bool includeJson);
    }
}
