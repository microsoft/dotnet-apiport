// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Reporting;
using ApiPortVS.Views;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ApiPortVS
{
    [Guid(Guids.ApiPortVSPkgString)]
    [InstalledProductRegistration("#110", "#112", "1.1.10808.0", IconResourceID = 400)] // Help->About info
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)] // load when a solution is opened
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionsPage), ".NET Portability Analyzer", "General", 110, 113, true)]
    [ProvideToolWindow(typeof(AnalysisOutputToolWindow))]
    public class ApiPortVSPackage : Package, IResultToolbar
    {
        private static ServiceProvider s_serviceProvider;

        internal static IServiceProvider LocalServiceProvider { get { return s_serviceProvider; } }

        public ApiPortVSPackage() : base()
        {
            s_serviceProvider = new ServiceProvider(this);
        }

        protected override void Dispose(bool disposing)
        {
            s_serviceProvider.Dispose();

            base.Dispose(disposing);
        }

        // Called after constructor when package is sited
        protected override void Initialize()
        {
            base.Initialize();

            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                var menuInitializer = LocalServiceProvider.GetService(typeof(AnalyzeMenu)) as AnalyzeMenu;

                // Add menu items for Analyze toolbar menu
                CommandID anazlyMenuCommandID = new CommandID(Guids.AnalyzeMenuItemCmdSet, (int)PkgCmdIDList.CmdIdAnalyzeMenuItem);
                MenuCommand menuItem = new MenuCommand(menuInitializer.AnalyzeMenuItemCallback, anazlyMenuCommandID);
                mcs.AddCommand(menuItem);

                CommandID analyzeMenuOptionsCommandID = new CommandID(Guids.AnalyzeMenuItemCmdSet, (int)PkgCmdIDList.CmdIdAnalyzeOptionsMenuItem);
                MenuCommand analyzeMenuOptionsItem = new MenuCommand(ShowOptionsPage, analyzeMenuOptionsCommandID);
                mcs.AddCommand(analyzeMenuOptionsItem);

                CommandID analyzeMenuToolbarCommandID = new CommandID(Guids.AnalyzeMenuItemCmdSet, (int)PkgCmdIDList.CmdIdAnalyzeToolbarMenuItem);
                MenuCommand analyzeMenuToolbarItem = new MenuCommand((_, __) => ShowToolbar(), analyzeMenuToolbarCommandID);
                mcs.AddCommand(analyzeMenuToolbarItem);

                // Add menu items for Project context menus
                CommandID projectContextMenuCmdId = new CommandID(Guids.ProjectContextMenuItemCmdSet, (int)PkgCmdIDList.CmdIdProjectContextMenuItem);
                OleMenuCommand contextMenuItem = new OleMenuCommand(async (_, __) => await menuInitializer.AnalyzeSelectedProjectsAsync(false), projectContextMenuCmdId);
                contextMenuItem.BeforeQueryStatus += menuInitializer.ProjectContextMenuItemBeforeQueryStatus;
                mcs.AddCommand(contextMenuItem);

                CommandID projectContextMenuDependentsCmdId = new CommandID(Guids.ProjectContextMenuItemCmdSet, (int)PkgCmdIDList.CmdIdProjectContextDependentsMenuItem);
                OleMenuCommand contextMenuDependentsItem = new OleMenuCommand(async (_, __) => await menuInitializer.AnalyzeSelectedProjectsAsync(true), projectContextMenuDependentsCmdId);
                contextMenuDependentsItem.BeforeQueryStatus += menuInitializer.ProjectContextMenuDependenciesItemBeforeQueryStatus;
                mcs.AddCommand(contextMenuDependentsItem);

                CommandID projectContextMenuOptionsCmdId = new CommandID(Guids.ProjectContextMenuItemCmdSet, (int)PkgCmdIDList.CmdIdProjectContextOptionsMenuItem);
                OleMenuCommand contextMenuOptionsItem = new OleMenuCommand(ShowOptionsPage, projectContextMenuOptionsCmdId);
                contextMenuOptionsItem.BeforeQueryStatus += menuInitializer.ProjectContextMenuItemBeforeQueryStatus;
                mcs.AddCommand(contextMenuOptionsItem);

                // Add menu items for Solution context menus
                CommandID solutionContextMenuCmdId = new CommandID(Guids.SolutionContextMenuItemCmdSet, (int)PkgCmdIDList.CmdIdSolutionContextMenuItem);
                OleMenuCommand solutionContextMenuItem = new OleMenuCommand(menuInitializer.SolutionContextMenuItemCallback, solutionContextMenuCmdId);
                solutionContextMenuItem.BeforeQueryStatus += menuInitializer.SolutionContextMenuItemBeforeQueryStatus;
                mcs.AddCommand(solutionContextMenuItem);

                CommandID solutionContextMenuOptionsCmdId = new CommandID(Guids.SolutionContextMenuItemCmdSet, (int)PkgCmdIDList.CmdIdSolutionContextOptionsMenuItem);
                OleMenuCommand solutionContextMenuOptionsItem = new OleMenuCommand(ShowOptionsPage, solutionContextMenuOptionsCmdId);
                solutionContextMenuOptionsItem.BeforeQueryStatus += menuInitializer.SolutionContextMenuItemBeforeQueryStatus;
                mcs.AddCommand(solutionContextMenuOptionsItem);
            }
        }

        public void ShowToolbar()
        {
            ToolWindowPane window = FindToolWindow(typeof(AnalysisOutputToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        private void ShowOptionsPage(object sender, EventArgs e)
        {
            ShowOptionPage(typeof(OptionsPage));
        }
    }
}