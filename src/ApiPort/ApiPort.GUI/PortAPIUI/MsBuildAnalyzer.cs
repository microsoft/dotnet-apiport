// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build;
using Microsoft.Fx.Portability.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet;
using PortAPI.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Versioning;
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

        private Info Items;

        public static bool MessageBox1 { get; private set; }

        public Info GetAssemblies(string path)
        {
            var ourPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            var ourDirectory = System.IO.Path.GetDirectoryName(ourPath);
            var analyzerPath = System.IO.Path.Combine(ourDirectory, "MSBuildAnalyzer\\BuildProj.exe");
            var jsonPath = System.IO.Path.Combine(ourDirectory, "MSBuildAnalyzer\\j.txt");
            Process process = new Process();
            process.StartInfo.FileName = analyzerPath;
            process.StartInfo.Arguments = $"\"{path}\" \"{jsonPath}\"";
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
                var consoleOutput = output.ToString();
                r.Close();
            }

            Message(Items);
            return Items;
        }

        public static void Message(Info answer)
        {
            if (answer.Package == false)
            {
                MessageBox1 = true;
            }
            else
            {
                MessageBox1 = false;
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
