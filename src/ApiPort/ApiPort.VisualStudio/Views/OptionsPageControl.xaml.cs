// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.ViewModels;
using Microsoft.Fx.Portability;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ApiPortVS.Views
{
    /// <summary>
    /// Interaction logic for OptionPageControl.xaml
    /// </summary>
    public partial class OptionsPageControl : UserControl
    {
        private OptionsViewModel ViewModel { get { return DataContext as OptionsViewModel; } }

        public OptionsPageControl(OptionsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            Loaded += async (s, e) => await viewModel.UpdateAsync().ConfigureAwait(false);
            Unloaded += (s, e) => viewModel.Save();
        }

        private void NavigateToPrivacyModel(object sender, RequestNavigateEventArgs e) => Process.Start(DocumentationLinks.About.OriginalString);

        private void NavigateToMoreInformation(object sender, RequestNavigateEventArgs e) => Process.Start(DocumentationLinks.About.OriginalString);

        private async void RefreshRequested(object sender, RoutedEventArgs e)
        {
            await ViewModel.UpdateAsync(force: true).ConfigureAwait(false);
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
