// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Common;
using ApiPortVS.Contracts;
using EnvDTE;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static Microsoft.Fx.Portability.Utils.FormattableStringHelper;

namespace ApiPortVS.VS2017
{
    public class ProjectBuilder2017 : DefaultProjectBuilder
    {
        public ProjectBuilder2017(
            IVsSolutionBuildManager2 buildManager,
            IProjectMapper projectMapper)
            : base(buildManager, projectMapper)
        { }

        /// <summary>
        /// Tries to fetch output items if it uses Common Project System then
        /// tries to fetch output items by retrieving FinalBuildOutput
        /// location using code snippet from:
        /// https://github.com/Microsoft/visualfsharp/blob/master/vsintegration/tests/unittests/Tests.ProjectSystem.Miscellaneous.fs#L168-L182
        /// </summary>
        /// <returns>null if it is unable to retrieve VS configuration objects</returns>
        public override async Task<IEnumerable<string>> GetBuildOutputFilesAsync(Project project, CancellationToken cancellationToken = default)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var output = await GetBuildOutputFilesFromCPSAsync(project, cancellationToken).ConfigureAwait(false);

            if (output != null)
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
            CancellationToken cancellationToken = default)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var hierarchy = await ProjectMapper.GetVsHierarchyAsync(project).ConfigureAwait(false);

            if (hierarchy == null)
            {
                Trace.TraceWarning(ToCurrentCulture($"Unable to locate {nameof(IVsHierarchy)} for {project.Name}"));
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

            // There are multiple loaded configurations for this project.
            // This is true for .NET Core projects that multi-target.
            // We'll return all those builds so APIPort can analyze them all.
            var configuredProjects = unconfigured.LoadedConfiguredProjects;

            var bag = new ConcurrentBag<string>();

            if (configuredProjects?.Count() > 1)
            {
                foreach (var proj in configuredProjects)
                {
                    try
                    {
                        var keyOutput = await proj.Services.OutputGroups.GetKeyOutputAsync(cancellationToken).ConfigureAwait(false);

                        if (!string.IsNullOrEmpty(keyOutput))
                        {
                            bag.Add(keyOutput);
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError(ToCurrentCulture($"Could not fetch key output from project configuration {proj.ProjectConfiguration.Name}. Exception: {e}"), e);
                    }
                }
            }

            // This is a typical CPS project that builds one component at a time.
            var configured = await unconfigured.GetSuggestedConfiguredProjectAsync().ConfigureAwait(false);

            if (configured != null)
            {
                var outputGroupsService = configured.Services.OutputGroups;
                var keyOutputFile = await outputGroupsService.GetKeyOutputAsync(cancellationToken).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(keyOutputFile))
                {
                    bag.Add(keyOutputFile);
                }
            }

            var outputs = bag.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray();

            return outputs.Any() ? outputs : null;
        }

        private static UnconfiguredProject GetUnconfiguredProject(Project project) => (project as IVsBrowseObjectContext)?.UnconfiguredProject;
    }
}
