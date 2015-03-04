// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analysis;
using Microsoft.Fx.Portability.ObjectModel;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Xunit;

namespace Microsoft.Fx.Portability.Web.Analyze.Tests
{
    public class AnalysisEngineTests
    {
        #region FindUnreferencedAssemblies

        static List<string> UnreferencedAssemblies = new List<string>()
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

            var result = engine.FindUnreferencedAssemblies(UnreferencedAssemblies, null).ToList();

            Assert.NotNull(result);
        }

        [Fact]
        public void FindUnreferencedAssemblies_NoUnreferencedAssemblies()
        {
            var catalog = Substitute.For<IApiCatalogLookup>();
            var recommendations = Substitute.For<IApiRecommendations>();
            var engine = new AnalysisEngine(catalog, recommendations);

            var specifiedUserAssemblies = UnreferencedAssemblies.Select(ua => new AssemblyInfo() { AssemblyIdentity = ua, FileVersion = "0.0.0.0" }).ToList();
            var unreferencedAssms = engine.FindUnreferencedAssemblies(UnreferencedAssemblies, specifiedUserAssemblies).ToList();

            // We don't expect to have any unreferenced assemblies.
            Assert.Empty(unreferencedAssms);
        }

        [Fact]
        public void FindUnreferencedAssemblies_UnreferencedAssemblies_1()
        {
            var catalog = Substitute.For<IApiCatalogLookup>();
            catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(UnreferencedAssemblies[0])).Returns(true);

            var recommendations = Substitute.For<IApiRecommendations>();
            var engine = new AnalysisEngine(catalog, recommendations);

            var specifiedUserAssemblies = new[] { new AssemblyInfo { FileVersion = "", AssemblyIdentity = "MyAssembly" } };
            var unreferencedAssms = engine.FindUnreferencedAssemblies(UnreferencedAssemblies, specifiedUserAssemblies).ToList();

            // 0 missing assembly since Microsoft.CSharp is a FX assembly and we specified MyAssembly
            Assert.Empty(unreferencedAssms);
        }

        [Fact]
        public void FindUnreferencedAssemblies_UnreferencedAssemblies_2()
        {
            var catalog = Substitute.For<IApiCatalogLookup>();
            catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(UnreferencedAssemblies[0])).Returns(true);

            var recommendations = Substitute.For<IApiRecommendations>();
            var engine = new AnalysisEngine(catalog, recommendations);

            var unreferencedAssms = engine.FindUnreferencedAssemblies(UnreferencedAssemblies, Enumerable.Empty<AssemblyInfo>()).ToList();

            // 1 missing assembly since Microsoft.CSharp is a FX assembly 
            Assert.Equal(1, unreferencedAssms.Count);
        }

        [Fact]
        public void FindUnreferencedAssemblies_UnreferencedAssemblies_WithNullInSpecifiedList()
        {
            var catalog = Substitute.For<IApiCatalogLookup>();
            catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(UnreferencedAssemblies[0])).Returns(true);

            var recommendations = Substitute.For<IApiRecommendations>();
            var engine = new AnalysisEngine(catalog, recommendations);

            var specifiedUserAssemblies = new List<AssemblyInfo>() { new AssemblyInfo() { FileVersion = "", AssemblyIdentity = "MyAssembly" }, null };
            var unreferencedAssms = engine.FindUnreferencedAssemblies(UnreferencedAssemblies, specifiedUserAssemblies).ToList();

            // 0 missing assembly since Microsoft.CSharp is a fx assembly and we specified MyAssembly
            Assert.Empty(unreferencedAssms);
        }

        [Fact]
        public void FindUnreferencedAssemblies_UnreferencedAssemblies_WithNullInUnrefList()
        {
            var catalog = Substitute.For<IApiCatalogLookup>();
            catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(UnreferencedAssemblies[0])).Returns(true);

            var recommendations = Substitute.For<IApiRecommendations>();
            var engine = new AnalysisEngine(catalog, recommendations);

            var specifiedUserAssemblies = new List<AssemblyInfo>() { new AssemblyInfo() { FileVersion = "", AssemblyIdentity = "MyAssembly" } };
            var listWithNulls = UnreferencedAssemblies.Concat(new List<string>() { null }).ToList();

            var unreferencedAssms = engine.FindUnreferencedAssemblies(listWithNulls, specifiedUserAssemblies).ToList();

            // 0 missing assembly since Microsoft.CSharp is a fx assembly and we specified MyAssembly
            Assert.Empty(unreferencedAssms);
        }
        #endregion

        [Fact]
        public void FindMembersNotInTargets_AllNull()
        {
            var engine = new AnalysisEngine(null, null);

            engine.FindMembersNotInTargets(null, null);
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
            var notInTarget = engine.FindMembersNotInTargets(targets, testData);

            Assert.Equal(2, notInTarget.Count);
        }

        [Fact]
        public void FindMembersNotInTargets_2()
        {
            // No member information passed through.
            var testData = new Dictionary<MemberInfo, ICollection<AssemblyInfo>>();

            var userAsm1 = new AssemblyInfo() { AssemblyIdentity = "userAsm1, Version=1.0.0.0", FileVersion = "1.0.0.0" };
            var userAsm2 = new AssemblyInfo() { AssemblyIdentity = "userAsm2, Version=2.0.0.0", FileVersion = "2.0.0.0" };
            var userAsm3 = new AssemblyInfo() { AssemblyIdentity = "userAsm3, Version=3.0.0.0", FileVersion = "3.0.0.0" };
            var mi1 = new MemberInfo() { DefinedInAssemblyIdentity = "System.Drawing, Version=1.0.136.0, PublicKeyToken=b03f5f7f11d50a3a", MemberDocId = "T:System.Drawing.Color" };
            var mi2 = new MemberInfo() { DefinedInAssemblyIdentity = "System.Data, Version=1.0.136.0, PublicKeyToken=b77a5c561934e089", MemberDocId = "T:System.Data.SqlTypes.SqlBoolean" };
            var mi3 = new MemberInfo() { DefinedInAssemblyIdentity = "userAsm1, Version=1.0.0.0", MemberDocId = "T:MyType" };

            var targets = new List<FrameworkName>() { new FrameworkName("Windows Phone, version=8.1") };

            var catalog = Substitute.For<IApiCatalogLookup>();
            catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(mi1.DefinedInAssemblyIdentity)).Returns(true);
            catalog.IsFrameworkAssembly(GetAssemblyIdentityWithoutCultureAndVersion(mi2.DefinedInAssemblyIdentity)).Returns(true);
            catalog.IsFrameworkMember(mi1.MemberDocId).Returns(true);
            catalog.IsFrameworkMember(mi2.MemberDocId).Returns(true);

            var recommendations = Substitute.For<IApiRecommendations>();
            var engine = new AnalysisEngine(catalog, recommendations);
            var notInTarget = engine.FindMembersNotInTargets(targets, testData);

            Assert.Equal(0, notInTarget.Count);
        }

        private static string GetAssemblyIdentityWithoutCultureAndVersion(string assemblyIdentity)
        {
            return new System.Reflection.AssemblyName(assemblyIdentity) { CultureInfo = null, Version = null }.ToString();
        }
    }
}
