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
        [InlineData("Arglist.cs", "M:TestClass.ArglistMethod(System.Int32,__arglist)")]
        [InlineData("Arglist.cs", "M:TestClass.ArglistMethod2(__arglist)")]
        [InlineData("GenericClassMemberWithDifferentGeneric.cs", "M:Microsoft.Fx.Portability.MetadataReader.Tests.Tests.GenericClass`1.MemberWithDifferentGeneric``1(``0)")]
        [InlineData("10-generic-params.cs", "M:Microsoft.Fx.Portability.MetadataReader.Tests.Tests.Microsoft.Fx.Portability.MetadataReader.Tests.Class_10_generic_params`10.InnerClass.#ctor(Microsoft.Fx.Portability.MetadataReader.Tests.Tests.Microsoft.Fx.Portability.MetadataReader.Tests.Class_10_generic_params{`0,`1,`2,`3,`4,`5,`6,`7,`8,`9},`2)")]
        [InlineData("OpImplicit.cs", "M:Microsoft.Fx.Portability.MetadataReader.Tests.Class2_OpImplicit`1.op_Implicit(Microsoft.Fx.Portability.MetadataReader.Tests.Class2_OpImplicit{`0})~Microsoft.Fx.Portability.MetadataReader.Tests.Class1_OpImplicit{`0}")]
        [InlineData("OpImplicitMethod.cs", "M:Microsoft.Fx.Portability.MetadataReader.Tests.OpImplicit_Method`1.op_Implicit(`0)~System.Int32")]
        [InlineData("OpImplicitMethod2Parameter.cs", "M:Microsoft.Fx.Portability.MetadataReader.Tests.OpImplicit_Method_2Parameter`1.op_Implicit(`0,`0)")]
        [InlineData("OpExplicit.cs", "M:Microsoft.Fx.Portability.MetadataReader.Tests.Class2_OpExplicit`1.op_Explicit(Microsoft.Fx.Portability.MetadataReader.Tests.Class2_OpExplicit{`0})~Microsoft.Fx.Portability.MetadataReader.Tests.Class1_OpExplicit{`0}")]
        [InlineData("NestedGenericTypesWithInvalidNames.cs", "M:Microsoft.Fx.Portability.MetadataReader.Tests.OtherClass.<GetValues>d__0`1.System#Collections#Generic#IEnumerable{System#Tuple{T@System#Int32}}#GetEnumerator")]
        [InlineData("modopt.dll", "M:TestClass.Foo(System.Int32 optmod System.Runtime.CompilerServices.IsConst)")]
        [InlineData("modopt.dll", "M:TestClass.Bar(System.SByte optmod System.Runtime.CompilerServices.IsConst reqmod System.Runtime.CompilerServices.IsSignUnspecifiedByte*)")]
        [InlineData("NestedGenericTypes.cs", "M:OuterClass`2.InnerClass`2.InnerInnerClass.InnerInnerMethod(OuterClass{`3,`2}.InnerClass{System.Int32,`0}.InnerInnerClass)")]
        [InlineData("NestedGenericTypes.cs", "M:OuterClass`2.InnerClass`2.InnerMethod(OuterClass{`2,`2}.InnerClass{`1,`1})")]
        [InlineData("NestedGenericTypes.cs", "M:OuterClass`2.OuterMethod(`0,OuterClass{`1,`0}.InnerClass{`1,`0})")]

        // IL can, bizarrely, define non-generic types that take generic paratmers
        [InlineData("NonGenericTypesWithGenericParameters.dll", "M:OuterClass.InnerClass.InnerMethod(OuterClass.InnerClass{`2,`2})")]
        [InlineData("NonGenericTypesWithGenericParameters.dll", "M:OuterClass.OuterMethod(`0,OuterClass.InnerClass{`1,`0,System.Object,`0})")]

        // The IL version of this test includes a nested generic type in which the outer type is closed by the inner one is open
        // This is not possible to construct in C#, but was being encoded incorrectly by the metadata reader parser.
        [InlineData("NestedGenericTypes.dll", "M:OuterClass`2.InnerClass`2.InnerMethod(OuterClass{`2,`2}.InnerClass`2)")]
        [InlineData("NestedGenericTypes.dll", "M:OuterClass`2.OuterMethod(`0,OuterClass{`1,`0}.InnerClass{`1,`0})")]

        [Theory]
        public void TestForDocId(string source, string docid)
        {
            TestForDocId(source, docid, false);
        }

        private void TestForDocId(string source, string docid, bool allowUnsafe)
        {
            var assembly = TestAssembly.Create(source, allowUnsafe);

            var dependencyFinder = new ReflectionMetadataDependencyFinder();
            var assemblyToTestFileInfo = new FileInfo(assembly.AssemblyPath);
            var progressReporter = Substitute.For<IProgressReporter>();

            var dependencies = dependencyFinder.FindDependencies(new[] { assemblyToTestFileInfo }, progressReporter);

            foreach (var dependency in dependencies.Dependencies)
            {
                if (string.Equals(dependency.Key.MemberDocId, docid, StringComparison.Ordinal))
                {
                    return;
                }
            }

            Assert.True(false, $"Could not find docid '{docid}'");
        }

        [Fact]
        public void EmptyProject()
        {
            var test = TestAssembly.Create("EmptyProject.cs");
            CompareDependencies(test.AssemblyPath, EmptyProjectMemberDocId());
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

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var item in foundDocIds)
            {
                sb.AppendLine(string.Format("yield return Tuple.Create(\"{0}\", 1);", item.Item1));
            }

            Assert.Equal(expectedOrdered.Count, foundDocIds.Count);

            foreach (var combined in expectedOrdered.Zip(foundDocIds, Tuple.Create))
            {
                var expectedItem = combined.Item1;
                var actualItem = combined.Item2;

                Assert.Equal(expectedItem.Item1, actualItem.Item1);
                Assert.Equal(expectedItem.Item2, actualItem.Item2);
            }
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
