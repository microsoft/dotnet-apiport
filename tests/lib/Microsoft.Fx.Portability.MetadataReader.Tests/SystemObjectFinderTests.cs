// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    public class SystemObjectFinderTests
    {
        private readonly ITestOutputHelper _output;

        public SystemObjectFinderTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void MultipleMscorlibReferencesFound()
        {
            var objectFinder = new SystemObjectFinder(new DotNetFrameworkFilter());
            var file = TestAssembly.Create("multiple-mscorlib.il", _output);

            using (var stream = file.OpenRead())
            using (var peFile = new PEReader(stream))
            {
                var metadataReader = peFile.GetMetadataReader();

                var assemblyInfo = objectFinder.GetSystemRuntimeAssemblyInformation(metadataReader);

                Assert.Equal("mscorlib", assemblyInfo.Name);
                Assert.Equal("4.0.0.0", assemblyInfo.Version.ToString());
                Assert.Equal("neutral", assemblyInfo.Culture);
                Assert.Equal("b77a5c561934e089", assemblyInfo.PublicKeyToken);
            }
        }

        /// <summary>
        /// Test that SystemObjectFinder works even for netstandard facade 
        /// assemblies that may not have references to mscorlib or system.runtime
        /// </summary>
        [Fact]
        public void NetstandardReferencesOnly()
        {
            var objectFinder = new SystemObjectFinder(new DotNetFrameworkFilter());
            var file = TestAssembly.Create("OnlyNetStandardReference.il", _output);

            using (var stream = file.OpenRead())
            using (var peFile = new PEReader(stream))
            {
                var metadataReader = peFile.GetMetadataReader();

                var assemblyInfo = objectFinder.GetSystemRuntimeAssemblyInformation(metadataReader);

                Assert.Equal("netstandard", assemblyInfo.Name);
                Assert.Equal("2.0.0.0", assemblyInfo.Version.ToString());
                Assert.Equal("neutral", assemblyInfo.Culture);
                Assert.Equal("cc7b13ffcd2ddd51", assemblyInfo.PublicKeyToken);
            }
        }

        /// <summary>
        /// Test that <see cref="SystemObjectFinder"/> doesn't throw on
        /// assemblies that don't reference System.Object, but also don't
        /// reference anything else. This is typically the case for resource
        /// assemblies (e.g., for localization).
        /// </summary>
        [Fact]
        public void ResourceAssembliesGetSkipped()
        {
            var objectFinder = new SystemObjectFinder(new DotNetFrameworkFilter());
            var file = TestAssembly.Create("ResourceAssembliesGetSkipped_NoReferences.il", _output);

            using (var stream = file.OpenRead())
            using (var peFile = new PEReader(stream))
            {
                var metadataReader = peFile.GetMetadataReader();

                var ex = Record.Exception(() =>
                {
                    var assemblyInfo = objectFinder.GetSystemRuntimeAssemblyInformation(metadataReader);

                    // we shouldn't receive anything back
                    Assert.Null(assemblyInfo);
                });

                // this should not throw
                Assert.Null(ex);
            }
        }
    }
}
