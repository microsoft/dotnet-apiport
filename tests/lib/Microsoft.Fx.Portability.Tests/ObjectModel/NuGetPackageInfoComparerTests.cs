// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;
using System.Runtime.Versioning;
using Xunit;

using static Microsoft.Fx.Portability.Tests.TestData.TestFrameworks;

namespace Microsoft.Fx.Portability.Tests.ObjectModel
{
    public class NuGetPackageInfoComparerTests
    {
        [Fact]
        public static void SortingTest()
        {
            var target1 = NetStandard16;
            var target2 = Net40;

            var packageId1 = "Remotion.Linq";
            var packageId2 = "Antlr3.Runtime";
            var packageId3 = "NHibernate";
            var nuget1 = new NuGetPackageInfo(packageId1, new Dictionary<FrameworkName, string>(), string.Empty);
            var nuget2 = new NuGetPackageInfo(packageId2, new Dictionary<FrameworkName, string>(), string.Empty);
            var nuget3 = new NuGetPackageInfo(packageId3, new Dictionary<FrameworkName, string>(), string.Empty);

            var nugetList = new List<NuGetPackageInfo>() { nuget1, nuget2, nuget3 };
            nugetList.Sort(new NuGetPackageInfoComparer());

            var expectedOrderedlist = new List<NuGetPackageInfo>() { nuget2, nuget3, nuget1 };
            Assert.Equal(expectedOrderedlist, nugetList);
        }
    }
}
