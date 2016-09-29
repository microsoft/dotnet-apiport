// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Analyze;
using ApiPortVS.Resources;
using Autofac;
using EnvDTE;
using Microsoft.Fx.Portability;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace ApiPortVS
{
    internal class AnalyzeMenu
    {
        private readonly TextWriter _output;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILifetimeScope _scope;

        public AnalyzeMenu(ILifetimeScope scope, IServiceProvider serviceProvider, TextWriter output)
        {
            _scope = scope;
            _serviceProvider = serviceProvider;
            _output = output;
        }

        public async void ContextMenuItemCallback(object sender, EventArgs e)
        {
            var selectedProject = GetSelectedProject();
            if (selectedProject == null)
            {
                return;
            }

            try
            {
                WriteHeader();

                using (var innerScope = _scope.BeginLifetimeScope())
                {
                    var projectAnalyzer = innerScope.Resolve<ProjectAnalyzer>();

                    await projectAnalyzer.AnalyzeProjectAsync(selectedProject);
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
            var menuItem = sender as Microsoft.VisualStudio.Shell.OleMenuCommand;
            if (menuItem == null)
            {
                return;
            }

            var selectedProject = GetSelectedProject();
            menuItem.Visible = selectedProject != null && selectedProject.IsDotNetProject();
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

        private Project GetSelectedProject()
        {
            try
            {
                var dte = _serviceProvider.GetService(typeof(DTE)) as DTE;
                var selection = dte.SelectedItems;
                var selectedItem = selection.Item(1); // selection container is indexed from 1

                return selectedItem.Project;
            }
            catch
            {
                return null;
            }
        }
    }
}
