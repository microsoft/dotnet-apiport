// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Analyze;
using ApiPortVS.Contracts;
using Microsoft.Fx.Portability.Reporting;
using NSubstitute;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ApiPortVS.Tests
{
    public class ApiPortVSPackageTests
    {
        [Fact]
        public void FileHasAnalyzableExtension_FileIsExe_ReturnsTrue()
        {
            var filename = "analyzable.exe";

            var package = GetProjectAnalyzer();

            var result = package.FileHasAnalyzableExtension(filename);

            Assert.True(result);
        }

        [Fact]
        public void FileHasAnalyzableExtension_FileIsDll_ReturnsTrue()
        {
            var filename = "analyzable.dll";

            var package = GetProjectAnalyzer();

            var result = package.FileHasAnalyzableExtension(filename);

            Assert.True(result);
        }

        [Fact]
        public void FileHasAnalyzableExtension_FilenameContainsVshost_ReturnsFalse()
        {
            var filename = "analyzable.vshost.exe";

            var package = GetProjectAnalyzer();

            var result = package.FileHasAnalyzableExtension(filename);

            Assert.False(result);
        }

        private ProjectAnalyzer GetProjectAnalyzer()
        {
            var threadingService = Substitute.For<IVSThreadingService>();

            threadingService.SwitchToMainThreadAsync().Returns(Task.CompletedTask);

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
                fileSystem,
                null,
                null,
                threadingService);
        }
    }
}
