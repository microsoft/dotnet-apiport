// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;
using Xunit;
using static Microsoft.Fx.Portability.Tests.TestData.TestFrameworks;

namespace Microsoft.Fx.Portability.Tests.ObjectModel
{
    public class NuGetPackageInfoComparerTests
    {
        [Fact]
        public void SortingTest()
        {
            var target1 = NetStandard16;
            var target2 = Net40;
            var assembly1 = "Remotion.Linq, Version=1.15.15.0, Culture=neutral, PublicKeyToken=fee00910d6e5f53b";
            var assembly2 = "Antlr3.Runtime, Version=3.5.0.2, Culture=neutral, PublicKeyToken=eb42632606e9261f";
            var assembly3 = "NHibernate, Version=5.0.0.1001, Culture=neutral, PublicKeyToken=aa95f207798dfdb4";
            var nuget1 = new NuGetPackageInfo(assembly1, target1, new List<NuGetPackageId>());
            var nuget2 = new NuGetPackageInfo(assembly1, target2, new List<NuGetPackageId>());
            var nuget3 = new NuGetPackageInfo(assembly2, target2, new List<NuGetPackageId>());
            var nuget4 = new NuGetPackageInfo(assembly2, target1, new List<NuGetPackageId>());
            var nuget5 = new NuGetPackageInfo(assembly3, target1, new List<NuGetPackageId>());
            var nuget6 = new NuGetPackageInfo(assembly3, target2, new List<NuGetPackageId>());

            var nugetList = new List<NuGetPackageInfo>() { nuget1, nuget2, nuget3, nuget4, nuget5, nuget6 };
            nugetList.Sort(new NuGetPackageInfoComparer());

            var expectedOrderedlist = new List<NuGetPackageInfo>() { nuget3, nuget4, nuget6, nuget5, nuget2, nuget1 };
            Assert.Equal(expectedOrderedlist, nugetList);
        }
    }
}
