
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
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace PortAPIUI
{
    public static class Rebuild
    {

        private static StringBuilder outputConsole = null;

        public static List<string> ChosenBuild(String path)
        {
            var ourPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var ourDirectory = System.IO.Path.GetDirectoryName(ourPath);
            var AnalyzerPath = System.IO.Path.Combine(ourDirectory, "MSBuildAnalyzer\\BuildProj.exe");
            Process process = new Process();
            process.StartInfo.FileName = AnalyzerPath;
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
            if (!String.IsNullOrEmpty(line.Data))
            {
                outputConsole.Append(line.Data);
            }
        }
    }
    public class info
    {
        public List<string> Configuration { get; set; }
        public List<string> Platform { get; set; }
        public List<string> Assembly { get; set; }
        public info(List<string> configuration, List<string> platform, List<string> assembly)
        {
            Configuration = configuration;
            Platform = platform;
            Assembly = assembly;
        }
    }

    class MsBuildAnalyzer
    {
        private static StringBuilder output = null;
        public static PortAPIUI.info GetAssemblies(string path)
        {
            var ourPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var ourDirectory = System.IO.Path.GetDirectoryName(ourPath);
            var AnalyzerPath = System.IO.Path.Combine(ourDirectory, "MSBuildAnalyzer\\BuildProj.exe");
            Process process = new Process();
            process.StartInfo.FileName = AnalyzerPath;
            process.StartInfo.Arguments = path;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            output = new StringBuilder();
            process.OutputDataReceived += SortOutputHandler;
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.Close();

            var ConsoleOutput = output.ToString();
            var start = ConsoleOutput.IndexOf("Plat:");
            var end = ConsoleOutput.IndexOf("Assembly:");
            var _configurations = ConsoleOutput.Substring(ConsoleOutput.IndexOf("Config:"), start).Split(" **");
            List<string> config = new List<string>();
            for (int i = 1; i < _configurations.Length; i++)

            {
                config.Add(_configurations[i]);
            }

            var _platforms = ConsoleOutput.Substring(start, end - start).Split(" **");

            List<string> plat = new List<string>();
            for (int i = 1; i < _platforms.Length; i++)
            {
                plat.Add(_platforms[i]);
            }
            var _assemblies = ConsoleOutput.Substring(end).Split(" **");
            List<string> assem = new List<string>();
            for (int i = 1; i < _assemblies.Length; i++)
            {
                assem.Add(_assemblies[i]);
            }
            PortAPIUI.info info = new PortAPIUI.info(config, plat, assem);

            return info;
        }
        private static void SortOutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                output.Append(outLine.Data);
            }
        }
    }
}
