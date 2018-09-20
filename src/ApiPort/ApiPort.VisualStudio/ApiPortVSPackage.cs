// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Reporting;
using ApiPortVS.Resources;
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
        private static ServiceProvider _serviceProvider;
        private readonly AssemblyRedirectResolver _assemblyResolver;

        internal static IServiceProvider LocalServiceProvider { get { return _serviceProvider; } }

        public ApiPortVSPackage()
            : base()
        {
            _serviceProvider = new ServiceProvider(this);
            _assemblyResolver = _serviceProvider.GetService(typeof(AssemblyRedirectResolver)) as AssemblyRedirectResolver;

            if (_assemblyResolver == default(AssemblyRedirectResolver))
            {
                throw new InvalidOperationException(LocalizedStrings.CouldNotFindAssemblyRedirectResolver);
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        protected override void Dispose(bool disposing)
        {
            _serviceProvider.Dispose();
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;

            base.Dispose(disposing);
        }

        // Called after constructor when package is sited
        protected override void Initialize()
        {
            base.Initialize();

            if (GetService(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                var menuInitializer = LocalServiceProvider.GetService(typeof(AnalyzeMenu)) as AnalyzeMenu;

                // Add menu items for Analyze toolbar menu
                CommandID anazlyMenuCommandID = new CommandID(Guids.AnalyzeMenuItemCmdSet, (int)PkgCmdID.CmdIdAnalyzeMenuItem);
                MenuCommand menuItem = new MenuCommand(menuInitializer.AnalyzeMenuItemCallback, anazlyMenuCommandID);
                mcs.AddCommand(menuItem);

                CommandID analyzeMenuOptionsCommandID = new CommandID(Guids.AnalyzeMenuItemCmdSet, (int)PkgCmdID.CmdIdAnalyzeOptionsMenuItem);
                MenuCommand analyzeMenuOptionsItem = new MenuCommand(ShowOptionsPage, analyzeMenuOptionsCommandID);
                mcs.AddCommand(analyzeMenuOptionsItem);

                CommandID analyzeMenuToolbarCommandID = new CommandID(Guids.AnalyzeMenuItemCmdSet, (int)PkgCmdID.CmdIdAnalyzeToolbarMenuItem);
                MenuCommand analyzeMenuToolbarItem = new MenuCommand(async (_, e) => await ShowToolbarAsync().ConfigureAwait(false), analyzeMenuToolbarCommandID);
                mcs.AddCommand(analyzeMenuToolbarItem);

                // Add menu items for Project context menus
                CommandID projectContextMenuCmdId = new CommandID(Guids.ProjectContextMenuItemCmdSet, (int)PkgCmdID.CmdIdProjectContextMenuItem);
                OleMenuCommand contextMenuItem = new OleMenuCommand(async (_, e) => await menuInitializer.AnalyzeSelectedProjectsAsync(false), projectContextMenuCmdId);
                contextMenuItem.BeforeQueryStatus += menuInitializer.ProjectContextMenuItemBeforeQueryStatus;
                mcs.AddCommand(contextMenuItem);

                CommandID projectContextMenuDependentsCmdId = new CommandID(Guids.ProjectContextMenuItemCmdSet, (int)PkgCmdID.CmdIdProjectContextDependentsMenuItem);
                OleMenuCommand contextMenuDependentsItem = new OleMenuCommand(async (_, e) => await menuInitializer.AnalyzeSelectedProjectsAsync(true), projectContextMenuDependentsCmdId);
                contextMenuDependentsItem.BeforeQueryStatus += menuInitializer.ProjectContextMenuDependenciesItemBeforeQueryStatus;
                mcs.AddCommand(contextMenuDependentsItem);

                CommandID projectContextMenuOptionsCmdId = new CommandID(Guids.ProjectContextMenuItemCmdSet, (int)PkgCmdID.CmdIdProjectContextOptionsMenuItem);
                OleMenuCommand contextMenuOptionsItem = new OleMenuCommand(ShowOptionsPage, projectContextMenuOptionsCmdId);
                contextMenuOptionsItem.BeforeQueryStatus += menuInitializer.ProjectContextMenuItemBeforeQueryStatus;
                mcs.AddCommand(contextMenuOptionsItem);

                // Add menu items for Solution context menus
                CommandID solutionContextMenuCmdId = new CommandID(Guids.SolutionContextMenuItemCmdSet, (int)PkgCmdID.CmdIdSolutionContextMenuItem);
                OleMenuCommand solutionContextMenuItem = new OleMenuCommand(menuInitializer.SolutionContextMenuItemCallback, solutionContextMenuCmdId);
                solutionContextMenuItem.BeforeQueryStatus += menuInitializer.SolutionContextMenuItemBeforeQueryStatus;
                mcs.AddCommand(solutionContextMenuItem);

                CommandID solutionContextMenuOptionsCmdId = new CommandID(Guids.SolutionContextMenuItemCmdSet, (int)PkgCmdID.CmdIdSolutionContextOptionsMenuItem);
                OleMenuCommand solutionContextMenuOptionsItem = new OleMenuCommand(ShowOptionsPage, solutionContextMenuOptionsCmdId);
                solutionContextMenuOptionsItem.BeforeQueryStatus += menuInitializer.SolutionContextMenuItemBeforeQueryStatus;
                mcs.AddCommand(solutionContextMenuOptionsItem);
            }
        }

        public async System.Threading.Tasks.Task ShowToolbarAsync()
        {
            // Calls to UI elements should use the main task thread.
            // https://blogs.msdn.microsoft.com/andrewarnottms/2014/05/07/asynchronous-and-multithreaded-programming-within-vs-using-the-joinabletaskfactory/
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            ToolWindowPane window = FindToolWindow(typeof(AnalysisOutputToolWindow), 0, true);
            if ((window == null) || (window.Frame == null))
            {
                throw new NotSupportedException(LocalizedStrings.CannotCreateToolWindow);
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) => _assemblyResolver.ResolveAssembly(args.Name, args.RequestingAssembly);

        private void ShowOptionsPage(object sender, EventArgs e)
        {
            ShowOptionPage(typeof(OptionsPage));
        }
    }
}
