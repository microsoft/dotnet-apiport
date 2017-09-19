// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Resources;
using Xunit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Tests.ObjectModel
{
    public class TargetInformationTests
    {
        [Fact]
        public void NotNullExpandedTargets()
        {
            var info = new TargetInformation();

            Assert.NotNull(info.ExpandedTargets);
        }

        [Fact]
        public void ToStringNoExpandedTargets()
        {
            const string name = "name";
            var info = new TargetInformation { Name = name };

            Assert.Equal(name, info.ToString());
        }

        [Fact]
        public void ToStringWithExpandedTargets()
        {
            const string group = "name";
            const string expanded1 = "expanded1";
            const string expanded2 = "expanded2";

            var expandedTargets = new[] { expanded1, expanded2 };
            var info = new TargetInformation { Name = group, ExpandedTargets = expandedTargets };

            var groupedToString = String.Format(CultureInfo.CurrentCulture, LocalizedStrings.TargetInformationGroups, group, String.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ", expandedTargets));
            Assert.Equal(groupedToString, info.ToString());
        }
    }
}
