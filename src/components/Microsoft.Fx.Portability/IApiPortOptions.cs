using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Fx.Portability
{
    public interface IApiPortOptions
    {
        string Description { get; }
        IEnumerable<FileInfo> InputAssemblies { get; }
        bool NoTelemetry { get; }
        IEnumerable<string> Targets { get; }
        ResultFormat OutputFormat { get; }
    }
}