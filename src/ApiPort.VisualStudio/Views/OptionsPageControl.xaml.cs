using ApiPortVS.Resources;
using ApiPortVS.ViewModels;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

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
            GuidanceLink.NavigateUri = new Uri(LocalizedStrings.MoreInformationUrl);

            Loaded += ControlLoaded;
            Unloaded += ControlUnloaded;
        }

        private void ControlUnloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= ControlUnloaded;

            ViewModel.SaveModel();
        }

        private async void ControlLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ControlLoaded;

            await ViewModel.UpdateTargets(); // checkboxes don't appear until this finishes
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.UpdateTargets();
        }
    }
}