// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.Net.Http;

namespace Microsoft.Fx.Portability.ConfigurationProvider
{
    public static class PortabilityServiceConfigurationExtensions
    {
        public static IConfigurationBuilder AddPortabilityServiceConfiguration(this IConfigurationBuilder builder,
                                                                               string UrlEnvironmentKeyName = ConfigurationProviderConstants.UrlEnvironmentKeyName,
                                                                               string configurationSection = ConfigurationProviderConstants.PortabilityServiceConfigurationRoot,
                                                                               bool optional = false)
            => AddPortabilityServiceConfiguration(builder, GetConfigurationServiceUrlFromEnvironment(UrlEnvironmentKeyName), configurationSection, optional);

        public static IConfigurationBuilder AddPortabilityServiceConfiguration(this IConfigurationBuilder builder, Uri configurationServiceUrl, string configurationSection = ConfigurationProviderConstants.PortabilityServiceConfigurationRoot, bool optional = false)
            => AddPortabilityServiceConfiguration(builder, CreateHttpClient(configurationServiceUrl), configurationSection, optional);

        public static IConfigurationBuilder AddPortabilityServiceConfiguration(this IConfigurationBuilder builder, HttpClient httpClient, string configurationSection = ConfigurationProviderConstants.PortabilityServiceConfigurationRoot, bool optional = false)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (string.IsNullOrEmpty(configurationSection))
            {
                throw new ArgumentNullException(nameof(configurationSection));
            }

            return builder.Add(new PortabilityServiceConfigurationSource(httpClient, configurationSection, optional));
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
                throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.Resources.EnvironmentNameInvalidMessage, urlFromEnvironment, urlEnvironmentKeyName));
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
