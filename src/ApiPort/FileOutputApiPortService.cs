// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPort
{
    /// <summary>
    /// Use this service implementation to output analyze requests to a json file
    /// </summary>
    internal class FileOutputApiPortService : IApiPortService
    {
        public Task<ServiceResponse<AnalyzeResponse>> GetAnalysisAsync(string submissionId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<byte[]>> GetAnalysisAsync(string submissionId, string format)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<ApiInformation>> GetApiInformationAsync(string docId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<IEnumerable<ResultFormatInformation>>> GetResultFormatsAsync()
        {
            var format = new ResultFormatInformation { DisplayName = "Excel", FileExtension = ".xlsx" };
            var response = new ServiceResponse<IEnumerable<ResultFormatInformation>>(new[] { format });

            return Task.FromResult(response);
        }

        public Task<ServiceResponse<IEnumerable<AvailableTarget>>> GetTargetsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<UsageDataCollection>> GetUsageDataAsync(int? skip = default(int?), int? top = default(int?), UsageDataFilter? filter = default(UsageDataFilter?), IEnumerable<string> targets = null)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<IReadOnlyCollection<ApiDefinition>>> SearchFxApiAsync(string query, int? top = default(int?))
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<AnalyzeResponse>> SendAnalysisAsync(AnalyzeRequest a)
        {
            WriteOutput(a);

            return Task.FromResult(new ServiceResponse<AnalyzeResponse>(new AnalyzeResponse()));
        }

        public Task<ServiceResponse<byte[]>> SendAnalysisAsync(AnalyzeRequest a, string format)
        {
            WriteOutput(a);

            return Task.FromResult(new ServiceResponse<byte[]>(new byte[] { }));
        }

        private void WriteOutput(AnalyzeRequest a)
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
                BreakingChangesToSuppress = new SortedSet<string>(a.BreakingChangesToSuppress, StringComparer.Ordinal),
                AssembliesToIgnore = a.AssembliesToIgnore.OrderBy(i => i.AssemblyIdentity)
            };

            var tmp = $"{Path.GetTempFileName()}.json";

            using (var ms = new MemoryStream(sortedAnalyzeRequest.Serialize()))
            using (var fs = File.OpenWrite(tmp))
            {
                ms.CopyTo(fs);
            }

            Process.Start(tmp);
        }
    }
}
