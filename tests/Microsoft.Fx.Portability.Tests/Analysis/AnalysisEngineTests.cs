// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analysis;
using Microsoft.Fx.Portability.ObjectModel;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Xunit;

namespace Microsoft.Fx.Portability.Web.Analyze.Tests
{
    public class AnalysisEngineTests
    {
        #region FindUnreferencedAssemblies

        private static List<string> s_unreferencedAssemblies = new List<string>()
            {
                "Microsoft.CSharp, Version=4.0.0.0, PublicKeyToken=b03f5f7f11d50a3a",
                "MyAssembly"
            };

        [Fact]
        public void FindUnreferencedAssemblies_AllNulls()
        {
            var engine = new AnalysisEngine(null, null);

            engine.FindUnreferencedAssemblies(null, null).ToList();
        }

        [Fact]
        public void FindUnreferencedAssemblies_SpecifiedAssembliesNull()
        {
            var catalog = Substitute.For<IApiCatalogLookup>();
            var recommendations = Substitute.For<IApiRecommendations>();
            var engine = new AnalysisEngine(catalog, recommendations);

            var result = engine.FindUnreferencedAssemblies(s_unreferencedAssemblies, null).ToList();

            Assert.NotNull(result);
        }

        [Fact]
        public void FindUnreferencedAssemblies_NoUnreferencedAssemblies()
        {
            var catalog = Substitute.For<IApiCatalogLookup>();
            var recommendations = Substitute.For<IApiRecommendations>();
            var engine = new AnalysisEngine(catalog, recommendations);

            var specifiedUserAssemblies = s_unreferencedAssemblies.Select(ua => new AssemblyInfo() { AssemblyIdentity = ua, FileVersion = "0.0.0.0" }).ToList();
            var unreferencedAssms = engine.FindUnreferencedAssemblies(s_unreferencedAssemblies, specifiedUserAssemblies).ToList();

            // We don't expect to have any unreferenced assemblies.
            Assert.Empty(unreferencedAssms);
        }

        [Fact]
        public void FindUnreferencedAssemblies_UnreferencedAssemblies_1()
        {
            var catalog = Substitute.For<IApiCatalogLookup>();
            catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(s_unreferencedAssemblies[0])).Returns(true);

            var recommendations = Substitute.For<IApiRecommendations>();
            var engine = new AnalysisEngine(catalog, recommendations);

            var specifiedUserAssemblies = new[] { new AssemblyInfo { FileVersion = "", AssemblyIdentity = "MyAssembly" } };
            var unreferencedAssms = engine.FindUnreferencedAssemblies(s_unreferencedAssemblies, specifiedUserAssemblies).ToList();

            // 0 missing assembly since Microsoft.CSharp is a FX assembly and we specified MyAssembly
            Assert.Empty(unreferencedAssms);
        }

        [Fact]
        public void FindUnreferencedAssemblies_UnreferencedAssemblies_2()
        {
            var catalog = Substitute.For<IApiCatalogLookup>();
            catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(s_unreferencedAssemblies[0])).Returns(true);

            var recommendations = Substitute.For<IApiRecommendations>();
            var engine = new AnalysisEngine(catalog, recommendations);

            var unreferencedAssms = engine.FindUnreferencedAssemblies(s_unreferencedAssemblies, Enumerable.Empty<AssemblyInfo>()).ToList();

