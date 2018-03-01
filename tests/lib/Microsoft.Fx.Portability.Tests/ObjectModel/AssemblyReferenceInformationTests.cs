// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using Xunit;

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    public class AssemblyReferenceInformationTests
    {
        [Fact]
        public static void EqualityTests()
        {
            var assemblyInfo1 = new AssemblyReferenceInformation("name", Version.Parse("4.0"), "neutral", "1234");
            var assemblyInfo2 = new AssemblyReferenceInformation("Name", Version.Parse("4.0"), "neutral", "1234");
            var assemblyInfo3 = new AssemblyReferenceInformation("name2", Version.Parse("4.0"), "neutral", "1234");

            Assert.Equal($"name, Version=4.0, Culture=neutral, PublicKeyToken=1234", assemblyInfo1.ToString());
            Assert.Equal($"Name, Version=4.0, Culture=neutral, PublicKeyToken=1234", assemblyInfo2.ToString());
            Assert.Equal($"name2, Version=4.0, Culture=neutral, PublicKeyToken=1234", assemblyInfo3.ToString());

            Assert.True(assemblyInfo1.Equals(assemblyInfo2));
            Assert.False(assemblyInfo1.Equals(assemblyInfo3));
            Assert.False(assemblyInfo2.Equals(assemblyInfo3));

            Assert.Equal(assemblyInfo1.GetHashCode(), assemblyInfo2.GetHashCode());
            Assert.NotEqual(assemblyInfo1.GetHashCode(), assemblyInfo3.GetHashCode());
            Assert.NotEqual(assemblyInfo2.GetHashCode(), assemblyInfo3.GetHashCode());
        }
    }
}
