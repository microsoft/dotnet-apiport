using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Tests.ObjectModel
{
    [TestClass]
    public class TargetInformationTests
    {
        [TestMethod]
        public void NotNullExpandedTargets()
        {
            var info = new TargetInformation();

            Assert.IsNotNull(info.ExpandedTargets);
        }

        [TestMethod]
        public void ToStringNoExpandedTargets()
        {
            const string name = "name";
            var info = new TargetInformation { Name = name };

            Assert.AreEqual(name, info.ToString());
        }

        [TestMethod]
        public void ToStringWithExpandedTargets()
        {
            const string group = "name";
            const string expanded1 = "expanded1";
            const string expanded2 = "expanded2";

            var expandedTargets = new[] { expanded1, expanded2 };
            var info = new TargetInformation { Name = group, ExpandedTargets = expandedTargets };

            var groupedToString = String.Format(LocalizedStrings.TargetInformationGroups, group, String.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ", expandedTargets));
            Assert.AreEqual(groupedToString, info.ToString());
        }
    }
}
