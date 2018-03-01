// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using NSubstitute;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Xunit;

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    public class SystemObjectFinderTests
    {
        [Fact]
        public static void MultipleMscorlibReferencesFound()
        {
            var objectFinder = new SystemObjectFinder(new DotNetFrameworkFilter());
            var file = TestAssembly.Create("multiple-mscorlib.exe");

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
    }
}
