using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ApiPortVS.Contracts
{
    public interface IProjectBuilder
    {
        Task<bool> BuildAsync(IEnumerable<Project> projects);

        Task<IEnumerable<string>> GetBuildOutputFilesAsync(Project project, CancellationToken cancellationToken = default(CancellationToken));
    }
}
