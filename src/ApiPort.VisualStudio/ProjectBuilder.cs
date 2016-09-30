// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System.Threading.Tasks;

namespace ApiPortVS
{
    public class ProjectBuilder
    {
        private IVsSolutionBuildManager _buildManager;

        public ProjectBuilder(IVsSolutionBuildManager buildManager)
        {
            _buildManager = buildManager;
        }

        public Task<bool> BuildAsync(Project project)
        {
            var projectHierarchy = project.GetHierarchy();
            int suppressUI = 0;
            uint buildUpdateFlags = (uint)(VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD);
            uint defQueryResults = 0; // enumerated in VSSOLNBUILDQUERYRESULTS

            // launches an asynchronous build operation, returns S_OK immediately if the build begins
            // => S_OK does not indicate completion or success of the build
            var updateErrCode = _buildManager.StartSimpleUpdateProjectConfiguration(projectHierarchy, null, null,
                buildUpdateFlags, defQueryResults, suppressUI);

            var tcs = new TaskCompletionSource<bool>();

            if (updateErrCode == VSConstants.S_OK)
            {
                var builder = new ProjectAsyncBuilder(_buildManager, tcs);
                _buildManager.AdviseUpdateSolutionEvents(builder, out builder.UpdateSolutionEventsCookie);
            }
            else
            {
                tcs.SetResult(false);
            }

            return tcs.Task;
        }

        private class ProjectAsyncBuilder : TaskCompletionSource<bool>, IVsUpdateSolutionEvents
        {
            private readonly TaskCompletionSource<bool> _completionSource;
            private readonly IVsSolutionBuildManager _buildManager;

            public uint UpdateSolutionEventsCookie;

            public ProjectAsyncBuilder(IVsSolutionBuildManager manager, TaskCompletionSource<bool> completionSource)
            {
                _buildManager = manager;
                _completionSource = completionSource;
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
                _buildManager.UnadviseUpdateSolutionEvents(UpdateSolutionEventsCookie);
                _completionSource.SetResult(false);

                return VSConstants.S_OK;
            }

            // called when entire solution is done building
            public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
            {
                _buildManager.UnadviseUpdateSolutionEvents(UpdateSolutionEventsCookie);
                var buildSucceeded = fSucceeded == 1; // no update actions failed
                _completionSource.SetResult(buildSucceeded);

                return VSConstants.S_OK;
            }

            public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
            {
                return VSConstants.S_OK;
            }
        }
    }
}