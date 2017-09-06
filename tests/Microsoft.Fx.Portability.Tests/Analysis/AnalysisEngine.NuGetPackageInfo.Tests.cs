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
    public class AnalysisEngineTestsNuGetPackageInfo
    {
        /// <summary>
        /// Tests that given a set of assemblies, the correct nuget package is returned.
        /// </summary>
        [Fact]
        public void TestGetNugetPackageInfo()
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

            var nugetPackageWin80 = GetNuGetPackage("TestNuGetPackage", "1.3.4");
            var nugetPackageNetStandard = GetNuGetPackage("TestNuGetPackage", "10.0.8");

            packageFinder.TryFindPackage(nugetPackageAssembly.AssemblyIdentity, targets, out var packages)
                .Returns(x =>
                {
                    // return this value in `out var packages`
                    x[2] = new Dictionary<FrameworkName, IEnumerable<NuGetPackageId>>
                    {
                        { Windows80,  new[] { nugetPackageWin80 } },
                        { NetStandard16,  new[] { nugetPackageNetStandard } }
                    }
                    .ToImmutableDictionary();
                    return true;
                });

            var engine = new AnalysisEngine(Substitute.For<IApiCatalogLookup>(), Substitute.For<IApiRecommendations>(), packageFinder);

            // Act
            var nugetPackageResult = engine.GetNuGetPackagesInfo(inputAssemblies.Select(x => x.AssemblyIdentity), targets).ToArray();

            // Assert

            // We expect that it was able to locate this particular package and
            // return a result for each input target framework.
            Assert.Equal(nugetPackageResult.Count(), targets.Length);

            var windows80Result = nugetPackageResult.Single(x => x.Target == Windows80);
            var netstandard16Result = nugetPackageResult.Single(x => x.Target == NetStandard16);
            var net11Result = nugetPackageResult.Single(x => x.Target == Net11);

            Assert.Equal(1, windows80Result.SupportedPackages.Count);
            Assert.Equal(1, netstandard16Result.SupportedPackages.Count);
            // We did not have any packages that supported .NET Standard 2.0
            Assert.Empty(net11Result.SupportedPackages);

            Assert.Equal(nugetPackageWin80, windows80Result.SupportedPackages.First());
            Assert.Equal(nugetPackageNetStandard, netstandard16Result.SupportedPackages.First());

            foreach (var result in nugetPackageResult)
            {
                Assert.Equal(result.AssemblyInfo, nugetPackageAssembly.AssemblyIdentity);
            }
        }

        /// <summary>
        /// Tests that if an assembly is not explicitly specified, it'll be in the set of assemblies to remove.
        /// </summary>
        [Fact]
        public void ComputeAssembliesToRemove_PackageFound()
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
            var packageId = new[] { GetNuGetPackage("SomeNuGetPackage", "2.0.1") };
            var engine = new AnalysisEngine(Substitute.For<IApiCatalogLookup>(), Substitute.For<IApiRecommendations>(), Substitute.For<IPackageFinder>());

            var nugetPackageResult = new[]
            {
                new NuGetPackageInfo(userNuGetPackage.AssemblyIdentity, Windows81, packageId),
                new NuGetPackageInfo(userNuGetPackage.AssemblyIdentity, NetStandard16, packageId)
            };

            // Act
            var assemblies = engine.ComputeAssembliesToRemove(inputAssemblies, targets, nugetPackageResult);

            // Assert
            Assert.Equal(1, assemblies.Count());
            Assert.Equal(assemblies.First(), userNuGetPackage.AssemblyIdentity);
        }

        /// <summary>
        /// Tests that if a matching NuGet package, BUT does not support all
        /// the given targets... we shouldn't remove it.
        /// </summary>
        [Fact]
        public void ComputeAssembliesToRemove_PackageNotFound()
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
            var packageId = new[] { GetNuGetPackage("SomeNuGetPackage", "2.0.1") };
            var engine = new AnalysisEngine(Substitute.For<IApiCatalogLookup>(), Substitute.For<IApiRecommendations>(), Substitute.For<IPackageFinder>());

            var nugetPackageResult = new[]
            {
                new NuGetPackageInfo(userNuGetPackage.AssemblyIdentity, Windows81, packageId),
            };

            var assemblies = engine.ComputeAssembliesToRemove(inputAssemblies, targets, nugetPackageResult);

            Assert.Empty(assemblies);
        }

        /// <summary>
        /// Tests that for an explicitly given assembly, if a matching NuGet
        /// package is found, AND all of its targets are supported, it is not
        /// removed.
        /// </summary>
        [Fact]
        public void ComputeAssembliesToRemove_AssemblyExplicitlyPassedIn()
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
            var packageId = new[] { GetNuGetPackage("SomeNuGetPackage", "2.0.1") };
            var engine = new AnalysisEngine(Substitute.For<IApiCatalogLookup>(), Substitute.For<IApiRecommendations>(), Substitute.For<IPackageFinder>());

            var nugetPackageResult = new[]
            {
                new NuGetPackageInfo(userNuGetPackage.AssemblyIdentity, Windows81, packageId),
                new NuGetPackageInfo(userNuGetPackage.AssemblyIdentity, NetStandard16, packageId)
            };

            var assemblies = engine.ComputeAssembliesToRemove(inputAssemblies, targets, nugetPackageResult);

            Assert.Empty(assemblies);
        }

        private static AssemblyInfo GetAssemblyInfo(string assemblyName, string version, bool isExplicitlySpecified)
        {
            return GetAssemblyInfo(assemblyName, version, string.Empty, isExplicitlySpecified);
        }

        private static AssemblyInfo GetAssemblyInfo(string assemblyName, string version, string location, bool isExplicitlySpecified)
        {
            var name = new FrameworkName(assemblyName, Version.Parse(version));
            return new AssemblyInfo { AssemblyIdentity = name.ToString(), IsExplicitlySpecified = isExplicitlySpecified };
        }

        private static NuGetPackageId GetNuGetPackage(string packageId, string version, string url = null)
        {
            return new NuGetPackageId(packageId, version, url);
        }
    }
}
