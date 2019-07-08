// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Windows;

namespace PortAPIUI
{
    internal class ApiAnalyzer
    {
        public static JArray AnalyzeAssemblies(string exelocation)
        {

         
            ExportResult.SetInputPath(exelocation);
            MessageBox.Show("Hi from Katie");
            string reportLocation = ExportResult.ExportApiResult(string.Empty, ".json", true);
            
            string textFromFile = System.IO.File.ReadAllText(reportLocation);
            dynamic dynobj = JsonConvert.DeserializeObject(textFromFile);

            // will contain a list of dictionaries with the following following;
            // {
            // "DefinedInAssemblyIdentity": "PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
            //  "MemberDocId": "M:System.Windows.Shapes.Shape.set_Stroke(System.Windows.Media.Brush)",
            //  "TypeDocId": "T:System.Windows.Shapes.Shape",
            //  "RecommendedChanges": "",
            //  "SourceCompatibleChange": "",
            //  "TargetStatus": [
            //    "3.0",
            //    null
            //  ]
            // }
            JArray apiList = dynobj.MissingDependencies;
            return apiList;
        }
    }
}
