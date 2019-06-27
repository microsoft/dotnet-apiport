using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.Build.Locator;
using Microsoft.Build.Utilities;
using System.Net.Http.Headers;
using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Build.Tasks;
using System.Diagnostics;
using BuildcsprojtoMem;

namespace MSBuildAnalyzer
{
    public class Program
    {
        static void Main(string[] args)
        {
            MSBuildLocator.RegisterDefaults();
            if (args.Length != 0)
            {
                string CsProjPath = args[0];
                if (args.Length == 1)
                {
                    Temp.MyMethod(CsProjPath);
                }
                if (args.Length > 1)
                {
                    string ChosenConfig = args[1];
                    string ChosenPlat = args[2];
                    Chosen.configure(CsProjPath, ChosenConfig, ChosenPlat);
                }
            }
        }
    }
    public class Temp
    {
        public static List<string> configurations;
        public static List<String> platforms;
        public static void MyMethod(string CsProjPath)
        {
            const string box1 = "Configuration";
            const string box2 = "Platform";
            Dictionary<String, String> dic = new Dictionary<string, string>
                {
                    { box1, "Debug" },
                    { box2, "AnyCPU" }
                };
            ProjectCollection pc = new ProjectCollection(dic, null, ToolsetDefinitionLocations.Default);
            var project = pc.LoadProject(CsProjPath);

            project.Build();

            configurations = project.ConditionedProperties[box1];
            platforms = project.ConditionedProperties[box2];
            var con = new string[0];
            var pla = new string[0];
            Console.Write("Config:");
            foreach (var config in configurations)
            {
                con.Append(config);
                Console.Write(" **" + config);
            }
            Console.WriteLine(" ");
            Console.Write("Plat:");
            foreach (var plat in platforms)
            {
                pla.Append(plat);
                Console.Write(" **" + plat);
            }
            Console.WriteLine(" ");
            Console.Write("Assembly:");
            if (project.Properties.Any(n => n.Name == "TargetPath"))
            {
                var myPath = System.Reflection.Assembly.GetEntryAssembly().Location;
                var targetPath = project.GetProperty("TargetPath");
                var targetPathString = targetPath.EvaluatedValue.ToString();
                var assembly = Assembly.LoadFrom(targetPathString);
                foreach (AssemblyName AssemblyName in assembly.GetReferencedAssemblies())
                {
                    Console.Write(" **" + Assembly.Load(AssemblyName).Location);
                }
                Console.Write(" **" + assembly.Location);
            }

        }
    }
}


