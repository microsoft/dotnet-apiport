// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Reporting
{
    public interface IFileWriter
    {
        Task<string> WriteReportAsync(byte[] report, string extension, string outputDirectory, string filename, bool overwrite);
    }
}
