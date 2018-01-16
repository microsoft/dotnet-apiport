// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability
{
    public interface IApiPortService
    {
        /// <summary>
        /// Gets the portability service endpoint
        /// </summary>
        Uri Endpoint { get; }

        /// <summary>
        /// Updates the service endpoint
        /// </summary>
        /// <param name="uri">Base uri for the service endpoint</param>
        void UpdateEndpoint(Uri uri);

        /// <summary>
        /// Gets all available targets for analysis
        /// </summary>
        Task<ServiceResponse<IEnumerable<AvailableTarget>>> GetTargetsAsync();
        
        /// <summary>
        /// Sends a request for portability analysis
        /// </summary>
        Task<ServiceResponse<AnalyzeResponse>> SendAnalysisAsync(AnalyzeRequest a);
        
        /// <summary>
        /// Sends a request for portability analysis and returns the report in
        /// multiple formats.
        /// </summary>
        /// <param name="format">An enumerable of <see cref="ResultFormatInformation.DisplayName"/>
        /// for the report to be returned as</param>
        Task<ServiceResponse<IEnumerable<ReportingResultWithFormat>>> SendAnalysisAsync(AnalyzeRequest a, IEnumerable<string> format);

        Task<ServiceResponse<UsageDataCollection>> GetUsageDataAsync(int? skip = null, int? top = null, UsageDataFilter? filter = null, IEnumerable<string> targets = null);
        
        /// <summary>
        /// Gets API information (ie. Supported .NET platforms, recommended
        /// changes, etc.) for the given docId
        /// </summary>
        Task<ServiceResponse<ApiInformation>> GetApiInformationAsync(string docId);

        /// <summary>
        /// Searches the API catalog and returns all of the results matching
        /// the given docId query. Will return the top results if it is specified.
        /// </summary>
        Task<ServiceResponse<IReadOnlyCollection<ApiDefinition>>> SearchFxApiAsync(string query, int? top = null);

        /// <summary>
        /// Gets all supported portability analysis report formats.
        /// </summary>
        Task<ServiceResponse<IEnumerable<ResultFormatInformation>>> GetResultFormatsAsync();

        /// <summary>
        /// Gets the default portability anaysis report format.
        /// </summary>
        /// <remarks>
        /// This format is the one that is used when a report format is not
        /// specified during portability analysis.
        /// </remarks>
        Task<ServiceResponse<ResultFormatInformation>> GetDefaultResultFormatAsync();

        /// <summary>
        /// Gets API information for a set of docIds
        /// </summary>
        /// <param name="docIds">Enumerable of docIds</param>
        /// <returns>Returns a list of valid docIds with their API information
        /// from the PortabilityService</returns>
        Task<ServiceResponse<IReadOnlyCollection<ApiInformation>>> QueryDocIdsAsync(IEnumerable<string> docIds);
    }
}
