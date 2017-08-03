using ApiPortVS.Common;
using ApiPortVS.Contracts;
using EnvDTE;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApiPortVS.VS2015
{
    public class ProjectBuilder2015 : DefaultProjectBuilder
    {
        public ProjectBuilder2015(IVsSolutionBuildManager2 buildManager, IVSThreadingService threadingService, IProjectMapper projectMapper)
            : base(buildManager, threadingService, projectMapper)
        { }

        /// <summary>
        /// Tries to fetch output items if it uses Common Project System then
        /// tries to fetch output items by retrieving FinalBuildOutput
        /// location using code snippet from:
        /// https://github.com/Microsoft/visualfsharp/blob/master/vsintegration/tests/unittests/Tests.ProjectSystem.Miscellaneous.fs#L168-L182
        /// </summary>
        /// <returns>null if it is unable to retrieve VS configuration objects</returns>
        public override async Task<IEnumerable<string>> GetBuildOutputFilesAsync(Project project, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var output = await GetBuildOutputFilesFromCPSAsync(project, cancellationToken).ConfigureAwait(false);

            if (output != null && output.Any())
            {
                return output;
            }

            return await base.GetBuildOutputFilesAsync(project, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Tries to fetch files if it is a project that uses the Common
        /// Project System (CPS) extensibility model.
        /// </summary>
        /// <returns>null if it is unable to find any output items or this
        /// project is not a CPS project.</returns>
        private async Task<IEnumerable<string>> GetBuildOutputFilesFromCPSAsync(
            Project project,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var hierarchy = await ProjectMapper.GetVsHierarchyAsync(project).ConfigureAwait(false);

            if (hierarchy == null)
            {
                Trace.TraceWarning($"Unable to locate {nameof(IVsHierarchy)} for {project.Name}");
                return null;
            }

            if (!hierarchy.IsCpsProject())
            {
                return null;
            }

            var unconfigured = GetUnconfiguredProject(project);

            if (unconfigured == null)
            {
                return null;
            }

            // This is a typical CPS project that builds one component at a time.
            var configured = await unconfigured.GetSuggestedConfiguredProjectAsync().ConfigureAwait(false);

            if (configured == null)
            {
                return null;
            }

            var outputGroupsService = configured.Services.OutputGroups;

            var keyOutputFile = await outputGroupsService.GetKeyOutputAsync(cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrEmpty(keyOutputFile))
            {
                var builtGroup = await outputGroupsService.GetOutputGroupAsync(Common.Constants.OutputGroups.BuiltProject).ConfigureAwait(false);

                if (builtGroup?.Outputs?.Any() ?? false)
                {
                    return builtGroup.Outputs.Select(x => x.Key);
                }
            }
            else
            {
                return new[] { keyOutputFile };
            }

            return null;
        }

        private UnconfiguredProject GetUnconfiguredProject(Project project)
        {
            return (project as IVsBrowseObjectContext)?.UnconfiguredProject;
        }
    }
}
