using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using PortAPIUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;

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

   // public ObservableCollection<AssemblyModel> AssemblyCollection { get; set; } = new ObservableCollection<AssemblyModel>();

    public static string _selectedAssembly;

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

        /*
                foreach(var assembly in Assemblies)
                {
                    AssemblyCollection.Add(new AssemblyModel(assembly));
                }*/
        // ApiAnalyzer.AnalyzeAssemblies(Assemblies);
    }




    private void ExecuteOpenFileDialog()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.Filter = "Project File (*.csproj)|*.csproj|All files (*.*)|*.*";
        dialog.InitialDirectory = @"C:\";
        dialog.ShowDialog();
        SelectedPath = dialog.FileName;
        ExportResult.InputPath = dialog.FileName;
        Info output = MsBuildAnalyzer.GetAssemblies(SelectedPath);
        Config = output.Configuration;
        Platform = output.Platform;
        List<string> assemblyNames = output.Assembly;

        foreach (var assembly in assemblyNames)
        {
            string name = assembly.Substring(0, assembly.IndexOf(","));
            if (!name.Equals("mcroslib"))
            {
                Hyperlink link = new Hyperlink();
                link.NavigateUri = new Uri(uriString: $"https://www.nuget.org/packages/{name}");
                ProcessStartInfo psi = new ProcessStartInfo(link.NavigateUri.ToString());
                psi.UseShellExecute = true;
                Process.Start(psi);
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
            ExportResult.ExportApiResult(savedialog.FileName, fileExtension, false);
        }

    }

}