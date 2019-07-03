using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;

namespace PortAPIUI
{
    class ApiAnalyzer
    {
        // gets input path of csproj file
        public static string ReportingPath;
        public static string InputPath;
        public static void AnalyzeAssemblies(List<string> assemblies)
        {
            MessageBox.Show("Hi from Katie");
          
            //    string ourPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            //    string apiDllPath = System.IO.Path.Combine(ourPath, "ApiPort", "netcoreapp2.1", "ApiPort.dll");
            //    string InputPathParent = System.IO.Directory.GetParent(InputPath).FullName;

            //    Process p = new Process();

            //    p.StartInfo.FileName = "dotnet.exe";
            //    string reportPath = GenerateReportPath();

            //    //output to Desktop

            //    p.StartInfo.Arguments = $"{apiDllPath} analyze -f \"{InputPathParent}\" -o \"{reportPath}\" -t \".NET Core, Version=3.0\"";
            //    var Hello = p.StartInfo.Arguments;
            //    p.StartInfo.CreateNoWindow = true;
            //    p.StartInfo.UseShellExecute = false;
            //    p.StartInfo.RedirectStandardOutput = true;
            //    p.EnableRaisingEvents = true;

            //    List<string> msg = new List<string>();

            //    p.OutputDataReceived += (s, o) =>
            //    {
            //        if (o.Data != null)
            //        {
            //            Application.Current.Dispatcher.Invoke(() =>
            //            {
            //                msg.Add(o.Data);
            //            });

            //        }
            //        //AnalzeBtn.IsEnabled = true;
            //    };

            //    p.Exited += delegate
            //    {
            //        Application.Current.Dispatcher.Invoke(() =>
            //        {
            //            string text;

            //            if (msg.Count != 17) // Was not successful
            //            {

            //                if (msg.Count < 10) // Exception was thrown in the API console tool
            //                {
            //                    text = $"Unable to analyze. The access to the specified path might be denied.";
            //                }
            //                else
            //                {
            //                    text = msg.FindLast(o => !String.IsNullOrEmpty(o));
            //                    text = text.Trim(new char[] { '*', ' ' });
            //                    if (!text.Equals("No files were found to analyze.", StringComparison.InvariantCultureIgnoreCase))
            //                    {
            //                        msg.RemoveRange(0, 10);
            //                        var details = String.Join(Environment.NewLine, msg.ToArray());
            //                        text = $"Unable to analyze.{Environment.NewLine}Details:{Environment.NewLine}{details}";
            //                    }
            //                }
            //            }
            //            else // Was successful
            //            {
            //                //text = $"Report saved in: {Environment.NewLine}{reportPath}";
            //                //OpenReportButton.Visible = true;
            //            }

            //            //OutputTextBox.Text = text;
            //            //UIline1.Visible = true;
            //        });

            //    };

            //    p.Start();

            //    //UIline1.Visible = true;
            //    //OutputTextBox.Text = "Analyzing...";

            //    p.BeginOutputReadLine();

            //}
            //private static string GenerateReportPath()
            //{
            //    var outputDirectory = System.IO.Path.GetTempPath();
            //    var outputName = "PortabilityReport";
            //    var outputExtension = ".xlsx";
            //    var counter = 1;
            //    var outputPath = System.IO.Path.Combine(outputDirectory, outputName + outputExtension);

            //    while (File.Exists(outputPath))
            //    {
            //        outputPath = System.IO.Path.Combine(outputDirectory, $"{outputName}({counter}){outputExtension}");
            //        counter++;
            //    }

            //    return outputPath;
        }
    }
}

