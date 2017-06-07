// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;

namespace ApiPortVS
{
    internal static class DteProjectExtensions
    {
        public static string GetProjectFileDirectory(this Project project)
        {
            return Path.GetDirectoryName(project.FullName); // FullName is the project file path
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

        private static bool IsSolutionFolder(Project project)
        {
            const string vsProjectKindSolutionFolder = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";

            return string.Equals(project.Kind, vsProjectKindSolutionFolder, StringComparison.OrdinalIgnoreCase);
        }
    }
}
