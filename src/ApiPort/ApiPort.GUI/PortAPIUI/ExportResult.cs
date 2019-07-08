// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;

namespace PortAPIUI
{
    internal class ExportResult
    {
        private static string inputPath;

        public static string GetInputPath()
        {
            return inputPath;
        }

        public static void SetInputPath(string value)
        {
            inputPath = value;
        }

        // returns location of the portabitlity analyzer result
        public static string ExportApiResult(string exportPath, string fileExtension, bool generateOwnExportPath)
        {
            string ourPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string apiDllPath = Path.Combine(ourPath, "..\\..\\..", "DotnetDll", "ApiPort.dll");
            //string inputPathParent1 = System.IO.Directory.GetParent(inputPath).FullName;
           // string apiDllPath = System.IO.Path.Combine(ourPath, "bin", "Debug", "ApiPort", "netcoreapp2.1", "ApiPort.dll");
            //string inputPathParent = System.IO.Directory.GetParent(GetInputPath()).FullName;
            Process p = new Process();
            p.StartInfo.FileName = "dotnet.exe";
            if (generateOwnExportPath)
            {
                exportPath = GenerateReportPath(fileExtension);
            }

            string specifyExportOption = string.Empty;
            switch (fileExtension)
            {
                case ".html":
                    specifyExportOption = " -r html";
                    break;
                case ".json":
                    specifyExportOption = " -r json";
                    break;
                case ".excel":
                    specifyExportOption = " -r excel";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(fileExtension);
            }

            p.StartInfo.Arguments = $"{apiDllPath} analyze -f \"{inputPath}\" -o \"{exportPath}\" -t \".NET Core, Version=3.0\"{specifyExportOption}";
            var hello = p.StartInfo.Arguments;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.EnableRaisingEvents = true;

            List<string> msg = new List<string>();

            p.OutputDataReceived += (s, o) =>
            {
                if (o.Data != null)
                {

                }


            };

            p.Start();


            p.WaitForExit();

            return exportPath;
        }

        private static string GenerateReportPath(string fileExtension)
        {
            var outputDirectory = System.IO.Path.GetTempPath();
            var outputName = "PortabilityReport";
            var outputExtension = fileExtension;
            var counter = 1;
            var outputPath = System.IO.Path.Combine(outputDirectory, outputName + outputExtension);

            while (File.Exists(outputPath))
            {
                outputPath = System.IO.Path.Combine(outputDirectory, $"{outputName}({counter}){outputExtension}");
                counter++;
            }

            return outputPath;
        }
    }
}
