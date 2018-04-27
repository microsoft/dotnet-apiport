// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PortabilityService.ConfigurationProvider
{
    public class PortabilityServiceConfigurationProvider : Microsoft.Extensions.Configuration.ConfigurationProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _configurationSection;

        public PortabilityServiceConfigurationProvider(HttpClient httpClient, string configurationSection = Constants.PortabilityServiceConfigurationRoot)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configurationSection = configurationSection ?? throw new ArgumentNullException(nameof(configurationSection));
        }

        public override void Load() => LoadAsync().GetAwaiter().GetResult();

        private async Task LoadAsync()
        {
            Data = await GetConfigurationSectionSettings();
        }

        /// <summary>
        /// Gets the configuration settings for the passed in section
        /// </summary>
        private async Task<IDictionary<string, string>> GetConfigurationSectionSettings()
        {
            var result = await GetJsonDataAsync<IEnumerable<KeyValuePair<string, string>>>($"/api/Configuration/sectionsettingslist/{_configurationSection}");

            if (result == null)
            {
                return new Dictionary<string, string>();
            }
            else
            {
                return result.ToDictionary(k => k.Key, v => v.Value);
            }
        }

        /// <summary>
        /// Returns Json from the WebAPI call with passed in URI
        /// </summary>
        private async Task<T> GetJsonDataAsync<T>(string uri)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                request.Headers.Add("Accept", "application/json");
                return await GetJsonDataAsync<T>(request).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Returns Json from the WebApi call with the passed in request message
        /// </summary>
        private async Task<T> GetJsonDataAsync<T>(HttpRequestMessage request)
        {
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return default;
            }

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var result = JsonConvert.DeserializeObject<T>(content);

            return result;
        }
    }
}
