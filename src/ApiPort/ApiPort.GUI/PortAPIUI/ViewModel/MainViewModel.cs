// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PortAPI.Shared;
using PortAPIUI;
using PortAPIUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

internal class MainViewModel : ViewModelBase
{
    public RelayCommand Browse { get; set; }

    public RelayCommand Export { get; set; }

    public RelayCommand Analyze { get; set; }

    private string _selectedPath;

    private List<string> _assemblies;
    private List<string> _assembliesPath;
    private List<string> config1;
    private List<string> platform1;
    private string exeFile;

    private string selectedConfig1;
    private string selectedPlatform1;
    private string selectedAssembly1;

    public ObservableCollection<ApiViewModel> AssemblyCollection
    {
        get
        {
            return AssemblyCollection;
        }

        set
        {
            AssemblyCollection = value;
            RaisePropertyChanged(nameof(AssemblyCollection));
        }
    }

    public string SelectedPath
    {
        get => _selectedPath;

        set
        {
            _selectedPath = value;
            RaisePropertyChanged(nameof(SelectedPath));
        }
    }

    public List<string> Config
    {
        get
        {
            return Config1;
        }

        set
        {
            Config1 = value;
            RaisePropertyChanged(nameof(Config));
        }
    }

    public List<string> Platform
    {
        get
        {
            return Platform1;
        }

        set
        {
            Platform1 = value;
            RaisePropertyChanged(nameof(Platform));
        }
    }

    public List<string> Assemblies
    {
        get
        {
            return _assemblies;
        }

        set
        {
            _assemblies = value;
            RaisePropertyChanged(nameof(Assemblies));
        }
    }

    public List<string> AssembliesPath
    {
        get => _assembliesPath;

        set
        {
            _assembliesPath = value;
            RaisePropertyChanged(nameof(AssembliesPath));
        }
    }

    public string SelectedConfig
    {
        get => SelectedConfig1;

        set
        {
            SelectedConfig1 = value;
            RaisePropertyChanged(nameof(SelectedConfig));
        }
    }

    public string SelectedPlatform
    {
        get
        {
            return SelectedPlatform1;
        }

        set
        {
            SelectedPlatform1 = value;
            RaisePropertyChanged("SelectedPlatfrom");
        }
    }

    public string SelectedAssembly
    {
        get
        {
            return SelectedAssembly1;
        }

        set
        {
            SelectedAssembly1 = value;
            RaisePropertyChanged(nameof(SelectedAssembly));
        }
    }

    public List<string> Config1 { get => config1; set => config1 = value; }

    public List<string> Platform1 { get => platform1; set => platform1 = value; }

    public string ExeFile { get => exeFile; set => exeFile = value; }

    public string SelectedConfig1 { get => selectedConfig1; set => selectedConfig1 = value; }

    public string SelectedPlatform1 { get => selectedPlatform1; set => selectedPlatform1 = value; }

    public string SelectedAssembly1 { get => selectedAssembly1; set => selectedAssembly1 = value; }

    public MainViewModel()
    {
        RegisterCommands();
        _assemblies = new List<string>();
        Config1 = new List<string>();
        Platform1 = new List<string>();
        AssemblyCollection = new ObservableCollection<ApiViewModel>();
    }

    private void RegisterCommands()
    {
        Browse = new RelayCommand(ExecuteOpenFileDialog);
        Export = new RelayCommand(ExecuteSaveFileDialog);
        Analyze = new RelayCommand(AnalyzeAPI);
    }

    private void AnalyzeAPI()
    {
        Assemblies = Rebuild.ChosenBuild(SelectedPath);

        // ApiAnalyzer.AnalyzeAssemblies(Assemblies);
    }

    public void AssemblyCollectionUpdate(string assem)
    {
        AssemblyCollection.Clear();
        foreach (var assembly in AssembliesPath)
        {
            if (assem.Equals(assembly))
            {
                AssemblyCollection.Add(new ApiViewModel(assembly, assembly + " API Name ", true));
            }
        }
    }

    private void ExecuteOpenFileDialog()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.Filter = "Project File (*.csproj)|*.csproj|All files (*.*)|*.*";
        dialog.InitialDirectory = @"C:\";
        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            SelectedPath = dialog.FileName;
        }
        else
        {
            SelectedPath = null;
        }

        MsBuildAnalyzer msBuild = new MsBuildAnalyzer();
        if (SelectedPath != null)
        {
            ExportResult.SetInputPath(SelectedPath);
            msBuild.GetAssemblies(SelectedPath);
            if (msBuild.MessageBox == true)
            {
                MessageBox.Show("error");
            }

            Info output = msBuild.GetAssemblies(SelectedPath);
            if (output != null)
            {
                Config = output.Configuration;
                Platform = output.Platform;
                AssembliesPath = output.Assembly;
                ExeFile = output.Location;
            }
        }
    }

    private void ExecuteSaveFileDialog()
    {
        var savedialog = new Microsoft.Win32.SaveFileDialog();
        savedialog.FileName = "PortablityAnalysisReoprt";
        savedialog.DefaultExt = ".text";
        savedialog.Filter = "HTML file (*.html)|*.html|Json (*.json)|*.json| Excel (*.excel)|*.excel";
        bool? result = savedialog.ShowDialog();
        if (result == true)
        {
            string fileExtension = Path.GetExtension(savedialog.FileName);
            ExportResult.ExportApiResult(savedialog.FileName, fileExtension, false);
        }
    }
}
