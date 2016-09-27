using System;
using System.Diagnostics;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace ApiPortVS
{
    public class ProjectBuilder : IVsUpdateSolutionEvents
    {
        private IVsSolutionBuildManager buildManager;
        private IServiceProvider serviceProvider;
        private uint updateSolutionEventsCookie;
        private TaskCompletionSource<bool> completionSource;
        
        public ProjectBuilder(IServiceProvider serviceProvider)
        {
            Debug.Assert(serviceProvider != null);

            this.serviceProvider = serviceProvider;
            buildManager = serviceProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
        }

        public void Build(Project project, TaskCompletionSource<bool> completionSource)
        {
            this.completionSource = completionSource;

            var projectHierarchy = project.GetHierarchy(serviceProvider);
            int suppressUI = 0;
            uint buildUpdateFlags = (uint)(VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD);
            uint defQueryResults = 0; // enumerated in VSSOLNBUILDQUERYRESULTS

            // launches an asynchronous build operation, returns S_OK immediately if the build begins
            // => S_OK does not indicate completion or success of the build
            var updateErrCode = buildManager.StartSimpleUpdateProjectConfiguration(projectHierarchy, null, null,
                buildUpdateFlags, defQueryResults, suppressUI);

            if (updateErrCode == VSConstants.S_OK)
            {
                buildManager.AdviseUpdateSolutionEvents(this, out updateSolutionEventsCookie);
            }
            else
            {
                completionSource.SetResult(false);
            }
        }

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Cancel()
        {
            buildManager.UnadviseUpdateSolutionEvents(updateSolutionEventsCookie);
            completionSource.SetResult(false);

            return VSConstants.S_OK;
        }

        // called when entire solution is done building
        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            buildManager.UnadviseUpdateSolutionEvents(updateSolutionEventsCookie);
            var buildSucceeded = fSucceeded == 1; // no update actions failed
            completionSource.SetResult(buildSucceeded);

            return VSConstants.S_OK;
        }

        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }
    }
}
