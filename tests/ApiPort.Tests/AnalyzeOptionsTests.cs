// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace ApiPort.Tests
{
    public class AnalyzeOptionsTests
    {
        /// <summary>
        /// Test that 'ExplicitlySpecified' flag is correctly set up when a directory is passed
        /// in command line for analyzing
        /// </summary>
        [Fact]
        public void TestAssemblyFlag_Directory()
        {
            var directoryPath = Directory.GetCurrentDirectory();
            var args = new string[3] {
                "analyze",
                "-f",
                directoryPath
            };

            var options = CommandLineOptions.ParseCommandLineOptions(args);

            Assert.Equal(AppCommands.AnalyzeAssemblies, options.Command);
            Assert.NotEmpty(options.InputAssemblies);

            foreach (var element in options.InputAssemblies)
            {
                // The bool with the meaning of 'ExplicitlySpecified' should be false
                Assert.False(element.Value);
            }
        }

        [Fact]
        public void NoArgs()
        {
            var options = CommandLineOptions.ParseCommandLineOptions(Array.Empty<string>());

            Assert.Equal(AppCommands.Exit, options.Command);
        }

        [Fact]
        public void AnalyzeNoFile()
        {
            var args = "analyze -f".Split(' ');

            var options = CommandLineOptions.ParseCommandLineOptions(args);

            Assert.Equal(AppCommands.Exit, options.Command);
        }

        [InlineData("analyze -f file.dll", CommandLineOptions.DefaultName)]
        [InlineData("analyze -f file.dll -o other", "other")]
        [InlineData("analyze -f file.dll --out other", "other")]
        [Theory]
        public void OutputFile(string args, string name)
        {
            var options = CommandLineOptions.ParseCommandLineOptions(args.Split(' '));

            Assert.Equal(AppCommands.AnalyzeAssemblies, options.Command);
            Assert.Equal(name, options.OutputFileName);
        }

        [InlineData("analyze -f file.dll", false)]
        [InlineData("analyze -f file.dll -o other", true)]
        [InlineData("analyze -f file.dll --force", true)]
        [InlineData("analyze -f file.dll -o other --force", true)]
        [Theory]
        public void OverwriteFile(string args, bool overwrite)
        {
            var options = CommandLineOptions.ParseCommandLineOptions(args.Split(' '));

            Assert.Equal(AppCommands.AnalyzeAssemblies, options.Command);
            Assert.Equal(overwrite, options.OverwriteOutputFile);
        }

        [InlineData("listTargets", AppCommands.ListTargets)]
        [InlineData("listtargets", AppCommands.ListTargets)]
        [InlineData("listOutputFormats", AppCommands.ListOutputFormats)]
        [InlineData("listoutputFormats", AppCommands.ListOutputFormats)]
        [InlineData("docId", AppCommands.DocIdSearch)]
        [InlineData("docid", AppCommands.DocIdSearch)]
        [Theory]
        public void SimpleCommandTests(string args, AppCommands command)
        {
            var options = CommandLineOptions.ParseCommandLineOptions(args.Split(' '));

            Assert.Equal(command, options.Command);
        }

        [Fact]
        public void TestAssemblyFlag_FileName()
        {
            var currentAssemblyPath = typeof(AnalyzeOptionsTests).GetTypeInfo().Assembly.Location;
            var args = new string[3] {
                "analyze",
                "-f",
                currentAssemblyPath
            };

            var options = CommandLineOptions.ParseCommandLineOptions(args);

            Assert.Equal(AppCommands.AnalyzeAssemblies, options.Command);
            var input = Assert.Single(options.InputAssemblies);

            // The bool with the meaning of 'ExplicitlySpecified' should be true
            Assert.True(input.Value);
        }

        [Fact]
        public void TestAssemblyFlag_DirectoryAndFileName()
        {
            var directoryPath = Directory.GetCurrentDirectory();
            var currentAssemblyPath = typeof(AnalyzeOptionsTests).GetTypeInfo().Assembly.Location;

            var args = new string[5] {
                "analyze",
                "-f",
                directoryPath,
                "-f",
                currentAssemblyPath
            };
            var options = CommandLineOptions.ParseCommandLineOptions(args);

            Assert.Equal(AppCommands.AnalyzeAssemblies, options.Command);
            Assert.NotEmpty(options.InputAssemblies);

            // The scenario tested is when an assembly is passed in twice, once explicitly and once as part of the folder
            // Assert that we test this scenario.
            Assert.Equal(Path.GetDirectoryName(currentAssemblyPath), directoryPath, StringComparer.OrdinalIgnoreCase);

            foreach (var element in options.InputAssemblies)
            {
                if (element.Key.Name.Equals(currentAssemblyPath, StringComparison.OrdinalIgnoreCase))
                {
                    Assert.True(element.Value);
                }
                else
                {
                    Assert.False(element.Value);
                }
            }
        }
    }
}
