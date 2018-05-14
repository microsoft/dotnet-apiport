// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.ConfigurationProvider.Tests
{
    internal class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _content;
        private readonly HttpStatusCode _statusCode;

        public TestHttpMessageHandler(string content, HttpStatusCode statusCode)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _statusCode = statusCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(BuildResponseMessage());

        private HttpResponseMessage BuildResponseMessage() => new HttpResponseMessage { Content = new StringContent(_content), StatusCode = _statusCode };
    }
}
