using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BuildcsprojtoMem
{
    class Chosen
    {
        public static void configure(string path, string config, string plat)
        {
            Dictionary<String, String> dic = new Dictionary<string, string>
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
                    var exeName = project.GetProperty("TargetName");
                 
                    var targetPathString = targetPath.EvaluatedValue.ToString();
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
