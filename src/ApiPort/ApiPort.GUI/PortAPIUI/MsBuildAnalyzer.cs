
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


namespace PortAPIUI

{
    public class Info
    {
        public List<string> Configuration { get; set; }

        public List<string> Platform { get; set; }

        public List<string> Assembly { get; set; }

        public Info(List<string> configuration, List<string> platform, List<string> assembly)
        {
            Configuration = configuration;
            Platform = platform;
            Assembly = assembly;
        }
    }
    internal class MsBuildAnalyzer
    {
        private static StringBuilder output = null;
        public bool MessageBox { get; set; }
        public static PortAPIUI.Info GetAssemblies(string path)
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


            var consoleOutput = output.ToString();
            if (!string.IsNullOrEmpty(consoleOutput))
            {
                var popUp = consoleOutput.Substring(consoleOutput.IndexOf("Build:"), consoleOutput.IndexOf("Config:"));
                string[] array = popUp.Split(" ");
                string answer = array[1];
                MsBuildAnalyzer msBuild = new MsBuildAnalyzer();
                msBuild.Message(answer);
                var start = consoleOutput.IndexOf("Plat:");
                var end = consoleOutput.IndexOf("Assembly:");
                var configurations = consoleOutput.Substring(consoleOutput.IndexOf("Config:"), start- consoleOutput.IndexOf("Config:")).Split(" **");
                List<string> config = new List<string>();
                for (int i = 1; i < configurations.Length; i++)
                {
                    config.Add(configurations[i]);
                }

                var platforms = consoleOutput.Substring(start, end - start).Split(" **");

                List<string> plat = new List<string>();
                for (int i = 1; i < platforms.Length; i++)
                {
                    plat.Add(platforms[i]);
                }

                var assemblies = consoleOutput.Substring(end).Split(" **");
                List<string> assem = new List<string>();
                for (int i = 1; i < assemblies.Length; i++)
                {
                    assem.Add(assemblies[i]);
                }

                PortAPIUI.Info info = new PortAPIUI.Info(config, plat, assem);

                return info;
            }

            return null;
        }

        public void Message(string answer)
        {
            if (answer.Equals("True"))
            {
                MessageBox = true;
            }
            MessageBox = false;
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

