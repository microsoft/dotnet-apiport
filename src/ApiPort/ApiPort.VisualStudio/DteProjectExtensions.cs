// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;

namespace ApiPortVS
{
    internal static class DteProjectExtensions
    {
        public static IEnumerable<Project> GetProjects(this Solution sln)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

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
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!(project.Object is VSLangProj.VSProject vsproj))
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
            ThreadHelper.ThrowIfNotOnUIThread();

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

            ThreadHelper.ThrowIfNotOnUIThread();
            return string.Equals(project.Kind, vsProjectKindSolutionFolder, StringComparison.OrdinalIgnoreCase);
        }
    }
}
