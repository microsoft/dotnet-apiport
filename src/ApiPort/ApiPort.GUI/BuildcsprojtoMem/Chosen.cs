using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BuildcsprojtoMem
{
    internal class Chosen
    {
        public static void Configure(string path, string config, string plat)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>
                {
                    { "Configuration", config },
                    { "Platform", plat }
                };
            ProjectCollection pc = new ProjectCollection(dic, null, ToolsetDefinitionLocations.Default);

            var project = pc.LoadProject(path);
            project.Build();
            if (project.IsBuildEnabled == true)
            {
                Console.Write("Assembly:");
                if (project.Properties.Any(n => n.Name == "TargetPath"))
                {
                    var mypath = System.Reflection.Assembly.GetEntryAssembly().Location;
                    var targetPath = project.GetProperty("TargetPath");
                    var targetPathString = targetPath.EvaluatedValue;
                    var assembly = Assembly.LoadFrom(targetPathString);
                    foreach (var dependent in assembly.GetReferencedAssemblies())
                    {
                        Console.Write(" **" + Assembly.Load(dependent).Location); // get the path of b
                    }

                    Console.Write(" **" + assembly.Location);
                }
            }
        }
    }
}
