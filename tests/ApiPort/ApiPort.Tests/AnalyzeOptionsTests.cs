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
        public static void TestAssemblyFlag_Directory()
        {
            var directoryPath = Directory.GetCurrentDirectory();
            var options = GetOptions($"analyze -f {directoryPath}");

            Assert.Equal(AppCommand.AnalyzeAssemblies, options.Command);
            Assert.NotEmpty(options.InputAssemblies);

            foreach (var element in options.InputAssemblies)
            {
                // The bool with the meaning of 'ExplicitlySpecified' should be false
                Assert.False(element.Value);
            }
        }

        [Fact]
        public static void NoArgs()
        {
            var options = CommandLineOptions.ParseCommandLineOptions(Array.Empty<string>());

            Assert.Equal(AppCommand.Exit, options.Command);
        }

        [Fact]
        public static void AnalyzeNoFile()
        {
            var options = GetOptions("analyze -f");

            Assert.Equal(AppCommand.Exit, options.Command);
        }

        [InlineData("dump -f file.dll", "file.dll", CommandLineOptions.DefaultName)]
        [InlineData("dump -f file.dll -o out.json", "file.dll", "out.json")]
        [Theory]
        public static void DumpAnalysis(string args, string file, string output)
        {
            var options = GetOptions(args);

            Assert.Equal(AppCommand.DumpAnalysis, options.Command);
            Assert.Equal(output, options.OutputFileName);

            // It will be in the invalid list because it cannot be found
            var input = Assert.Single(options.InvalidInputFiles);
            Assert.Equal(file, input);
        }

        [InlineData("analyze -f file.dll", CommandLineOptions.DefaultName)]
        [InlineData("analyze -f file.dll -o other", "other")]
        [InlineData("analyze -f file.dll --out other", "other")]
        [Theory]
        public static void OutputFile(string args, string name)
        {
            var options = CommandLineOptions.ParseCommandLineOptions(args.Split(' '));

            Assert.Equal(AppCommand.AnalyzeAssemblies, options.Command);
            Assert.Equal(name, options.OutputFileName);
        }

        [InlineData("analyze -f file.dll", false)]
        [InlineData("analyze -f file.dll -o other", true)]
        [InlineData("analyze -f file.dll --force", true)]
        [InlineData("analyze -f file.dll -o other --force", true)]
        [Theory]
        public static void OverwriteFile(string args, bool overwrite)
        {
            var options = GetOptions(args);

            Assert.Equal(AppCommand.AnalyzeAssemblies, options.Command);
            Assert.Equal(overwrite, options.OverwriteOutputFile);
        }

        [InlineData("listTargets", AppCommand.ListTargets)]
        [InlineData("listOutputFormats", AppCommand.ListOutputFormats)]
        [InlineData("docId", AppCommand.DocIdSearch)]
        [Theory]
        public static void SimpleCommandTests(string args, AppCommand command)
        {
            var options = GetOptions(args);

            Assert.Equal(command, options.Command);
        }

        [Fact]
        public static void TestAssemblyFlag_FileName()
        {
            var currentAssemblyPath = typeof(AnalyzeOptionsTests).GetTypeInfo().Assembly.Location;
            var options = GetOptions($"analyze -f {currentAssemblyPath}");

            Assert.Equal(AppCommand.AnalyzeAssemblies, options.Command);
            var input = Assert.Single(options.InputAssemblies);

            // The bool with the meaning of 'ExplicitlySpecified' should be true
            Assert.True(input.Value);
        }

        [Fact]
        public static void TestAssemblyFlag_DirectoryAndFileName()
        {
            var directoryPath = Directory.GetCurrentDirectory();
            var currentAssemblyPath = typeof(AnalyzeOptionsTests).GetTypeInfo().Assembly.Location;

            var options = GetOptions($"analyze -f {directoryPath} -f {currentAssemblyPath}");

            Assert.Equal(AppCommand.AnalyzeAssemblies, options.Command);
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

        private static ICommandLineOptions GetOptions(string args)
        {
            return CommandLineOptions.ParseCommandLineOptions(args.Split(' '));
        }
    }
}
