// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using System;
using System.IO;

namespace Microsoft.Fx.Portability.Reports.DGML
{
    public class DGMLOutputWriter : IReportWriter
    {
        public ResultFormatInformation Format => new ResultFormatInformation()
        {
            DisplayName="DGML",
            MimeType="application/xml",
            FileExtension=".dgml"
        };

        public void WriteStream(Stream stream, AnalyzeResponse response, AnalyzeRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
