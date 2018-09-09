// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Fx.Portability.Cci.Tests
{
    public class ManagedMetadataReaderTests
    {
        private static readonly string s_emptyProjectPath = TestAssembly.EmptyProject;
        private static readonly string s_withGenericsAndReferencePath = TestAssembly.WithGenericsAndReference;

        [Fact]
        public static void EmptyProject()
        {
            CompareFinders(s_emptyProjectPath);
        }

        [Fact]
        public static void WithGenericsAndReference()
        {
            CompareFinders(s_withGenericsAndReferencePath);
        }

        [Fact]
        public static void WithGenericsAndReferenceAndEmptyProject()
        {
            CompareFinders(s_withGenericsAndReferencePath, s_emptyProjectPath);
        }

        private static void CompareFinders(params string[] paths)
        {
            CompareFinders((IEnumerable<string>)paths);
        }

        private static void CompareFinders(IEnumerable<string> paths)
        {
            // CompareFinders(new ManagedMetadataReaderDependencyFinder(), new CciDependencyFinder(), paths);
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

            Assert.Equal(enumerable1.ToList(), enumerable2.ToList());
        }

        private static void CompareDictionary<TKey, TValue>(IDictionary<TKey, ICollection<TValue>> dictionary1, IDictionary<TKey, ICollection<TValue>> dictionary2)
        {
            Assert.Equal(dictionary1.Keys.ToList(), dictionary2.Keys.ToList());

            foreach (var key in dictionary1.Keys)
            {
                Assert.Equal(dictionary1[key].ToList(), dictionary2[key].ToList());
            }
        }
    }
}
