// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Xunit;

namespace Microsoft.Fx.Portability.Tests
{
    public class TargetMapTests
    {
        [Fact]
        public static void UnknownTarget()
        {
            var map = new TargetMapper();

            Assert.Equal("Target", map.GetNames("Target").Single());
        }

        [Fact]
        public static void TwoItems()
        {
            var map = new TargetMapper();

            map.AddAlias("alias1", "target1");
            map.AddAlias("alias1", "target2");

            AreCollectionsEqual(new[] { "target1", "target2" }, map.GetNames("alias1"));
        }

        [Fact]
        public static void AliasList()
        {
            var map = new TargetMapper();

            map.AddAlias("alias1", "Target1");
            map.AddAlias("alias2", "target1");
            map.AddAlias("Alias1", "target2");

            AreCollectionsEqual(new[] { "alias1", "alias2" }, map.Aliases);
        }

        [Fact]
        public static void AliasEqualsTarget()
        {
            var map = new TargetMapper();

            map.AddAlias("TestTarget2", "TestTarget1");

            try
            {
                map.AddAlias("ProjectAlias", "TestTarget2");
            }
            catch (TargetMapperException e)
            {
                Assert.Equal(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.AliasCannotBeEqualToTargetNameError, "TestTarget2"), e.Message);
                return;
            }

            Assert.True(false, "Expected exception was not thrown");
        }

        [Fact]
        public static void CaseInsensitiveAlias()
        {
            var map = new TargetMapper();

            map.AddAlias("alias1", "target");
            map.AddAlias("Alias1", "target");

            Assert.Equal("target", map.GetNames("Alias1").Single());
            Assert.Equal("target", map.GetNames("alias1").Single());
        }

        [Fact]
        public static void UndefinedAlias()
        {
            var map = new TargetMapper();
            var alias = "alias";

            Assert.Equal(alias, map.GetAlias(alias));
        }

        [Fact]
        public static void CaseInsensitiveTarget()
        {
            var map = new TargetMapper();

            map.AddAlias("alias1", "target1");
            map.AddAlias("alias1", "Target1");

            Assert.Equal("target1", map.GetNames("alias1").Single());
        }

        [Fact]
        public static void VerifySingleAliasMapping()
        {
            Assert.Throws<AliasMappedToMultipleNamesException>(() =>
            {
                var map = new TargetMapper();

                map.AddAlias("alias", "name1");
                map.AddAlias("alias", "name2");

                map.VerifySingleAlias();
            });
        }

        [Fact]
        public static void VerifySingleAliasMappingValid()
        {
            var map = new TargetMapper();

            map.AddAlias("alias1", "name1");
            map.AddAlias("alias2", "name2");

            map.VerifySingleAlias();
        }

        [Fact]
        public static void ParseGroupings1Group()
        {
            var map = new TargetMapper();
            var groupings = "group1: target1,target2";

            map.ParseAliasString(groupings);

            AreCollectionsEqual(new[] { "target1", "target2" }, map.GetNames("group1"));
        }

        [Fact]
        public static void ParseGroupings2Groups()
        {
            var map = new TargetMapper();
            var groupings = "group1: target1,target2; group2: target1, target3";

            map.ParseAliasString(groupings);

            AreCollectionsEqual(new[] { "target1", "target2" }, map.GetNames("group1"));
            AreCollectionsEqual(new[] { "target1", "target3" }, map.GetNames("group2"));
        }

        [Fact]
        public static void ParseGroupingsNull()
        {
            var map = new TargetMapper();

            map.ParseAliasString(null);
        }

        [Fact]
        public static void ParseInvalidGroups()
        {
            var map = new TargetMapper();
            var groupings = "group1 target1,target2; group2: target1, target3";

            map.ParseAliasString(groupings);

            Assert.Equal("group1", map.GetNames("group1").Single());
            AreCollectionsEqual(new[] { "target1", "target3" }, map.GetNames("group2"));
        }

        [Fact]
        public static void ParseInvalidGroupsValidateAtomicOperation()
        {
            var map = new TargetMapper();
            var groupings = "group1: target1,target2; group2 target1, target3";

            Assert.Throws<ArgumentOutOfRangeException>(() => map.ParseAliasString(groupings, true));

            Assert.Equal("group1", map.GetNames("group1").Single());
            Assert.Equal("group2", map.GetNames("group2").Single());
        }

