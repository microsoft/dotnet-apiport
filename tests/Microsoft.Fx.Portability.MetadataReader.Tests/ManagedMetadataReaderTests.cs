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

        [Fact]
        public void NestedGenericTypes()
        {
            CompareDependencies(TestAssembly.NestedGenericTypes, NestedGenericTypesMemberDocId());
        }

        [Fact]
        // The IL version of this test includes a nested generic type in which the outer type is closed by the inner one is open
        // This is not possible to construct in C#, but was being encoded incorrectly by the metadata reader parser.
        public void NestedGenericTypesFromIL()
        {
            CompareDependencies(TestAssembly.NestedGenericTypesFromIL, NestedGenericTypesFromILMemberDocId());
        }

        [Fact]
        // IL can, bizarrely, define non-generic types that take generic paratmers
        public void NonGenericTypesWithGenericParametersFromIL()
        {
            CompareDependencies(TestAssembly.NonGenericTypesWithGenericParametersFromIL, NonGenericTypesWithGenericParametersFromILLMemberDocId());
        }

        [Fact]
        public void ModsFromIL()
        {
            const string expected1 = "M:TestClass.Foo(System.Int32 optmod System.Runtime.CompilerServices.IsConst)";
            const string expected2 = "M:TestClass.Bar(System.SByte optmod System.Runtime.CompilerServices.IsConst reqmod System.Runtime.CompilerServices.IsSignUnspecifiedByte*)";
            CompareSpecificDependency(TestAssembly.ModsFromIL, expected1);
            CompareSpecificDependency(TestAssembly.ModsFromIL, expected2);
        }

        [Fact]
        public void Arglist()
        {
            const string expected = "M:TestClass.ArglistMethod(System.Int32,__arglist)";
            const string expected2 = "M:TestClass.ArglistMethod2(__arglist)";
            CompareSpecificDependency(TestAssembly.Arglist, expected);
            CompareSpecificDependency(TestAssembly.Arglist, expected2);
        }

        [Fact]
        public void GenericWithGenericMember()
        {
            const string expected = "M:ConsoleApplication2.GenericClass`1.MemberWithDifferentGeneric``1(``0)";

            CompareSpecificDependency(TestAssembly.GenericClassWithGenericMethod, expected);
        }

        [Fact]
        public void MoreThan9GenericParams()
        {
            const string expected = "M:Microsoft.Fx.Portability.MetadataReader.Tests.Class_10_generic_params`10.InnerClass.#ctor(Microsoft.Fx.Portability.MetadataReader.Tests.Class{`0,`1,`2,`3,`4,`5,`6,`7,`8,`9},`2)";

            CompareSpecificDependency(TestAssembly.MoreThan9GenericParams, expected);
        }

        [Fact]
        public void OpImplicit()
        {
            const string expected = "M:Microsoft.Fx.Portability.MetadataReader.Tests.Class2_OpImplicit`1.op_Implicit(Microsoft.Fx.Portability.MetadataReader.Tests.Class2_OpImplicit{`0})~Microsoft.Fx.Portability.MetadataReader.Tests.Class1_OpImplicit{`0}";

            CompareSpecificDependency(TestAssembly.OpImplicit, expected);
        }

        [Fact]
        public void OpImplicitMethod()
        {
            // This is case where we mark it as a special name when it really isn't. Don't have the info to fix with just member refs
            const string expected = "M:Microsoft.Fx.Portability.MetadataReader.Tests.OpImplicit_Method`1.op_Implicit(`0)~System.Int32";

            CompareSpecificDependency(TestAssembly.OpImplicitMethod, expected);
        }

        [Fact]
        public void OpImplicitMethod2Parameter()
        {
            const string expected = "M:Microsoft.Fx.Portability.MetadataReader.Tests.OpImplicit_Method_2Parameter`1.op_Implicit(`0,`0)";

            CompareSpecificDependency(TestAssembly.OpImplicitMethod2Parameter, expected);
        }

        [Fact]
        public void OpExplicit()
        {
            const string expected = "M:Microsoft.Fx.Portability.MetadataReader.Tests.Class2_OpExplicit`1.op_Explicit(Microsoft.Fx.Portability.MetadataReader.Tests.Class2_OpExplicit{`0})~Microsoft.Fx.Portability.MetadataReader.Tests.Class1_OpExplicit{`0}";

            CompareSpecificDependency(TestAssembly.OpExplicit, expected);
        }

        private void CompareSpecificDependency(string path, string v)
        {
            var dependencyFinder = new ReflectionMetadataDependencyFinder();
            var assemblyToTestFileInfo = new FileInfo(path);
            var progressReporter = Substitute.For<IProgressReporter>();

            var dependencies = dependencyFinder.FindDependencies(new[] { assemblyToTestFileInfo }, progressReporter);

            foreach (var dependency in dependencies.Dependencies)
            {
                if (string.Equals(dependency.Key.MemberDocId, v, StringComparison.Ordinal))
                {
                    return;
                }
            }

            Assert.True(false, $"Could not find docid '{v}'");
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

            Assert.Equal(expectedOrdered.Count, foundDocIds.Count);

            foreach (var combined in expectedOrdered.Zip(foundDocIds, Tuple.Create))
            {
                var expectedItem = combined.Item1;
                var actualItem = combined.Item2;

                Assert.Equal(expectedItem.Item1, actualItem.Item1);
                Assert.Equal(expectedItem.Item2, actualItem.Item2);
            }
        }

        private static IEnumerable<Tuple<string, int>> NonGenericTypesWithGenericParametersFromILLMemberDocId()
        {
            yield return Tuple.Create("M:OuterClass.InnerClass.InnerMethod(OuterClass.InnerClass{`2,`2})", 1);
            yield return Tuple.Create("M:OuterClass.OuterMethod(`0,OuterClass.InnerClass{`1,`0,System.Object,`0})", 1);
            yield return Tuple.Create("T:OuterClass", 1);
            yield return Tuple.Create("T:OuterClass.InnerClass", 1);
            yield return Tuple.Create("T:System.Object", 1);
        }

        private static IEnumerable<Tuple<string, int>> NestedGenericTypesFromILMemberDocId()
        {
            yield return Tuple.Create("M:OuterClass`2.InnerClass`2.InnerMethod(OuterClass{`2,`2}.InnerClass`2)", 1);
            yield return Tuple.Create("M:OuterClass`2.OuterMethod(`0,OuterClass{`1,`0}.InnerClass{`1,`0})", 1);
            yield return Tuple.Create("T:OuterClass`2", 1);
            yield return Tuple.Create("T:OuterClass`2.InnerClass`2", 1);
            yield return Tuple.Create("T:System.Object", 1);
        }

        private static IEnumerable<Tuple<string, int>> NestedGenericTypesMemberDocId()
        {
            yield return Tuple.Create("M:OuterClass`2.InnerClass`2.InnerInnerClass.InnerInnerMethod(OuterClass{`3,`2}.InnerClass{System.Int32,`0}.InnerInnerClass)", 1);
            yield return Tuple.Create("M:OuterClass`2.InnerClass`2.InnerMethod(OuterClass{`2,`2}.InnerClass{`1,`1})", 1);
            yield return Tuple.Create("M:OuterClass`2.OuterMethod(`0,OuterClass{`1,`0}.InnerClass{`1,`0})", 1);
            yield return Tuple.Create("M:System.Diagnostics.DebuggableAttribute.#ctor(System.Diagnostics.DebuggableAttribute.DebuggingModes)", 1);
            yield return Tuple.Create("M:System.Object.#ctor", 1);
            yield return Tuple.Create("M:System.Runtime.CompilerServices.CompilationRelaxationsAttribute.#ctor(System.Int32)", 1);
            yield return Tuple.Create("M:System.Runtime.CompilerServices.RuntimeCompatibilityAttribute.#ctor", 1);
            yield return Tuple.Create("M:System.Runtime.Versioning.TargetFrameworkAttribute.#ctor(System.String)", 1);
            yield return Tuple.Create("T:OuterClass`2", 1);
            yield return Tuple.Create("T:OuterClass`2.InnerClass`2", 1);
            yield return Tuple.Create("T:OuterClass`2.InnerClass`2.InnerInnerClass", 1);
            yield return Tuple.Create("T:System.Diagnostics.DebuggableAttribute", 1);
            yield return Tuple.Create("T:System.Diagnostics.DebuggableAttribute.DebuggingModes", 1);
            yield return Tuple.Create("T:System.Object", 1);
            yield return Tuple.Create("T:System.Runtime.CompilerServices.CompilationRelaxationsAttribute", 1);
            yield return Tuple.Create("T:System.Runtime.CompilerServices.RuntimeCompatibilityAttribute", 1);
            yield return Tuple.Create("T:System.Runtime.Versioning.TargetFrameworkAttribute", 1);
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
