using ApiPortVS.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System.Threading;

namespace ApiPortVS.VS2017
{
    public class ProjectBuilder : IProjectBuilder
    {
        private readonly IVSThreadingService _threadingService;

        public ProjectBuilder(IVSThreadingService threadingService)
        {
            _threadingService = threadingService;
        }

        public Task<bool> BuildAsync(IEnumerable<Project> projects) => throw new NotImplementedException();
        public Task<IEnumerable<string>> GetBuildOutputFilesAsync(Project project, CancellationToken cancellationToken = default(CancellationToken)) => throw new NotImplementedException();
        public Task<IVsHierarchy> GetVsHierarchyAsync(Project project) => throw new NotImplementedException();
    }
}
