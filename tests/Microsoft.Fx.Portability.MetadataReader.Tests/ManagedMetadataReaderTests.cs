using Microsoft.Fx.Portability.Analyzer;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    public class ManagedMetadataReaderTests
    {
        [Fact(Skip = "System.Collections.Immutable is in flux and Roslyn and System.Reflection.Metadata use different versions.")]
        public void FindDependencies()
        {
            var dependencyFinder = new ReflectionMetadataDependencyFinder();
            var path = new FileInfo(TestAssembly.EmptyProject);
            var progressReporter = Substitute.For<IProgressReporter>();

            var dependencies = dependencyFinder.FindDependencies(new[] { path }, progressReporter);

            var foundDocIds = dependencies
                .Dependencies
                .Select(o => Tuple.Create(o.Key.MemberDocId, o.Value.Count))
                .OrderBy(o => o.Item1, StringComparer.Ordinal)
                .ToList();

            var expected = EmptyProjectMemberDocId()
                .OrderBy(o => o.Item1, StringComparer.Ordinal)
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