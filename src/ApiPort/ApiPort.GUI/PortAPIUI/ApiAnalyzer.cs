using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace PortAPIUI
{
    class ApiAnalyzer
    {
        public static JArray AnalyzeAssemblies(List<string> assemblies)
        {
            MessageBox.Show("Hi from Katie");
            string reportLocation = ExportResult.ExportApiResult("", ".json", true);
            string textFromFile = System.IO.File.ReadAllText(reportLocation);
            dynamic dynobj = JsonConvert.DeserializeObject(textFromFile);

            //will contain a list of dictionaries with the following following;
            //{ 
            //"DefinedInAssemblyIdentity": "PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
            //  "MemberDocId": "M:System.Windows.Shapes.Shape.set_Stroke(System.Windows.Media.Brush)",
            //  "TypeDocId": "T:System.Windows.Shapes.Shape",
            //  "RecommendedChanges": "",
            //  "SourceCompatibleChange": "",
            //  "TargetStatus": [
            //    "3.0",
            //    null
            //  ]
            //}
            JArray apiList = dynobj.MissingDependencies;
            return apiList;


        }
    }
}

