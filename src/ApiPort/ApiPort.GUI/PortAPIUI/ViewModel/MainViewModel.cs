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
using System.Threading.Tasks;
using Microsoft.Fx.Portability.ObjectModel;

internal class MainViewModel : ViewModelBase
{
    public RelayCommand Browse { get; set; }

    public RelayCommand Export { get; set; }

    public RelayCommand Analyze { get; set; }

    public IApiPortService Service { get; set; }



    private string _selectedPath;



    private List<string> _assemblies;

    private HashSet<string> _assembliesPath;

    public static List<string> _config;

    public static List<string> _platform;

    public static string ExeFile;


    public static string _selectedConfig;

    public static string _selectedPlatform;


    public ObservableCollection<ApiViewModel> _assemblyCollection { get; set; }

    public static string _selectedAssembly;


    private IList<MemberInfo> members;


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

    public HashSet<string> AssembliesPath
    {
        get { return _assembliesPath; }

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

    public IList<MemberInfo> Members { get => members; set => members = value; }

    public MainViewModel()
    {
        RegisterCommands();
        _assemblies = new List<string>();


        _config = new List<string>();
        _platform = new List<string>();
        _assembliesPath = new HashSet<string>();

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
        var analyzeAssembliesTask = Task.Run<IList<MemberInfo>>(async () => { return await analyzer.AnalyzeAssemblies(ExeFile, Service); } );
        analyzeAssembliesTask.Wait();
        members = analyzeAssembliesTask.Result;
        foreach (var r in members)
        {
            AssembliesPath.Add(r.DefinedInAssemblyIdentity);
        }

    }

    public void AssemblyCollectionUpdate(string assem)
    {

        AssemblyCollection.Clear();

        foreach (var r in members)
        {
            
            foreach (var assembly in AssembliesPath)
            {
                if (assem.Equals(assembly))
                 {
                     AssemblyCollection.Add(new ApiViewModel(assembly, r.MemberDocId, false, r.RecommendedChanges));
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

                //AssembliesPath = output.Assembly;

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
