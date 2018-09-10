// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analysis;
using Microsoft.Fx.Portability.ObjectModel;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Versioning;
using Xunit;

using static Microsoft.Fx.Portability.Tests.TestData.TestFrameworks;

namespace Microsoft.Fx.Portability.Tests.Analysis
{
    /// <summary>
    /// Tests GetNuGetPackageInfo for AnalysisEngine
    /// </summary>
    public class AnalysisEngineNuGetPackageInfoTests
    {
        /// <summary>
        /// Tests that given a set of assemblies, the correct nuget package is returned.
        /// </summary>
        [Fact]
        public static void TestGetNugetPackageInfo()
        {
            // Arrange
            var nugetPackageAssembly = GetAssemblyInfo("NugetPackageAssembly", "2.0.5.0", isExplicitlySpecified: false);
            var inputAssemblies = new[]
            {
                GetAssemblyInfo("TestUserAssembly", "2.0.5.0", isExplicitlySpecified: true),
                nugetPackageAssembly,
                GetAssemblyInfo("TestUserLibrary", "5.0.0", isExplicitlySpecified: true),
            };
            var targets = new[] { Windows80, Net11, NetStandard16 };
            var packageFinder = Substitute.For<IPackageFinder>();

            var packageId = "TestNuGetPackage";
            var nugetPackageWin80Version = "1.3.4";  // supported version of the package
            var nugetPackageNetStandardVersion = "10.0.8";

            var packagesList = new[]
                    {
                        new NuGetPackageInfo(packageId, new Dictionary<FrameworkName, string>()
                        {
                            {Windows80, nugetPackageWin80Version },
                            { NetStandard16, nugetPackageNetStandardVersion},
                        })
                    };

            packageFinder.TryFindPackages(nugetPackageAssembly.AssemblyIdentity, targets, out var packages)
                    .Returns(x =>
                    {
                        // return this value in `out var packages`
                        x[2] = packagesList.ToImmutableList();
                        return true;
                    });

            var engine = new AnalysisEngine(Substitute.For<IApiCatalogLookup>(), Substitute.For<IApiRecommendations>(), packageFinder);

            // Act
            var nugetPackageResult = engine.GetNuGetPackagesInfoFromAssembly(inputAssemblies.Select(x => x.AssemblyIdentity), targets).ToArray();

            // Assert
            Assert.Single(nugetPackageResult);

            Assert.Equal(nugetPackageResult[0].SupportedVersions[Windows80], nugetPackageWin80Version);
            Assert.Equal(nugetPackageResult[0].SupportedVersions[NetStandard16], nugetPackageNetStandardVersion);
            // We did not have any packages that supported .NET Standard 2.0
            Assert.True(!nugetPackageResult[0].SupportedVersions.TryGetValue(Net11, out var value) || string.IsNullOrEmpty(value));

            Assert.Equal(nugetPackageResult[0].AssemblyInfo, nugetPackageAssembly.AssemblyIdentity);
        }

        /// <summary>
        /// Tests that if an assembly is not explicitly specified, and packages for this assembly are found, it'll be in the set of assemblies to remove.
        /// </summary>
        [Fact]
        public static void ComputeAssembliesToRemove_PackageFound()
        {
            // Arrange
            var userNuGetPackage = GetAssemblyInfo("NugetPackageAssembly", "2.0.5.0", isExplicitlySpecified: false);
            var inputAssemblies = new[]
            {
                GetAssemblyInfo("TestUserAssembly", "2.0.5.0", isExplicitlySpecified: true),
                userNuGetPackage,
                GetAssemblyInfo("TestUserLibrary", "5.0.0", isExplicitlySpecified: true),
            };

            var targets = new[] { Windows81, NetStandard16 };
            var packageId = "SomeNuGetPackage";
            var packageVersion = "2.0.1";
            var engine = new AnalysisEngine(Substitute.For<IApiCatalogLookup>(), Substitute.For<IApiRecommendations>(), Substitute.For<IPackageFinder>());

            var nugetPackageResult = new[]
            {
                new NuGetPackageInfo(packageId, new Dictionary<FrameworkName, string>() { { Windows81, packageVersion }, { NetStandard16, packageVersion } },userNuGetPackage.AssemblyIdentity)
            };

            // Act
            var assemblies = engine.ComputeAssembliesToRemove(inputAssemblies, targets, nugetPackageResult);

            // Assert
            Assert.Single(assemblies);
            Assert.Equal(assemblies.First(), userNuGetPackage.AssemblyIdentity);
        }

