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
            if (File.Exists(path: project.GetProperty("TargetPath").EvaluatedValue.ToString()))
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
                        assemblyCode.Add(assemblyName.ToString());
                    }

                    Info info = new Info(string.Format("{0}", File.Exists(path: project.GetProperty("TargetPath").EvaluatedValue.ToString())), null, null, targetPathString, assemblyCode, assembly.Location,false);
                    serializer.Serialize(writer, info);
                    sw.Close();
                    writer.Close();
                }
            }
            else
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;
                using (StreamWriter sw = new StreamWriter(json1Path, false))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    Info info = new Info(string.Format("{0}", File.Exists(path: project.GetProperty("TargetPath").EvaluatedValue.ToString())), null, null, null, null, null,false);
                    serializer.Serialize(writer, info);
                    sw.Close();
                    writer.Close();
                }
            }
        }
    }
}
