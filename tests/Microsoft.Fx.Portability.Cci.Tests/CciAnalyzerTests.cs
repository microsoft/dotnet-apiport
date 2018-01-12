// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Fx.Portability.Analyzer;
using NSubstitute;
using Xunit;

namespace Microsoft.Fx.Portability.Cci.Tests
{
    public class CciAnalyzerTests
    {
        [Fact]
        public static void FindDependencies()
        {
            var cci = new CciDependencyFinder();
            var path = new TestAssemblyFile(TestAssembly.EmptyProject);
            var progressReporter = Substitute.For<IProgressReporter>();

            var dependencies = cci.FindDependencies(new[] { path }, progressReporter);

            var foundDocIds = dependencies.Dependencies
                .Select(o => Tuple.Create(o.Key.MemberDocId, o.Value.Count))
                .OrderBy(x => x.Item1)
                .ToList();

            var expected = EmptyProjectMemberDocId()
                .OrderBy(x => x.Item1)
                .ToList();

            Assert.Equal(expected, foundDocIds);
        }

        private static IEnumerable<Tuple<string, int>> EmptyProjectMemberDocId()
        {
            yield return Tuple.Create("M:System.Diagnostics.DebuggableAttribute.#ctor(System.Diagnostics.DebuggableAttribute.DebuggingModes)", 1);
            yield return Tuple.Create("M:System.Object.#ctor", 1);
            yield return Tuple.Create("M:System.Runtime.CompilerServices.CompilationRelaxationsAttribute.#ctor(System.Int32)", 1);
            yield return Tuple.Create("M:System.Runtime.CompilerServices.RuntimeCompatibilityAttribute.#ctor", 1);
            yield return Tuple.Create("M:System.Runtime.Versioning.TargetFrameworkAttribute.#ctor(System.String)", 1);
            yield return Tuple.Create("T:System.Diagnostics.DebuggableAttribute", 1);
            yield return Tuple.Create("T:System.Diagnostics.DebuggableAttribute.DebuggingModes", 1);
            yield return Tuple.Create("T:System.Object", 1);
            yield return Tuple.Create("T:System.Runtime.CompilerServices.CompilationRelaxationsAttribute", 1);
            yield return Tuple.Create("T:System.Runtime.CompilerServices.RuntimeCompatibilityAttribute", 1);
            yield return Tuple.Create("T:System.Runtime.Versioning.TargetFrameworkAttribute", 1);
        }
    }
}
