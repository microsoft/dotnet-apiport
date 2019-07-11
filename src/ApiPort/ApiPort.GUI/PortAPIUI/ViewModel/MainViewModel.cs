// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Fx.Portability;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PortAPI.Shared;
using PortAPIUI;
using PortAPIUI.ViewModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Newtonsoft.Json.Linq;
using Microsoft.Fx.Portability;

internal class MainViewModel : ViewModelBase
{
    public RelayCommand Browse { get; set; }

    public RelayCommand Export { get; set; }

    public RelayCommand Analyze { get; set; }
    public IApiPortService Service { get; set; }



    private string _selectedPath;



    private List<string> _assemblies;

    private List<string> _assembliesPath;

    public static List<string> _config;

    public static List<string> _platform;

    public static string ExeFile;


    public static string _selectedConfig;

    public static string _selectedPlatform;


    public ObservableCollection<ApiViewModel> _assemblyCollection { get; set; }

    public static string _selectedAssembly;

    public static JArray _analyzeAssem;



    public ObservableCollection<ApiViewModel> AssemblyCollection
    {
        get
        {

            return _assemblyCollection;

        }

        set
        {


            _assemblyCollection = value;


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


    public JArray AnalyzeAssem
    {
        get { return _analyzeAssem; }

        set
        {
            _analyzeAssem = value;
            RaisePropertyChanged(nameof(AnalyzeAssem));
        }
    }


    public List<string> Config
    {
        get
        {


            return _config;

        }

        set
        {

            _config = value;


            RaisePropertyChanged(nameof(Config));
        }
    }

    public List<string> Platform

    {


        get { return _platform; }



        set

        {



            _platform = value;

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

        get => _selectedConfig;


       set
        {
            _selectedConfig = value;

            RaisePropertyChanged(nameof(SelectedConfig));
        }
    }

    public string SelectedPlatform
    {
        get
        {


            return _selectedPlatform;

        }

        set
        {
            _selectedPlatform = value;
            RaisePropertyChanged("SelectedPlatfrom");
        }
    }

    public string SelectedAssembly
    {
        get
        {


            return _selectedAssembly;


        }

        set
        {


            _selectedAssembly = value;


            RaisePropertyChanged(nameof(SelectedAssembly));
        }
    }


    public MainViewModel()
    {
        RegisterCommands();
        _assemblies = new List<string>();


        _config = new List<string>();
        _platform = new List<string>();


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

        ApiAnalyzer analyzer = new ApiAnalyzer();
        analyzer.AnalyzeAssemblies(ExeFile, Service);

    }

    public void AssemblyCollectionUpdate(string assem)
    {
        
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

            msBuild.GetAssemblies(SelectedPath);
            if (msBuild.MessageBox == true)
            {
                MessageBox.Show("Build your project first.");
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
        savedialog.FileName = "PortablityAnalysisReport";
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
