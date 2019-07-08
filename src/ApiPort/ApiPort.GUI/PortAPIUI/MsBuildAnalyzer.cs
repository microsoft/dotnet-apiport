// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PortAPI.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace PortAPIUI
{
    internal class MsBuildAnalyzer
    {
        private static StringBuilder output = null;

        public bool MessageBox { get; set; }

        public Info Items { get => items; set => items = value; }

        private Info items;

        public Info GetAssemblies(string path)
        {
            var ourPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var ourDirectory = System.IO.Path.GetDirectoryName(ourPath);
            var analyzerPath = System.IO.Path.Combine(ourDirectory, "MSBuildAnalyzer\\BuildProj.exe");
            var jsonPath = System.IO.Path.Combine(ourDirectory, "MSBuildAnalyzer\\j.txt");
            Process process = new Process();
            process.StartInfo.FileName = analyzerPath;
            process.StartInfo.Arguments = $"{path} {jsonPath}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            output = new StringBuilder();
            process.OutputDataReceived += SortOutputHandler;
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.Close();
            using (StreamReader r = new StreamReader(jsonPath))
            {
                string json = r.ReadToEnd();
                Items = JsonConvert.DeserializeObject<Info>(json);
                Message(Items);
                var consoleOutput = output.ToString();
                r.Close();
            }

            return Items;

            // var consoleOutput = output.ToString();
            // if (consoleOutput.Length > 1)
            // {
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

            // var platforms = consoleOutput.Substring(start, end - start).Split(" **");

            // List<string> plat = new List<string>();
            //        for (int i = 1; i < platforms.Length; i++)
            //        {
            //            plat.Add(platforms[i]);
            //        }

            // var assemblies = consoleOutput.Substring(end).Split(" **");
            //        List<string> assem = new List<string>();
            //        for (int i = 1; i < assemblies.Length; i++)
            //        {
            //            assem.Add(assemblies[i]);
            //        }

            // Info info = new Info(null, config, plat, null, assem,null);

            // return info;
            //    }
            // }
            // return null;
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
