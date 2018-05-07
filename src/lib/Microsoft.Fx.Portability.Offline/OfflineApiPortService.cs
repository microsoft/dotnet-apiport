// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability
{
    public class OfflineApiPortService : IApiPortService
    {
        private readonly ITargetMapper _mapper;
        private readonly IApiCatalogLookup _lookup;
        private readonly ICollection<FrameworkName> _defaultTargets;
        private readonly IRequestAnalyzer _requestAnalyzer;
        private readonly ICollection<IReportWriter> _reportWriters;
        private readonly ISearcher<string> _searcher;
        private readonly IApiRecommendations _apiRecommendations;

        private AnalyzeResult AnalyzeResult { get; set; }

        public OfflineApiPortService(IApiCatalogLookup lookup, IRequestAnalyzer requestAnalyzer, ITargetMapper mapper, ICollection<IReportWriter> reportWriters, ITargetNameParser targetNameParser, IApiRecommendations apiRecommendations)
        {
            _lookup = lookup;
            _requestAnalyzer = requestAnalyzer;
            _mapper = mapper;
            _reportWriters = reportWriters;
            _defaultTargets = new HashSet<FrameworkName>(targetNameParser.DefaultTargets);
            _searcher = new StringContainsSearch(lookup);
            _apiRecommendations = apiRecommendations;
        }

        public Task<IEnumerable<AvailableTarget>> GetTargetsAsync()
        {
            var targets = _lookup
                .GetPublicTargets()
                .Select(target => new AvailableTarget { Name = target.Identifier, Version = target.Version, IsSet = _defaultTargets.Contains(target) })
                .OrderBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
                .AsEnumerable();

            return Task.FromResult(targets);
        }

        public Task<IEnumerable<ResultFormatInformation>> GetResultFormatsAsync()
        {
            var formats = _reportWriters.Select(r => r.Format);

            return Task.FromResult(formats);
        }

        public Task<ResultFormatInformation> GetDefaultResultFormatAsync()
        {
            var format = new JsonReportWriter().Format;

            return Task.FromResult(format);
        }

        public Task<AnalyzeResponse> RequestAnalysisAsync(AnalyzeRequest analyzeRequest)
        {
            // IApiPortService separates requesting analysis from retrieving reports so that
            // each function can be performed by a different backend service. This makes the
            // offline implementation a little awkward. This method performs the analysis and
            // stores the result. GetReportingResultAsync uses this stored result to write a report.
            AnalyzeResult = _requestAnalyzer.AnalyzeRequest(analyzeRequest, Guid.NewGuid().ToString());

            return Task.FromResult(new AnalyzeResponse());
        }

        public async Task<ReportingResultWithFormat> GetReportingResultAsync(AnalyzeResponse analyzeResponse, ResultFormatInformation format)
        {
            format = format ?? await GetDefaultResultFormatAsync().ConfigureAwait(false);

            var writer = _reportWriters.First(w => w.Format.Equals(format));

            using (var ms = new MemoryStream())
            {
                writer.WriteStream(ms, AnalyzeResult);

                return new ReportingResultWithFormat
                {
                    Format = writer.Format.DisplayName,
                    Data = ms.ToArray()
                };
            }
        }

        public async Task<IReadOnlyCollection<ApiDefinition>> SearchFxApiAsync(string query, int? top = null)
        {
            var queryResult = await _searcher.SearchAsync(query, top ?? 10).ConfigureAwait(false);

            // TODO: This currently only populates the docid, as that is the only thing the offline service currently requires
            var result = queryResult.Select(r => new ApiDefinition { DocId = r })
                .ToList()
                .AsReadOnly();

            return result;
        }
    }
}
