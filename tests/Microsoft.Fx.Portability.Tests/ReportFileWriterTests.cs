using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Tests
{
    [TestClass]
    public class ReportFileWriterTests
    {
        [TestMethod]
        public void UniquelyNamedFileStream_FileExists_AppendsNumberToName()
        {
            var dir = "dir";
            var fileName = "file";

            var format = ResultFormat.HTML;
            var extension = ".htm";

            var path = Path.Combine(dir, Path.ChangeExtension(fileName, extension));

            var progressReporter = Substitute.For<IProgressReporter>();
            var fileSystem = Substitute.ForPartsOf<WindowsFileSystem>();
            fileSystem.FileExists(path).Returns(true);

            var expectedResult = Path.Combine(dir, string.Concat(fileName, "(1)", extension));

            var exception = new IOException("this avoids having to fake more of the filesystem");
            fileSystem.CreateFile(Arg.Any<string>()).Returns(x => { throw exception; });

            var writer = new ReportFileWriter(fileSystem, progressReporter);
            var report = Encoding.UTF8.GetBytes("This is a test report.");

            string reportPath = null;
            try
            {
                reportPath = writer.WriteReportAsync(report, format, dir, fileName, overwrite: false).Result;
            }
            catch (IOException e)
            {
                if (e != exception)
                {
                    Assert.Fail();
                }
            }

            Assert.IsTrue(string.IsNullOrEmpty(reportPath));
            fileSystem.Received().CreateFile(expectedResult);
        }

        [TestMethod]
        public void UniquelyNamedFileStream_NumberedFileExists_IncrementsNumberInNewName()
        {
            var dir = "dir";
            var fileName = "file";
            var fileNameFormat = fileName + "({0})";

            var format = ResultFormat.Excel;
            var extension = ".xlsx";
            var path = Path.Combine(dir, Path.ChangeExtension(fileName, extension));


            var fileSystem = Substitute.ForPartsOf<WindowsFileSystem>();
            var progressReporter = Substitute.For<IProgressReporter>();

            int fileNumber = 1;
            do
            {
                fileSystem.FileExists(path).Returns(true);
                var nextFileName = string.Format(fileNameFormat, fileNumber);
                path = Path.Combine(dir, Path.ChangeExtension(nextFileName, extension));
            } while (fileNumber++ < 11);

            var exception = new IOException("this avoids having to fake more of the filesystem");
            fileSystem.CreateFile(Arg.Any<string>()).Returns(x => { throw exception; });

            var writer = new ReportFileWriter(fileSystem, progressReporter);
            var report = Encoding.UTF8.GetBytes("This is a test report.");

            string reportPath = null;
            try
            {
                reportPath = writer.WriteReportAsync(report, format, dir, fileName, overwrite: false).Result;
            }
            catch (IOException e)
            {
                if (e != exception)
                {
                    Assert.Fail();
                }
            }

            fileSystem.Received().CreateFile(path);
            Assert.IsTrue(string.IsNullOrEmpty(reportPath));
        }

        [TestMethod]
        public void VerifyReportHTMLContents()
        {
            var dir = "dir";
            var fileName = "file";

            var format = ResultFormat.HTML;
            var extension = ".htm";

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

            string reportPath = writer.WriteReportAsync(reportArray, format, dir, fileName, overwrite: false).Result;

            fileSystem.Received().CreateFile(expectedResult);
            Assert.AreEqual(expectedResult, reportPath);

            byte[] writtenBytes = memoryStream.ToArray();
            Assert.AreEqual(report, Encoding.UTF8.GetString(writtenBytes));
        }
    }
}
