using Microsoft.Fx.Portability.Reporting.ObjectModel;
using Microsoft.Fx.Portability.Resources;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Reporting
{
    public class ReportFileWriter : IReportWriter
    {
        private readonly IFileSystem _fileSystem;
        private readonly IProgressReporter _progressReporter;

        public ReportFileWriter(IFileSystem fileSystem, IProgressReporter progressReporter)
        {
            _fileSystem = fileSystem;
            _progressReporter = progressReporter;
        }

        public async Task<string> WriteReportAsync(byte[] report, ResultFormat format, string outputDirectory, string reportFileName, bool overwrite)
        {
            if (report == null || report.Length == 0)
                return null;

            var filename = GetFileName(outputDirectory, reportFileName, format, isUnique: !overwrite);

            if (string.IsNullOrEmpty(filename))
                return null;

            var filePath = _fileSystem.CombinePaths(outputDirectory, filename);
            var isWritten = await TryWriteReportAsync(report, format, filePath);
            if (isWritten)
                return filePath;

            return null;
        }

        private async Task<bool> TryWriteReportAsync(byte[] report, ResultFormat format, string filePath)
        {
            try
            {
                using (Stream destinationStream = _fileSystem.CreateFile(filePath))
                {
                    switch (format)
                    {
                        case ResultFormat.Excel:
                            using (var memoryStream = new MemoryStream(report))
                                await memoryStream.CopyToAsync(destinationStream);
                            break;
                        case ResultFormat.HTML:
                            string html = Encoding.UTF8.GetString(report);
                            using (StreamWriter writer = new StreamWriter(destinationStream))
                                await writer.WriteAsync(html);
                            break;
                        default:
                            throw new NotSupportedException("This format is not supported, yet: " + format);
                    }
                }
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }

            return true;
        }

        private string GetFileName(string directory, string fileName, ResultFormat format, bool isUnique)
        {
            // We want to change the extension of the filename given regardless 
            // of whether the user gave an extension or not. However, if they give
            // us an extension and it doesn't match the expected one, we'll report
            // the problem to them.
            var originalExtension = Path.GetExtension(fileName);
            var extension = format.GetFileExtension();

            if (!string.IsNullOrEmpty(originalExtension) && !originalExtension.Equals(extension, StringComparison.InvariantCultureIgnoreCase))
            {
                _progressReporter.ReportIssue(LocalizedStrings.ChangingFileExtension, fileName, originalExtension, extension);
            }

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            var fileNameFormatString = string.Concat(fileNameWithoutExtension, "{0}", extension);
            var uniqueName = string.Format(fileNameFormatString, string.Empty);
            
            int i = 1;

            while (_fileSystem.FileExists(_fileSystem.CombinePaths(directory, uniqueName)))
            {
                // This file exists already but since we don't care about uniqueness, we'll overwrite it.
                if (!isUnique)
                {
                    _progressReporter.ReportIssue(LocalizedStrings.OverwriteFile, uniqueName);
                    return uniqueName;
                }

                var suffix = string.Format("({0})", i++);
                uniqueName = string.Format(fileNameFormatString, suffix);
            }

            return uniqueName;
        }
    }
}
