// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.IO;

namespace Microsoft.Fx.Portability.Reporting
{
    public interface IReportWriter
    {
        ResultFormat Format { get; }
        void WriteStream(Stream stream, AnalyzeResponse response);
    }
}
