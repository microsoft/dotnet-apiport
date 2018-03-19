// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using PortabilityService.Gateway.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PortabilityService.Gateway.Tests.Helpers
{
    /// <summary>
    /// Simple WebHost that records incoming requests.
    /// Used as an endpoint to redirect requests to for test purposes.
    /// </summary>
    internal sealed class DownstreamListener : IDisposable
    {
        public List<HttpRequestDto> Requests { get; }
        public IWebHost Host { get; }

        public DownstreamListener(IEnumerable<string> paths, int port)
        {
            Requests = new List<HttpRequestDto>();

            Host = WebHost.CreateDefaultBuilder()
                .Configure(app =>
                {
                    app.Run(async context =>
                    {
                        // We extract path, method, and header information from the 
                        // request instead of storing it directly because ASP.NET Core WebHosts
                        // modify requests as part of returning a response (headers are lost, 
                        // for example).
                        Requests.Add(new HttpRequestDto(context.Request));

                        // If the request comes in on an expected path, return 200 otherwise, 404
                        if (paths.Contains(context.Request.Path.Value))
                        {
                            // Wait a moment before returning so that we can test our timeout policy
                            var wait = context.Request.Query["slow"].FirstOrDefault();
                            if (int.TryParse(wait, out int waitDuration))
                            {
                                await Task.Delay(waitDuration);
                            }

                            context.Response.StatusCode = 200;
                        }
                        else
                        {
                            context.Response.StatusCode = 404;
                        }
                    });
                })
                // Listen on indicated port
                .UseUrls($"http://localhost:{port}")
                .Build();

            Host.Start();
        }

// Don't need the full disposable pattern since this type is sealed and has no native resources
#pragma warning disable CA1063 
        public void Dispose() => Host?.Dispose();
#pragma warning restore CA1063
    }
}
