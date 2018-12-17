// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Analyze;
using Microsoft.Fx.Portability.Reporting;
using NSubstitute;
using System.IO;
using Xunit;

namespace ApiPortVS.Tests
{
    public class ApiPortVSPackageTests
    {
        [InlineData("analyzable.exe", true)]
        [InlineData("analyzable.dll", true)]
        [InlineData("analyzable.vshost.exe", false)]
        [Theory]
        public static void FileHasAnalyzableExtensionTest(string filename, bool expected)
        {
            var fileSystem = Substitute.For<IFileSystem>();

            fileSystem.GetFileExtension(Arg.Any<string>()).Returns(arg =>
            {
                var path = arg.Arg<string>();

                return Path.GetExtension(path);
            });

            var package = new ProjectAnalyzer(
                null,
                null,
                null,
                null,
                null,
                fileSystem,
                null,
                null);

            Assert.Equal(expected, package.FileHasAnalyzableExtension(filename));
        }
    }
}
