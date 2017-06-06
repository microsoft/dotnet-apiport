using ApiPortVS.Contracts;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Threading.Tasks;
using VisualStudio = Microsoft.VisualStudio.Shell;

namespace ApiPortVS
{
    /// <summary>
    /// You need to switch to the UI thread before calling any COM interfaces (e.g. IVsHierarchy) using
    /// </summary>
    public class COMProjectMapper : IProjectMapper
    {
        private readonly IVSThreadingService _threadingService;

        public COMProjectMapper(IVSThreadingService threadingService)
        {
            _threadingService = threadingService;
        }

        /// <summary>
        /// Gets the VSHierarchy using COM calls to the package service.
        /// </summary>
        public async Task<IVsHierarchy> GetVsHierarchyAsync(Project project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            await _threadingService.SwitchToMainThreadAsync();

            var solution = VisualStudio.Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            if (solution == null)
            {
                return null;
            }

            IVsHierarchy hierarchy;
            solution.GetProjectOfUniqueName(project.FullName, out hierarchy);

            return hierarchy;
        }
    }
}
