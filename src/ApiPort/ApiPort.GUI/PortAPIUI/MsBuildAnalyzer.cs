
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.Build;

using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using PortAPI.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PortAPIUI

{
    internal class MsBuildAnalyzer
    {
        private static StringBuilder output = null;

        public bool MessageBox { get; set; }

        public Info GetAssemblies(string path)
        {
            var ourPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var ourDirectory = System.IO.Path.GetDirectoryName(ourPath);
            var analyzerPath = System.IO.Path.Combine(ourDirectory, "MSBuildAnalyzer\\BuildProj.exe");
            Process process = new Process();
            process.StartInfo.FileName = analyzerPath;
            process.StartInfo.Arguments = $"{path}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            output = new StringBuilder();
            process.OutputDataReceived += SortOutputHandler;
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.Close();

            JsonSerializer se = new JsonSerializer();
            var pathOf = System.Reflection.Assembly.GetEntryAssembly().Location;
            var directory = System.IO.Path.GetDirectoryName(pathOf);
            var read = System.IO.Path.Combine(directory, "json.txt");
            using (StreamReader r = new StreamReader(read))
            {
                string json = r.ReadToEnd();
                Info items = JsonConvert.DeserializeObject<Info>(json);
                Message(items);
                var consoleOutput = output.ToString();
                if (consoleOutput.Length > 1)
                {
                    return items;
                }
            }
            return null;

            //var consoleOutput = output.ToString();
            //if (consoleOutput.Length > 1)
            //{
            //    var popUp = consoleOutput.Substring(consoleOutput.IndexOf("Build:"), consoleOutput.IndexOf("??"));
            //    string[] array = popUp.Split(" ");
            //    string answer = array[1];
            //    Message(answer);
            //    if (answer.Equals("True"))
            //    {
            //        var start = consoleOutput.IndexOf("Plat:");
            //        var end = consoleOutput.IndexOf("Assembly:");
            //        var configurations = consoleOutput.Substring(consoleOutput.IndexOf("Config:"), start - consoleOutput.IndexOf("Config:")).Split(" **");
            //        List<string> config = new List<string>();
            //        for (int i = 1; i < configurations.Length; i++)
            //        {
            //            config.Add(configurations[i]);
            //        }

            //        var platforms = consoleOutput.Substring(start, end - start).Split(" **");

            //        List<string> plat = new List<string>();
            //        for (int i = 1; i < platforms.Length; i++)
            //        {
            //            plat.Add(platforms[i]);
            //        }

            //        var assemblies = consoleOutput.Substring(end).Split(" **");
            //        List<string> assem = new List<string>();
            //        for (int i = 1; i < assemblies.Length; i++)
            //        {
            //            assem.Add(assemblies[i]);
            //        }

            //        Info info = new Info(null, config, plat,null, assem);

            //        return info;
            //    }
            //}
            //return null;
        }
        public void Message(Info answer)
        {
            if (answer.Build.Equals("False"))
            {
                MessageBox = true;
            }
            else
            {
                MessageBox = false;
            }
        }

        private static void SortOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {

            if (!string.IsNullOrEmpty(outLine.Data))
            {
                output.Append(outLine.Data);
            }
        }
    }
}

