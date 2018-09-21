// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Xunit;

namespace ApiPortVS.Tests
{
    public class TargetPlatformTests
    {
        [Fact]
        public void TestAreEqual()
        {
            const string frameworkName = ".NETFramework";

            var platform = new TargetPlatform
            {
                Name = frameworkName,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = frameworkName },
                    new TargetPlatformVersion { Version = new Version("5.8.3"), PlatformName = frameworkName },
                }.OrderBy(x => x.Version).ToList()
            };

            var compared = new TargetPlatform
            {
                Name = frameworkName,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = frameworkName },
                    new TargetPlatformVersion { Version = new Version("5.8.3"), PlatformName = frameworkName },
                }.OrderBy(x => x.Version).ToList()
            };

            Assert.Equal(platform, compared);

            Assert.True(platform.CompareTo(compared) == 0);
            Assert.True(compared.CompareTo(platform) == 0);
        }

        [Fact]
        public void AreNotEqual_DifferentName()
        {
            var name1 = ".NETFramework";
            var platform = new TargetPlatform
            {
                Name = name1,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = name1 },
                    new TargetPlatformVersion { Version = new Version("5.8.3"), PlatformName = name1 },
                }.OrderBy(x => x.Version).ToList()
            };

            var name2 = ".NETFramework_Not";
            var compared = new TargetPlatform
            {
                Name = name2,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = name2 },
                    new TargetPlatformVersion { Version = new Version("5.8.3"), PlatformName = name2 },
                }.OrderBy(x => x.Version).ToList()
            };

            Assert.NotEqual(platform, compared);

            Assert.Equal(string.CompareOrdinal(name1, name2), platform.CompareTo(compared));
            Assert.Equal(string.CompareOrdinal(name2, name1), compared.CompareTo(platform));
        }

        [Fact]
        public void AreNotEqual_DifferentVersionCount()
        {
            var name1 = ".NETFramework";
            var platform = new TargetPlatform
            {
                Name = name1,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = name1 },
                    new TargetPlatformVersion { Version = new Version("5.8.3"), PlatformName = name1 },
                }.OrderBy(x => x.Version).ToList()
            };

            var compared = new TargetPlatform
            {
                Name = name1,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("2.8"), PlatformName = name1 },
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = name1 },
                    new TargetPlatformVersion { Version = new Version("5.8.3"), PlatformName = name1 },
                }.OrderBy(x => x.Version).ToList()
            };

            Assert.NotEqual(platform, compared);

            // We are expecting that `platform` should come after `compared`
            // because the second version number is greater in `platform`
            Assert.True(platform.CompareTo(compared) == 1);
            Assert.True(compared.CompareTo(platform) == -1);
        }

        [Fact]
        public void AreNotEqual_DifferentVersionNumbers()
        {
            var name1 = ".NETFramework";
            var platform = new TargetPlatform
            {
                Name = name1,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = name1 },
                    new TargetPlatformVersion { Version = new Version("5.8.3"), PlatformName = name1 },
                }.OrderBy(x => x.Version).ToList()
            };

            var compared = new TargetPlatform
            {
                Name = name1,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = name1 },
                    new TargetPlatformVersion { Version = new Version("5.7.3"), PlatformName = name1 },
                }.OrderBy(x => x.Version).ToList()
            };

            Assert.NotEqual(platform, compared);

            // We are expecting that `platform` should come after `compared` in
            // the list because of the version number
            Assert.True(platform.CompareTo(compared) == 1);
            Assert.True(compared.CompareTo(platform) == -1);
        }
    }
}
