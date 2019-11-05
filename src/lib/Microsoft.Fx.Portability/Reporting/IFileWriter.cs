// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Reporting
{
    public interface IFileWriter
    {
        /// <summary>
        /// Writes a report.
        /// </summary>
        /// <param name="report">Contents of the report.</param>
        /// <param name="extension">File extension of the report.</param>
        /// <param name="outputDirectory">Directory for the output.</param>
        /// <param name="filename">File name for report.</param>
        /// <param name="overwrite">true to overwrite file; otherwise will find a file name that is not conflicting by adding a numerical suffix to the filename.</param>
        /// <returns>The fully qualified name to the report or null if unable to write report.</returns>
        Task<string> WriteReportAsync(byte[] report, string extension, string outputDirectory, string filename, bool overwrite);
    }
}
