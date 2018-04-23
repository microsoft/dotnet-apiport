// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;

namespace PortabilityService.ConfigurationProvider
{
    public class PortabilityServiceConfigurationSource : IConfigurationSource
    {
        private readonly string _configurationSection;
        private readonly HttpClient _httpClient;

        public PortabilityServiceConfigurationSource(HttpClient httpClient, string configurationSection)
        {
            _configurationSection = configurationSection ?? throw new ArgumentNullException(nameof(configurationSection));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder) => new PortabilityServiceConfigurationProvider(_httpClient, _configurationSection);            
    }
}
