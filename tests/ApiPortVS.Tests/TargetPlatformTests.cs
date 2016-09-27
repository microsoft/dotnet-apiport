// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ApiPortVS.Tests
{
    [TestClass]
    public class TargetPlatformTests
    {
        [TestMethod]
        public void TestAreEqual()
        {
            const string frameworkName = ".NETFramework";

            var platform = new TargetPlatform
            {
                Name = frameworkName,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = frameworkName },
                    new TargetPlatformVersion { Version = new Version("5.8.3"), PlatformName = frameworkName },
                }
            };

            var compared = new TargetPlatform
            {
                Name = frameworkName,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = frameworkName },
                    new TargetPlatformVersion { Version = new Version("5.8.3"), PlatformName = frameworkName },
                }
            };

            Assert.AreEqual(platform, compared);
        }

        [TestMethod]
        public void AreNotEqual_DifferentName()
        {
            var name1 = ".NETFramework";
            var platform = new TargetPlatform
            {
                Name = name1,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = name1 },
                    new TargetPlatformVersion { Version = new Version("5.8.3"), PlatformName = name1 },
                }
            };

            var name2 = ".NETFramework_Not";
            var compared = new TargetPlatform
            {
                Name = name2,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = name2 },
                    new TargetPlatformVersion { Version = new Version("5.8.3"), PlatformName = name2 },
                }
            };

            Assert.AreNotEqual(platform, compared);
        }

        [TestMethod]
        public void AreNotEqual_DifferentVersionCount()
        {
            var name1 = ".NETFramework";
            var platform = new TargetPlatform
            {
                Name = name1,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = name1 },
                    new TargetPlatformVersion { Version = new Version("5.8.3"), PlatformName = name1 },
                }
            };

            var compared = new TargetPlatform
            {
                Name = name1,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("2.8"), PlatformName = name1 },
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = name1 },
                    new TargetPlatformVersion { Version = new Version("5.8.3"), PlatformName = name1 },
                }
            };

            Assert.AreNotEqual(platform, compared);
        }

        [TestMethod]
        public void AreNotEqual_DifferentVersionNumbers()
        {
            var name1 = ".NETFramework";
            var platform = new TargetPlatform
            {
                Name = name1,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = name1 },
                    new TargetPlatformVersion { Version = new Version("5.8.3"), PlatformName = name1 },
                }
            };

            var compared = new TargetPlatform
            {
                Name = name1,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = name1 },
                    new TargetPlatformVersion { Version = new Version("5.7.3"), PlatformName = name1 },
                }
            };

            Assert.AreNotEqual(platform, compared);
        }


        [TestMethod]
        public void AreNotEqual_DifferentVersionOrder()
        {
            var name1 = ".NETFramework";
            var platform = new TargetPlatform
            {
                Name = name1,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = name1 },
                    new TargetPlatformVersion { Version = new Version("5.8.3"), PlatformName = name1 },
                }
            };

            var compared = new TargetPlatform
            {
                Name = name1,
                Versions = new[] {
                    new TargetPlatformVersion { Version = new Version("5.8.3"), PlatformName = name1 },
                    new TargetPlatformVersion { Version = new Version("1.0"), PlatformName = name1 },
                }
            };

            Assert.AreNotEqual(platform, compared);
        }
    }
}
