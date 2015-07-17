// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    public class ReflectionMetadataInfoTests
    {
        [Fact]
        public void UnresolvedAssemblyTest()
        {
            var finder = new Microsoft.Fx.Portability.Analyzer.ReflectionMetadataDependencyFinder();
            var progressReport = Substitute.For<IProgressReporter>();

            var path = this.GetType().GetTypeInfo().Assembly.Location;
            var testInfo = new FileInfo(path);

            var dependencies = finder.FindDependencies(new[] { testInfo }, progressReport);
            var actual = dependencies.UnresolvedAssemblies
                            .Select(u => u.Key)
                            .OrderBy(u => u);

            Assert.Equal(_expectedResult.Count(), actual.Count());

            // Use this instead of Assert.Equal so it will output the missing item
            foreach (var items in _expectedResult.Zip(actual, Tuple.Create))
            {
                Assert.Equal(items.Item1, items.Item2);
            }

            Assert.True(dependencies.UnresolvedAssemblies.All(o => o.Value.Count == 1));
            Assert.True(dependencies.UnresolvedAssemblies.All(o => string.Equals(o.Value.First(), "Microsoft.Fx.Portability.MetadataReader.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", StringComparison.Ordinal)));

            // Make sure no issues were found
            progressReport.Received(0).ReportIssue(Arg.Any<string>());
        }

        private IEnumerable<string> _expectedResult = new[]
        {
            "Microsoft.CodeAnalysis, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
            "Microsoft.CodeAnalysis.CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
            "Microsoft.Fx.Portability, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
            "Microsoft.Fx.Portability.MetadataReader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
            "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
            "NSubstitute, Version=1.8.1.0, Culture=neutral, PublicKeyToken=92dd2e9066daa5ca",
            "System.Collections.Immutable, Version=1.1.36.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
            "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
            "xunit.assert, Version=2.1.0.3029, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c",
            "xunit.core, Version=2.1.0.3029, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c"
        }.OrderBy(o => o);
    }
}