            // 1 missing assembly since Microsoft.CSharp is a FX assembly 
            Assert.Equal(1, unreferencedAssms.Count);
        }

        [Fact]
        public void FindUnreferencedAssemblies_UnreferencedAssemblies_WithNullInSpecifiedList()
        {
            var catalog = Substitute.For<IApiCatalogLookup>();
            catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(s_unreferencedAssemblies[0])).Returns(true);

            var recommendations = Substitute.For<IApiRecommendations>();
            var engine = new AnalysisEngine(catalog, recommendations);

            var specifiedUserAssemblies = new List<AssemblyInfo>() { new AssemblyInfo() { FileVersion = "", AssemblyIdentity = "MyAssembly" }, null };
            var unreferencedAssms = engine.FindUnreferencedAssemblies(s_unreferencedAssemblies, specifiedUserAssemblies).ToList();

            // 0 missing assembly since Microsoft.CSharp is a fx assembly and we specified MyAssembly
            Assert.Empty(unreferencedAssms);
        }

        [Fact]
        public void FindUnreferencedAssemblies_UnreferencedAssemblies_WithNullInUnrefList()
        {
            var catalog = Substitute.For<IApiCatalogLookup>();
            catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(s_unreferencedAssemblies[0])).Returns(true);

            var recommendations = Substitute.For<IApiRecommendations>();
            var engine = new AnalysisEngine(catalog, recommendations);

            var specifiedUserAssemblies = new List<AssemblyInfo>() { new AssemblyInfo() { FileVersion = "", AssemblyIdentity = "MyAssembly" } };
            var listWithNulls = s_unreferencedAssemblies.Concat(new List<string>() { null }).ToList();

            var unreferencedAssms = engine.FindUnreferencedAssemblies(listWithNulls, specifiedUserAssemblies).ToList();

            // 0 missing assembly since Microsoft.CSharp is a fx assembly and we specified MyAssembly
            Assert.Empty(unreferencedAssms);
        }
        #endregion

        [Fact]
        public void FindMembersNotInTargets_AllNull()
        {
            var engine = new AnalysisEngine(null, null);

            engine.FindMembersNotInTargets(null, null, null);
        }

        [Fact]
        public void FindMembersNotInTargets_1()
        {
            var testData = new Dictionary<MemberInfo, ICollection<AssemblyInfo>>();

            var userAsm1 = new AssemblyInfo() { AssemblyIdentity = "userAsm1, Version=1.0.0.0", FileVersion = "1.0.0.0" };
            var userAsm2 = new AssemblyInfo() { AssemblyIdentity = "userAsm2, Version=2.0.0.0", FileVersion = "2.0.0.0" };
            var userAsm3 = new AssemblyInfo() { AssemblyIdentity = "userAsm3, Version=3.0.0.0", FileVersion = "3.0.0.0" };
            var mi1 = new MemberInfo() { DefinedInAssemblyIdentity = "System.Drawing, Version=1.0.136.0, PublicKeyToken=b03f5f7f11d50a3a", MemberDocId = "T:System.Drawing.Color" };
            var mi2 = new MemberInfo() { DefinedInAssemblyIdentity = "System.Data, Version=1.0.136.0, PublicKeyToken=b77a5c561934e089", MemberDocId = "T:System.Data.SqlTypes.SqlBoolean" };
            var mi3 = new MemberInfo() { DefinedInAssemblyIdentity = "userAsm1, Version=1.0.0.0", MemberDocId = "T:MyType" };

            var usedIn1 = new HashSet<AssemblyInfo>() { userAsm1, userAsm2 };
            testData.Add(mi1, usedIn1);

            var usedIn2 = new HashSet<AssemblyInfo>() { userAsm2, userAsm3 };
            testData.Add(mi2, usedIn2);
            testData.Add(mi3, usedIn2);

            var targets = new List<FrameworkName>() { new FrameworkName("Windows Phone, version=8.1") };

            var catalog = Substitute.For<IApiCatalogLookup>();
            catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(mi1.DefinedInAssemblyIdentity)).Returns(true);
            catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(mi2.DefinedInAssemblyIdentity)).Returns(true);
            catalog.IsFrameworkMember(mi1.MemberDocId).Returns(true);
            catalog.IsFrameworkMember(mi2.MemberDocId).Returns(true);

            var recommendations = Substitute.For<IApiRecommendations>();
            var engine = new AnalysisEngine(catalog, recommendations);
            var notInTarget = engine.FindMembersNotInTargets(targets, Array.Empty<string>(), testData);

            Assert.Equal(2, notInTarget.Count);
        }


        [Fact]
        public void FindMembersNotInTargetsWithSuppliedAssembly()
        {
            var testData = new Dictionary<MemberInfo, ICollection<AssemblyInfo>>();

            var userAsm1 = new AssemblyInfo() { AssemblyIdentity = "userAsm1, Version=1.0.0.0", FileVersion = "1.0.0.0" };
            var userAsm2 = new AssemblyInfo() { AssemblyIdentity = "userAsm2, Version=2.0.0.0", FileVersion = "2.0.0.0" };
            var userAsm3 = new AssemblyInfo() { AssemblyIdentity = "userAsm3, Version=3.0.0.0", FileVersion = "3.0.0.0" };
            var mi1 = new MemberInfo() { DefinedInAssemblyIdentity = "System.Drawing, Version=1.0.136.0, PublicKeyToken=b03f5f7f11d50a3a", MemberDocId = "T:System.Drawing.Color" };
            var mi2 = new MemberInfo() { DefinedInAssemblyIdentity = "System.Data, Version=1.0.136.0, PublicKeyToken=b77a5c561934e089", MemberDocId = "T:System.Data.SqlTypes.SqlBoolean" };
            var mi3 = new MemberInfo() { DefinedInAssemblyIdentity = "userAsm1, Version=1.0.0.0", MemberDocId = "T:MyType" };

            var usedIn1 = new HashSet<AssemblyInfo>() { userAsm1, userAsm2 };
            testData.Add(mi1, usedIn1);

            var usedIn2 = new HashSet<AssemblyInfo>() { userAsm2, userAsm3 };
            testData.Add(mi2, usedIn2);
            testData.Add(mi3, usedIn2);

            var targets = new List<FrameworkName>() { new FrameworkName("Windows Phone, version=8.1") };

            var catalog = Substitute.For<IApiCatalogLookup>();
            catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(mi1.DefinedInAssemblyIdentity)).Returns(true);
            catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(mi2.DefinedInAssemblyIdentity)).Returns(true);
            catalog.IsFrameworkMember(mi1.MemberDocId).Returns(true);
            catalog.IsFrameworkMember(mi2.MemberDocId).Returns(true);

            var recommendations = Substitute.For<IApiRecommendations>();
            var engine = new AnalysisEngine(catalog, recommendations);
            var notInTarget = engine.FindMembersNotInTargets(targets, new[] { mi1.DefinedInAssemblyIdentity }, testData);

            Assert.Equal(1, notInTarget.Count);
        }

        [Fact]
        public void FindMembersNotInTargets_2()
        {
            // No member information passed through.
            var testData = new Dictionary<MemberInfo, ICollection<AssemblyInfo>>();
            var targets = new List<FrameworkName>() { new FrameworkName("Windows Phone, version=8.1") };
            var catalog = Substitute.For<IApiCatalogLookup>();

            GenerateTestData(catalog);

            var recommendations = Substitute.For<IApiRecommendations>();
            var engine = new AnalysisEngine(catalog, recommendations);
            var notInTarget = engine.FindMembersNotInTargets(targets, Array.Empty<string>(), testData);

            Assert.Equal(0, notInTarget.Count);
        }

        [Fact]
        public void BreakingChangesFullFrameworkAfterBreakBeforeFix()
        {
            TestBreakingChangeWithFixedEntry(Version.Parse("4.5.1"), false);
            TestBreakingChangeWithoutFixedEntry(Version.Parse("4.5.1"), false);
            TestBreakingChangeWithFixedInServicingEntry(Version.Parse("4.5.1"), true);
        }

        [Fact]
        public void BreakingChangesFullFrameworkOnBreakingVersion()
        {
            TestBreakingChangeWithFixedEntry(Version.Parse("4.5"), false);
            TestBreakingChangeWithoutFixedEntry(Version.Parse("4.5"), false);
            TestBreakingChangeWithFixedInServicingEntry(Version.Parse("4.5"), false);
        }

        [Fact]
        public void BreakingChangesFullFrameworkOnFix()
        {
            TestBreakingChangeWithFixedEntry(Version.Parse("4.5.2"), true);
            TestBreakingChangeWithoutFixedEntry(Version.Parse("4.5.2"), false);
            TestBreakingChangeWithFixedInServicingEntry(Version.Parse("4.5.2"), true);
        }

        [Fact]
        public void BreakingChangesFullFrameworkAfterFix()
        {
            TestBreakingChangeWithFixedEntry(Version.Parse("4.5.3"), true);
            TestBreakingChangeWithoutFixedEntry(Version.Parse("4.5.3"), false);
            TestBreakingChangeWithFixedInServicingEntry(Version.Parse("4.5.3"), true);
        }

        [Fact]
        public void BreakingChangesFullFrameworkBeforeBreak()
        {
            TestBreakingChangeWithFixedEntry(Version.Parse("4.0"), true);
            TestBreakingChangeWithoutFixedEntry(Version.Parse("4.0"), true);
            TestBreakingChangeWithFixedInServicingEntry(Version.Parse("4.0"), true);
        }

        [Fact]
        public void BreakingChangesNotFullFramework()
        {
            var catalog = Substitute.For<IApiCatalogLookup>();
            var recommendations = GenerateTestRecommendationsWithFixedEntry();
            var testData = GenerateTestData(catalog);
            var engine = new AnalysisEngine(catalog, recommendations);

            var framework = new FrameworkName(".NET Core Framework,Version=4.5.1");

            var breakingChanges = engine.FindBreakingChanges(new[] { framework }, testData, null, null, Array.Empty<string>()).ToList();

            Assert.Empty(breakingChanges);
        }

        [Fact]
        public void BreakingChangesWithAssemblyIgnores()
        {
            TestBreakingChangeWithIgnoreList(Version.Parse("4.5"), false, Enumerable.Empty<AssemblyInfo>());
            TestBreakingChangeWithIgnoreList(Version.Parse("4.5"), true, new AssemblyInfo[] { new AssemblyInfo() { AssemblyIdentity = "userAsm1, Version=1.0.0.0" } });
            TestBreakingChangeWithIgnoreList(Version.Parse("4.5"), true, new AssemblyInfo[] { new AssemblyInfo() { AssemblyIdentity = "userAsm1, Version=1.0.0.0" }, new AssemblyInfo() { AssemblyIdentity = "userAsm2, Version=2.0.0.0" } });
            TestBreakingChangeWithIgnoreList(Version.Parse("4.5"), false, new AssemblyInfo[] { new AssemblyInfo() { AssemblyIdentity = "userAsm2, Version=2.0.0.0" } });
        }

        [Fact]
        public void BreakingChangesWithSuppressions()
        {
            TestBreakingChangeWithSuppression(Version.Parse("4.5"), false, Enumerable.Empty<string>());
            TestBreakingChangeWithSuppression(Version.Parse("4.5"), false, new[] { "15" });
            TestBreakingChangeWithSuppression(Version.Parse("4.5"), true, new[] { "5" });
            TestBreakingChangeWithSuppression(Version.Parse("4.5"), true, new[] { "6", "Foo", "5" });
        }

        [Fact]
        public void BreakingChangeIgnoreSelection()
        {
            var catalog = Substitute.For<IApiCatalogLookup>();
            var recommendations = GenerateTestRecommendationsWithFixedEntry();
            var testData = GenerateTestData(catalog);
            var engine = new AnalysisEngine(catalog, recommendations);

            var framework = new FrameworkName(".NET Framework, Version = v4.5.1");
            var framework2 = new FrameworkName(".NET Framework, Version = v4.5.2");

            // Vanilla
            var result = engine.FindBreakingChangeSkippedAssemblies(new[] { framework }, testData.SelectMany(kvp => kvp.Value).Distinct(), GenerateIgnoreAssemblies(false, new[] { ".NET Framework,Version=v4.5.1" }));
            Assert.Equal(1, result.Count());
            Assert.Equal("userAsm1, Version=1.0.0.0", result.FirstOrDefault().AssemblyIdentity);

            // Empty ignore targets
            result = engine.FindBreakingChangeSkippedAssemblies(new[] { framework }, testData.SelectMany(kvp => kvp.Value).Distinct(), GenerateIgnoreAssemblies(false, new string[0]));
            Assert.Equal(1, result.Count());
            Assert.Equal("userAsm1, Version=1.0.0.0", result.FirstOrDefault().AssemblyIdentity);

            // Empty ignore targets with multiple targets
            result = engine.FindBreakingChangeSkippedAssemblies(new[] { framework, framework2 }, testData.SelectMany(kvp => kvp.Value).Distinct(), GenerateIgnoreAssemblies(false, new string[0]));
            Assert.Equal(1, result.Count());
            Assert.Equal("userAsm1, Version=1.0.0.0", result.FirstOrDefault().AssemblyIdentity);

            // Ignore a different target
            result = engine.FindBreakingChangeSkippedAssemblies(new[] { framework }, testData.SelectMany(kvp => kvp.Value).Distinct(), GenerateIgnoreAssemblies(false, new[] { ".NET Framework,Version=v4.5.2" }));
            Assert.Equal(0, result.Count());

            // Ignore some but not all targets
            result = engine.FindBreakingChangeSkippedAssemblies(new[] { framework, framework2 }, testData.SelectMany(kvp => kvp.Value).Distinct(), GenerateIgnoreAssemblies(false, new[] { ".NET Framework,Version=v4.5.1" }));
            Assert.Equal(0, result.Count());

            // Ignore all targets
            result = engine.FindBreakingChangeSkippedAssemblies(new[] { framework, framework2 }, testData.SelectMany(kvp => kvp.Value).Distinct(), GenerateIgnoreAssemblies(false, new[] { ".NET Framework,Version=v4.5.1", ".NET Framework,Version=v4.5.2" }));
            Assert.Equal(1, result.Count());
            Assert.Equal("userAsm1, Version=1.0.0.0", result.FirstOrDefault().AssemblyIdentity);

            // Ignore different assembly
            result = engine.FindBreakingChangeSkippedAssemblies(new[] { framework }, testData.SelectMany(kvp => kvp.Value).Distinct(), GenerateIgnoreAssemblies(true, new string[0]));
            Assert.Equal(1, result.Count());
            Assert.Equal("userAsm2, Version=2.0.0.0", result.FirstOrDefault().AssemblyIdentity);
        }

        [Fact]
        public void ShowRetargettingIssuesFalseShouldReturnOnlyRuntimeIssues()
        {
            var catalog = Substitute.For<IApiCatalogLookup>();
            var recommendations = GenerateTestRecommendationsForShowRetargetting(2, 3);
            var testData = GenerateTestData(catalog);
            var engine = new AnalysisEngine(catalog, recommendations);

            var framework = new FrameworkName(".NET Framework, Version = v4.5");

            // ShowRetargettingIssues
            IEnumerable<BreakingChangeDependency> result = engine.FindBreakingChanges(targets: new[] { framework },
                                                                                      dependencies: testData,
                                                                                      assembliesToIgnore: null,
                                                                                      breakingChangesToSuppress: null,
                                                                                      submittedAssemblies: Array.Empty<string>(),
                                                                                      showRetargettingIssues: false);

            Assert.Equal(3, result.Count());

            //verify only 3, 4 and 5 are in the list
            int expectedID = 3;
            foreach (BreakingChangeDependency bcd in result)
            {
                Assert.Equal(expectedID.ToString(), bcd.Break.Id);
                expectedID++;
            }
        }

        [Fact]
        public void ShowRetargettingIssuesTrueShouldReturnRuntimeAndRetargettingIssues()
        {
            var catalog = Substitute.For<IApiCatalogLookup>();
            var recommendations = GenerateTestRecommendationsForShowRetargetting(2, 3);
            var testData = GenerateTestData(catalog);
            var engine = new AnalysisEngine(catalog, recommendations);

            var framework = new FrameworkName(".NET Framework, Version = v4.5");

            // ShowRetargettingIssues
            IEnumerable<BreakingChangeDependency> result = engine.FindBreakingChanges(targets: new[] { framework },
                                                                                      dependencies: testData,
                                                                                      assembliesToIgnore: null,
                                                                                      breakingChangesToSuppress: null,
                                                                                      submittedAssemblies: Array.Empty<string>(),
                                                                                      showRetargettingIssues: true);

            Assert.Equal(5, result.Count());

            //verify 1, 2, 3, 4 and 5 are in the list
            int expectedID = 1;
            foreach (BreakingChangeDependency bcd in result)
            {
                Assert.Equal(expectedID.ToString(), bcd.Break.Id);
                expectedID++;
            }
        }

        private static void TestBreakingChangeWithoutFixedEntry(Version version, bool noBreakingChangesExpected)
        {
            TestBreakingChange(version, GenerateTestRecommendationsWithoutFixedEntry(), noBreakingChangesExpected, null, Enumerable.Empty<string>());
        }

        private static void TestBreakingChangeWithFixedEntry(Version version, bool noBreakingChangesExpected)
        {
            TestBreakingChange(version, GenerateTestRecommendationsWithFixedEntry(), noBreakingChangesExpected, null, Enumerable.Empty<string>());
        }

        private static void TestBreakingChangeWithFixedInServicingEntry(Version version, bool noBreakingChangesExpected)
        {
            TestBreakingChange(version, GenerateTestRecommendationsWithFixedInServicingEntry(), noBreakingChangesExpected, null, Enumerable.Empty<string>());
        }

        private static void TestBreakingChangeWithIgnoreList(Version version, bool noBreakingChangesExpected, IEnumerable<AssemblyInfo> assembliesToIgnore)
        {
            TestBreakingChange(version, GenerateTestRecommendationsWithoutFixedEntry(), noBreakingChangesExpected, assembliesToIgnore, Enumerable.Empty<string>());
        }

        private static void TestBreakingChangeWithSuppression(Version version, bool noBreakingChangesExpected, IEnumerable<string> suppressions)
        {
            TestBreakingChange(version, GenerateTestRecommendationsWithoutFixedEntry(), noBreakingChangesExpected, null, suppressions);
        }

        private static void TestBreakingChange(Version version, IApiRecommendations recommendations, bool noBreakingChangesExpected, IEnumerable<AssemblyInfo> assembliesToIgnore, IEnumerable<string> breakingChangesToSuppress)
        {
            var catalog = Substitute.For<IApiCatalogLookup>();
            var testData = GenerateTestData(catalog);
            var engine = new AnalysisEngine(catalog, recommendations);

            // Value from AnalysisEngine.FullFrameworkIdentifier
            var framework = new FrameworkName(".NET Framework" + ",Version=" + version);

            var breakingChanges = engine.FindBreakingChanges(new[] { framework }, testData, assembliesToIgnore, breakingChangesToSuppress, Array.Empty<string>()).ToList();

            if (noBreakingChangesExpected)
            {
                Assert.Empty(breakingChanges);
            }
            else
            {
                Assert.Equal(1, breakingChanges.Count);
                Assert.Equal("5", breakingChanges.First().Break.Id);
            }
        }

        private static IApiRecommendations GenerateTestRecommendationsWithFixedEntry()
        {
            var recommendations = Substitute.For<IApiRecommendations>();

            var breakingChange1 = new BreakingChange
            {
                ApplicableApis = new[] { TestDocId1 },
                Id = "5",
                VersionBroken = Version.Parse("4.5"),
                VersionFixed = Version.Parse("4.5.2")
            };

            recommendations.GetBreakingChanges(TestDocId1).Returns(new[] { breakingChange1 });

            return recommendations;
        }

        // Compat servicing updates can cause VersionBroken and VersionFixed to be the same
        private static IApiRecommendations GenerateTestRecommendationsWithFixedInServicingEntry()
        {
            var recommendations = Substitute.For<IApiRecommendations>();

            var breakingChange1 = new BreakingChange
            {
                ApplicableApis = new[] { TestDocId1 },
                Id = "5",
                VersionBroken = Version.Parse("4.5"),
                VersionFixed = Version.Parse("4.5")
            };

            recommendations.GetBreakingChanges(TestDocId1).Returns(new[] { breakingChange1 });

            return recommendations;
        }


        private static IApiRecommendations GenerateTestRecommendationsWithoutFixedEntry()
        {
            var recommendations = Substitute.For<IApiRecommendations>();

            var breakingChange1 = new BreakingChange
            {
                ApplicableApis = new[] { TestDocId1 },
                Id = "5",
                VersionBroken = Version.Parse("4.5")
            };

            recommendations.GetBreakingChanges(TestDocId1).Returns(new[] { breakingChange1 });

            return recommendations;
        }

        /// <summary>
        /// This method generates breaking changes recomendations for testing the ShowRetargettingIssues
        /// </summary>
        /// <param name="numOfRetargettingIssues">Number of retargetting issues to put in the recommendations result</param>
        /// <param name="numOfRuntimeIssues">Number of runtime issues to put in the recommendations result</param>
        /// <returns>API recommendations result containing the number of issues to be included</returns>
        private static IApiRecommendations GenerateTestRecommendationsForShowRetargetting(int numOfRetargettingIssues = 1,
                                                                                          int numOfRuntimeIssues = 1)
        {
            int lastIDUsed = 1;
            var recommendations = Substitute.For<IApiRecommendations>();
            List<BreakingChange> breakingChanges = new List<BreakingChange>();

            //add requested number of retargetting issues
            for (int i = 0; i < numOfRetargettingIssues; i++)
            {
                //add a new breaking change
                BreakingChange bc = new BreakingChange
                {
                    ApplicableApis = new[] { TestDocId1 },
                    Id = lastIDUsed.ToString(),
                    VersionBroken = Version.Parse("4.5"),
                    IsQuirked = true
                };

                breakingChanges.Add(bc);

                lastIDUsed++;
            }

            //add requested number of runtime issues
            for (int i = 0; i < numOfRuntimeIssues; i++)
            {
                //add a new breaking change
                BreakingChange bc = new BreakingChange
                {
                    ApplicableApis = new[] { TestDocId1 },
                    Id = lastIDUsed.ToString(),
                    VersionBroken = Version.Parse("4.5"),
                    IsQuirked = false,
                    IsBuildTime = false
                };

                breakingChanges.Add(bc);
                lastIDUsed++;
            }

            recommendations.GetBreakingChanges(TestDocId1).Returns(breakingChanges.ToArray());

            return recommendations;
        }

        private const string TestDocId1 = "T:System.Drawing.Color";
        private const string TestDocId2 = "T:System.Data.SqlTypes.SqlBoolean";

        private static ICollection<IgnoreAssemblyInfo> GenerateIgnoreAssemblies(bool otherAssm, string[] targetFrameworks)
        {
            return new[] {
                new IgnoreAssemblyInfo() { AssemblyIdentity = otherAssm? "userAsm2, Version=2.0.0.0" : "userAsm1, Version=1.0.0.0", TargetsIgnored = targetFrameworks }
            };
        }

        private static IDictionary<MemberInfo, ICollection<AssemblyInfo>> GenerateTestData(IApiCatalogLookup catalog)
        {
            var userAsm1 = new AssemblyInfo { AssemblyIdentity = "userAsm1, Version=1.0.0.0", FileVersion = "1.0.0.0" };
            var userAsm2 = new AssemblyInfo { AssemblyIdentity = "userAsm2, Version=2.0.0.0", FileVersion = "2.0.0.0" };
            var userAsm3 = new AssemblyInfo { AssemblyIdentity = "userAsm3, Version=3.0.0.0", FileVersion = "3.0.0.0" };
            var mi1 = new MemberInfo { DefinedInAssemblyIdentity = "System.Drawing, Version=1.0.136.0, PublicKeyToken=b03f5f7f11d50a3a", MemberDocId = TestDocId1 };
            var mi2 = new MemberInfo { DefinedInAssemblyIdentity = "System.Data, Version=1.0.136.0, PublicKeyToken=b77a5c561934e089", MemberDocId = TestDocId2 };
            var mi3 = new MemberInfo { DefinedInAssemblyIdentity = "userAsm1, Version=1.0.0.0", MemberDocId = "T:MyType" };

            catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(mi1.DefinedInAssemblyIdentity)).Returns(true);
            catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(mi2.DefinedInAssemblyIdentity)).Returns(true);
            catalog.IsFrameworkMember(mi1.MemberDocId).Returns(true);
            catalog.IsFrameworkMember(mi2.MemberDocId).Returns(true);

            return new Dictionary<MemberInfo, ICollection<AssemblyInfo>>
            {
                {mi1, new[] { userAsm1 } },
                {mi2, new[] { userAsm2 } },
                {mi3, new[] { userAsm3 } },
            };
        }

        private static string GetAssemblyIdentityWithoutCultureAndVersion(string assemblyIdentity)
        {
            var assembly = new System.Reflection.AssemblyName(assemblyIdentity) { Version = null };
#if FEATURE_ASSEMBLYNAME_CULTUREINFO
            assembly.CultureInfo = null;
#else
            assembly.CultureName = null;
#endif
            return assembly.ToString();
        }
    }
}
