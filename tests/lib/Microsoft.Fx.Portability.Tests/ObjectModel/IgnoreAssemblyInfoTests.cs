// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Xunit;

namespace Microsoft.Fx.Portability.Tests.ObjectModel
{
    public class IgnoreAssemblyInfoTests
    {
        private readonly IgnoreAssemblyInfo[] _set1 = new[]
        {
            new IgnoreAssemblyInfo() { AssemblyIdentity = "Foo", TargetsIgnored = new[] { "v1", "v2" } },
            new IgnoreAssemblyInfo() { AssemblyIdentity = "Bar", TargetsIgnored = new[] { "v2" } }
        };

        private readonly IgnoreAssemblyInfo[] _set2 = new[]
        {
            new IgnoreAssemblyInfo() { AssemblyIdentity = "Foo", TargetsIgnored = new[] { "v1", "v3" } },
            new IgnoreAssemblyInfo() { AssemblyIdentity = "Bar", TargetsIgnored = new[] { "v1" } },
            new IgnoreAssemblyInfo() { AssemblyIdentity = "Baz", TargetsIgnored = new string[0] }
        };

        private readonly IgnoreAssemblyInfo[] _set3 = new[]
        {
            new IgnoreAssemblyInfo() { AssemblyIdentity = "Foo", TargetsIgnored = new[] { "V1" } },
            new IgnoreAssemblyInfo() { AssemblyIdentity = "Bar" },
            new IgnoreAssemblyInfo() { AssemblyIdentity = "Baz", TargetsIgnored = new[] { "v1" } }
        };

        private readonly IgnoreAssemblyInfo[] _combined = new[]
        {
            new IgnoreAssemblyInfo() { AssemblyIdentity = "Foo", TargetsIgnored = new[] { "v1", "v2", "v3" } },
            new IgnoreAssemblyInfo() { AssemblyIdentity = "Bar" },
            new IgnoreAssemblyInfo() { AssemblyIdentity = "Baz" }
        };

        [Fact]
        public void MergeWithOverlap()
        {
            IgnoreAssemblyInfoList list1 = new IgnoreAssemblyInfoList();
            list1.Load(_set1);
            list1.Load(_set2);
            list1.Load(_set3);

            Assert.Equal(list1, _combined, new IgnoreAssemblyInfoComparer());
        }
    }
}
