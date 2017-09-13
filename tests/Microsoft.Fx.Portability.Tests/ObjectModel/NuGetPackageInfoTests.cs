using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Linq;
using System.Runtime.Versioning;
using Xunit;

namespace Microsoft.Fx.Portability.Tests.ObjectModel
{
    public class NuGetPackageInfoTests
    {
        [Fact]
        public void NuGetPackageInfoCreated()
        {
            var assemblyInfo = "MyNuGetPackage, Version=1.5.3";
            var frameworkName = new FrameworkName("SomeFramework", Version.Parse("5.6.7.2"));
            var supportedPackages = new[] {
                new NuGetPackageId("MyNuGetPackageId.1", "1.2.2", "https://aurl.com"),
                new NuGetPackageId("AnotherNuGetPackage.32", "3.4.4", "https://nourl"),
                new NuGetPackageId("Something", "13.3.4", null)
            };

            var packageInfo = new NuGetPackageInfo(assemblyInfo, frameworkName, supportedPackages);
            var noPackagesInfo = new NuGetPackageInfo(assemblyInfo, frameworkName, null);

            Assert.Equal(assemblyInfo, packageInfo.AssemblyInfo);
            Assert.Equal(frameworkName, packageInfo.Target);

            var ordered = supportedPackages.OrderBy(x => x.PackageId);
            Assert.True(packageInfo.SupportedPackages.SequenceEqual(ordered));

            Assert.Equal(assemblyInfo, noPackagesInfo.AssemblyInfo);
            Assert.Equal(frameworkName, noPackagesInfo.Target);
            Assert.Empty(noPackagesInfo.SupportedPackages);
        }

        [Fact]
        public void Equality()
        {
            // Set up
            var assemblyName = "MyNuGetPackage, Version=1.0.4";
            var frameworkName = new FrameworkName("SomeFramework", Version.Parse("5.6.7.2"));
            var supportedPackages = new[] {
                new NuGetPackageId("MyNuGetPackageId.1", "1.2.2", "https://aurl.com"),
                new NuGetPackageId("AnotherNuGetPackage.32", "3.4.4", "https://nourl"),
                new NuGetPackageId("Something", "13.3.4", null)
            };
            var supportedPackagesRemove1 = supportedPackages.Take(2);

            var original = new NuGetPackageInfo(assemblyName, frameworkName, supportedPackages);
            var compared = new NuGetPackageInfo(assemblyName, frameworkName, supportedPackages);
            var comparedNotSamePackages = new NuGetPackageInfo(assemblyName, frameworkName, supportedPackagesRemove1);

            // Act & Assert
            Assert.True(original.Equals(compared));

            Assert.False(original.Equals("something"));
            Assert.False(original.Equals(null));
            Assert.False(original.Equals(comparedNotSamePackages));
        }

        [Fact]
        public void InvalidConstruction()
        {
            var assemblyName = "MyNuGetPackage, Version=1.0.4";
            var frameworkName = new FrameworkName("SomeFramework", Version.Parse("5.6.7.2"));
            var supportedPackages = new[] {
                new NuGetPackageId("MyNuGetPackageId.1", "1.2.2", "https://aurl.com"),
                new NuGetPackageId("AnotherNuGetPackage.32", "3.4.4", "https://nourl"),
                new NuGetPackageId("Something", "13.3.4", null)
            };

            Assert.Throws<ArgumentNullException>(() => new NuGetPackageInfo(null, frameworkName, supportedPackages));
            Assert.Throws<ArgumentNullException>(() => new NuGetPackageInfo(assemblyName, null, supportedPackages));
        }
    }
}
