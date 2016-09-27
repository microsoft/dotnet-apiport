// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Fx.Portability.Tests.ObjectModel
{
    public class AncestorApiRecommendationsTests
    {
        private const string DefaultRecommendedAction = "";

        /// <summary>
        /// Tests that the parent recommended change can be obtained if the docId's does not.
        /// </summary>
        [Fact]
        public void GetRecommendedChange_Parent()
        {
            var docId = "Property:Foo";
            var matchingParentDocId = "Class:Test";
            var ancestors = new[] { matchingParentDocId, "Namespace:MyTestNamespace" };
            var expectedAction = "This is the recommended action for Class:Test";

            var catalog = Substitute.For<IApiCatalogLookup>();
            var recommendations = AncestorApiRecommendations.Create(catalog);

            catalog.GetAncestors(docId).Returns(ancestors);
            catalog.GetRecommendedChange(matchingParentDocId).Returns(expectedAction);

            var actual = recommendations.GetRecommendedChanges(docId);

            Assert.Equal(expectedAction, actual);

            catalog.Received(1).GetAncestors(docId);
        }

        /// <summary>
        /// Tests that string.Empty is returned if a recommended change is not found.
        /// </summary>
        [Fact]
        public void GetRecommendedChange_DoesNotExist()
        {
            var docId = "Property:Foo";
            var ancestors = new[] { "Class:Test", "Namespace:MyTestNamespace" };

            var catalog = Substitute.For<IApiCatalogLookup>();
            var recommendations = AncestorApiRecommendations.Create(catalog);

            catalog.GetAncestors(docId).Returns(ancestors);

            var actual = recommendations.GetRecommendedChanges(docId);

            Assert.Equal(DefaultRecommendedAction, actual);

            catalog.Received(1).GetAncestors(docId);
        }

        [Fact]
        public void GetRecommendedChange_NoAncestors()
        {
            var docId = "Namespace:MyTestNamespace";
            var ancestors = new string[0];

            var catalog = Substitute.For<IApiCatalogLookup>();
            var recommendations = AncestorApiRecommendations.Create(catalog);

            catalog.GetAncestors(docId).Returns(ancestors);

            var actual = recommendations.GetRecommendedChanges(docId);

            Assert.Equal(DefaultRecommendedAction, actual);

            catalog.Received(1).GetAncestors(docId);
        }

        /// <summary>
        /// Verify that the first recommended change (closest to the docId is 
        /// returned if we have multiple matching ones).
        /// </summary>
        [Fact]
        public void GetRecommendedChange_FirstMatchingRecommendation()
        {
            var recommendedChanges = new[]
            {
                new { DocId = "Property:Foo", Change = "This is the first recommendation!" },
                new { DocId = "Class:FooTest", Change = "This is the second recommendation!" },
                new { DocId = "Namespace:MyTestNamespace", Change = "This is the 3rd recommendation!" }
            };
            var expected = recommendedChanges[0];

            var ancestors = recommendedChanges.Select(r => r.DocId).ToArray();

            var catalog = Substitute.For<IApiCatalogLookup>();
            var recommendations = AncestorApiRecommendations.Create(catalog);

            catalog.GetAncestors(expected.DocId).Returns(ancestors);

            foreach (var item in recommendedChanges)
            {
                catalog.GetRecommendedChange(item.DocId).Returns(item.Change);
            }

            var actual = recommendations.GetRecommendedChanges(expected.DocId);

            Assert.Equal(expected.Change, actual);
        }

        /// <summary>
        /// Tests that the parent Breaking change can be obtained if the docId's does not.
        /// </summary>
        [Fact]
        public void GetBreakingChange_Parent()
        {
            var docId = "Property:Foo";
            var matchingParentDocId = "Class:Test";
            var ancestors = new[] { matchingParentDocId, "Namespace:MyTestNamespace" };
            var expected = new[] { new BreakingChange { Id = "5" }, new BreakingChange { Id = "7" } };
            var breakingChanges = new Dictionary<string, IEnumerable<BreakingChange>>(StringComparer.Ordinal)
            {
                { docId, null },
                { matchingParentDocId, expected }
            };
            var catalog = Substitute.For<IApiCatalogLookup>();
            var recommendations = BreakingChangeRecommendationsMock.Create(catalog, breakingChanges);

            catalog.GetAncestors(docId).Returns(ancestors);

            var actual = recommendations.GetBreakingChanges(docId);

            Assert.Equal(expected, actual);

            catalog.Received(1).GetAncestors(docId);
        }

        /// <summary>
        /// Tests that string.Empty is returned if a Breaking change is not found.
        /// </summary>
        [Fact]
        public void GetBreakingChange_DoesNotExist()
        {
            var docId = "Property:Foo";
            var ancestors = new[] { "Class:Test", "Namespace:MyTestNamespace" };

            var catalog = Substitute.For<IApiCatalogLookup>();
            var recommendations = BreakingChangeRecommendationsMock.Create(catalog);

            catalog.GetAncestors(docId).Returns(ancestors);

            var actual = recommendations.GetBreakingChanges(docId);

            Assert.Empty(actual);

            catalog.Received(1).GetAncestors(docId);
        }

        [Fact]
        public void GetBreakingChange_NoAncestors()
        {
            var docId = "Namespace:MyTestNamespace";
            var ancestors = new string[0];

            var catalog = Substitute.For<IApiCatalogLookup>();
            var recommendations = BreakingChangeRecommendationsMock.Create(catalog);

            catalog.GetAncestors(docId).Returns(ancestors);

            var actual = recommendations.GetBreakingChanges(docId);

            Assert.Empty(actual);

            catalog.Received(1).GetAncestors(docId);
        }

        /// <summary>
        /// Verify that the first Breaking change (closest to the docId is 
        /// returned if we have multiple matching ones).
        /// </summary>
        [Fact]
        public void GetBreakingChange_FirstMatchingRecommendation()
        {
            var breakingChanges = new Dictionary<string, IEnumerable<BreakingChange>>(StringComparer.Ordinal)
            {
                { "Property:Foo", new[] { new BreakingChange { Id = "5" }, new BreakingChange { Id = "7" } } },
                { "Class:FooTest", new[] { new BreakingChange { Id = "6" }, new BreakingChange { Id = "2" } } },
                { "Namespace:MyTestNamespace", new[] { new BreakingChange { Id = "8" }, new BreakingChange { Id = "17" } } }
            };
            var expected = breakingChanges.First();
            var ancestors = breakingChanges.Keys.ToArray();

            var catalog = Substitute.For<IApiCatalogLookup>();
            var recommendations = BreakingChangeRecommendationsMock.Create(catalog, breakingChanges);

            catalog.GetAncestors(expected.Key).Returns(ancestors);

            var actual = recommendations.GetBreakingChanges(expected.Key);

            Assert.Equal(expected.Value, actual);
        }

        /// <summary>
        /// Provides an easy mock to inject breaking changes into AncestorApiRecommendations
        /// </summary>
        private class BreakingChangeRecommendationsMock : AncestorApiRecommendations
        {
            private readonly IDictionary<string, IEnumerable<BreakingChange>> _breakingChanges;

            private BreakingChangeRecommendationsMock(IApiCatalogLookup catalog, IDictionary<string, IEnumerable<BreakingChange>> breakingChanges)
                : base(catalog)
            {
                _breakingChanges = breakingChanges;
            }

            public static IApiRecommendations Create(IApiCatalogLookup catalog, IDictionary<string, IEnumerable<BreakingChange>> breakingChanges = null)
            {
                return new BreakingChangeRecommendationsMock(catalog, breakingChanges ?? new Dictionary<string, IEnumerable<BreakingChange>>());
            }

            protected override IEnumerable<BreakingChange> GetBreakingChanges(string docId)
            {
                IEnumerable<BreakingChange> result;

                if (_breakingChanges.TryGetValue(docId, out result))
                {
                    return result;
                }
                else
                {
                    return Enumerable.Empty<BreakingChange>();
                }
            }
        }
    }
}
