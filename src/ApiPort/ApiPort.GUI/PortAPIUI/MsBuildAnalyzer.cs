
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PortAPIUI
{
    internal class MsBuildAnalyzer
    {
        private static StringBuilder output = null;

        public static PortAPIUI.Info GetAssemblies(string path)
        {
            var ourPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var ourDirectory = System.IO.Path.GetDirectoryName(ourPath);
            var analyzerPath = System.IO.Path.Combine(ourDirectory, "MSBuildAnalyzer\\BuildProj.exe");
            Process process = new Process();
            process.StartInfo.FileName = analyzerPath;
            process.StartInfo.Arguments = path;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            output = new StringBuilder();
            process.OutputDataReceived += SortOutputHandler;
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.Close();

            var consoleOutput = output.ToString();
            var start = consoleOutput.IndexOf("Plat:");
            var end = consoleOutput.IndexOf("Assembly:");
            var configurations = consoleOutput.Substring(consoleOutput.IndexOf("Config:"), start).Split(" **");
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

        private static void SortOutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                output.Append(outLine.Data);
            }
        }
    }
}
