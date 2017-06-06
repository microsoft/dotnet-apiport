using ApiPortVS.Contracts;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ApiPortVS.VS2015
{
    public class ProjectBuilder : IProjectBuilder
    {
        public Task<bool> BuildAsync(IEnumerable<Project> projects) => throw new NotImplementedException();
        public Task<IEnumerable<string>> GetBuildOutputFilesAsync(Project project, CancellationToken cancellationToken = default(CancellationToken)) => throw new NotImplementedException();
    }
}
