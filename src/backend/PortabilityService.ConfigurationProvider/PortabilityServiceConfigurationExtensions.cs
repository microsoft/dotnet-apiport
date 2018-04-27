// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Net.Http;

namespace PortabilityService.ConfigurationProvider
{
    public static class PortabilityServiceConfigurationExtensions
    {
        public static IConfigurationBuilder AddPortabilityServiceConfiguration(this IConfigurationBuilder builder, string urlEnvironmentKeyName = Constants.UrlEnvironmentKeyName, string configurationSection = Constants.PortabilityServiceConfigurationRoot)
            => AddPortabilityServiceConfiguration(builder, GetConfigurationServiceUrlFromEnvironment(urlEnvironmentKeyName), configurationSection);

        public static IConfigurationBuilder AddPortabilityServiceConfiguration(this IConfigurationBuilder builder, Uri configurationServiceUrl, string configurationSection = Constants.PortabilityServiceConfigurationRoot)
            => AddPortabilityServiceConfiguration(builder, CreateHttpClient(configurationServiceUrl), configurationSection);

        public static IConfigurationBuilder AddPortabilityServiceConfiguration(this IConfigurationBuilder builder, HttpClient httpClient, string configurationSection = Constants.PortabilityServiceConfigurationRoot)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (string.IsNullOrEmpty(configurationSection))
            {
                throw new ArgumentNullException(nameof(configurationSection));
            }

            return builder.Add(new PortabilityServiceConfigurationSource(httpClient, configurationSection));
        }

        /// <summary>
        /// Gets the Portability configuration service URL from the environment
        /// </summary>
        /// <returns></returns>
        private static Uri GetConfigurationServiceUrlFromEnvironment(string urlEnvironmentKeyName)
        {
            if (string.IsNullOrEmpty(urlEnvironmentKeyName))
            {
                throw new ArgumentNullException(nameof(urlEnvironmentKeyName));
            }

            var urlFromEnvironment = Environment.GetEnvironmentVariable(urlEnvironmentKeyName);

            try
            {
                return new Uri(urlFromEnvironment);
            }
            catch
            {
                throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.Resources.EnvironmentNameInvalid, urlFromEnvironment, urlEnvironmentKeyName));
            }
        }

        /// <summary>
        /// Creates an HTTPClient with the passed in Uri
        /// </summary>
        private static HttpClient CreateHttpClient(Uri configurationServiceUrl)
        {
            if (configurationServiceUrl == null)
            {
                throw new ArgumentNullException(nameof(configurationServiceUrl));
            }

            return new HttpClient() { BaseAddress = configurationServiceUrl };
        }
    }
}
