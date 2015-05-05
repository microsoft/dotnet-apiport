// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        [Fact]
        public void EmptyProject()
        {
            CompareDependencies(TestAssembly.EmptyProject, EmptyProjectMemberDocId());
        }

        [Fact(Skip = "Requires an updated version of System.Reflection.Metadata")]
        public void GenericWithGenericMember()
        {
            CompareDependencies(TestAssembly.GenericClassWithGenericMethod, GenericWithGenericMemberDocId());
        }

        [Fact(Skip = "Requires an updated version of System.Reflection.Metadata")]
        public void MoreThan9GenericParams()
        {
            const string expected = "M:Microsoft.Fx.Portability.MetadataReader.Tests.Class_10_generic_params`10.InnerClass.#ctor(Microsoft.Fx.Portability.MetadataReader.Tests.Class{`0,`1,`2,`3,`4,`5,`6,`7,`8,`9},`2)";

            CompareSpecificDependency(TestAssembly.MoreThan9GenericParams, expected);
        }

        [Fact(Skip = "Requires an updated version of System.Reflection.Metadata")]
        public void OpImplicit()
        {
            const string expected = "M:Microsoft.Fx.Portability.MetadataReader.Tests.Class2_OpImplicit`1.op_Implicit(Microsoft.Fx.Portability.MetadataReader.Tests.Class2_OpImplicit{`0})~Microsoft.Fx.Portability.MetadataReader.Tests.Class1_OpImplicit{`0}";

            CompareSpecificDependency(TestAssembly.OpImplicit, expected);
        }

        [Fact(Skip = "Requires an updated version of System.Reflection.Metadata")]
        public void OpExplicit()
        {
            const string expected = "M:Microsoft.Fx.Portability.MetadataReader.Tests.Class2_OpExplicit`1.op_Explicit(Microsoft.Fx.Portability.MetadataReader.Tests.Class2_OpExplicit{`0})~Microsoft.Fx.Portability.MetadataReader.Tests.Class1_OpExplicit{`0}";

            CompareSpecificDependency(TestAssembly.OpExplicit, expected);
        }

        private void CompareSpecificDependency(string opImplicit, string v)
        {
            var dependencyFinder = new ReflectionMetadataDependencyFinder();
            var assemblyToTestFileInfo = new FileInfo(TestAssembly.EmptyProject);
            var progressReporter = Substitute.For<IProgressReporter>();

            var dependencies = dependencyFinder.FindDependencies(new[] { assemblyToTestFileInfo }, progressReporter);

            var found = dependencies.Dependencies
                .Any(d => string.Equals(d.Key.MemberDocId, v, StringComparison.Ordinal));

            Assert.True(found, $"Could not find docid '{v}'");
        }

        private void CompareDependencies(string path, IEnumerable<Tuple<string, int>> expected)
        {
            var dependencyFinder = new ReflectionMetadataDependencyFinder();
            var assemblyToTestFileInfo = new FileInfo(path);
            var progressReporter = Substitute.For<IProgressReporter>();

            var dependencies = dependencyFinder.FindDependencies(new[] { assemblyToTestFileInfo }, progressReporter);

            var foundDocIds = dependencies
                .Dependencies
                .Select(o => Tuple.Create(o.Key.MemberDocId, o.Value.Count))
                .OrderBy(o => o.Item1, StringComparer.Ordinal)
                .ToList();

            var expectedOrdered = expected
                .OrderBy(o => o.Item1, StringComparer.Ordinal)
                .ToList();

            Assert.Equal(expectedOrdered, foundDocIds);
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

        private static IEnumerable<Tuple<string, int>> GenericWithGenericMemberDocId()
        {
            yield return Tuple.Create("M:System.Reflection.AssemblyConfigurationAttribute.#ctor(System.String)", 1);
            yield return Tuple.Create("T:System.Reflection.AssemblyProductAttribute", 1);
            yield return Tuple.Create("M:System.Diagnostics.DebuggableAttribute.#ctor(System.Diagnostics.DebuggableAttribute.DebuggingModes)", 1);
            yield return Tuple.Create("M:System.Runtime.CompilerServices.RuntimeCompatibilityAttribute.#ctor", 1);
            yield return Tuple.Create("T:System.Runtime.InteropServices.ComVisibleAttribute", 1);
            yield return Tuple.Create("M:System.Reflection.AssemblyTrademarkAttribute.#ctor(System.String)", 1);
            yield return Tuple.Create("T:System.Reflection.AssemblyCopyrightAttribute", 1);
            yield return Tuple.Create("M:System.Reflection.AssemblyTitleAttribute.#ctor(System.String)", 1);
            yield return Tuple.Create("T:System.Diagnostics.DebuggableAttribute.DebuggingModes", 1);
            yield return Tuple.Create("M:System.Reflection.AssemblyCopyrightAttribute.#ctor(System.String)", 1);
            yield return Tuple.Create("T:System.Object", 1);
            yield return Tuple.Create("T:System.Runtime.CompilerServices.RuntimeCompatibilityAttribute", 1);
            yield return Tuple.Create("M:System.Reflection.AssemblyVersionAttribute.#ctor(System.String)", 1);
            yield return Tuple.Create("M:System.Runtime.InteropServices.GuidAttribute.#ctor(System.String)", 1);
            yield return Tuple.Create("M:System.Reflection.AssemblyDescriptionAttribute.#ctor(System.String)", 1);
            yield return Tuple.Create("T:System.Reflection.AssemblyDescriptionAttribute", 1);
            yield return Tuple.Create("T:System.Reflection.AssemblyCompanyAttribute", 1);
            yield return Tuple.Create("M:ConsoleApplication2.GenericClass`1.MemberWithDifferentGeneric``1(``0)", 1);
            yield return Tuple.Create("M:System.Runtime.CompilerServices.CompilationRelaxationsAttribute.#ctor(System.Int32)", 1);
            yield return Tuple.Create("M:System.Reflection.AssemblyFileVersionAttribute.#ctor(System.String)", 1);
            yield return Tuple.Create("T:System.Reflection.AssemblyFileVersionAttribute", 1);
            yield return Tuple.Create("T:ConsoleApplication2.GenericClass`1", 1);
            yield return Tuple.Create("T:System.Reflection.AssemblyTitleAttribute", 1);
            yield return Tuple.Create("T:System.Runtime.CompilerServices.CompilationRelaxationsAttribute", 1);
            yield return Tuple.Create("T:System.Runtime.InteropServices.GuidAttribute", 1);
            yield return Tuple.Create("T:System.Diagnostics.DebuggableAttribute", 1);
            yield return Tuple.Create("T:System.Reflection.AssemblyTrademarkAttribute", 1);
            yield return Tuple.Create("M:System.Runtime.InteropServices.ComVisibleAttribute.#ctor(System.Boolean)", 1);
            yield return Tuple.Create("M:System.Reflection.AssemblyCompanyAttribute.#ctor(System.String)", 1);
            yield return Tuple.Create("T:System.Reflection.AssemblyConfigurationAttribute", 1);
            yield return Tuple.Create("T:System.Reflection.AssemblyVersionAttribute", 1);
            yield return Tuple.Create("M:System.Reflection.AssemblyCultureAttribute.#ctor(System.String)", 1);
            yield return Tuple.Create("T:System.Reflection.AssemblyCultureAttribute", 1);
            yield return Tuple.Create("T:System.Runtime.Versioning.TargetFrameworkAttribute", 1);
            yield return Tuple.Create("M:System.Runtime.Versioning.TargetFrameworkAttribute.#ctor(System.String)", 1);
            yield return Tuple.Create("M:System.Reflection.AssemblyProductAttribute.#ctor(System.String)", 1);
            yield return Tuple.Create("M:System.Object.#ctor", 1);
        }
    }
}
