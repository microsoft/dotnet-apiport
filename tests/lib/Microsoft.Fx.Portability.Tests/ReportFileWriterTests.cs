// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Reporting;
using NSubstitute;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Fx.Portability.Tests
{
    public class ReportFileWriterTests
    {
        [Fact]
        public static async Task FileExists_OverwriteTrue()
        {
            var dir = "dir";
            var fileName = "file";
            var extension = ".html";

            var path = Path.Combine(dir, Path.ChangeExtension(fileName, extension));

            var progressReporter = Substitute.For<IProgressReporter>();
            var fileSystem = Substitute.ForPartsOf<WindowsFileSystem>();

            fileSystem.FileExists(path).Returns(true);
            fileSystem.CreateFile(Arg.Any<string>()).Returns(x => new MemoryStream());

            var expectedResult = path;
            var writer = new ReportFileWriter(fileSystem, progressReporter);
            var report = Encoding.UTF8.GetBytes("This is a test report.");

            var reportPath = await writer.WriteReportAsync(report, extension, dir, fileName, overwrite: true);

            Assert.Equal(expectedResult, reportPath);

            fileSystem.Received(1).CreateFile(Arg.Any<string>());
            fileSystem.Received().CreateFile(expectedResult);
        }

        [Fact]
        public static async Task UniquelyNamedFileStream_FileExists_AppendsNumberToName()
        {
            var dir = "dir";
            var fileName = "file";
            var extension = ".html";

            var path = Path.Combine(dir, Path.ChangeExtension(fileName, extension));

            var progressReporter = Substitute.For<IProgressReporter>();
            var fileSystem = Substitute.ForPartsOf<WindowsFileSystem>();

            fileSystem.FileExists(path).Returns(true);
            fileSystem.CreateFile(Arg.Any<string>()).Returns(x => new MemoryStream());

            var expectedResult = Path.Combine(dir, string.Concat(fileName, "(1)", extension));
            var writer = new ReportFileWriter(fileSystem, progressReporter);
            var report = Encoding.UTF8.GetBytes("This is a test report.");

            var reportPath = await writer.WriteReportAsync(report, extension, dir, fileName, overwrite: false);

            Assert.Equal(expectedResult, reportPath);

            fileSystem.Received(1).CreateFile(Arg.Any<string>());
            fileSystem.Received().CreateFile(expectedResult);
        }

        [Fact]
        public static async Task UniquelyNamedFileStream_NumberedFileExists_IncrementsNumberInNewName()
        {
            const int FileExistsCount = 11;
            var dir = "dir";
            var fileName = "file";
            var fileNameFormat = fileName + "({0})";
            var extension = ".xlsx";
            var path = Path.Combine(dir, Path.ChangeExtension(fileName, extension));

            var fileSystem = Substitute.For<IFileSystem>();
            var progressReporter = Substitute.For<IProgressReporter>();

            fileSystem.CombinePaths(Arg.Any<string[]>()).Returns(a => Path.Combine(a.Arg<string[]>()));
            fileSystem.CreateFile(Arg.Any<string>()).Returns(x => new MemoryStream());

            int fileNumber = 1;
            do
            {
                fileSystem.FileExists(path).Returns(true);
                var nextFileName = string.Format(CultureInfo.CurrentCulture, fileNameFormat, fileNumber);
                path = Path.Combine(dir, Path.ChangeExtension(nextFileName, extension));
            }
            while (fileNumber++ < FileExistsCount);

            var writer = new ReportFileWriter(fileSystem, progressReporter);
            var report = Encoding.UTF8.GetBytes("This is a test report.");

            var reportPath = await writer.WriteReportAsync(report, extension, dir, fileName, overwrite: false);

            Assert.Equal(path, reportPath);

            fileSystem.Received(1).CreateFile(Arg.Any<string>());
            fileSystem.Received().CreateFile(path);
        }

        [Fact]
        public static async Task VerifyReportHTMLContents()
        {
            var dir = "dir";
            var fileName = "file";
            var extension = ".html";

            var path = Path.Combine(dir, Path.ChangeExtension(fileName, extension));

            var progressReporter = Substitute.For<IProgressReporter>();
            var memoryStream = new MemoryStream();
            var fileSystem = Substitute.ForPartsOf<WindowsFileSystem>();
            fileSystem.FileExists(path).Returns(true);
            fileSystem.CreateFile(Arg.Any<string>()).Returns(memoryStream);

            var expectedResult = Path.Combine(dir, string.Concat(fileName, "(1)", extension));

            var writer = new ReportFileWriter(fileSystem, progressReporter);
            var report = "This is a test report.";
            var reportArray = Encoding.UTF8.GetBytes(report);

            string reportPath = await writer.WriteReportAsync(reportArray, extension, dir, fileName, overwrite: false);

            fileSystem.Received(1).CreateFile(Arg.Any<string>());
            fileSystem.Received().CreateFile(expectedResult);
            Assert.Equal(expectedResult, reportPath);

            byte[] writtenBytes = memoryStream.ToArray();
            Assert.Equal(report, Encoding.UTF8.GetString(writtenBytes));
        }

        [Fact]
        public static async Task DoNotUpdateExtensionIfInputExtensionIsEmpty()
        {
            var dir = "dir";
            var fileName = "file.test";
            var extension = string.Empty;

            var path = Path.Combine(dir, fileName);

            var progressReporter = Substitute.For<IProgressReporter>();
            var memoryStream = new MemoryStream();
            var fileSystem = Substitute.ForPartsOf<WindowsFileSystem>();
            fileSystem.CreateFile(Arg.Any<string>()).Returns(memoryStream);

            var writer = new ReportFileWriter(fileSystem, progressReporter);
            var report = "This is a test report.";
            var reportArray = Encoding.UTF8.GetBytes(report);

            string reportPath = await writer.WriteReportAsync(reportArray, extension, dir, fileName, overwrite: false);

            fileSystem.Received(1).CreateFile(Arg.Any<string>());
            fileSystem.Received().CreateFile(path);
            Assert.Equal(path, reportPath);

            byte[] writtenBytes = memoryStream.ToArray();
            Assert.Equal(report, Encoding.UTF8.GetString(writtenBytes));
        }
    }
}
