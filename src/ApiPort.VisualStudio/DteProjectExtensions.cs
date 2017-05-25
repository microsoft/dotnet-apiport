// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Build;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApiPortVS
{
    internal static class DteProjectExtensions
    {
        private const string FSharpProjectKindString = "{f2a71f9b-5d33-465a-a702-920d77279786}";

        /// <summary>
        /// Tries to fetch output items if it uses Common Project System then
        /// tries to fetch output items by retrieving FinalBuildOutput
        /// location using code snippet from:
        /// https://github.com/Microsoft/visualfsharp/blob/master/vsintegration/tests/unittests/Tests.ProjectSystem.Miscellaneous.fs#L168-L182
        /// </summary>
        /// <returns>null if it is unable to retrieve VS configuration objects</returns>
        public static async Task<IEnumerable<string>> GetBuildOutputFilesAsync(this Project project, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var output = await project.GetBuildOutputFilesFromCPSAsync(cancellationToken).ConfigureAwait(false);

            if (output != null)
            {
                return output;
            }

            var configuration = GetVsProjectConfiguration(project);

            if (configuration == null)
            {
                return null;
            }

            if (!(configuration is IVsProjectCfg2 configuration2))
            {
                Trace.TraceError($"IVsCfg returned {configuration.GetType()} is not of the right type. Expected: {nameof(IVsProjectCfg2)}");
                return null;
            }

            if (ErrorHandler.Failed(configuration2.OpenOutputGroup(Constants.OutputGroups.BuiltProject, out IVsOutputGroup outputGroup)))
            {
                Trace.TraceError($"Could not retrieve {nameof(IVsOutputGroup)} from project: {project.Name}");
                return null;
            }

            if (!(outputGroup is IVsOutputGroup2 outputGroup2))
            {
                Trace.TraceError($"Could not retrieve {nameof(IVsOutputGroup2)} from project: {project.Name}");
                return null;
            }

            if (ErrorHandler.Failed(outputGroup2.get_KeyOutputObject(out IVsOutput2 keyGroup)))
            {
                Trace.TraceError($"Could not retrieve {nameof(IVsOutput2)} from project: {project.Name}");
                return null;
            }

            if (ErrorHandler.Succeeded(keyGroup.get_Property(Constants.MetadataNames.OutputLocation, out object outputLoc)))
            {
                return new[] { outputLoc as string };
            }

            if (ErrorHandler.Succeeded(keyGroup.get_Property(Constants.MetadataNames.FinalOutputPath, out object finalOutputPath)))
            {
                return new[] { finalOutputPath as string };
            }

            return null;
        }

        public static IVsCfg GetVsProjectConfiguration(this Project project)
        {
            object browseObject = null;

            project.GetHierarchy()?.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_BrowseObject, out browseObject);

            var getConfigurationProvider = browseObject as IVsGetCfgProvider;

            if (getConfigurationProvider == null)
            {
                Trace.TraceError($"Could not retrieve {nameof(IVsGetCfgProvider)} from project: {project.Name}");
                return null;
            }

            if (ErrorHandler.Failed(getConfigurationProvider.GetCfgProvider(out IVsCfgProvider provider)))
            {
                Trace.TraceError($"Could not retrieve {nameof(IVsCfgProvider)} from project: {project.Name}");
                return null;
            }
            if (!(provider is IVsCfgProvider2))
            {
                Trace.TraceError($"IVsCfgProvider returned {provider.GetType()} is not of the right type. Expected: {nameof(IVsCfgProvider2)}");
                return null;
            }

            var provider2 = (IVsCfgProvider2)provider;
            var activeConfiguration = project.ConfigurationManager.ActiveConfiguration;

            if (ErrorHandler.Failed(provider2.GetCfgOfName(activeConfiguration.ConfigurationName, activeConfiguration.PlatformName, out IVsCfg configuration)))
            {
                Trace.TraceError($"Could not retrieve {nameof(IVsCfg)} from project: {project.Name}");
                return null;
            }

            return configuration;
        }

        /// <summary>
        /// Tries to fetch files if it is a project that uses the Common
        /// Project System (CPS) extensibility model.
        /// </summary>
        /// <returns>null if it is unable to find any output items or this
        /// project is not a CPS project.</returns>
        private static async Task<IEnumerable<string>> GetBuildOutputFilesFromCPSAsync(
            this Project project,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (!project.GetHierarchy()?.IsCpsProject() ?? true)
            {
                return null;
            }

            var unconfigured = project.GetUnconfiguredProject();

            if (unconfigured == null)
            {
                return null;
            }

            // There are multiple loaded configurations for this project.
            // This is true for .NET Core projects that multi-target.
            // We'll return all those builds so APIPort can analyze them all.
            var configuredProjects = unconfigured.LoadedConfiguredProjects;

            if (configuredProjects?.Count() > 1)
            {
                var bag = new ConcurrentBag<string>();

                foreach (var proj in configuredProjects)
                {
                    var keyOutput = await proj.Services.OutputGroups.GetKeyOutputAsync(cancellationToken).ConfigureAwait(false);
                    bag.Add(keyOutput);
                }

                return bag;
            }

            // This is a typical CPS project that builds one component at a time.
            var configured = await unconfigured.GetSuggestedConfiguredProjectAsync().ConfigureAwait(false);

            if (configured == null)
            {
                return null;
            }

            var outputGroupsService = configured.Services.OutputGroups;
            var keyOutputFile = await outputGroupsService.GetKeyOutputAsync(cancellationToken).ConfigureAwait(false);

            return new[] { keyOutputFile };
        }

        private static UnconfiguredProject GetUnconfiguredProject(this Project project)
        {
            return (project as IVsBrowseObjectContext)?.UnconfiguredProject;
        }

        public static string GetProjectFileDirectory(this Project project)
        {
            return Path.GetDirectoryName(project.FullName); // FullName is the project file path
        }

        public static bool IsDotNetProject(this Project project)
        {
            if (project.CodeModel == null) // e.g. F# projects
            {
                return IsFSharpProject(project);
            }

            switch (project.CodeModel.Language)
            {
                case Constants.CodeModelLanguageConstants.vsCMLanguageCSharp:
                case Constants.CodeModelLanguageConstants.vsCMLanguageVB:
                    return true;
                case Constants.CodeModelLanguageConstants.vsCMLanguageVC:
                    return project.IsManagedCppProject();
            }

            return false;
        }

        public static IVsHierarchy GetHierarchy(this Project project)
        {
            var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
            if (solution == null)
            {
                return null;
            }

            IVsHierarchy hierarchy;
            solution.GetProjectOfUniqueName(project.FullName, out hierarchy);

            return hierarchy;
        }

        public static IEnumerable<Project> GetProjects(this Solution sln)
        {
            foreach (Project project in sln.Projects)
            {
                if (IsSolutionFolder(project))
                {
                    foreach (var prj in GetSolutionFolderProjects(project))
                    {
                        yield return prj;
                    }
                }
                else
                {
                    yield return project;
                }
            }
        }

        public static IEnumerable<Project> GetReferences(this Project project)
        {
            var vsproj = project.Object as VSLangProj.VSProject;

            if (vsproj == null)
            {
                yield break;
            }

            foreach (VSLangProj.Reference reference in vsproj.References)
            {
                if (reference.SourceProject != null)
                {
                    yield return reference.SourceProject;
                }
            }
        }

        private static IEnumerable<Project> GetSolutionFolderProjects(Project solutionFolder)
        {
            foreach (ProjectItem project in solutionFolder.ProjectItems)
            {
                var subProject = project.SubProject;

                if (subProject == null)
                {
                    continue;
                }

                if (IsSolutionFolder(subProject))
                {
                    foreach (var prj in GetSolutionFolderProjects(subProject))
                    {
                        yield return prj;
                    }
                }
                else
                {
                    yield return subProject;
                }
            }
        }

        private static bool IsFSharpProject(this Project project)
        {
            return string.Equals(project.Kind, FSharpProjectKindString, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsManagedCppProject(this Project project)
        {
            var clrSupport = GetPropertyValueFromBuildProject(project, "CLRSupport");

            return bool.Parse(clrSupport);
        }

        private static string GetPropertyValueFromBuildProject(Project project, string property)
        {
            var buildProject = new Microsoft.Build.Evaluation.Project(project.FullName);
            var value = buildProject.GetPropertyValue(property);
            Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.UnloadProject(buildProject);

            return value;
        }

        private static bool IsSolutionFolder(Project project)
        {
            const string vsProjectKindSolutionFolder = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";

            return string.Equals(project.Kind, vsProjectKindSolutionFolder, StringComparison.OrdinalIgnoreCase);
        }
    }
}
