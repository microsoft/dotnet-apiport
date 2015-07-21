// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
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

        public OfflineApiPortService(IApiCatalogLookup lookup, IRequestAnalyzer requestAnalyzer, ITargetMapper mapper, ICollection<IReportWriter> reportWriters,ITargetNameParser targetNameParser)
        {
            _lookup = lookup;
            _requestAnalyzer = requestAnalyzer;
            _mapper = mapper;
            _reportWriters = reportWriters;
            _defaultTargets = new HashSet<FrameworkName>(targetNameParser.DefaultTargets);
        }

        public Task<ServiceResponse<IEnumerable<AvailableTarget>>> GetTargetsAsync()
        {
            var targets = _lookup
                .GetPublicTargets()
                .Select(target => new AvailableTarget { Name = target.Identifier, Version = target.Version, IsSet = _defaultTargets.Contains(target) });

            var response = new ServiceResponse<IEnumerable<AvailableTarget>>(targets);

            return Task.FromResult(response);
        }

        public Task<ServiceResponse<AnalyzeResponse>> SendAnalysisAsync(AnalyzeRequest a)
        {
            var response = _requestAnalyzer.AnalyzeRequest(a, Guid.NewGuid().ToString());
            var serviceResponse = new ServiceResponse<AnalyzeResponse>(response);

            return Task.FromResult(serviceResponse);
        }

        public Task<ServiceResponse<byte[]>> SendAnalysisAsync(AnalyzeRequest a, string format)
        {
            var response = _requestAnalyzer.AnalyzeRequest(a, Guid.NewGuid().ToString());

            var writer = _reportWriters.FirstOrDefault(w => string.Equals(w.Format.DisplayName, format, StringComparison.OrdinalIgnoreCase));

            if (writer == null)
            {
                throw new UnknownReportFormatException(format);
            }

            using (var ms = new MemoryStream())
            {
                writer.WriteStream(ms, response);

                return WrapResponse(ms.ToArray());
            }
        }

        public Task<ServiceResponse<IEnumerable<ResultFormatInformation>>> GetResultFormatsAsync()
        {
            var formats = _reportWriters.Select(r => r.Format);

            return WrapResponse(formats);
        }

        private Task<ServiceResponse<T>> WrapResponse<T>(T data)
        {
            var response = new ServiceResponse<T>(data);

            return Task.FromResult(response);
        }

        public Task<ServiceResponse<UsageDataCollection>> GetUsageDataAsync(int? skip = null, int? top = null, UsageDataFilter? filter = null, IEnumerable<string> targets = null)
        {
            throw new NotImplementedException();
        }

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

        public Task<ServiceResponse<IReadOnlyCollection<ApiDefinition>>> SearchFxApiAsync(string query, int? top = null)
        {
            throw new NotImplementedException();
        }
    }
}
