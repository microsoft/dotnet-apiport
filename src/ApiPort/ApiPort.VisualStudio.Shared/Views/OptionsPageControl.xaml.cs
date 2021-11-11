// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Resources;
using ApiPortVS.ViewModels;
using Microsoft.Fx.Portability;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using Task = System.Threading.Tasks.Task;

namespace ApiPortVS.Views
{
    /// <summary>
    /// Interaction logic for OptionPageControl.xaml.
    /// </summary>
    public partial class OptionsPageControl : UserControl
    {
        private readonly IVsStatusbar _statusBar;

        private OptionsViewModel ViewModel => DataContext as OptionsViewModel;

        public OptionsPageControl(OptionsViewModel viewModel, IVsStatusbar statusBar)
        {
            InitializeComponent();
            DataContext = viewModel;

            _statusBar = statusBar;

            Loaded += (s, e) => UpdateModelAsync(false).FileAndForget(ApiPortVSPackage.FaultEventName);
            Unloaded += (s, e) => viewModel.Save();
        }

        private void NavigateToPrivacyModel(object sender, RequestNavigateEventArgs e) => Process.Start(DocumentationLinks.PrivacyPolicy.OriginalString);

        private void NavigateToMoreInformation(object sender, RequestNavigateEventArgs e) => Process.Start(DocumentationLinks.About.OriginalString);

        private void RefreshRequested(object sender, RoutedEventArgs e) => UpdateModelAsync(true).FileAndForget(ApiPortVSPackage.FaultEventName);

        private async Task UpdateModelAsync(bool force)
        {
            await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _statusBar.SetText(LocalizedStrings.RefreshingPlatforms);

            // using a local here to capture ViewModel on the UI thread
            var viewModel = ViewModel;
            await viewModel.UpdateAsync(force: force);

            if (viewModel.HasError)
            {
                _statusBar.SetText(viewModel.ErrorMessage);
            }
            else
            {
                _statusBar.SetText(LocalizedStrings.RefreshingPlatformsComplete);
            }
        }

        private void UpdateDirectoryClick(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = ViewModel.OutputDirectory;

                var result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    ViewModel.OutputDirectory = dialog.SelectedPath;
                }
            }

            UpdateDirectoryButton.Focus();
        }
    }
}
