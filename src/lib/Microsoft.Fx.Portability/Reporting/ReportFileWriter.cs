// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Reporting
{
    public class ReportFileWriter : IFileWriter
    {
        private readonly IFileSystem _fileSystem;
        private readonly IProgressReporter _progressReporter;

        public ReportFileWriter(IFileSystem fileSystem, IProgressReporter progressReporter)
        {
            _fileSystem = fileSystem;
            _progressReporter = progressReporter;
        }

        public async Task<string> WriteReportAsync(byte[] report, string extension, string outputDirectory, string reportFileName, bool overwrite)
        {
            if (report == null || report.Length == 0)
            {
                return null;
            }

            var filename = GetFileName(outputDirectory, reportFileName, extension, isUnique: !overwrite);

            if (string.IsNullOrEmpty(filename))
            {
                return null;
            }

            var filePath = _fileSystem.CombinePaths(outputDirectory, filename);
            var isWritten = await TryWriteReportAsync(report, filePath);

            return isWritten ? filePath : null;
        }

        private async Task<bool> TryWriteReportAsync(byte[] report, string filePath)
        {
            if (filePath == null)
            {
                return false;
            }

            try
            {
                using (var destinationStream = _fileSystem.CreateFile(filePath))
                using (var memoryStream = new MemoryStream(report))
                {
                    await memoryStream.CopyToAsync(destinationStream);
                }
            }
            catch (IOException ex)
            {
                _progressReporter.ReportIssue(string.Format(CultureInfo.InvariantCulture, ex.Message));
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                _progressReporter.ReportIssue(string.Format(CultureInfo.InvariantCulture, ex.Message));
                return false;
            }

            return true;
        }

        private string GetFileName(string directory, string fileName, string inputExtension, bool isUnique)
        {
            // We want to change the extension of the filename given regardless
            // of whether the user gave an extension or not. However, if they give
            // us an extension and it doesn't match the expected one, we'll report
            // the problem to them.
            var originalExtension = Path.GetExtension(fileName);
            var extension = string.IsNullOrWhiteSpace(inputExtension) ? originalExtension : inputExtension;

            if (!string.IsNullOrEmpty(originalExtension) && !originalExtension.Equals(extension, StringComparison.OrdinalIgnoreCase))
            {
                _progressReporter.ReportIssue(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.ChangingFileExtension, fileName, originalExtension, extension));
            }

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            var fileNameFormatString = string.Concat(fileNameWithoutExtension, "{0}", extension);
            var uniqueName = string.Format(CultureInfo.InvariantCulture, fileNameFormatString, string.Empty);

            int i = 1;

            while (_fileSystem.FileExists(_fileSystem.CombinePaths(directory, uniqueName)))
            {
                // This file exists already but since we don't care about uniqueness, we'll overwrite it.
                if (!isUnique)
                {
                    _progressReporter.ReportIssue(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.OverwriteFile, uniqueName));
                    return uniqueName;
                }

                var suffix = string.Format(CultureInfo.InvariantCulture, "({0})", i++);
                uniqueName = string.Format(CultureInfo.InvariantCulture, fileNameFormatString, suffix);
            }

            return uniqueName;
        }
    }
}
