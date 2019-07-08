// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Evaluation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PortAPI.Shared;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace BuildcsprojtoMem
{
    internal class Chosen
    {
        public static void Configure(string path, string config, string plat, string json1Path)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>
                {
                    { "Configuration", config },
                    { "Platform", plat }
                };
            ProjectCollection p = new ProjectCollection(dic, null, ToolsetDefinitionLocations.Default);

            var project = p.LoadProject(path);
            project.Build();
            if (project.Build() == true)
            {
                var targetPath = project.GetProperty("TargetPath");
                var targetPathString = project.GetProperty("TargetPath").EvaluatedValue.ToString();
                var assembly = Assembly.ReflectionOnlyLoadFrom(targetPathString);
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;
                using (StreamWriter sw = new StreamWriter(json1Path, false))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    List<string> assemblyCode = new List<string>();
                    foreach (var assemblyName in assembly.GetReferencedAssemblies())
                    {
                        assemblyCode.Add(Assembly.Load(assemblyName).ToString());
                    }

                    Info info = new Info(null, null, null, targetPathString, assemblyCode, assembly.Location);
                    serializer.Serialize(writer, info);
                    sw.Close();
                    writer.Close();
                }

                    // Console.Write("Assembly:");
                    // if (project.Properties.Any(n => n.Name == "TargetPath"))
                    // {
                    //    var mypath = System.Reflection.Assembly.GetEntryAssembly().Location;
                    //    var targetPath = project.GetProperty("TargetPath");
                    //    var targetPathString = targetPath.EvaluatedValue;
                    //    var assembly = Assembly.LoadFrom(targetPathString);
                    //    foreach (var dependent in assembly.GetReferencedAssemblies())
                    //    {
                    //        Console.Write(" **" + Assembly.Load(dependent).Location); // get the path of b
                    //    }

                    // Console.Write(" **" + assembly.Location);
                    // }
                }
        }
    }
}
