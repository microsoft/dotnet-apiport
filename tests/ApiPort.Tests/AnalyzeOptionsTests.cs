// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.CommandLine;
using System;
using System.IO;
using System.Linq;
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

            Assert.True(options is AnalyzeOptions.AnalyzeCommandLineOption);
            var analyzeOptions = options as AnalyzeOptions.AnalyzeCommandLineOption;

            Assert.True(analyzeOptions.InputAssemblies.Any());

            foreach (var element in analyzeOptions.InputAssemblies)
            {
                // The bool with the meaning of 'ExplicitlySpecified' should be false
                Assert.False(element.Value);
            }
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
            Assert.True(options is AnalyzeOptions.AnalyzeCommandLineOption);

            var analyzeOptions = options as AnalyzeOptions.AnalyzeCommandLineOption;
            Assert.True(analyzeOptions.InputAssemblies.Count() == 1);

            // The bool with the meaning of 'ExplicitlySpecified' should be true
            Assert.True(analyzeOptions.InputAssemblies.First().Value);
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

            Assert.True(options is AnalyzeOptions.AnalyzeCommandLineOption);
            var analyzeOptions = options as AnalyzeOptions.AnalyzeCommandLineOption;

            Assert.True(analyzeOptions.InputAssemblies.Any());

            // The scenario tested is when an assembly is passed in twice, once explicitly and once as part of the folder
            // Assert that we test this scenario.
            Assert.True(Path.GetDirectoryName(currentAssemblyPath).Equals(directoryPath, StringComparison.OrdinalIgnoreCase));

            foreach (var element in analyzeOptions.InputAssemblies)
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
