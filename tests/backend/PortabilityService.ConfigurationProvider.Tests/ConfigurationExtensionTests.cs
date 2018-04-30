// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using Xunit;

namespace PortabilityService.ConfigurationProvider.Tests
{
    public class ConfigurationExtensionTests
    {
        // IConfigurationBuilder AddPortabilityServiceConfiguration(this IConfigurationBuilder builder, string urlEnvironmentKeyName = Constants.UrlEnvironmentName, string configurationSection = Constants.PortabilityServiceConfigurationRoot)
        [Fact]
        public static void CtorNullurlEnvironmentKeyNameThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationBuilder().AddPortabilityServiceConfiguration((string)null));
        }

        [Fact]
        public static void CtorNonExistantUrlEnvironmentKeyNameThrowsException()
        {
            var exception = Assert.Throws<Exception>(() => new ConfigurationBuilder().AddPortabilityServiceConfiguration("NonExistingEnvironmentName"));
            Assert.Equal("Url '' from environment setting 'NonExistingEnvironmentName' is not a valid Url!", exception.Message);
        }

        // IConfigurationBuilder AddPortabilityServiceConfiguration(this IConfigurationBuilder builder, Uri configurationServiceUrl, string configurationSection = Constants.PortabilityServiceConfigurationRoot)
        [Fact]
        public static void CtorUriAndNullConfigurationSectionThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationBuilder().AddPortabilityServiceConfiguration(new Uri("http://localhost"), null));
        }

        [Fact]
        public static void CtorConfigurationSectionAndNullUriThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationBuilder().AddPortabilityServiceConfiguration((Uri)null, "TestSection"));
        }

        // IConfigurationBuilder AddPortabilityServiceConfiguration(this IConfigurationBuilder builder, HttpClient httpClient, string configurationSection = Constants.PortabilityServiceConfigurationRoot)
        [Fact]
        public static void CtorHttpClientAndNullConfigurationSectionThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationBuilder().AddPortabilityServiceConfiguration(new HttpClient(), null));
        }

        [Fact]
        public static void CtorConfigurationSectionAndNullHttpClientThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationBuilder().AddPortabilityServiceConfiguration((HttpClient)null, "TestSection"));
        }
    }
}
