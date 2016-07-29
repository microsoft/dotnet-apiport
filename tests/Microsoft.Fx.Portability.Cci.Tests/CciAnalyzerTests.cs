using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Microsoft.Fx.Portability.Cci.Tests
{
    [TestClass]
    public class CciAnalyzerTests
    {
        [TestMethod]
        public void FindDependencies()
        {
            var cci = new CciDependencyFinder();
            var path = new TestAssemblyFile(TestAssembly.EmptyProject);
            var progressReporter = Substitute.For<IProgressReporter>();

            var dependencies = cci.FindDependencies(new[] { path }, progressReporter);

            var foundDocIds = dependencies.Dependencies.Select(o => Tuple.Create(o.Key.MemberDocId, o.Value.Count)).ToList();
            CollectionAssert.AreEquivalent(EmptyProjectMemberDocId().ToList(), foundDocIds);
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
