// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Contracts;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static Microsoft.Fx.Portability.Utils.FormattableStringHelper;
using static Microsoft.VisualStudio.VSConstants;

namespace ApiPortVS
{
    /// <summary>
    /// Default project builder that has existed before Common Project System.
    /// </summary>
    public class DefaultProjectBuilder : IProjectBuilder
    {
        private readonly IVsSolutionBuildManager2 _buildManager;
        private readonly IVSThreadingService _threadingService;

        public DefaultProjectBuilder(
            IVsSolutionBuildManager2 buildManager,
            IVSThreadingService threadingService,
            IProjectMapper projectMapper)
        {
            ProjectMapper = projectMapper;
            _threadingService = threadingService;
            _buildManager = buildManager;
        }

        protected IProjectMapper ProjectMapper { get; }

        public virtual async Task<bool> BuildAsync(IEnumerable<Project> projects)
        {
            if (projects == null)
            {
                throw new ArgumentNullException(nameof(projects));
            }

            if (!projects.Any())
            {
                return true;
            }

            var bag = new ConcurrentBag<IVsHierarchy>();

            foreach (var project in projects)
            {
                var hierarchy = await ProjectMapper.GetVsHierarchyAsync(project).ConfigureAwait(false);
                bag.Add(hierarchy);
            }

            var hierarchies = bag.ToArray();

            var buildUpdateFlags = Enumerable.Repeat((uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD, hierarchies.Length).ToArray();

            // Launches an asynchronous build operation and returns S_OK immediately if the build begins.
            // The result does not indicate completion or success of the build
            var updateErrorCode = _buildManager.StartUpdateSpecificProjectConfigurations(
                (uint)projects.Count(),
                hierarchies,
                null,
                null,
                buildUpdateFlags,
                null,
                (uint)VSSOLNBUILDUPDATEFLAGS.SBF_OPERATION_BUILD,
                0);

            var tcs = new TaskCompletionSource<bool>();

            if (updateErrorCode == S_OK)
            {
                var builder = new ProjectAsyncBuilder(_buildManager, tcs);
                _buildManager.AdviseUpdateSolutionEvents(builder, out builder.UpdateSolutionEventsCookie);
            }
            else
            {
                tcs.SetResult(false);
            }

            return await tcs.Task;
        }

        public virtual async Task<IEnumerable<string>> GetBuildOutputFilesAsync(Project project, CancellationToken cancellationToken = default)
        {
            var configuration = await ProjectMapper.GetVsProjectConfigurationAsync(project).ConfigureAwait(false);

            if (configuration == null)
            {
                return null;
            }

            if (!(configuration is IVsProjectCfg2 configuration2))
            {
                Trace.TraceError(ToCurrentCulture($"IVsCfg returned {configuration.GetType()} is not of the right type. Expected: {nameof(IVsProjectCfg2)}"));
                return null;
            }

            await _threadingService.SwitchToMainThreadAsync();

            if (ErrorHandler.Failed(configuration2.OpenOutputGroup(Common.Constants.OutputGroups.BuiltProject, out IVsOutputGroup outputGroup)))
            {
                Trace.TraceError(ToCurrentCulture($"Could not retrieve {nameof(IVsOutputGroup)} from project: {project.Name}"));
                return null;
            }

            if (!(outputGroup is IVsOutputGroup2 outputGroup2))
            {
                Trace.TraceError(ToCurrentCulture($"Could not retrieve {nameof(IVsOutputGroup2)} from project: {project.Name}"));
                return null;
            }

            if (ErrorHandler.Failed(outputGroup2.get_KeyOutputObject(out IVsOutput2 keyGroup)))
            {
                Trace.TraceError(ToCurrentCulture($"Could not retrieve {nameof(IVsOutput2)} from project: {project.Name}"));
                return null;
            }

            if (ErrorHandler.Succeeded(keyGroup.get_Property(Common.Constants.MetadataNames.OutputLocation, out object outputLoc)))
            {
                return new[] { outputLoc as string };
            }

            if (ErrorHandler.Succeeded(keyGroup.get_Property(Common.Constants.MetadataNames.FinalOutputPath, out object finalOutputPath)))
            {
                return new[] { finalOutputPath as string };
            }

            return null;
        }

        private class ProjectAsyncBuilder : IVsUpdateSolutionEvents
        {
            private readonly TaskCompletionSource<bool> _completionSource;
            private readonly IVsSolutionBuildManager _buildManager;

#pragma warning disable SA1401 // Cookie is required by the DefaultProjectBuilder
            /// <summary>
            /// A cookie used to track this instance in IVsSolutionBuildManager solution events.
            /// </summary>
            internal uint UpdateSolutionEventsCookie;
#pragma warning restore SA1401

            public ProjectAsyncBuilder(IVsSolutionBuildManager manager, TaskCompletionSource<bool> completionSource)
            {
                _buildManager = manager;
                _completionSource = completionSource;
            }

            /// <summary>
            /// Called when the active project configuration for a project in the solution has changed.
            /// </summary>
            /// <param name="pIVsHierarchy">Pointer to an IVsHierarchy object.</param>
            /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
            public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) => S_OK;

            /// <summary>
            /// Called before any build actions have begun. This is the last chance to cancel the build before any building begins.
            /// </summary>
            /// <param name="pfCancelUpdate">Pointer to a flag indicating cancel update.</param>
            /// <returns></returns>
            public int UpdateSolution_Begin(ref int pfCancelUpdate) => S_OK;

            /// <summary>
            /// Called when a build is being cancelled.
            /// </summary>
            /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
            public int UpdateSolution_Cancel() => S_OK;

            /// <summary>
            /// Called when entire solution is done building
            /// </summary>
            /// <param name="fSucceeded">true if no update actions failed</param>
            /// <param name="fModified">true if any update actions succeeded</param>
            /// <param name="fCancelCommand">true if update actions were canceled</param>
            /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
            public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
            {
                const int True = 1;

                _buildManager.UnadviseUpdateSolutionEvents(UpdateSolutionEventsCookie);

                if (fCancelCommand == True)
                {
                    _completionSource.SetResult(false);
                }
                else if (fSucceeded == True)
                {
                    _completionSource.SetResult(true);
                }
                else
                {
                    _completionSource.SetResult(false);
                }

                return S_OK;
            }

            /// <summary>
            /// Called before the first project configuration is about to be built.
            /// </summary>
            /// <param name="pfCancelUpdate">Pointer to a flag indicating cancel update.</param>
            /// <returns>If the method succeeds, it returns S_OK. If it fails, it returns an error code.</returns>
            public int UpdateSolution_StartUpdate(ref int pfCancelUpdate) => S_OK;
        }
    }
}
