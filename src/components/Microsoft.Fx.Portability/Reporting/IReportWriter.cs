using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Reporting
{
    public interface IReportWriter
    {
        Task<string> WriteReportAsync(byte[] report, ResultFormat format, string outputDirectory, string filename, bool overwrite);
    }
}
