using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PortAPIUI
{
    public static class Rebuild
    {
        private static StringBuilder outputConsole = null;
        public static List<string> ChosenBuild(string path)
        {
            var ourPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var ourDirectory = System.IO.Path.GetDirectoryName(ourPath);
            var analyzerPath = System.IO.Path.Combine(ourDirectory, "MSBuildAnalyzer\\BuildProj.exe");
            Process process = new Process();
            process.StartInfo.FileName = analyzerPath;
            process.StartInfo.Arguments = $"{path} {MainViewModel._selectedConfig} {MainViewModel._selectedPlatform}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            outputConsole = new StringBuilder();
            process.OutputDataReceived += OutputHandler;
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.Close();
            List<string> assemblies = new List<string>();

            var splitAssembly = outputConsole.ToString().Split(" **");
            for (int i = 1; i < splitAssembly.Length; i++)
            {
                assemblies.Add(splitAssembly[i]);
            }
            return assemblies;
        }
        private static void OutputHandler(object sendingProcess, DataReceivedEventArgs line)
        {
            if (!string.IsNullOrEmpty(line.Data))
            {
                outputConsole.Append(line.Data);
            }
        }
    }
}
