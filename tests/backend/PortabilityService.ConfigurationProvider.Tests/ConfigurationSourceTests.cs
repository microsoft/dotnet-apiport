// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net.Http;
using Xunit;

namespace PortabilityService.ConfigurationProvider.Tests
{
    public class ConfigurationSourceTests
    {
        // PortabilityServiceConfigurationSource(HttpClient httpClient, string configurationSection = Constants.PortabilityServiceConfigurationRoot)
        [Fact]
        public static void CtorHttpClientAndNullConfigurationSectionThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PortabilityServiceConfigurationSource(new HttpClient(), null));
        }

        [Fact]
        public static void CtorConfigurationSectionAndNullHttpClientThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PortabilityServiceConfigurationSource((HttpClient)null, "TestSection"));
        }
    }
}
