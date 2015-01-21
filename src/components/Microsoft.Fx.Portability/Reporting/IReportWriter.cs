// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Reporting
{
    public interface IReportWriter
    {
        Task<string> WriteReportAsync(byte[] report, ResultFormat format, string outputDirectory, string filename, bool overwrite);
    }
}
