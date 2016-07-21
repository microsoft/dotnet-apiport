using ApiPortVS.Resources;
using EnvDTE;
using Microsoft.Fx.Portability;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ApiPortVS
{
    internal static class DteProjectExtensions
    {
        private const string FSharpProjectKindString = "{f2a71f9b-5d33-465a-a702-920d77279786}";

        public static string GetBuildOutputFileName(this Project project)
        {
            var fileName = project.Properties.Item("OutputFileName").Value.ToString();

            return Path.Combine(project.GetBuildOutputPath(), fileName);
        }

        public static IEnumerable<string> GetAssemblyPaths(this Project project, Func<string, IEnumerable<string>> getAllDirectories = null)
        {
            if (getAllDirectories == null)
            {
                return new[] { project.GetBuildOutputFileName() };
            }
            else
            {
                var buildDir = project.GetBuildOutputPath();

                var targetAssemblies = getAllDirectories(buildDir);

                if (!targetAssemblies.Any())
                {
                    var message = string.Format(LocalizedStrings.NoAnalyzableAssemblies, buildDir);
                    throw new FileNotFoundException(message);
                }

                return targetAssemblies;
            }
        }

        public static string GetBuildOutputPath(this Project project)
        {
            try
            {
                var configMgr = project.ConfigurationManager as ConfigurationManager;
                var activeCfg = configMgr.ActiveConfiguration as Configuration;

                // This is the path displayed in project's Properties->Build->Output->Output path
                // and may not be correct if the user's customized the build process elsewhere
                var outputPath = activeCfg.Properties.Item("OutputPath").Value.ToString();

                return Path.IsPathRooted(outputPath) ? outputPath
                                                     : Path.Combine(project.GetProjectFileDirectory(), outputPath);
            }
            catch (Exception ex)
            {
                if (ex is NullReferenceException || ex is ArgumentException)
                {
                    throw new PortabilityAnalyzerException(LocalizedStrings.FailedToLocateBuildOutputDir);
                }

                throw;
            }
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
                case CodeModelLanguageConstants.vsCMLanguageCSharp:
                case CodeModelLanguageConstants.vsCMLanguageVB:
                    return true;
                case CodeModelLanguageConstants.vsCMLanguageVC:
                    return project.IsManagedCppProject();
            }

            return false;
        }

        public static IVsHierarchy GetHierarchy(this Project project, IServiceProvider serviceProvider)
        {
            var solution = serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            if (solution == null)
            {
                return null;
            }

            IVsHierarchy hierarchy;
            solution.GetProjectOfUniqueName(project.FullName, out hierarchy);

            return hierarchy;
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
    }
}
