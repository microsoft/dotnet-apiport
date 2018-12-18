// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Reports
{
    public class JsonReportWriter : IReportWriter
    {
        public JsonReportWriter()
        {
            Format = new ResultFormatInformation
            {
                DisplayName = "Json",
                MimeType = "application/json",
                FileExtension = ".json"
            };
        }

        public ResultFormatInformation Format { get; }

        public Task WriteStreamAsync(Stream stream, AnalyzeResponse response)
        {
            using (var streamWriter = new StreamWriter(stream))
            {
                using (var writer = new JsonTextWriter(streamWriter))
                {
                    DataExtensions.Serializer.Serialize(writer, response);
                }
            }

            return Task.CompletedTask;
        }
    }
}
