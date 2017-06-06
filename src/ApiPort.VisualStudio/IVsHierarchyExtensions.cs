using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace ApiPortVS
{
    internal static class IVsHierarchyExtensions
    {
        /// <summary>
        /// Detects if a project uses the Common Project System Extension model.
        /// https://github.com/Microsoft/VSProjectSystem/blob/master/doc/automation/detect_whether_a_project_is_a_CPS_project.md
        /// </summary>
        internal static bool IsCpsProject(this IVsHierarchy project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            return project.IsCapabilityMatch(Constants.ProjectCapabilities.CPS);
        }

        /// <summary>
        /// Capability inferring that the project uses project.json to manage its source items.
        /// https://github.com/Microsoft/VSProjectSystem/blob/master/doc/overview/project_capabilities.md
        /// </summary>
        private static bool IsDnxProject(this IVsHierarchy project)
        {
            return !project.IsCapabilityMatch(Constants.ProjectCapabilities.DeclaredSourceItems);
        }
    }
}
