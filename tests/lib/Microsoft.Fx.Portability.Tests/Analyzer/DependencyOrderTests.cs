// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using NSubstitute;
using Xunit;

namespace Microsoft.Fx.Portability.Analyzer.Tests
{
    public class DependencyOrderTests
    {
        [Fact]
        public void NoEntrypoint()
        {
            var filter = Substitute.For<IDependencyFilter>();
            var orderer = new DependencyOrderer(filter);
            var result = orderer.GetOrder(null, new[] { new AssemblyInfo() });

            Assert.Empty(result);
        }

        [Fact]
        public void AssembliesFiltered()
        {
            var filter = Substitute.For<IDependencyFilter>();
            var orderer = new DependencyOrderer(filter);
            var test1 = new AssemblyInfo { AssemblyIdentity = "Test1" };
            var test2 = new AssemblyInfo { AssemblyIdentity = "Test2" };
            var entryPoint = new AssemblyInfo
            {
                AssemblyIdentity = "Test",
                AssemblyReferences = new[]
                {
                    new AssemblyReferenceInformation(test1),
                    new AssemblyReferenceInformation(test2)
                }
            };

            filter.IsFrameworkAssembly("Test1", Arg.Any<PublicKeyToken>()).Returns(true);

            var result = orderer.GetOrder(entryPoint, new[] { entryPoint, test1, test2 });

            Assert.Collection(result,
                t => Assert.Same(test2, t),
                t => Assert.Same(entryPoint, t));
        }

        /// <summary>
        /// Tests the following graph.
        ///
        /// <![CDATA[
        ///      +---+
        ///      | A |
        ///      +---+
        ///        |
        ///        |
        ///        v
        ///      +---+
        ///      | B |
        ///      +---+
        ///        |
        ///   +----+----+
        ///   |         |
        ///   v         v
        /// +---+     +---+
        /// | C | --> | D |
        /// +---+     +---+
        ///   |         |
        ///   |         |
        ///   |         v
        ///   |       +---+
        ///   ------> | E |
        ///           +---+
        /// ]]>
        /// </summary>
        [Fact]
        public void DeepDependency()
        {
            var filter = Substitute.For<IDependencyFilter>();
            var orderer = new DependencyOrderer(filter);
            var e = new AssemblyInfo { AssemblyIdentity = "e" };
            var d = new AssemblyInfo
            {
                AssemblyIdentity = "d",
                AssemblyReferences = new[] { new AssemblyReferenceInformation(e) }
            };
            var c = new AssemblyInfo
            {
                AssemblyIdentity = "c",
                AssemblyReferences = new[]
                {
                    new AssemblyReferenceInformation(e),
                    new AssemblyReferenceInformation(d),
                }
            };
            var b = new AssemblyInfo
            {
                AssemblyIdentity = "b",
                AssemblyReferences = new[]
                {
                    new AssemblyReferenceInformation(c),
                    new AssemblyReferenceInformation(d),
                }
            };
            var a = new AssemblyInfo
            {
                AssemblyIdentity = "a",
                AssemblyReferences = new[]
                {
                    new AssemblyReferenceInformation(b),
                }
            };

            var result = orderer.GetOrder(a, new[] { a, b, c, d, e });

            Assert.Collection(result,
                t => Assert.Same(e, t),
                t => Assert.Same(d, t),
                t => Assert.Same(c, t),
                t => Assert.Same(b, t),
                t => Assert.Same(a, t));
        }
    }
}
