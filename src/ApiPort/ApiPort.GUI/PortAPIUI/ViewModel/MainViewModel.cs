// Copyright (c) Microsoft. All rights reserved.

// Licensed under the MIT license. See LICENSE file in the project root for full license information.



using GalaSoft.MvvmLight;

using GalaSoft.MvvmLight.Command;

using PortAPI.Shared;

using PortAPIUI;

using PortAPIUI.ViewModel;
<<<<<<< HEAD

using Newtonsoft.Json;

=======
using Newtonsoft.Json;
>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33
using System;

using System.Collections.Generic;

using System.Collections.ObjectModel;

using System.IO;

using System.Windows;
using Newtonsoft.Json.Linq;

using Newtonsoft.Json.Linq;



internal class MainViewModel : ViewModelBase

{

    public RelayCommand Browse { get; set; }



    public RelayCommand Export { get; set; }



    public RelayCommand Analyze { get; set; }



    private string _selectedPath;



<<<<<<< HEAD




    private List<string> _assemblies;



    private List<string> _assembliesPath;



    public static List<string> _config;



    public static List<string> _platform;



=======
    private List<string> _assemblies;

    private List<string> _assembliesPath;

    public static List<string> _config;

    public static List<string> _platform;

>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33
    public static string ExeFile;



<<<<<<< HEAD




    public static string _selectedConfig;



=======
    public static string _selectedConfig;

>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33
    public static string _selectedPlatform;



<<<<<<< HEAD




=======
>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33
    public ObservableCollection<ApiViewModel> _assemblyCollection { get; set; }



<<<<<<< HEAD




    public static string _selectedAssembly;



    public static JArray _analyzeAssem;




=======
    public static string _selectedAssembly;

    public static JArray _analyzeAssem;

>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33

    public ObservableCollection<ApiViewModel> AssemblyCollection

    {

        get

        {

<<<<<<< HEAD


            return _assemblyCollection;



=======
            return _assemblyCollection;

>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33
        }



        set

        {

<<<<<<< HEAD


            _assemblyCollection = value;



=======
            _assemblyCollection = value;

>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33
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


<<<<<<< HEAD

=======
    public JArray AnalyzeAssem
    {
        get { return _analyzeAssem; }

        set
        {
            _analyzeAssem = value;
            RaisePropertyChanged(nameof(AnalyzeAssem));
        }
    }
>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33


    public List<string> Config

    {

        get

        {

<<<<<<< HEAD


            return _config;



=======
            return _config;

>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33
        }



        set

        {

<<<<<<< HEAD


            _config = value;



=======
            _config = value;

>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33
            RaisePropertyChanged(nameof(Config));

        }

    }



    public List<string> Platform

<<<<<<< HEAD


    {





        get { return _platform; }





        set



        {





            _platform = value;



            RaisePropertyChanged(nameof(Platform));



        }



=======
    {


        get { return _platform; }


        set

        {


            _platform = value;

            RaisePropertyChanged(nameof(Platform));

        }

>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33
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

<<<<<<< HEAD


        get => _selectedConfig;


=======
        get => _selectedConfig;
>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33

        set

        {
<<<<<<< HEAD

            _selectedConfig = value;



=======
            _selectedConfig = value;

>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33
            RaisePropertyChanged(nameof(SelectedConfig));

        }

    }



    public string SelectedPlatform

    {

        get

        {

<<<<<<< HEAD


            return _selectedPlatform;



=======
            return _selectedPlatform;

>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33
        }



        set

        {
<<<<<<< HEAD

            _selectedPlatform = value;

=======
            _selectedPlatform = value;
>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33
            RaisePropertyChanged("SelectedPlatfrom");

        }

    }



    public string SelectedAssembly

    {

        get

        {

<<<<<<< HEAD


            return _selectedAssembly;



=======
            return _selectedAssembly;

>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33
        }



        set

        {

<<<<<<< HEAD


            _selectedAssembly = value;



=======
            _selectedAssembly = value;

>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33
            RaisePropertyChanged(nameof(SelectedAssembly));

        }

    }

<<<<<<< HEAD





=======

>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33

    public MainViewModel()

    {

        RegisterCommands();

        _assemblies = new List<string>();

<<<<<<< HEAD


        _config = new List<string>();

        _platform = new List<string>();



=======
        _config = new List<string>();
        _platform = new List<string>();

>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33
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
<<<<<<< HEAD

       // AnalyzeAssem = ApiAnalyzer.AnalyzeAssemblies(ExeFile);



=======
        AnalyzeAssem = ApiAnalyzer.AnalyzeAssemblies(ExeFile);
>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33


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

<<<<<<< HEAD


=======
>>>>>>> 30e515ac18110789e7d10e573bb4d5b1a42ded33
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