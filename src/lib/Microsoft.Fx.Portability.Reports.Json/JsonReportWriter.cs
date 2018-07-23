// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Newtonsoft.Json;
using System.IO;

namespace Microsoft.Fx.Portability.Reports
{
    public class JsonReportWriter : IReportWriter
    {
        private readonly ResultFormatInformation _formatInformation;

        public JsonReportWriter()
        {
            _formatInformation = new ResultFormatInformation
            {
                DisplayName = "Json",
                MimeType = "application/json",
                FileExtension = ".json"
            };
        }

        public ResultFormatInformation Format
        {
            get { return _formatInformation; }
        }

        public void WriteStream(Stream stream, AnalyzeResponse response, AnalyzeRequest request)
        {
            using (var streamWriter = new StreamWriter(stream))
            using (var writer = new JsonTextWriter(streamWriter))
            {
                DataExtensions.Serializer.Serialize(writer, response);
            }
        }
    }
}
