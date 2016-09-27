// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        private IVsSolutionBuildManager _buildManager;
        private IServiceProvider _serviceProvider;
        private uint _updateSolutionEventsCookie;
        private TaskCompletionSource<bool> _completionSource;

        public ProjectBuilder(IServiceProvider serviceProvider)
        {
            Debug.Assert(serviceProvider != null);

            _serviceProvider = serviceProvider;
            _buildManager = serviceProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
        }

        public void Build(Project project, TaskCompletionSource<bool> completionSource)
        {
            _completionSource = completionSource;

            var projectHierarchy = project.GetHierarchy(_serviceProvider);
            int suppressUI = 0;
            uint buildUpdateFlags = (uint)(VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD);
            uint defQueryResults = 0; // enumerated in VSSOLNBUILDQUERYRESULTS

            // launches an asynchronous build operation, returns S_OK immediately if the build begins
            // => S_OK does not indicate completion or success of the build
            var updateErrCode = _buildManager.StartSimpleUpdateProjectConfiguration(projectHierarchy, null, null,
                buildUpdateFlags, defQueryResults, suppressUI);

            if (updateErrCode == VSConstants.S_OK)
            {
                _buildManager.AdviseUpdateSolutionEvents(this, out _updateSolutionEventsCookie);
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
            _buildManager.UnadviseUpdateSolutionEvents(_updateSolutionEventsCookie);
            _completionSource.SetResult(false);

            return VSConstants.S_OK;
        }

        // called when entire solution is done building
        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            _buildManager.UnadviseUpdateSolutionEvents(_updateSolutionEventsCookie);
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
