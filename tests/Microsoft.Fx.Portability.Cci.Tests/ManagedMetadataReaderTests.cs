using System.Collections.Generic;
using System.Linq;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Microsoft.Fx.Portability.Cci.Tests
{
    [TestClass]
    public class ManagedMetadataReaderTests
    {
        private readonly static string EmptyProjectPath = TestAssembly.EmptyProject;
        private readonly static string WithGenericsAndReferencePath = TestAssembly.WithGenericsAndReference;

        [TestMethod]
        public void EmptyProject()
        {
            CompareFinders(EmptyProjectPath);
        }

        [TestMethod]
        public void WithGenericsAndReference()
        {
            CompareFinders(WithGenericsAndReferencePath);
        }

        [TestMethod]
        public void WithGenericsAndReferenceAndEmptyProject()
        {
            CompareFinders(WithGenericsAndReferencePath, EmptyProjectPath);
        }

        private static void CompareFinders(params string[] paths)
        {
            CompareFinders((IEnumerable<string>)paths);
        }

        private static void CompareFinders(IEnumerable<string> paths)
        {
            //CompareFinders(new ManagedMetadataReaderDependencyFinder(), new CciDependencyFinder(), paths);
        }

        private static void CompareFinders(IDependencyFinder finder1, IDependencyFinder finder2, IEnumerable<string> paths)
        {
            var fi = paths.Select(p => new TestAssemblyFile(p));
            var progressReporter = Substitute.For<IProgressReporter>();
            var dependencies1 = finder1.FindDependencies(fi, progressReporter);
            var dependencies2 = finder2.FindDependencies(fi, progressReporter);

            CompareEnumerable(dependencies1.AssembliesWithErrors, dependencies2.AssembliesWithErrors);
            CompareEnumerable(dependencies1.UserAssemblies, dependencies2.UserAssemblies);

            CompareDictionary(dependencies1.UnresolvedAssemblies, dependencies2.UnresolvedAssemblies);
            CompareDictionary(dependencies1.Dependencies, dependencies2.Dependencies);
        }

        private static void CompareEnumerable<T>(IEnumerable<T> enumerable1, IEnumerable<T> enumerable2)
        {
            var f1 = enumerable1.FirstOrDefault();
            var f2 = enumerable2.FirstOrDefault();

            var b = Equals(f1, f2);

            CollectionAssert.AreEquivalent(enumerable1.ToList(), enumerable2.ToList());
        }

        private static void CompareDictionary<TKey, TValue>(IDictionary<TKey, ICollection<TValue>> dictionary1, IDictionary<TKey, ICollection<TValue>> dictionary2)
        {
            CollectionAssert.AreEquivalent(dictionary1.Keys.ToList(), dictionary2.Keys.ToList());

            foreach (var key in dictionary1.Keys)
            {
                CollectionAssert.AreEquivalent(dictionary1[key].ToList(), dictionary2[key].ToList());
            }
        }
    }
}
