using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace PortAPIUI
{
    class ApiAnalyzer
    {
        public static void AnalyzeAssemblies(List<string> assemblies)
        {
            MessageBox.Show("Hi from Katie");
            string reportLocation = ExportResult.ExportApiResult("", ".json", true);
            string textFromFile = System.IO.File.ReadAllText(reportLocation);
            JObject apiResult = JObject.Parse(textFromFile);
            foreach (var target in apiResult["MissingDependencies"])
            {
                Debug.WriteLine(target);
            }

        }
    }
}

