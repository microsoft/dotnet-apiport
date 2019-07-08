// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using BuildcsprojtoMem;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PortAPI.Shared;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MSBuildAnalyzer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            MSBuildLocator.RegisterDefaults();
            string csProjPath = args[0];
            string jsonPath = args[1];
            if (args.Length == 2)
            {
                Temp.BuildIt(csProjPath, jsonPath);
            }

            if (args.Length > 2)
            {
                string chosenConfig = args[2];
                string chosenPlat = args[3];
                string json1Path = args[4];
                Chosen.Configure(csProjPath, chosenConfig, chosenPlat, json1Path);
            }
        }
    }

    internal class Temp
    {
        private static List<string> configurations;

        private static List<string> platforms;

        public static List<string> Configurations { get => configurations; set => configurations = value; }

        public static List<string> Platforms { get => platforms; set => platforms = value; }

        public static void BuildIt(string csProjPath, string jsonPath)
        {
            const string box1 = "Configuration";
            const string box2 = "Platform";
            Dictionary<string, string> dic = new Dictionary<string, string>
                {
                    { "Configuration", "Debug" },
                    { "Platform", "AnyCPU" }
                };
            ProjectCollection pc = new ProjectCollection(dic, null, ToolsetDefinitionLocations.Default);

            var project = pc.LoadProject(csProjPath);

            // Console.WriteLine("Build: {0}", File.Exists(project.GetProperty("TargetPath").EvaluatedValue.ToString()));
            // Console.WriteLine(" ??");
            if (!File.Exists(path: project.GetProperty("TargetPath").EvaluatedValue.ToString()))
            {
                Info info = new Info("False", null, null, null, null, null);
            }
            else
            {
                Configurations = project.ConditionedProperties[box1];
                Platforms = project.ConditionedProperties[box2];

                // var con = new string[0];
                // var pla = new string[0];
                // Console.Write("Config:");
                // foreach (var config in Configurations)
                // {
                //    con.Append(config);
                //    Console.Write(" **" + config);
                // }
                // Console.WriteLine(" ");
                // Console.Write("Plat:");
                // foreach (var plat in Platforms)
                // {
                //    pla.Append(plat);
                //    Console.Write(" **" + plat);
                // }
                // Console.WriteLine(" ");
                // Console.Write("Assembly:");
                // if (project.Properties.Any(n => n.Name == "TargetPath"))
                // {
                var targetPath = project.GetProperty("TargetPath");
                var targetPathString = project.GetProperty("TargetPath").EvaluatedValue.ToString();
                var assembly = Assembly.ReflectionOnlyLoadFrom(targetPathString);
                JsonSerializer serializer = new JsonSerializer();
                StreamWriter sw = new StreamWriter(jsonPath, false);
                serializer.Converters.Add(new JavaScriptDateTimeConverter());
                serializer.NullValueHandling = NullValueHandling.Ignore;
                using (sw)
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    // Console.Write(" * *" + targetPathString);
                    List<string> assemblyCode = new List<string>();
                    foreach (var assemblyName in assembly.GetReferencedAssemblies())
                    {
                        assemblyCode.Add(Assembly.Load(assemblyName).ToString());
                    }

                    Info info = new Info(string.Format("{0}", File.Exists(project.GetProperty("TargetPath").EvaluatedValue.ToString())), Configurations, Platforms, targetPathString, assemblyCode, assembly.Location);
                    serializer.Serialize(writer, info);
                    sw.Close();
                    writer.Close();

                    // }
                }
            }
        }
    }
}
