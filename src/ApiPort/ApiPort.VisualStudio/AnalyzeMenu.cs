// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Analyze;
using ApiPortVS.Common;
using ApiPortVS.Contracts;
using ApiPortVS.Resources;
using Autofac;
using EnvDTE;
using Microsoft.Fx.Portability;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ApiPortVS
{
    internal class AnalyzeMenu
    {
        private readonly IOutputWindowWriter _output;
        private readonly ProductInformation _productInfo;
        private readonly DTE _dte;
        private readonly ILifetimeScope _scope;

        public AnalyzeMenu(ILifetimeScope scope, DTE dte, IOutputWindowWriter output)
        {
            _scope = scope;
            _dte = dte;
            _output = output;
            _productInfo = new ProductInformation("AnalyzeMenu");
        }

        public async Task AnalyzeSelectedProjectsAsync(bool includeDependencies)
        {
            var projects = GetSelectedProjects();

            await AnalyzeProjectsAsync(includeDependencies ? GetTransitiveReferences(projects, new HashSet<Project>()) : projects, projects.FirstOrDefault()).ConfigureAwait(false);
        }

        public async void SolutionContextMenuItemCallback(object sender, EventArgs e)
        {
            await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            await AnalyzeProjectsAsync(_dte.Solution.GetProjects().Where(x => x.IsDotNetProject()).ToList(), null).ConfigureAwait(false);
        }

        private async Task AnalyzeProjectsAsync(ICollection<Project> projects, Project entrypoint)
        {
            if (!projects.Any())
            {
                return;
            }

            try
            {
                await WriteHeaderAsync().ConfigureAwait(false);

                using (var innerScope = _scope.BeginLifetimeScope())
                {
                    var projectAnalyzer = innerScope.Resolve<ProjectAnalyzer>();

                    await projectAnalyzer.AnalyzeProjectAsync(projects, entrypoint).ConfigureAwait(false);
                }
            }
            catch (PortabilityAnalyzerException ex)
            {
                _output.WriteLine();
                _output.WriteLine(ex.Message);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _output.WriteLine();
                _output.WriteLine(LocalizedStrings.UnknownError);
                _output.WriteLine(ex.ToString());
            }
            finally
            {
                _output.WriteLine();
            }
        }

        public void ProjectContextMenuItemBeforeQueryStatus(object sender, EventArgs e)
        {
            ContextMenuItemBeforeQueryStatus(sender, GetSelectedProjects(), false);
        }

        public void ProjectContextMenuDependenciesItemBeforeQueryStatus(object sender, EventArgs e)
        {
            ContextMenuItemBeforeQueryStatus(sender, GetSelectedProjects(), true);
        }

        public void SolutionContextMenuItemBeforeQueryStatus(object sender, EventArgs e)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            ContextMenuItemBeforeQueryStatus(sender, _dte.Solution.GetProjects(), false);
        }

        private static void ContextMenuItemBeforeQueryStatus(object sender, IEnumerable<Project> projects, bool checkDependencies)
        {
            var menuItem = sender as Microsoft.VisualStudio.Shell.OleMenuCommand;
            if (menuItem == null)
            {
                return;
            }

            menuItem.Visible = projects.Any(p => p.IsDotNetProject());

            // Only need to check dependents if menuItem is still visible
            if (checkDependencies && menuItem.Visible)
            {
                menuItem.Visible = projects.SelectMany(p => p.GetReferences()).Any();
            }
        }

        public async void AnalyzeMenuItemCallback(object sender, EventArgs e)
        {
            var inputAssemblyPaths = PromptForAssemblyPaths();
            if (inputAssemblyPaths == null)
            {
                return;
            }

            try
            {
                await WriteHeaderAsync().ConfigureAwait(false);

                using (var innerScope = _scope.BeginLifetimeScope())
                {
                    var fileListAnalyzer = innerScope.Resolve<FileListAnalyzer>();

                    await fileListAnalyzer.AnalyzeProjectAsync(inputAssemblyPaths).ConfigureAwait(false);
                }
            }
            catch (PortabilityAnalyzerException ex)
            {
                _output.WriteLine();
                _output.WriteLine(ex.Message);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _output.WriteLine();
                _output.WriteLine(LocalizedStrings.UnknownError);

                Trace.WriteLine(ex);
            }
            finally
            {
                _output.WriteLine();
            }
        }

        private string[] PromptForAssemblyPaths()
        {
            var openDialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = LocalizedStrings.AssemblyFiles + "|*.exe;*.dll",
                Multiselect = true,
                Title = LocalizedStrings.SelectAssemblyFiles
            };

            return (bool)openDialog.ShowDialog() ? openDialog.FileNames : null;
        }

        private async Task WriteHeaderAsync()
        {
            await _output.ClearWindowAsync().ConfigureAwait(false);

            var header = new StringBuilder();

            header.AppendLine(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.CopyrightFormat, _productInfo.Version));
            header.AppendLine($"{LocalizedStrings.AboutTool} {DocumentationLinks.About}");
            header.AppendLine($"{LocalizedStrings.AboutPrivacy} {DocumentationLinks.PrivacyPolicy}");

            _output.WriteLine(header.ToString());
        }

        private ICollection<Project> GetSelectedProjects()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            return _dte.SelectedItems
                .OfType<SelectedItem>()
                .Select(i => i.Project)
                .Distinct()
                .ToList();
        }

        private ICollection<Project> GetTransitiveReferences(IEnumerable<Project> projects, HashSet<Project> expandedProjects)
        {
            foreach (var project in projects)
            {
                if (expandedProjects.Add(project))
                {
                    GetTransitiveReferences(project.GetReferences(), expandedProjects);
                }
            }

            return expandedProjects;
        }
    }
}
