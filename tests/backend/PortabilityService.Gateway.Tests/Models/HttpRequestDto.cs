// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace PortabilityService.Gateway.Tests.Models
{
    /// <summary>
    /// This model is needed because HttpRequest objects are not preserved
    /// by ASP.NET Core web hosts through returning a response. Therefore, 
    /// the tests store necessary information about incoming requests (for
    /// later validation) using this simple type.
    /// </summary>
    class HttpRequestDto
    {
        public string Method { get; }
        public string Uri { get; private set; }
        public Dictionary<string, string> Headers { get; }

        public HttpRequestDto()
        {
            Headers = new Dictionary<string, string>();
        }

        public HttpRequestDto(HttpRequest request) : this()
        {
            // Store the method, URI, and headers from the request
            Method = request.Method;

            Uri = $"{request.Scheme}://{request.Host}";
            if (request.Path.HasValue)
            {
                Uri += request.Path.Value;
            }
            if (request.QueryString.HasValue)
            {
                Uri += request.QueryString.Value;
            }
            
            foreach (var header in request.Headers)
            {
                Headers.Add(header.Key, header.Value.ToString());
            }
        }
    }
}
