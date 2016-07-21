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

        private IList<TargetPlatform> _platforms;

        static OptionsModel()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var directory = Path.GetDirectoryName(assembly.Location);

            OptionsFilePath = Path.Combine(directory, OptionsFileName);
        }

        public OptionsModel()
        {
            _platforms = new List<TargetPlatform>();
        }

        public IList<TargetPlatform> Platforms
        {
            get
            {
                return _platforms;
            }
            set
            {
                UpdateProperty(ref _platforms, value.ToList());
            }
        }

        public static OptionsModel Load()
        {
            try
            {
                var bytes = File.ReadAllBytes(OptionsFilePath);

                return bytes.Deserialize<OptionsModel>();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());

                return new OptionsModel();
            }
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
