// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Analyze;
using ApiPortVS.Resources;
using Autofac;
using EnvDTE;
using Microsoft.Fx.Portability;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ApiPortVS
{
    internal class AnalyzeMenu
    {
        private readonly TextWriter _output;
        private readonly DTE _dte;
        private readonly ILifetimeScope _scope;

        public AnalyzeMenu(ILifetimeScope scope, DTE dte, TextWriter output)
        {
            _scope = scope;
            _dte = dte;
            _output = output;
        }

        public async void ProjectContextMenuItemCallback(object sender, EventArgs e)
        {
            await AnalyzeProjects(GetSelectedProjects());
        }

        public async void SolutionContextMenuItemCallback(object sender, EventArgs e)
        {
            await AnalyzeProjects(_dte.Solution.GetProjects().ToList());
        }

        private async Task AnalyzeProjects(ICollection<Project> projects)
        {
            if (!projects.Any())
            {
                return;
            }

            try
            {
                WriteHeader();

                using (var innerScope = _scope.BeginLifetimeScope())
                {
                    var projectAnalyzer = innerScope.Resolve<ProjectAnalyzer>();

                    await projectAnalyzer.AnalyzeProjectAsync(projects);
                }
            }
            catch (PortabilityAnalyzerException ex)
            {
                _output.WriteLine();
                _output.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                _output.WriteLine();
                _output.WriteLine(LocalizedStrings.UnknownError);

                Trace.WriteLine(ex);
            }
        }


        // called when the project-level context menu is about to be displayed,
        // i.e. the user has right-clicked a single project in the solution explorer
        // (the command is sited in a menu which only appears on single selections)
        public void ProjectContextMenuItemBeforeQueryStatus(object sender, EventArgs e)
        {
            ContextMenuItemBeforeQueryStatus(sender, GetSelectedProjects());
        }

        public void SolutionContextMenuItemBeforeQueryStatus(object sender, EventArgs e)
        {
            ContextMenuItemBeforeQueryStatus(sender, _dte.Solution.GetProjects());
        }

        private void ContextMenuItemBeforeQueryStatus(object sender, IEnumerable<Project> projects)
        {
            var menuItem = sender as Microsoft.VisualStudio.Shell.OleMenuCommand;
            if (menuItem == null)
            {
                return;
            }

            menuItem.Visible = projects.Any() && projects.All(p => p.IsDotNetProject());
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
                WriteHeader();

                using (var innerScope = _scope.BeginLifetimeScope())
                {
                    var fileListAnalyzer = innerScope.Resolve<FileListAnalyzer>();

                    await fileListAnalyzer.AnalyzeProjectAsync(inputAssemblyPaths);
                }
            }
            catch (PortabilityAnalyzerException ex)
            {
                _output.WriteLine();
                _output.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                _output.WriteLine();
                _output.WriteLine(LocalizedStrings.UnknownError);

                Trace.WriteLine(ex);
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

        private void WriteHeader()
        {
            var header = new StringBuilder();
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            header.AppendLine(string.Format(LocalizedStrings.CopyrightFormat, assemblyVersion));
            header.AppendLine(string.Concat(LocalizedStrings.MoreInformationAvailableAt, " ", LocalizedStrings.MoreInformationUrl));

            _output.WriteLine(header.ToString());
        }

        private ICollection<Project> GetSelectedProjects()
        {
            return _dte.SelectedItems
                .OfType<SelectedItem>()
                .Select(i => i.Project)
                .Distinct()
                .ToList();
        }
    }
}
