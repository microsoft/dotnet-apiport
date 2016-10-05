// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Resources;
using Microsoft.Fx.Portability;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ApiPortVS.Models
{
    /// <summary>
    /// The options for ApiPort VS that is persisted
    /// </summary>
    public class OptionsModel : NotifyPropertyBase
    {
        private const string OptionsFileName = "options.dat";

        public static readonly string OptionsFilePath;

        private IList<SelectedResultFormat> _formats;
        private IList<TargetPlatform> _platforms;
        private string _outputDirectory;
        private string _defaultOutputName;

        static OptionsModel()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var directory = Path.GetDirectoryName(assembly.Location);

            OptionsFilePath = Path.Combine(directory, OptionsFileName);
        }

        public OptionsModel()
        {
            _platforms = Array.Empty<TargetPlatform>();
            _formats = Array.Empty<SelectedResultFormat>();
            _defaultOutputName = "ApiPortAnalysis";

            OutputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Portability Analysis");
        }

        public IList<SelectedResultFormat> Formats
        {
            get { return _formats; }
            set { UpdateProperty(ref _formats, value); }
        }

        public IList<TargetPlatform> Platforms
        {
            get { return _platforms; }
            set { UpdateProperty(ref _platforms, value.ToList()); }
        }

        public string OutputDirectory
        {
            get { return _outputDirectory; }
            set
            {
                UpdateProperty(ref _outputDirectory, value);

                if (!Directory.Exists(_outputDirectory))
                {
                    Directory.CreateDirectory(_outputDirectory);
                }
            }
        }

        public string DefaultOutputName
        {
            get { return _defaultOutputName; }
            set
            {
                UpdateProperty(ref _defaultOutputName, value);
            }
        }

        public static OptionsModel Load()
        {
            try
            {
                if (File.Exists(OptionsFilePath))
                {
                    var bytes = File.ReadAllBytes(OptionsFilePath);

                    return bytes.Deserialize<OptionsModel>();
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
            }

            return new OptionsModel();
        }

        public bool Save()
        {
            try
            {
                File.WriteAllBytes(OptionsFilePath, this.Serialize());

                return true;
            }
            catch (IOException)
            {
                Debug.WriteLine(string.Format(LocalizedStrings.UnableToSaveFileFormat, OptionsFilePath));

                return false;
            }
        }
    }
}
