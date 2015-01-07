using System.Collections.Generic;
using System.IO;

namespace Microsoft.Fx.Portability.Analyzer
{
    public interface IDependencyFinder
    {
        IDependencyInfo FindDependencies(IEnumerable<FileInfo> inputAssemblyPaths, IProgressReporter progressReport);
    }
}
