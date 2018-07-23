// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using System.IO;

namespace Microsoft.Fx.Portability.Reports
{
    public class ExcelReportWriter : IReportWriter
    {
        private readonly ResultFormatInformation _formatInformation;
        private readonly ITargetMapper _targetMapper;

        public ExcelReportWriter(ITargetMapper targetMapper)
        {
            _targetMapper = targetMapper;

            _formatInformation = new ResultFormatInformation
            {
                DisplayName = "Excel",
                MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileExtension = ".xlsx"
            };
        }

        public ResultFormatInformation Format { get { return _formatInformation; } }

        public void WriteStream(Stream stream, AnalyzeResponse response, AnalyzeRequest request)
        {
            var excelWriter = new ExcelOpenXmlOutputWriter(_targetMapper, response.ReportingResult, response.BreakingChanges, response.CatalogLastUpdated, description: null);

            excelWriter.WriteTo(stream);
        }
    }
}
