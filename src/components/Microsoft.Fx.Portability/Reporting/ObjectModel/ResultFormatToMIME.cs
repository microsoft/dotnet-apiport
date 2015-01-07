using System;

namespace Microsoft.Fx.Portability.Reporting.ObjectModel
{
    public static class ResultFormatToMIME
    {
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
