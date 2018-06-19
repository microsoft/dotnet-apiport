// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Cache;
using Microsoft.Fx.Portability.ObjectModel;
using NSubstitute;
using PortabilityService.AnalysisService.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PortabilityService.AnalysisService.Tests
{
    public class ReadyControllerTests
    {
        [Fact]
        public void ReadyWhenCatalogAndGitHubDataReady()
        {
            var apiPortData = new ApiPortData
            {
                BreakingChangesDictionary = new Dictionary<string, ICollection<BreakingChange>>
                {
                    ["foo"] = new List<BreakingChange>(new[] { new BreakingChange() })
                },
                RecommendedChanges = new Dictionary<string, string> { ["a"] = "b" }
            };

            var catalogCache = TestCatalogCache();
            var controller = new ReadyController(apiPortData, catalogCache);

            var response = controller.Ready();

            Assert.IsType<OkResult>(response);
        }

        [Fact]
        public void NotReadyWhenCatalogNotReady()
        {
            var apiPortData = new ApiPortData
            {
                BreakingChangesDictionary = new Dictionary<string, ICollection<BreakingChange>>
                {
                    ["foo"] = new List<BreakingChange>(new[] { new BreakingChange() })
                },
                RecommendedChanges = new Dictionary<string, string> { ["a"] = "b" }
            };
            var catalogCache = TestCatalogCache(true);
            var controller = new ReadyController(apiPortData, catalogCache);

            var response = controller.Ready();

            Assert.IsType<StatusCodeResult>(response);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, (response as StatusCodeResult).StatusCode);
        }

        [Fact]
        public void NotReadyWhenNoBreakingChanges()
        {
            var apiPortData = new ApiPortData
            {
                BreakingChangesDictionary = new Dictionary<string, ICollection<BreakingChange>>(),
                RecommendedChanges = new Dictionary<string, string> { ["a"] = "b" }
            };

            var catalogCache = TestCatalogCache();
            var controller = new ReadyController(apiPortData, catalogCache);

            var response = controller.Ready();

            Assert.IsType<StatusCodeResult>(response);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, (response as StatusCodeResult).StatusCode);
        }

        [Fact]
        public void NotReadyWhenNoRecommendedChanges()
        {
            var apiPortData = new ApiPortData
            {
                BreakingChangesDictionary = new Dictionary<string, ICollection<BreakingChange>>
                {
                    ["foo"] = new List<BreakingChange>(new[] { new BreakingChange() })
                },
                RecommendedChanges = new Dictionary<string, string>()
            };

            var catalogCache = TestCatalogCache();
            var controller = new ReadyController(apiPortData, catalogCache);

            var response = controller.Ready();

            Assert.IsType<StatusCodeResult>(response);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, (response as StatusCodeResult).StatusCode);
        }

        private static IObjectCache<CatalogIndex> TestCatalogCache(bool empty = false)
        {
            var catalog = Substitute.For<IApiCatalogLookup>();
            var docIds = empty ? Enumerable.Empty<string>() : new[] { "T:a.doc.id", "T:a.nother.doc.id" };
            catalog.DocIds.Returns(docIds);
            var catalogIndex = new CatalogIndex(catalog, Substitute.For<ISearcher<string>>());
            var catalogCache = Substitute.For<IObjectCache<CatalogIndex>>();
            catalogCache.Value.Returns(catalogIndex);
            catalogCache.LastUpdated.Returns(DateTimeOffset.Now);

            return catalogCache;
        }
    }
}
