// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Reporting;
using NSubstitute;
using System.IO;
using System.Text;
using Xunit;

namespace Microsoft.Fx.Portability.Tests
{
    public class ReportFileWriterTests
    {
        [Fact]
        public void UniquelyNamedFileStream_FileExists_AppendsNumberToName()
        {
            var dir = "dir";
            var fileName = "file.htm";
            var newFileName = "file(1).htm";

            var path = Path.Combine(dir, fileName);

            var progressReporter = Substitute.For<IProgressReporter>();
            var fileSystem = Substitute.ForPartsOf<WindowsFileSystem>();
            fileSystem.FileExists(path).Returns(true);

            var expectedResult = Path.Combine(dir, newFileName);

            var exception = new IOException("this avoids having to fake more of the filesystem");
            fileSystem.CreateFile(Arg.Any<string>()).Returns(x => { throw exception; });

            var writer = new ReportFileWriter(fileSystem, progressReporter);
            var report = Encoding.UTF8.GetBytes("This is a test report.");

            string reportPath = null;
            try
            {
                reportPath = writer.WriteReportAsync(report, dir, fileName, overwrite: false).Result;
            }
            catch (IOException e)
            {
                if (e != exception)
                {
                    Assert.True(false, "Fail");
                }
            }

            Assert.True(string.IsNullOrEmpty(reportPath));
            fileSystem.Received().CreateFile(expectedResult);
        }

        [Fact]
        public void UniquelyNamedFileStream_NumberedFileExists_IncrementsNumberInNewName()
        {
            var dir = "dir";
            var fileName = "file.xlsx";
            var fileNameFormat = "file({0}).xlsx";

            var path = Path.Combine(dir, fileName);

            var fileSystem = Substitute.For<IFileSystem>();
            var progressReporter = Substitute.For<IProgressReporter>();

            fileSystem.CombinePaths(Arg.Any<string[]>()).Returns(a => Path.Combine(a.Arg<string[]>()));

            int fileNumber = 1;
            do
            {
                fileSystem.FileExists(path).Returns(true);
                var nextFileName = string.Format(fileNameFormat, fileNumber);
                path = Path.Combine(dir, nextFileName);
            } while (fileNumber++ < 11);

            var exception = new IOException("this avoids having to fake more of the filesystem");
            fileSystem.CreateFile(Arg.Any<string>()).Returns(x => { throw exception; });

            var writer = new ReportFileWriter(fileSystem, progressReporter);
            var report = Encoding.UTF8.GetBytes("This is a test report.");

            string reportPath = null;
            try
            {
                reportPath = writer.WriteReportAsync(report,  dir, fileName, overwrite: false).Result;
            }
            catch (IOException e)
            {
                if (e != exception)
                {
                    Assert.True(false, "Fail");
                }
            }

            fileSystem.Received().CreateFile(path);
            Assert.True(string.IsNullOrEmpty(reportPath));
        }

        [Fact]
        public void VerifyReportHTMLContents()
        {
            var dir = "dir";
            var fileName = "file.htm";
            var newFileName = "file(1).htm";

            var path = Path.Combine(dir, fileName);

            var progressReporter = Substitute.For<IProgressReporter>();
            var memoryStream = new MemoryStream();
            var fileSystem = Substitute.ForPartsOf<WindowsFileSystem>();
            fileSystem.FileExists(path).Returns(true);
            fileSystem.CreateFile(Arg.Any<string>()).Returns(memoryStream);

            var expectedResult = Path.Combine(dir, newFileName);

            var writer = new ReportFileWriter(fileSystem, progressReporter);
            var report = "This is a test report.";
            var reportArray = Encoding.UTF8.GetBytes(report);

            string reportPath = writer.WriteReportAsync(reportArray, dir, fileName, overwrite: false).Result;

            fileSystem.Received().CreateFile(expectedResult);
            Assert.Equal(expectedResult, reportPath);

            byte[] writtenBytes = memoryStream.ToArray();
            Assert.Equal(report, Encoding.UTF8.GetString(writtenBytes));
        }
    }
}
