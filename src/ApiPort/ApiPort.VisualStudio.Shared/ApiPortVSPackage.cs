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
using System.Threading;

namespace ApiPortVS
{
    [Guid(Guids.ApiPortVSPkgString)]
    [InstalledProductRegistration("#110", "#112", "1.1.10808.0", IconResourceID = 400)] // Help->About info
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)] // load when a solution is opened
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionsPage), ".NET Portability Analyzer", "General", 110, 113, true)]
    [ProvideToolWindow(typeof(AnalysisOutputToolWindow))]
    [ProvideService(typeof(ApiPortVSPackage), IsAsyncQueryable = true)]
    public class ApiPortVSPackage : AsyncPackage, IResultToolbar
    {
        private static ServiceProvider _serviceProvider;
        private AssemblyRedirectResolver _assemblyResolver;

        internal static IServiceProvider LocalServiceProvider => _serviceProvider;

        internal static IAsyncServiceProvider LocalServiceProviderAsync => _serviceProvider;

        protected override void Dispose(bool disposing)
        {
            _serviceProvider?.Dispose();

            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;

            base.Dispose(disposing);
        }

        // Called after constructor when package is sited
        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);

            _serviceProvider = await ServiceProvider.CreateAsync(this);
            _assemblyResolver = _serviceProvider.GetService(typeof(AssemblyRedirectResolver)) as AssemblyRedirectResolver;

            if (_assemblyResolver == default(AssemblyRedirectResolver))
            {
                throw new InvalidOperationException(LocalizedStrings.CouldNotFindAssemblyRedirectResolver);
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            if (await GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService mcs)
            {
                var menuInitializer = await LocalServiceProviderAsync.GetServiceAsync(typeof(AnalyzeMenu)) as AnalyzeMenu;

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                // Add menu items for Analyze toolbar menu
                var anazlyMenuCommandID = new CommandID(Guids.AnalyzeMenuItemCmdSet, (int)PkgCmdID.CmdIdAnalyzeMenuItem);
                var menuItem = new MenuCommand(menuInitializer.AnalyzeMenuItemCallback, anazlyMenuCommandID);
                mcs.AddCommand(menuItem);

                var analyzeMenuOptionsCommandID = new CommandID(Guids.AnalyzeMenuItemCmdSet, (int)PkgCmdID.CmdIdAnalyzeOptionsMenuItem);
                var analyzeMenuOptionsItem = new MenuCommand(ShowOptionsPage, analyzeMenuOptionsCommandID);
                mcs.AddCommand(analyzeMenuOptionsItem);

                var analyzeMenuToolbarCommandID = new CommandID(Guids.AnalyzeMenuItemCmdSet, (int)PkgCmdID.CmdIdAnalyzeToolbarMenuItem);
                var analyzeMenuToolbarItem = new MenuCommand(async (_, e) => await ShowToolbarAsync().ConfigureAwait(false), analyzeMenuToolbarCommandID);
                mcs.AddCommand(analyzeMenuToolbarItem);

                // Add menu items for Project context menus
                var projectContextMenuCmdId = new CommandID(Guids.ProjectContextMenuItemCmdSet, (int)PkgCmdID.CmdIdProjectContextMenuItem);
                var contextMenuItem = new OleMenuCommand(async (_, e) => await menuInitializer.AnalyzeSelectedProjectsAsync(false), projectContextMenuCmdId);
                contextMenuItem.BeforeQueryStatus += menuInitializer.ProjectContextMenuItemBeforeQueryStatus;
                mcs.AddCommand(contextMenuItem);

                var projectContextMenuDependentsCmdId = new CommandID(Guids.ProjectContextMenuItemCmdSet, (int)PkgCmdID.CmdIdProjectContextDependentsMenuItem);
                var contextMenuDependentsItem = new OleMenuCommand(async (_, e) => await menuInitializer.AnalyzeSelectedProjectsAsync(true), projectContextMenuDependentsCmdId);
                contextMenuDependentsItem.BeforeQueryStatus += menuInitializer.ProjectContextMenuDependenciesItemBeforeQueryStatus;
                mcs.AddCommand(contextMenuDependentsItem);

                var projectContextMenuOptionsCmdId = new CommandID(Guids.ProjectContextMenuItemCmdSet, (int)PkgCmdID.CmdIdProjectContextOptionsMenuItem);
                var contextMenuOptionsItem = new OleMenuCommand(ShowOptionsPage, projectContextMenuOptionsCmdId);
                contextMenuOptionsItem.BeforeQueryStatus += menuInitializer.ProjectContextMenuItemBeforeQueryStatus;
                mcs.AddCommand(contextMenuOptionsItem);

                // Add menu items for Solution context menus
                var solutionContextMenuCmdId = new CommandID(Guids.SolutionContextMenuItemCmdSet, (int)PkgCmdID.CmdIdSolutionContextMenuItem);
                var solutionContextMenuItem = new OleMenuCommand(menuInitializer.SolutionContextMenuItemCallback, solutionContextMenuCmdId);
                solutionContextMenuItem.BeforeQueryStatus += menuInitializer.SolutionContextMenuItemBeforeQueryStatus;
                mcs.AddCommand(solutionContextMenuItem);

                var solutionContextMenuOptionsCmdId = new CommandID(Guids.SolutionContextMenuItemCmdSet, (int)PkgCmdID.CmdIdSolutionContextOptionsMenuItem);
                var solutionContextMenuOptionsItem = new OleMenuCommand(ShowOptionsPage, solutionContextMenuOptionsCmdId);
                solutionContextMenuOptionsItem.BeforeQueryStatus += menuInitializer.SolutionContextMenuItemBeforeQueryStatus;
                mcs.AddCommand(solutionContextMenuOptionsItem);
            }
        }

        public async System.Threading.Tasks.Task ShowToolbarAsync()
        {
            // Calls to UI elements should use the main task thread.
            // https://blogs.msdn.microsoft.com/andrewarnottms/2014/05/07/asynchronous-and-multithreaded-programming-within-vs-using-the-joinabletaskfactory/
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var window = FindToolWindow(typeof(AnalysisOutputToolWindow), 0, true);
            if ((window == null) || (window.Frame == null))
            {
                throw new NotSupportedException(LocalizedStrings.CannotCreateToolWindow);
            }

            var windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args) => _assemblyResolver.ResolveAssembly(args.Name, args.RequestingAssembly);

        private void ShowOptionsPage(object sender, EventArgs e) => ShowOptionPage(typeof(OptionsPage));
    }
}
