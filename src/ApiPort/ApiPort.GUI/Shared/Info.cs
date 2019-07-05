using System;
using System.Collections.Generic;

namespace PortAPI.Shared
{
    public class Info
    {
        public string Build { get; set; }
        public List<string> Configuration { get; set; }

        public List<string> Platform { get; set; }

        public string TargetPath { get; set; }

        public List<string> Assembly { get; set; }

        public string Location { get; set; }

        public Info(string build, List<string> configuration, List<string> platform, string targetPath, List<string> assembly, string location)
        {
            Build = build;
            Configuration = configuration;
            Platform = platform;
            TargetPath = targetPath;
            Assembly = assembly;
            Location = location;
        }
    }
}
