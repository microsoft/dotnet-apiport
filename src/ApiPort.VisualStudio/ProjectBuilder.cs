// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPortVS
{
    public class ProjectBuilder
    {
        private IVsSolutionBuildManager2 _buildManager;

        public ProjectBuilder(IVsSolutionBuildManager2 buildManager)
        {
            _buildManager = buildManager;
        }

        public Task<bool> BuildAsync(ICollection<Project> projects)
        {
            var projectHierarchy = projects.Select(project => project.GetHierarchy()).ToArray()
            var suppressUI = 0;
            var buildUpdateFlags = Enumerable.Repeat((uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD, projectHierarchy.Length).ToArray();

            // launches an asynchronous build operation, returns S_OK immediately if the build begins
            // => S_OK does not indicate completion or success of the build
            var updateErrorCode = _buildManager.StartUpdateSpecificProjectConfigurations((uint)projects.Count, projectHierarchy, null, null, buildUpdateFlags, null, (uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD, suppressUI);

            var tcs = new TaskCompletionSource<bool>();

            if (updateErrorCode == VSConstants.S_OK)
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