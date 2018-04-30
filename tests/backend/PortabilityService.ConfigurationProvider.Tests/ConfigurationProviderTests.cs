// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace PortabilityService.ConfigurationProvider.Tests
{
    public class ConfigurationProviderTests
    {
        private static readonly Uri _testBaseUri = new Uri("http://localhost");

        // PortabilityServiceConfigurationProvider(HttpClient httpClient, string configurationSection = Constants.PortabilityServiceConfigurationRoot)
        [Fact]
        public static void CtorHttpClientAndNullConfigurationSectionThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PortabilityServiceConfigurationProvider(new HttpClient(), null));
        }

        [Fact]
        public static void CtorConfigurationSectionAndNullHttpClientThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PortabilityServiceConfigurationProvider((HttpClient)null, "TestSection"));
        }

        [Fact]
        public static void LoadWithSimpleJsonLoadsConfiguration()
        {
            using (var httpClient = new HttpClient(new TestHttpMessageHandler(@"[{ ""Key"":""testKey"",""Value"":""testValue"" }]", HttpStatusCode.OK)) { BaseAddress = _testBaseUri })
            {
                // arrange
                var configurationProvider = new PortabilityServiceConfigurationProvider(httpClient);

                // act
                configurationProvider.Load();

                // assert
                Assert.Equal(new List<string> { "testKey" }, configurationProvider.GetChildKeys(Enumerable.Empty<string>(), null));
            }
        }

        [Fact]
        public static void LoadWithMoreComplexJsonResultLoadsConfiguration()
        {
            using (var httpClient = new HttpClient(new TestHttpMessageHandler(BuildMoreComplexJsonResult(), HttpStatusCode.OK)) { BaseAddress = _testBaseUri })
            {
                // arrange
                var configurationProvider = new PortabilityServiceConfigurationProvider(httpClient);

                // act
                configurationProvider.Load();

                // assert
                Assert.Equal(9, configurationProvider.GetChildKeys(Enumerable.Empty<string>(), null).Count());
                Assert.Equal(new List<string> { "outputTemplate" }, configurationProvider.GetChildKeys(Enumerable.Empty<string>(), "PortabilityServiceSettings:Serilog:WriteTo:0:Args"));
            }
        }

        [Fact]
        public static void LoadWithBadHttpRequestResultLoadsEmptyConfiguration()
        {
            using (var httpClient = new HttpClient(new TestHttpMessageHandler(@"[{ ""Key"":""testKey"",""Value"":""testValue"" }]", HttpStatusCode.BadRequest)) { BaseAddress = _testBaseUri })
            {
                // arrange
                var configurationProvider = new PortabilityServiceConfigurationProvider(httpClient);

                // act
                configurationProvider.Load();

                // assert
                Assert.Equal(Enumerable.Empty<string>(), configurationProvider.GetChildKeys(Enumerable.Empty<string>(), null));
            }
        }

        private static string BuildMoreComplexJsonResult() =>
            @"[ 
                {""key"":""PortabilityServiceSettings:Serilog"",""value"":null},
                {""key"":""PortabilityServiceSettings:Serilog:WriteTo"",""value"":null},
                {""key"":""PortabilityServiceSettings:Serilog:WriteTo:1"",""value"":null},
                {""key"":""PortabilityServiceSettings:Serilog:WriteTo:1:Name"",""value"":""Debug""},
                {""key"":""PortabilityServiceSettings:Serilog:WriteTo:0"",""value"":null},
                {""key"":""PortabilityServiceSettings:Serilog:WriteTo:0:Name"",""value"":""Console""},
                {""key"":""PortabilityServiceSettings:Serilog:WriteTo:0:Args"",""value"":null},
                {""key"":""PortabilityServiceSettings:Serilog:WriteTo:0:Args:outputTemplate"",""value"":""{Timestamp:HH:mm:ss} {Level:u3} | {RequestId} - {Message}{NewLine}{Exception}""},
                {""key"":""PortabilityServiceSettings:Serilog:MinimumLevel"",""value"":""Information""}
            ]";
    }
}
