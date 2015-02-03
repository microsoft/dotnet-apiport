using Microsoft.Fx.Portability.Reporting.ObjectModel;
using Xunit;

namespace Microsoft.Fx.Portability.Tests.ObjectModel.Reporting
{
    public class ResultFormatToMIMETests
    {
        [Fact]
        public void ExcelMime()
        {
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ResultFormat.Excel.GetMIMEType());
        }

        [Fact]
        public void JsonMime()
        {
            Assert.Equal("application/json", ResultFormat.Json.GetMIMEType());
        }

        [Fact]
        public void HtmlMime()
        {
            Assert.Equal("text/html", ResultFormat.HTML.GetMIMEType());
        }
    }
}
