// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var path = new FileInfo("Microsoft.Fx.Portability.MetadataReader.Tests.dll");

            var dependencies = finder.FindDependencies(new[] { path }, progressReport);
            var actual = dependencies.UnresolvedAssemblies
                            .Select(u => u.Key)
                            .OrderBy(u => u);

            Assert.Equal(_expectedResult, actual);
            Assert.True(dependencies.UnresolvedAssemblies.All(o => o.Value.Count == 1));
            Assert.True(dependencies.UnresolvedAssemblies.All(o => string.Equals(o.Value.First(), "Microsoft.Fx.Portability.MetadataReader.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", StringComparison.Ordinal)));

            // Make sure no issues were found
            progressReport.Received(0).ReportIssue(Arg.Any<string>());
        }

        private IEnumerable<string> _expectedResult = new[]
        {
            "xunit.assert, Version=2.1.0.2945, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c",
            "NSubstitute, Version=1.8.1.0, Culture=neutral, PublicKeyToken=92dd2e9066daa5ca",
            "Microsoft.CodeAnalysis.CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
            "Microsoft.Fx.Portability.MetadataReader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
            "Microsoft.CodeAnalysis.CSharp.Desktop, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
            "Microsoft.CodeAnalysis, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35",
            "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
            "System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
            "Microsoft.Fx.Portability, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
            "xunit.core, Version=2.1.0.2945, Culture=neutral, PublicKeyToken=8d05b1bb7a6fdb6c",
            "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        }.OrderBy(o => o);
    }
}
