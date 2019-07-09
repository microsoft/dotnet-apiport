// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
[assembly: System.Windows.Media.DisableDpiAwareness]
namespace PortAPIUI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = new MainViewModel { Service = App.Resolve<IApiPortService>()};
            InitializeComponent();
        }

        // Opens hyperlink to Portablitity Analyzer documentation
        private void About_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = new Hyperlink();
            link.NavigateUri = new Uri("https://docs.microsoft.com/en-us/dotnet/standard/analyzers/portability-analyzer");
            ProcessStartInfo psi = new ProcessStartInfo(link.NavigateUri.ToString());
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        // Opens hyperlink to Microsoft Privacy Statement
        private void Privacy_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = new Hyperlink();
            link.NavigateUri = new Uri("https://privacy.microsoft.com/en-us/privacystatement");
            ProcessStartInfo psi = new ProcessStartInfo(link.NavigateUri.ToString());
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        // Enables export button, and table wehn analyze button is clicked
        private void BStart_Click(object sender, RoutedEventArgs e)
        {
            ExportBtn.IsEnabled = true;
            APIGrid.IsEnabled = true;
            AssemComboBox.IsEnabled = true;
        }

        // Removes IsInDesignMode Column from datagrid
        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            PropertyDescriptor propertyDescriptor = (PropertyDescriptor)e.PropertyDescriptor;
            e.Column.Header = propertyDescriptor.DisplayName;
            if (propertyDescriptor.DisplayName == "IsInDesignMode")
            {
                e.Cancel = true;
            }
        }

        // Populates datagrid based on selected assembly
        private void AssemComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AssemCompatibility.Visibility = Visibility.Visible;
            var vm = this.DataContext as MainViewModel;
            var assem = vm.SelectedAssembly;
            vm.AssemblyCollectionUpdate(assem);
        }
    }
}
