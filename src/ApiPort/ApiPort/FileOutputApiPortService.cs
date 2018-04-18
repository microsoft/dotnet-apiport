// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.Resources;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPort
{
    /// <summary>
    /// Use this service implementation to output analyze requests to a json file
    /// </summary>
    internal class FileOutputApiPortService : IApiPortService
    {
        private static readonly IReadOnlyCollection<ApiDefinition> s_emptySearchResults = new ApiDefinition[] { };
        private static readonly IReadOnlyCollection<ApiInformation> s_emptyQueryDocIds = new ApiInformation[] { };
        private static readonly IEnumerable<ResultFormatInformation> s_formats = new[]
        {
            new ResultFormatInformation
            {
                DisplayName = "json",
                FileExtension = ".json"
            }
        };

        private readonly IProgressReporter _progress;

        private AnalyzeRequest AnalyzeRequest { get; set; }

        public FileOutputApiPortService(IProgressReporter progress)
        {
            _progress = progress;
        }

        public Task<ApiInformation> GetApiInformationAsync(string docId)
        {
            _progress.ReportIssue(LocalizedStrings.FileOutputServiceNotSupported);

            return Task.FromResult(new ApiInformation());
        }

        public Task<IReadOnlyCollection<ApiInformation>> QueryDocIdsAsync(IEnumerable<string> docIds)
        {
            _progress.ReportIssue(LocalizedStrings.FileOutputServiceNotSupported);

            return Task.FromResult(s_emptyQueryDocIds);
        }

        public Task<IReadOnlyCollection<ApiDefinition>> SearchFxApiAsync(string query, int? top = default)
        {
            _progress.ReportIssue(LocalizedStrings.FileOutputServiceNotSupported);

            return Task.FromResult(s_emptySearchResults);
        }

        public Task<ResultFormatInformation> GetDefaultResultFormatAsync() => Task.FromResult(s_formats.Single());

        public Task<IEnumerable<ResultFormatInformation>> GetResultFormatsAsync() => Task.FromResult(s_formats);

        public Task<IEnumerable<AvailableTarget>> GetTargetsAsync() => Task.FromResult(Enumerable.Empty<AvailableTarget>());

        public Task<AnalyzeResponse> RequestAnalysisAsync(AnalyzeRequest analyzeRequest)
        {
            // IApiPortService separates requesting analysis from retrieving reports, which
            // is awkward for this offline implementation, so this method stores the request
            // for use in GetReportingResultAsync, and returns an empty AnalyzeResponse.
            AnalyzeRequest = SortedAnalyzeRequest(analyzeRequest);

            return Task.FromResult(new AnalyzeResponse());
        }

        private static AnalyzeRequest SortedAnalyzeRequest(AnalyzeRequest analyzeRequest) => new AnalyzeRequest
        {
            RequestFlags = analyzeRequest.RequestFlags,
            Dependencies = analyzeRequest.Dependencies
                .OrderBy(t => t.Key.MemberDocId)
                .ThenBy(t => t.Key.TypeDocId)
                .ToDictionary(t => t.Key, t => t.Value.OrderBy(tt => tt.AssemblyIdentity).ToList() as ICollection<AssemblyInfo>),
            UnresolvedAssemblies = new SortedSet<string>(analyzeRequest.UnresolvedAssemblies, StringComparer.Ordinal),
            UnresolvedAssembliesDictionary = analyzeRequest.UnresolvedAssembliesDictionary
                .OrderBy(t => t.Key)
                .ToDictionary(t => t.Key, t => new SortedSet<string>(t.Value) as ICollection<string>),
            UserAssemblies = new SortedSet<AssemblyInfo>(analyzeRequest.UserAssemblies),
            AssembliesWithErrors = new SortedSet<string>(analyzeRequest.AssembliesWithErrors, StringComparer.Ordinal),
            Targets = new SortedSet<string>(analyzeRequest.Targets, StringComparer.Ordinal),
            ApplicationName = analyzeRequest.ApplicationName,
            Version = analyzeRequest.Version,
            BreakingChangesToSuppress = new SortedSet<string>(analyzeRequest.BreakingChangesToSuppress ?? Enumerable.Empty<string>(), StringComparer.Ordinal),
            AssembliesToIgnore = analyzeRequest.AssembliesToIgnore.OrderBy(i => i.AssemblyIdentity)
        };

        public Task<ReportingResultWithFormat> GetReportingResultAsync(AnalyzeResponse analyzeResponse, ResultFormatInformation format)
            => Task.FromResult(new ReportingResultWithFormat { Data = AnalyzeRequest.Serialize(), Format = format.DisplayName });
    }
}
