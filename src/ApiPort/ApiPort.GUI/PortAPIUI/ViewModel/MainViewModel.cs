// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Linq;
using PortAPI.Shared;
using PortAPIUI;
using PortAPIUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

internal class MainViewModel : ViewModelBase
{
    public RelayCommand Browse { get; set; }

    public RelayCommand Export { get; set; }

    public RelayCommand Analyze { get; set; }

    public IApiPortService Service { get; set; }

    private string _selectedPath;

    private List<string> _assemblies;

    private List <string> _assembliesPath;

    private HashSet<string> _chooseAssemblies;

    public static List<string> _config;

    public static List<string> _platform;

    public static string ExeFile;

    public static string _selectedConfig;

    public static string _selectedPlatform;

    public ObservableCollection<ApiViewModel> _assemblyCollection { get; set; }

    public static string _selectedAssembly;

    public IList<MemberInfo> _members;

    public IList<MemberInfo> Members
    {
        get
        {
            return _members;
        }

        set
        {
            _members = value;
            RaisePropertyChanged(nameof(Members));
        }
    }

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
        get { return _assembliesPath; }

        set
        {
            _assembliesPath = value;
            RaisePropertyChanged(nameof(AssembliesPath));
        }
    }

    public HashSet<string> ChooseAssemblies
    {
        get { return _chooseAssemblies; }

        set
        {
            _chooseAssemblies = value;
            RaisePropertyChanged(nameof(ChooseAssemblies));
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
        _chooseAssemblies = new HashSet<string>();
        _assembliesPath = new List<string>();


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
        Rebuild.ChosenBuild(SelectedPath);
        if (Rebuild.MessageBox == true)
        {
            MessageBox.Show("Build your project first.");
        }

        Info info = Rebuild.ChosenBuild(SelectedPath);
        AssembliesPath = info.Assembly;
        ExeFile = info.Location;

        ApiAnalyzer analyzer = new ApiAnalyzer();
        var analyzeAssembliesTask = Task.Run<IList<MemberInfo>>(async () => { return await analyzer.AnalyzeAssemblies(ExeFile, Service); } );
        analyzeAssembliesTask.Wait();
        Members = analyzeAssembliesTask.Result;
        foreach (var r in Members)
        {
            ChooseAssemblies.Add(r.DefinedInAssemblyIdentity);
        }

    }

    public void AssemblyCollectionUpdate(string assem)
    {


        AssemblyCollection.Clear();

        foreach (var r in Members)
        {
            foreach (var assembly in ChooseAssemblies)
            {
                if (assem.Equals(assembly))
                 {
                    AssemblyCollection.Add(new ApiViewModel(r.DefinedInAssemblyIdentity, r.MemberDocId, false, r.RecommendedChanges));
                 }
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
            Info output = msBuild.GetAssemblies(SelectedPath);
            if (output != null)
            {
                if (MsBuildAnalyzer.MessageBox1 == true)
                {
                    MainWindow mv = new MainWindow();
                    mv.AssemCompatibility.Visibility = Visibility.Visible;
                    mv.AssemCompatibility.Text ="Warning: In order to port to .NET Core," +
                        "NuGet References need to be in PackageReference format, not Packages.config.";
                }

                Config = output.Configuration;
                Platform = output.Platform;
                ExeFile = output.Location;

            }
        }
    }

    private void ExecuteSaveFileDialog()
    {
        var savedialog = new Microsoft.Win32.SaveFileDialog();
        savedialog.FileName = "PortablityAnalysisReport";
        savedialog.DefaultExt = ".text";
        savedialog.Filter = "HTML file (*.html)|*.html|Json (*.json)|*.json|Csv (*.csv)|*.csv";
        bool? result = savedialog.ShowDialog();
        if (result == true)
        {
            ExportResult exportClass= new ExportResult();
            exportClass.ExportApiResult(_selectedPath, Service, savedialog.FileName);
        }
    }
}
