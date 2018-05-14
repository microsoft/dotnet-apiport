// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;

namespace Microsoft.Fx.Portability.ConfigurationProvider
{
    public class PortabilityServiceConfigurationSource : IConfigurationSource
    {
        private readonly string _configurationSection;
        private readonly HttpClient _httpClient;
        private readonly bool _optional;

        public PortabilityServiceConfigurationSource(HttpClient httpClient, string configurationSection = ConfigurationProviderConstants.PortabilityServiceConfigurationRoot, bool optional = false)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configurationSection = configurationSection ?? throw new ArgumentNullException(nameof(configurationSection));
            _optional = optional;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder) => new PortabilityServiceConfigurationProvider(_httpClient, _configurationSection, _optional);
    }
}
