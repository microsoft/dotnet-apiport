// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Microsoft.Fx.Portability
{
    public class ServiceHeaders
    {
        // Do not change these 
        public const string EndpointHeader = "WebsiteUrl";
        public const string ApiInfoHeader = "DotNetApiInfoUrl";
        public const string SubmissionUrlHeader = "SubmissionUrl";

        public ServiceHeaders(HttpResponseMessage response)
        {
            var headers = response.Headers;

            Status = DetermineEndpointStatus(headers);
            WebsiteEndpoint = GetHeaderString(headers, EndpointHeader);
            ApiInfoUrl = GetHeaderString(headers, ApiInfoHeader);
            SubmissionUrl = GetHeaderString(headers, SubmissionUrlHeader);
        }

        public ServiceHeaders(HttpRequestMessage request)
        {
            var headers = request.Headers;

            Status = DetermineEndpointStatus(headers);
            WebsiteEndpoint = GetHeaderString(headers, EndpointHeader);
            ApiInfoUrl = GetHeaderString(headers, ApiInfoHeader);
            SubmissionUrl = GetHeaderString(headers, SubmissionUrlHeader);
        }

        public ServiceHeaders() { }

        public EndpointStatus Status { get; set; }

        public string WebsiteEndpoint { get; set; }

        public string SubmissionUrl { get; set; }

        public string ApiInfoUrl { get; set; }


        private static EndpointStatus DetermineEndpointStatus(HttpHeaders headers)
        {
            var statusHeader = GetHeaderString(headers, typeof(EndpointStatus).Name);

            EndpointStatus status;
            if (Enum.TryParse(statusHeader, out status))
            {
                return status;
            }

            // If no header or multiple entries present, assume endpoint is deprecated
            return EndpointStatus.Deprecated;
        }

        private static string GetHeaderString(HttpHeaders headers, string name)
        {
            IEnumerable<string> values;

            if (headers.TryGetValues(name, out values))
            {
                return values.FirstOrDefault();
            }

            return null;
        }

        public bool HasApiEndpoint
        {
            get
            {
                return !string.IsNullOrWhiteSpace(WebsiteEndpoint) && !string.IsNullOrWhiteSpace(ApiInfoUrl);
            }
        }

        public bool HasSubmissionEndpoint
        {
            get
            {
                return !string.IsNullOrWhiteSpace(WebsiteEndpoint) && !string.IsNullOrWhiteSpace(SubmissionUrl);
            }
        }

        public Uri GetDocIdUrl(string docId)
        {
            return new Uri(string.Format("{0}{1}{2}", WebsiteEndpoint, ApiInfoUrl, WebUtility.UrlEncode(docId)));
        }

        public Uri GetSubmissionUrl(string submissionId)
        {
            return new Uri(string.Format("{0}{1}{2}", WebsiteEndpoint, SubmissionUrl, WebUtility.UrlEncode(submissionId)));
        }
    }
}
