// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Analyze;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.IO;

namespace ApiPortVS.Tests
{
    [TestClass]
    public class ApiPortVSPackageTests
    {
        [TestMethod]
        public void FileHasAnalyzableExtension_FileIsExe_ReturnsTrue()
        {
            var filename = "analyzable.exe";

            var package = GetProjectAnalyzer();

            var result = package.FileHasAnalyzableExtension(filename);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void FileHasAnalyzableExtension_FileIsDll_ReturnsTrue()
        {
            var filename = "analyzable.dll";

            var package = GetProjectAnalyzer();

            var result = package.FileHasAnalyzableExtension(filename);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void FileHasAnalyzableExtension_FilenameContainsVshost_ReturnsFalse()
        {
            var filename = "analyzable.vshost.exe";

            var package = GetProjectAnalyzer();

            var result = package.FileHasAnalyzableExtension(filename);

            Assert.IsFalse(result);
        }
        private ProjectAnalyzer GetProjectAnalyzer()
        {
            var fileSystem = Substitute.For<IFileSystem>();

            fileSystem.GetFileExtension(Arg.Any<string>()).Returns(arg =>
            {
                var path = arg.Arg<string>();

                return Path.GetExtension(path);
            });

            return new ProjectAnalyzer(
                null,
                null,
                null,
                null,
                null,
                fileSystem,
                null);
        }
    }
}
