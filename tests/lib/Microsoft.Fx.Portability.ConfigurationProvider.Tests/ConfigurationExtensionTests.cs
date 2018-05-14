// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace Microsoft.Fx.Portability.ConfigurationProvider.Tests
{
    public class ConfigurationExtensionTests
    {
        private const string TestSectionName = "TestSection";

        private static readonly Uri _testBaseUri = new Uri("http://localhost2");

        // IConfigurationBuilder AddPortabilityServiceConfiguration(this IConfigurationBuilder builder, string urlEnvironmentKeyName = Constants.UrlEnvironmentName, string configurationSection = Constants.PortabilityServiceConfigurationRoot)
        [Fact]
        public static void CtorNullUrlEnvironmentKeyNameThrowsArgumentNullException()
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
            Assert.Throws<ArgumentNullException>(() => new ConfigurationBuilder().AddPortabilityServiceConfiguration(_testBaseUri, null));
        }

        [Fact]
        public static void CtorConfigurationSectionAndNullUriThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationBuilder().AddPortabilityServiceConfiguration((Uri)null, TestSectionName));
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
            Assert.Throws<ArgumentNullException>(() => new ConfigurationBuilder().AddPortabilityServiceConfiguration((HttpClient)null, TestSectionName));
        }

        [Fact]
        public static void AddWithNonExistingUriAndOptionalSetToFalseThrowsHttpRequestException()
        {
            var configBuilder = new ConfigurationBuilder().AddPortabilityServiceConfiguration(_testBaseUri, ConfigurationProviderConstants.PortabilityServiceConfigurationRoot, false);
            Assert.Throws<HttpRequestException>(() => configBuilder.Build());
        }

        [Fact]
        public static void AddWithNonExistingUriAndOptionalSetToTrueShouldAddEmptyConfiguration()
        {
            using (var httpClient = new HttpClient(new TestHttpMessageHandler(@"[{ ""Key"":""testKey"",""Value"":""testValue"" }]", HttpStatusCode.BadRequest)) { BaseAddress = _testBaseUri })
            {
                var config = new ConfigurationBuilder().AddPortabilityServiceConfiguration(httpClient, ConfigurationProviderConstants.PortabilityServiceConfigurationRoot, true).Build();
                Assert.Equal(new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>(ConfigurationProviderConstants.PortabilityServiceConfigurationRoot, null) },
                            config.GetSection(ConfigurationProviderConstants.PortabilityServiceConfigurationRoot).AsEnumerable());
            }
        }
    }
}
