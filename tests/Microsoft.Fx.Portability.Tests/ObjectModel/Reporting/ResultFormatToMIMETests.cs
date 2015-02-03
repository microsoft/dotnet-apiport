using Microsoft.Fx.Portability.Reporting.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Fx.Portability.Tests.ObjectModel.Reporting
{
    [TestClass]
    public class ResultFormatToMIMETests
    {
        [TestMethod]
        public void ExcelMime()
        {
            Assert.AreEqual("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ResultFormat.Excel.GetMIMEType());
        }

        [TestMethod]
        public void JsonMime()
        {
            Assert.AreEqual("application/json", ResultFormat.Json.GetMIMEType());
        }

        [TestMethod]
        public void HtmlMime()
        {
            Assert.AreEqual("text/html", ResultFormat.HTML.GetMIMEType());
        }
    }
}
