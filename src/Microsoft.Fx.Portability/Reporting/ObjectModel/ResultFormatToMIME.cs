// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Fx.Portability.Reporting.ObjectModel
{
    public static class ResultFormatToMIME
    {
        public static ResultFormatInformation ToResultFormatInformation(this ResultFormat format)
        {
            return new ResultFormatInformation
            {
                DisplayName = format.ToString(),
                MimeType = format.GetMIMEType()
            };
        }

        public static string GetMIMEType(this ResultFormat format)
        {
            switch (format)
            {
                case ResultFormat.Excel:
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case ResultFormat.Json:
                    return "application/json";
                case ResultFormat.HTML:
                    return "text/html";
                default:
                    throw new ArgumentOutOfRangeException("format", format.ToString());
            }
        }

        public static string GetFileExtension(this ResultFormat format)
        {
            switch (format)
            {
                case ResultFormat.Excel:
                    return ".xlsx";
                case ResultFormat.Json:
                    return ".json";
                case ResultFormat.HTML:
                    return ".htm";
                default:
                    throw new ArgumentOutOfRangeException("format", format.ToString());
            }
        }
    }
}
