// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Logging;
using Ocelot.Configuration;
using Ocelot.Requester.QoS;

namespace PortabilityService.Gateway.Reliability
{
    /// <summary>
    /// Custom quality-of-service provider factory to generate Polly policies
    /// tailored to PortabilityService scenarios
    /// </summary>
    public class PortabilityServiceQoSProviderFactory: IQoSProviderFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public PortabilityServiceQoSProviderFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public IQoSProvider Get(DownstreamReRoute reRoute)
        {
            if (reRoute.IsQos)
            {
                return new PortabilityServiceQoSProvider(reRoute, _loggerFactory);
            }

            return new NoQoSProvider();
        }
    }
}