        /// <summary>
        /// Tests that if a matching NuGet package, BUT does not support all
        /// the given targets... we shouldn't remove it.
        /// </summary>
        [Fact]
        public static void ComputeAssembliesToRemove_PackageNotFound()
        {
            // Arrange
            var userNuGetPackage = GetAssemblyInfo("NugetPackageAssembly", "2.0.5.0", isExplicitlySpecified: false);
            var inputAssemblies = new[]
            {
                GetAssemblyInfo("TestUserAssembly", "2.0.5.0", isExplicitlySpecified: true),
                userNuGetPackage,
                GetAssemblyInfo("TestUserLibrary", "5.0.0", isExplicitlySpecified: true),
            };

            var targets = new[] { Windows81, NetStandard16 };
            var packageId = "SomeNuGetPackage";
            var packageVersion = "2.0.1";
            var engine = new AnalysisEngine(Substitute.For<IApiCatalogLookup>(), Substitute.For<IApiRecommendations>(), Substitute.For<IPackageFinder>());

            var nugetPackageResult = new[]
            {
                new NuGetPackageInfo(packageId, new Dictionary<FrameworkName, string>(){{Windows81, packageVersion }, { NetStandard16, null } }, userNuGetPackage.AssemblyIdentity)
            };

            var assemblies = engine.ComputeAssembliesToRemove(inputAssemblies, targets, nugetPackageResult);

            Assert.Empty(assemblies);

            var nugetPackageResult2 = new[]
            {
                new NuGetPackageInfo(packageId, new Dictionary<FrameworkName, string>(){{Windows81, packageVersion }}, userNuGetPackage.AssemblyIdentity)
            };

            assemblies = engine.ComputeAssembliesToRemove(inputAssemblies, targets, nugetPackageResult);

            Assert.Empty(assemblies);
        }

        /// <summary>
        /// Tests that for an explicitly given assembly, if a matching NuGet
        /// package is found, AND all of its targets are supported, it is not
        /// removed.
        /// </summary>
        [Fact]
        public static void ComputeAssembliesToRemove_AssemblyExplicitlyPassedIn()
        {
            // Arrange
            var userNuGetPackage = GetAssemblyInfo("NugetPackageAssembly", "2.0.5.0", isExplicitlySpecified: true);
            var inputAssemblies = new[]
            {
                GetAssemblyInfo("TestUserAssembly", "2.0.5.0", isExplicitlySpecified: true),
                userNuGetPackage,
                GetAssemblyInfo("TestUserLibrary", "5.0.0", isExplicitlySpecified: true),
            };

            var targets = new[] { Windows81, NetStandard16 };
            var packageId = "SomeNuGetPackage";
            var packageVersion = "2.0.1";
            var engine = new AnalysisEngine(Substitute.For<IApiCatalogLookup>(), Substitute.For<IApiRecommendations>(), Substitute.For<IPackageFinder>());

            var nugetPackageResult = new[]
            {
                new NuGetPackageInfo(packageId, new Dictionary<FrameworkName, string>(){{Windows81, packageVersion }, { NetStandard16, packageVersion } }, userNuGetPackage.AssemblyIdentity)
            };

            var assemblies = engine.ComputeAssembliesToRemove(inputAssemblies, targets, nugetPackageResult);

            Assert.Empty(assemblies);
        }

        /// <summary>
        /// Tests that if the flag isExplicitlySpecified is not set in AssemblyInfo,
        /// it defaults to being true (which means assembly is not removed).
        /// That is important for compatibility of old ApiPort tool with the service, after
        /// 'isExplicitlySpecified' was added.
        /// </summary>
        [Fact]
        public static void ComputeAssembliesToRemove_AssemblyFlagNotSet()
        {
            // Arrange
            var userNuGetPackage = GetAssemblyInfo("NugetPackageAssembly", "2.0.5.0");
            var inputAssemblies = new[] { userNuGetPackage };

            var targets = new[] { Windows81, NetStandard16 };
            var packageId = "SomeNuGetPackage";
            var packageVersion = "2.0.1";
            var engine = new AnalysisEngine(Substitute.For<IApiCatalogLookup>(), Substitute.For<IApiRecommendations>(), Substitute.For<IPackageFinder>());

            var nugetPackageResult = new[]
            {
                new NuGetPackageInfo(packageId, new Dictionary<FrameworkName, string>(){{Windows81, packageVersion }, { NetStandard16, packageVersion } }, userNuGetPackage.AssemblyIdentity)
            };

            var assemblies = engine.ComputeAssembliesToRemove(inputAssemblies, targets, nugetPackageResult);

            Assert.Empty(assemblies);
        }

        private static AssemblyInfo GetAssemblyInfo(string assemblyName, string version, bool isExplicitlySpecified)
        {
            var name = new FrameworkName(assemblyName, Version.Parse(version));
            return new AssemblyInfo { AssemblyIdentity = name.ToString(), IsExplicitlySpecified = isExplicitlySpecified };
        }

        private static AssemblyInfo GetAssemblyInfo(string assemblyName, string version)
        {
            var name = new FrameworkName(assemblyName, Version.Parse(version));
            return new AssemblyInfo { AssemblyIdentity = name.ToString() };
        }
    }
}
