using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using PortAPIUI;
using PortAPIUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

class MainViewModel : ViewModelBase
{
    public RelayCommand Browse { get; set; }

    public RelayCommand Export { get; set; }

    public RelayCommand Analyze { get; set; }

    private string _selectedPath;

    private List<string> _assemblies;
    public static List<string> _config;
    public static List<string> _platform;

    public static string _selectedConfig;
    public static string _selectedPlatform;

    public ObservableCollection<ApiViewModel> _assemblyCollection { get; set; }

    public static string _selectedAssembly;


    public ObservableCollection<ApiViewModel> AssemblyCollection
    {
        get { return _assemblyCollection; }
        set
        {
            _assemblyCollection = value;
            RaisePropertyChanged("AssemblyCollection");
        }
    }

    public string SelectedPath
    {
        get { return _selectedPath; }
        set
        {
            _selectedPath = value;
            RaisePropertyChanged("SelectedPath");
        }
    }

    public List<string> Config

    {
        get { return _config; }
        set
        {
            _config = value;
            RaisePropertyChanged("Config");
        }
    }

    public List<string> Platform
    {
        get { return _platform; }
        set
        {
            _platform = value;
            RaisePropertyChanged("Platform");
        }
    }
    public List<string> Assemblies
    {
        get { return _assemblies; }
        set
        {
            _assemblies = value;
            RaisePropertyChanged("Assemblies");
        }
    }

    public string SelectedConfig
    {
        get { return _selectedConfig; }
        set
        {
            _selectedConfig = value;
            RaisePropertyChanged("SelectedConfig");
        }
    }

    public string SelectedPlatform
    {
        get { return _selectedPlatform; }
        set
        {
            _selectedPlatform = value;
            RaisePropertyChanged("SelectedPlatfrom");
        }
    }

    public string SelectedAssembly
    {
        get { return _selectedAssembly; }
        set
        {
            _selectedAssembly = value;
            RaisePropertyChanged("SelectedAssembly");
        }
    }
    public MainViewModel()
    {
        RegisterCommands();
        _assemblies = new List<string>();
        _config = new List<string>();
        _platform = new List<string>();
        _assemblyCollection = new ObservableCollection<ApiViewModel>();

    }

    private void RegisterCommands()
    {
        Browse = new RelayCommand(ExecuteOpenFileDialog);
        Export = new RelayCommand(ExecuteSaveFileDialog);
        Analyze = new RelayCommand(AnalyzeAPI);

    }


    private void AnalyzeAPI()
    {
        MsBuildAnalyzer msBuild = new MsBuildAnalyzer();
        if (msBuild.MessageBox.Equals(false))
        {
            MessageBox.Show("Error: Please build your project first.");
        }
        else
        {
            Assemblies = Rebuild.ChosenBuild(SelectedPath);
            ApiAnalyzer.AnalyzeAssemblies(Assemblies);
        }

    }
    
    public void AssemblyCollectionUpdate(string assem)
    {
        AssemblyCollection.Clear();
        foreach (var assembly in Assemblies)
        {
            if (assem.Equals(assembly))
            {
                AssemblyCollection.Add(new ApiViewModel(assembly, assembly+ " API Name ", true));
            }

        }

    }

    private void ExecuteOpenFileDialog()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.Filter = "Project File (*.csproj)|*.csproj|All files (*.*)|*.*";
        dialog.InitialDirectory = @"C:\";

        Nullable<bool> result = dialog.ShowDialog();
        if (result == true)
        {
            SelectedPath = dialog.FileName;
        }
        else { SelectedPath = null; }

        if (SelectedPath != null)
        {
           
           // else
            {
                ExportResult.InputPath = SelectedPath;

                Info output = MsBuildAnalyzer.GetAssemblies(SelectedPath);


                Config = output.Configuration;
                Platform = output.Platform;

                List<string> assemblyNames = output.Assembly;
            }
        }

    }

        private void ExecuteSaveFileDialog()
    {
        var savedialog = new Microsoft.Win32.SaveFileDialog();
        savedialog.FileName = "PortablityAnalysisReoprt";
        savedialog.DefaultExt = ".text";
        savedialog.Filter = "HTML file (*.html)|*.html|Json (*.json)|*.json| Excel (*.excel)|*.excel";
        Nullable<bool> result = savedialog.ShowDialog();
        if (result == true)
        {
            
            string fileExtension = Path.GetExtension(savedialog.FileName);
            ExportResult.ExportApiResult(savedialog.FileName, fileExtension);
        }

    }

}