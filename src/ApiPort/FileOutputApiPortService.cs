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

        public FileOutputApiPortService(IProgressReporter progress)
        {
            _progress = progress;
        }

        public Task<ServiceResponse<AnalyzeResponse>> GetAnalysisAsync(string submissionId)
        {
            _progress.ReportIssue(LocalizedStrings.FileOutputServiceNotSupported);
            var result = ServiceResponse.Create(new AnalyzeResponse());

            return Task.FromResult(result);
        }

        public Task<ServiceResponse<IEnumerable<ReportingResultWithFormat>>> GetAnalysisAsync(string submissionId, IEnumerable<string> format)
        {
            _progress.ReportIssue(LocalizedStrings.FileOutputServiceNotSupported);
            var result = ServiceResponse.Create(Enumerable.Empty<ReportingResultWithFormat>());

            return Task.FromResult(result);
        }

        public Task<ServiceResponse<ApiInformation>> GetApiInformationAsync(string docId)
        {
            _progress.ReportIssue(LocalizedStrings.FileOutputServiceNotSupported);
            var response = ServiceResponse.Create(new ApiInformation());

            return Task.FromResult(response);
        }

        public Task<ServiceResponse<IEnumerable<ResultFormatInformation>>> GetResultFormatsAsync()
        {
            var response = ServiceResponse.Create(s_formats);

            return Task.FromResult(response);
        }

        public Task<ServiceResponse<ResultFormatInformation>> GetDefaultResultFormatAsync()
        {
            // s_formats contains one element
            var response = ServiceResponse.Create(s_formats.First());

            return Task.FromResult(response);
        }

        public Task<ServiceResponse<IEnumerable<AvailableTarget>>> GetTargetsAsync()
        {
            var targets = new[]
            {
                new AvailableTarget
                {
                    Name = "Target1",
                    Version = Version.Parse("1.0"),
                    Description = "Description1"
                },
                new AvailableTarget
                {
                    Name = "Target1",
                    Version = Version.Parse("2.0"),
                    Description = "Description2"
                },
                new AvailableTarget
                {
                    Name = "Target1",
                    Version = Version.Parse("3.0"),
                    Description = "Description3"
                },
                new AvailableTarget
                {
                    Name = "Target2",
                    Version = Version.Parse("1.0"),
                },
                new AvailableTarget
                {
                    Name = "Target2",
                    Version = Version.Parse("2.0"),
                },
            };

            var response = ServiceResponse.Create<IEnumerable<AvailableTarget>>(targets);

            return Task.FromResult(response);
        }

        public Task<ServiceResponse<UsageDataCollection>> GetUsageDataAsync(int? skip = default(int?), int? top = default(int?), UsageDataFilter? filter = default(UsageDataFilter?), IEnumerable<string> targets = null)
        {
            _progress.ReportIssue(LocalizedStrings.FileOutputServiceNotSupported);
            var response = ServiceResponse.Create(new UsageDataCollection());

            return Task.FromResult(response);
        }

        public Task<ServiceResponse<IReadOnlyCollection<ApiInformation>>> QueryDocIdsAsync(IEnumerable<string> docIds)
        {
            _progress.ReportIssue(LocalizedStrings.FileOutputServiceNotSupported);
            var response = ServiceResponse.Create(s_emptyQueryDocIds);

            return Task.FromResult(response);
        }

        public Task<ServiceResponse<IReadOnlyCollection<ApiDefinition>>> SearchFxApiAsync(string query, int? top = default(int?))
        {
            _progress.ReportIssue(LocalizedStrings.FileOutputServiceNotSupported);
            var response = ServiceResponse.Create(s_emptySearchResults);

            return Task.FromResult(response);
        }

        public Task<ServiceResponse<AnalyzeResponse>> SendAnalysisAsync(AnalyzeRequest a)
        {
            _progress.ReportIssue(LocalizedStrings.FileOutputServiceNotSupported);

            return Task.FromResult(new ServiceResponse<AnalyzeResponse>(new AnalyzeResponse()));
        }

        /// <summary>
        /// Returns the analysis as <see cref="s_formats"/>. Input <paramref name="formats"/> is ignored.
        /// </summary>
        public Task<ServiceResponse<IEnumerable<ReportingResultWithFormat>>> SendAnalysisAsync(AnalyzeRequest a, IEnumerable<string> formats)
        {
            var result = s_formats.Select(f => new ReportingResultWithFormat
            {
                Data = SendAnalysisAsync(a, f.DisplayName),
                Format = f.DisplayName
            });

            return Task.FromResult(new ServiceResponse<IEnumerable<ReportingResultWithFormat>>(result.ToList()));
        }

        private byte[] SendAnalysisAsync(AnalyzeRequest a, string format)
        {
            var sortedAnalyzeRequest = new AnalyzeRequest
            {
                RequestFlags = a.RequestFlags,
                Dependencies = a.Dependencies
                    .OrderBy(t => t.Key.MemberDocId)
                    .ThenBy(t => t.Key.TypeDocId)
                    .ToDictionary(t => t.Key, t => t.Value.OrderBy(tt => tt.AssemblyIdentity).ToList() as ICollection<AssemblyInfo>),
                UnresolvedAssemblies = new SortedSet<string>(a.UnresolvedAssemblies, StringComparer.Ordinal),
                UnresolvedAssembliesDictionary = a.UnresolvedAssembliesDictionary
                    .OrderBy(t => t.Key)
                    .ToDictionary(t => t.Key, t => new SortedSet<string>(t.Value) as ICollection<string>),
                UserAssemblies = new SortedSet<AssemblyInfo>(a.UserAssemblies),
                AssembliesWithErrors = new SortedSet<string>(a.AssembliesWithErrors, StringComparer.Ordinal),
                Targets = new SortedSet<string>(a.Targets, StringComparer.Ordinal),
                ApplicationName = a.ApplicationName,
                Version = a.Version,
                BreakingChangesToSuppress = new SortedSet<string>(a.BreakingChangesToSuppress ?? Enumerable.Empty<string>(), StringComparer.Ordinal),
                AssembliesToIgnore = a.AssembliesToIgnore.OrderBy(i => i.AssemblyIdentity)
            };

            return sortedAnalyzeRequest.Serialize();
        }
    }
}
