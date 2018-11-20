// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace ApiPortVS.Common
{
    public static class IVsHierarchyExtensions
    {
        public static bool IsDotNetProject(this Project project)
        {
            // e.g. F# projects
            if (project.CodeModel == null)
            {
                return IsFSharpProject(project);
            }

            switch (project.CodeModel.Language)
            {
                case CodeModelLanguageConstants.VsCMLanguageCSharp:
                case CodeModelLanguageConstants.VsCMLanguageVB:
                    return true;
                case CodeModelLanguageConstants.VsCMLanguageVC:
                    return project.IsManagedCppProject();
            }

            return false;
        }

        /// <summary>
        /// Detects if a project uses the Common Project System Extension model.
        /// https://github.com/Microsoft/VSProjectSystem/blob/master/doc/automation/detect_whether_a_project_is_a_CPS_project.md
        /// </summary>
        public static bool IsCpsProject(this IVsHierarchy project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            return project.IsCapabilityMatch(ProjectCapabilities.CPS);
        }

        /// <summary>
        /// Capability inferring that the project uses project.json to manage its source items.
        /// https://github.com/Microsoft/VSProjectSystem/blob/master/doc/overview/project_capabilities.md
        /// </summary>
        private static bool IsDnxProject(this IVsHierarchy project)
        {
            return !project.IsCapabilityMatch(ProjectCapabilities.DeclaredSourceItems);
        }

        private static bool IsFSharpProject(this Project project)
        {
            return string.Equals(project.Kind, ProjectKindConstants.FSharpProjectKindString, StringComparison.OrdinalIgnoreCase);
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

        private class ProjectKindConstants
        {
            internal const string FSharpProjectKindString = "{f2a71f9b-5d33-465a-a702-920d77279786}";
        }

        /// <summary>
        /// <see cref="EnvDTE.CodeModelLanguageConstants"/>
        /// https://msdn.microsoft.com/en-us/library/envdte.codemodellanguageconstants.aspx
        /// </summary>
        private static class CodeModelLanguageConstants
        {
            internal const string VsCMLanguageVC = "{B5E9BD32-6D3E-4B5D-925E-8A43B79820B4}";
            internal const string VsCMLanguageVB = "{B5E9BD33-6D3E-4B5D-925E-8A43B79820B4}";
            internal const string VsCMLanguageCSharp = "{B5E9BD34-6D3E-4B5D-925E-8A43B79820B4}";
        }

        /// <summary>
        /// Well-known project capabilities from:
        /// https://github.com/Microsoft/VSProjectSystem/blob/master/doc/overview/project_capabilities.md
        /// </summary>
        private static class ProjectCapabilities
        {
            /// <summary>
            ///  Project is based on the Project System Extensibility SDK
            /// </summary>
            internal const string CPS = nameof(CPS);

            /// <summary>
            /// Indicates that the project is a typical MSBuild project (not DNX)
            /// in that it declares source items in the project itself (rather
            /// than a project.json file that assumes all files in the directory
            /// are part of a compilation).
            /// </summary>
            internal const string DeclaredSourceItems = nameof(DeclaredSourceItems);
        }
    }
}