        [Fact]
        public static void ParseInvalidGroupsValidate()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var map = new TargetMapper();
                var groupings = "group1 target1,target2; group2: target1, target3";

                map.ParseAliasString(groupings, true);
            });
        }

        [Fact]
        public static void LoadXmlFromFile()
        {
            var map = new TargetMapper();

            Assert.False(map.LoadFromConfig("doesnotexist.xml"));
        }

        [Fact]
        public static void LoadXmlFromDefault()
        {
            var xml = @"<ApiTool>
        <Targets>
            <Target Name='target1' Alias='alias1' />
       </Targets>
     </ApiTool> ";
            var file = new FileInfo("TargetMap.xml");
            try
            {
                using (var fs = file.OpenWrite())
                using (var writer = new StreamWriter(fs))
                {
                    writer.Write(xml);
                }

                var map = new TargetMapper();
                Assert.True(map.LoadFromConfig(file.FullName));
                Assert.Equal("target1", map.GetNames("alias1").Single());
            }
            finally
            {
                file.Delete();
            }
        }

        [Fact]
        public static void XmlNoTargets()
        {
            var xml = @"<ApiTool>
        <Targets>
       </Targets>
     </ApiTool> ";

            var map = LoadXml(xml);

            Assert.Equal("TestTarget", map.GetNames("TestTarget").Single());
        }

        [Fact]
        public static void XmlWithAlias()
        {
            var xml = @"<ApiTool>
        <Targets>
            <Target Name='TestTarget' Alias='InternalProject' />
       </Targets>
     </ApiTool> ";

            var map = LoadXml(xml);

            Assert.Equal("TestTarget", map.GetNames("InternalProject").Single());
        }

        [Fact]
        public static void XmlAliasSameAsName()
        {
            var xml = @"<ApiTool>
        <Targets>
            <Target Name='TestTarget1' Alias='TestTarget2' />
            <Target Name='TestTarget2' Alias='ProjectAlias' />
       </Targets>
     </ApiTool> ";

            try
            {
                var map = LoadXml(xml);
            }
            catch (TargetMapperException e)
            {
                Assert.Equal(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.AliasCannotBeEqualToTargetNameError, "TestTarget2"), e.Message);
                return;
            }

            Assert.True(false, "Expected exception was not thrown");
        }

        [Fact]
        public static void XmlDuplicateAlias()
        {
            var xml = @"<ApiTool>
        <Targets>
            <Target Name='TestTarget1' Alias='ProjectAlias' />
            <Target Name='TestTarget2' Alias='ProjectAlias' />
       </Targets>
     </ApiTool> ";

            var map = LoadXml(xml);

            AreCollectionsEqual(new[] { "TestTarget1", "TestTarget2" }, map.GetNames("ProjectAlias"));
        }

        [Fact]
        public static void XmlMalformedXml()
        {
            var xml = @"<ApiTool>
        <Targets>
            <Target Name='TestTarget1' Alias='TestTarget2' />
            <Target Name='TestTarget2' Alias='ProjectAlias />
       </Targets>
     </ApiTool> ";

            try
            {
                var map = LoadXml(xml);
            }
            catch (TargetMapperException e)
            {
                Assert.NotNull(e.InnerException);
                Assert.Equal(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.MalformedMap, e.InnerException.Message), e.Message);
                return;
            }

            Assert.True(false, "Expected exception was not thrown");
        }

        [Fact]
        public static void XmlNotInSchema()
        {
            var xml = @"<ApiTool>
        <Targets>
            <Target Name='TestTarget1' Alias2='TestTarget2' />
       </Targets>
     </ApiTool> ";

            try
            {
                var map = LoadXml(xml);
            }
            catch (TargetMapperException e)
            {
#if FEATURE_XML_SCHEMA
                Assert.NotNull(e.InnerException);
                Assert.Equal(string.Format(CultureInfo.CurrentCulture, e.InnerException.Message), e.Message);
#else
                Assert.Equal(String.Format(CultureInfo.CurrentCulture, LocalizedStrings.MalformedMap, string.Empty), e.Message);
#endif
                return;
            }

            Assert.True(false, "Expected exception was not thrown");
        }

        [Fact]
        public static void XmlGetAlias()
        {
            var xml = @"<ApiTool>
        <Targets>
            <Target Name='TestTarget1' Alias='Alias1' />
            <Target Name='TestTarget2' Alias='Alias2' />
       </Targets>
     </ApiTool> ";

            var map = LoadXml(xml);

            Assert.Equal("Alias1", map.GetAlias("TestTarget1"));
        }

        [Fact]
        public static void XmlGetAliasMultipleAliases()
        {
            var xml = @"<ApiTool>
        <Targets>
            <Target Name='TestTarget1' Alias='Alias1' />
            <Target Name='TestTarget1' Alias='Alias2' />
       </Targets>
     </ApiTool> ";

            var map = LoadXml(xml);

            Assert.Equal("Alias1", map.GetAlias("TestTarget1"));
        }

        [Fact]
        public static void XmlGetNameMultipleAliases()
        {
            var xml = @"<ApiTool>
        <Targets>
            <Target Name='TestTarget1' Alias='Alias1' />
            <Target Name='TestTarget1' Alias='Alias2' />
       </Targets>
     </ApiTool> ";

            var map = LoadXml(xml);

            Assert.Equal("TestTarget1", map.GetNames("Alias1").Single());
            Assert.Equal("TestTarget1", map.GetNames("Alias2").Single());
        }

        [Fact]
        public static void GetTargetNamesForDistinctTargets()
        {
            var mapper = new TargetMapper();
            mapper.AddAlias("Mobile", "Windows Phone");
            mapper.AddAlias("Mobile", "Windows");
            mapper.AddAlias("Mobile", "Mono");
            mapper.AddAlias("Desktop", ".NET Framework Test");

            var netFramework4 = new FrameworkName(".NET Framework,Version=4.0");
            var windowsPhone = new FrameworkName("Windows Phone,Version=8.1");
            var windows8 = new FrameworkName("Windows,Version=8.0");

            var targets = new List<FrameworkName> { netFramework4, windowsPhone, windows8 };
            var targetNames = mapper.GetTargetNames(targets, includeVersion: false).ToArray();
            var targetNamesWithVersions = mapper.GetTargetNames(targets, includeVersion: true).ToArray();

            AreCollectionsEqual(new string[] { ".NET Framework", "Windows Phone", "Windows" }, targetNames);
            AreCollectionsEqual(new string[] { netFramework4.FullName, windowsPhone.FullName, windows8.FullName }, targetNamesWithVersions);
        }

        [Fact]
        public static void GetTargetNamesForMultipleTargetVersions()
        {
            var mapper = new TargetMapper();
            mapper.AddAlias("Mobile", "Windows Phone");
            mapper.AddAlias("Mobile", "Windows");
            mapper.AddAlias("Mobile", "Mono");
            mapper.AddAlias("Desktop", ".NET Framework Test");

            var netFramework451 = new FrameworkName(".NET Framework,Version=4.5.1");
            var netFramework4 = new FrameworkName(".NET Framework,Version=4.0");
            var windowsPhone = new FrameworkName("Windows Phone,Version=8.1");
            var windows81 = new FrameworkName("Windows,Version=8.1");
            var windows8 = new FrameworkName("Windows,Version=8.0");

            var targets = new List<FrameworkName> { netFramework4, windows81, netFramework451, windowsPhone, windows8 };
            var targetNames = mapper.GetTargetNames(targets, includeVersion: false).ToArray();
            var targetNamesWithVersions = mapper.GetTargetNames(targets, includeVersion: true).ToArray();

            AreCollectionsEqual(
                new string[] {
                    netFramework4.FullName, windows81.FullName,
                    netFramework451.FullName, "Windows Phone",
                    windows8.FullName },
                targetNames);

            AreCollectionsEqual(
                new string[] {
                    netFramework4.FullName, windows81.FullName,
                    netFramework451.FullName, windowsPhone.FullName,
                    windows8.FullName },
                targetNamesWithVersions);
        }

        private static void AreCollectionsEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            var expectedList = expected.OrderBy(k => k).ToList();
            var actualList = actual.OrderBy(k => k).ToList();

            Assert.Equal<List<T>>(expectedList, actualList);
        }

        private static TargetMapper LoadXml(string config)
        {
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms) { AutoFlush = true })
            {
                writer.Write(config);
                ms.Seek(0, SeekOrigin.Begin);

                var targetMapper = new TargetMapper();
                targetMapper.Load(ms);
                return targetMapper;
            }
        }
    }
}
