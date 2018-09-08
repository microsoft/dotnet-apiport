// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Fx.Portability.Offline.Tests
{
    public class OffLineApiPortServiceTests
    {
        private const string ValidDocId = "validDocId";
        private const string InvalidDocId = "invalidDocId";
        private const string ParamText = "(int param1, EventArgs[] args)";
        private const int MaxDocIdSetCount = 3;

        private readonly OfflineApiPortService _OfflineApiPortService = new OfflineApiPortService(
            CreateApiCatalogLookup(),
            CreateRequestAnalyzer(),
            CreateTargetMapper(),
            CreateCollectionOfReportWriters(),
            CreateTargetNameParser(),
            CreateApiRecommendations());

        [Fact]
        public async Task QueryDocIdsWithNullThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _OfflineApiPortService.QueryDocIdsAsync(null));
        }

        [Fact]
        public async Task QueryDocIdsWithEmptyListOfValidIds()
        {
            var expectedDocIds = new List<string>();

            var result = await _OfflineApiPortService.QueryDocIdsAsync(expectedDocIds);
            Assert.Empty(result.Response);
        }

        [Fact]
        public async Task QueryDocIdsWithAllValidIds()
        {
            var expectedDocIds = new List<string>
            {
                $"T:{ValidDocId}0",
                $"M:{ValidDocId}0",
                $"P:{ValidDocId}0",
                $"T:{ValidDocId}{ParamText}",
                $"M:{ValidDocId}{ParamText}"
            };

            var result = await _OfflineApiPortService.QueryDocIdsAsync(expectedDocIds);
            Assert.Equal(expectedDocIds.Count, result.Response.Count);
            Assert.Empty(expectedDocIds.Except(result.Response.Select(r => r.Definition.DocId)));
        }

        [Fact]
        public async Task QueryDocIdsWithParameterDocIds()
        {
            var expectedDocIds = new List<string>
            {
                $"M:{ValidDocId}0",
                $"T:{ValidDocId}0",
            };

            var result = await _OfflineApiPortService.QueryDocIdsAsync(expectedDocIds);
            Assert.Equal(expectedDocIds.Count, result.Response.Count);
            Assert.Empty(expectedDocIds.Except(result.Response.Select(r => r.Definition.DocId)));
        }

        [Fact]
        public async Task QueryDocIdsWithNoValidIds()
        {
            var expectedDocIds = new List<string>
            {
                $"{InvalidDocId}{MaxDocIdSetCount + 1}",
                $"{InvalidDocId}{MaxDocIdSetCount + 2}",
                $"{InvalidDocId}{MaxDocIdSetCount + 3}"
            };

            var result = await _OfflineApiPortService.QueryDocIdsAsync(expectedDocIds);
            Assert.Empty(result.Response);
        }

        [Fact]
        public async Task QueryDocIdsWithSomeValidAndNonValidIds()
        {
            var docIdsToPass = new List<string>
            {
                $"T:{ValidDocId}0",
                $"M:{ValidDocId}1",
                $"P:{ValidDocId}0",
                $"{InvalidDocId}{MaxDocIdSetCount + 1}",
                $"{InvalidDocId}{MaxDocIdSetCount + 2}",
                $"{InvalidDocId}{MaxDocIdSetCount + 3}"
            };

            var expectedDocIds = new List<string>
            {
                $"T:{ValidDocId}0",
                $"M:{ValidDocId}1",
                $"P:{ValidDocId}0"
            };

            var result = await _OfflineApiPortService.QueryDocIdsAsync(docIdsToPass);
            Assert.Equal(expectedDocIds.Count, result.Response.Count);
            Assert.Empty(expectedDocIds.Except(result.Response.Select(d => d.Definition.DocId)));
        }

        [Fact]
        public async Task QueryDocIdsWithOneItemEmptyString()
        {
            var docIdsToPass = new List<string>
            {
                $"T:{ValidDocId}0",
                $"T:{ValidDocId}1",
                $"P:{ValidDocId}1",
                "",
                $"{InvalidDocId}{MaxDocIdSetCount + 1}",
                $"{InvalidDocId}{MaxDocIdSetCount + 2}",
                $"{InvalidDocId}{MaxDocIdSetCount + 3}",
            };

            var expectedDocIds = new List<string>
            {
                $"T:{ValidDocId}0",
                $"T:{ValidDocId}1",
                $"P:{ValidDocId}1"
            };

            var result = await _OfflineApiPortService.QueryDocIdsAsync(docIdsToPass);
            Assert.Equal(expectedDocIds.Count, result.Response.Count);
            Assert.Empty(expectedDocIds.Except(result.Response.Select(d => d.Definition.DocId)));
        }

        [Fact]
        public async Task QueryDocIdsWithDifferentDocIDCassingAreInvalid()
        {
            var expectedDocIds = new List<string>
            {
                $"T:{ValidDocId.ToUpper(CultureInfo.InvariantCulture)}0",
                $"M:{ValidDocId.ToUpper(CultureInfo.InvariantCulture)}1",
                $"P:{ValidDocId}0"
            };

            var result = await _OfflineApiPortService.QueryDocIdsAsync(expectedDocIds);
            Assert.Equal(1, result.Response.Count);
            Assert.Equal($"P:{ValidDocId}0", result.Response.First().Definition.DocId);
        }

        [Fact]
        public async Task QueryDocIdsWithSomeDuplicateValidIds()
        {
            var docIdsToPass = new List<string>
            {
                $"T:{ValidDocId}0",
                $"T:{ValidDocId}1",
                $"M:{ValidDocId}0",
                $"M:{ValidDocId}0",
                $"P:{ValidDocId}0",
                $"P:{ValidDocId}1",
                $"P:{ValidDocId}1"
            };

            var expectedDocIds = new List<string>
            {
                 $"T:{ValidDocId}0",
                 $"T:{ValidDocId}1",
                 $"M:{ValidDocId}0",
                 $"P:{ValidDocId}0",
                 $"P:{ValidDocId}1",
            };

            var result = await _OfflineApiPortService.QueryDocIdsAsync(docIdsToPass);
            Assert.Equal(expectedDocIds.Count, result.Response.Count);
            Assert.Empty(expectedDocIds.Except(result.Response.Select(r => r.Definition.DocId)));
        }

        private static IRequestAnalyzer CreateRequestAnalyzer()
        {
            return Substitute.For<IRequestAnalyzer>();
        }

        private static ITargetMapper CreateTargetMapper()
        {
            return Substitute.For<ITargetMapper>();
        }

        private static ICollection<IReportWriter> CreateCollectionOfReportWriters()
        {
            return Substitute.For<ICollection<IReportWriter>>();
        }

        private static ITargetNameParser CreateTargetNameParser()
        {
            return Substitute.For<ITargetNameParser>();
        }

        private static IApiRecommendations CreateApiRecommendations()
        {
            return Substitute.For<IApiRecommendations>();
        }

        /// <summary>
        /// Creates a test IApiCatelog with some framework DocIds
        /// </summary>
        private static IApiCatalogLookup CreateApiCatalogLookup()
        {
            var catalog = Substitute.For<IApiCatalogLookup>();

            // Add some different types of DocIds
            AddDocIdsForType("T", catalog);
            AddDocIdsForType("P", catalog);
            AddDocIdsForType("M", catalog);
            AddDocIdsForType("E", catalog);

            // Add some different type of DocIds with Parameters
            AddDocIdWithParameter("M", catalog);
            AddDocIdWithParameter("T", catalog);

            return catalog;
        }

        /// <summary>
        /// Adds a DocId to the catalog
        /// </summary>
        /// <param name="docIDType">Type of Id to add (M, T, P, E, etc)</param>
        /// <param name="catalog">The catalog to add to</param>
        private static void AddDocIdsForType(string docIDType, IApiCatalogLookup catalog)
        {
            for (int iCounter = 0; iCounter < MaxDocIdSetCount; iCounter++)
            {
                var docId = $"{docIDType}:{ValidDocId}{iCounter}";
                catalog.IsFrameworkMember(docId).Returns(r => true);
                catalog.GetApiDefinition(docId).Returns(r => new ApiDefinition { DocId = docId });
            }
        }

        /// <summary>
        /// Adds a DocId with a parameter to the catalog
        /// </summary>
        /// <param name="docIDType">Type of ID to add (M, T, P, E, etc)</param>
        /// <param name="catalog">The catalog to add to</param>
        private static void AddDocIdWithParameter(string docIDType, IApiCatalogLookup catalog)
        {
            var docId = $"{docIDType}:{ValidDocId}{ParamText}";
            catalog.IsFrameworkMember(docId).Returns(r => true);
            catalog.GetApiDefinition(docId).Returns(r => new ApiDefinition { DocId = docId });
        }
    }
}
