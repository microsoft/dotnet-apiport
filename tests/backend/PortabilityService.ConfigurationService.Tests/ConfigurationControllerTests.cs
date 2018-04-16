// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using PortabilityService.ConfigurationService.Controllers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using Xunit;

namespace PortabilityService.ConfigurationService.Tests
{
    public partial class ConfigurationControllerTests
    {
        private const string NonExisting = "NonExisting";

        // string constants representing logged messages from the controller method calls
        private const string BaseConfigSectionName = "Root";
        private const string ReturningAllConfigSettings = "Returning all the configuration settings.";
        private const string SectionNameNullOrEmpty = "Section name cannot be null or empty!";
        private const string SettingNameNullOrEmpty = "Setting name cannot be null or empty!";
        private const string ReturningSectionSettingsMessage = "Returning section settings for section {0}.";
        private const string NonExistingSettingMessage = "NonExistingSetting is not a valid configuration setting!";
        private const string ReturningSettingName = "Returning value of {0} for setting {1}.";
        private const string ReturningEnvironmentMessage = "Returning environment setting of {0}.";

        // Fields used to create the string localizer, httpcontext and also construction of ConfigurationController in tests
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IStringLocalizer<ConfigurationController> _stringLocalizer;
        private readonly IEnumerable<KeyValuePair<string, string>> _allConfigSettings;

        public ConfigurationControllerTests()
        {
            _configuration = CreateTestIConfiguration();
            _serviceProvider = CreateTestServiceProvider();
            _stringLocalizer = _serviceProvider.GetRequiredService<IStringLocalizer<ConfigurationController>>();
            _allConfigSettings = CreateAllConfigEnum();
        }

        [Fact]
        public void CtorWithNullIConfigurationThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationController(null, new TestHostingEnvironment(), _stringLocalizer));
        }

        [Fact]
        public void CtorWithNullIHostingEnvironmentThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationController(CreateTestIConfiguration(), null, _stringLocalizer));
        }

        [Fact]
        public static void CtorWithNullIStringLocalizerThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationController(CreateTestIConfiguration(), new TestHostingEnvironment(), null));
        }

        [Fact]
        public void CtorWithNullBaseConfigSectionNameThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigurationController(CreateTestIConfiguration(), new TestHostingEnvironment(), _stringLocalizer, null, null));
        }

        [Fact]
        public void GetReturnsAllConfigSettings()
        {
            // arrange
            var testLogger = CreateTestLogger();
            var controller = CreateTestConfigurationController(_configuration, _stringLocalizer, testLogger);

            // act
            var result = controller.Get();

            // assert
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            Assert.Equal(new List<string> { testLogger.GetLogString(LogLevel.Information, nameof(ConfigurationController.Get), ReturningAllConfigSettings) }, testLogger.LoggedMessages);
            Assert.Equal(_allConfigSettings, result.Value);
        }

        /// <summary>
        /// Creates an enumerable that would be all the settings expected to be returned from a Get call to the ConfigController
        /// </summary>
        private static IEnumerable<KeyValuePair<string, string>> CreateAllConfigEnum() => new List<KeyValuePair<string, string>>
        {
            { new KeyValuePair<string, string>("Root", null)},
            { new KeyValuePair<string, string>("Root:SettingString0", "Value for setting string 0")},
            { new KeyValuePair<string, string>("Root:RootGroup2", null)},
            { new KeyValuePair<string, string>("Root:RootGroup2:Setting2Url", "http://portabilityService")},
            { new KeyValuePair<string, string>("Root:RootGroup2:Setting1Url", "http://portabilityconfigservice")},
            { new KeyValuePair<string, string>("Root:RootGroup1", null)},
            { new KeyValuePair<string, string>("Root:RootGroup1:SettingBool", "False")},
            { new KeyValuePair<string, string>("Root:RootGroup1:Group2", null)},
            { new KeyValuePair<string, string>("Root:RootGroup1:Group2:Group1", null)},
            { new KeyValuePair<string, string>("Root:RootGroup1:Group2:Group1:SettingString2", "Value for setting string 2")},
            { new KeyValuePair<string, string>("Root:RootGroup1:Group1", null)},
            { new KeyValuePair<string, string>("Root:RootGroup1:Group1:Group1", null)},
            { new KeyValuePair<string, string>("Root:RootGroup1:Group1:Group1:SettingString1", "Value for setting string 1")},
        };

        [Theory, MemberData(nameof(NullOrEmptyTestData))]
        public void GetSectionWithNullOrEmptySectionNameReturnsBadRequest(string sectionName)
        {
            // arrange
            var testLogger = CreateTestLogger();
            var controller = CreateTestConfigurationController(_configuration, _stringLocalizer, testLogger);

            // act
            var result = controller.GetSection(sectionName);

            // assert
            VerifyCallResult(testLogger, result, StatusCodes.Status400BadRequest, SectionNameNullOrEmpty, new List<string> { testLogger.GetLogString(LogLevel.Error, nameof(ConfigurationController.GetSection), SectionNameNullOrEmpty) });
        }

        [Theory, MemberData(nameof(NullOrEmptyTestData))]
        public void GetSettingWithNullOrEmptySectionNameReturnsBadRequest(string settingName)
        {
            // arrange
            var testLogger = CreateTestLogger();
            var controller = CreateTestConfigurationController(_configuration, _stringLocalizer, testLogger);

            // act
            var result = controller.GetSetting(settingName);

            // assert
            VerifyCallResult(testLogger, result, StatusCodes.Status400BadRequest, SettingNameNullOrEmpty, new List<string> { testLogger.GetLogString(LogLevel.Error, nameof(ConfigurationController.GetSetting), SettingNameNullOrEmpty) });
        }

        [Theory, MemberData(nameof(NullOrEmptyTestData))]
        public void GetSectionSettingsListWithNullOrEmptySectionNameReturnsBadRequest(string sectionName)
        {
            // arrange
            var testLogger = CreateTestLogger();
            var controller = CreateTestConfigurationController(_configuration, _stringLocalizer, testLogger);

            // act
            var result = controller.GetSectionSettingsList(sectionName);

            // assert
            VerifyCallResult(testLogger, result, StatusCodes.Status400BadRequest, SectionNameNullOrEmpty, new List<string> { testLogger.GetLogString(LogLevel.Error, nameof(ConfigurationController.GetSectionSettingsList), SectionNameNullOrEmpty) });
        }

        public static IEnumerable<object[]> NullOrEmptyTestData() => new[] { new object[] { null }, new object[] { string.Empty } };

        [Theory, MemberData(nameof(GetSectionTestData))]
        public void GetSectionReturnsConfigurationSection(string sectionName, IEnumerable<string> childrenKeys)
        {
            // arrange
            var testLogger = CreateTestLogger();
            var controller = CreateTestConfigurationController(_configuration, _stringLocalizer, testLogger);

            // act
            var result = controller.GetSection(sectionName);

            // assert
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            Assert.Equal(new List<string> { testLogger.GetLogString(LogLevel.Information, nameof(ConfigurationController.GetSection), string.Format(CultureInfo.InvariantCulture, ReturningSectionSettingsMessage, sectionName)) }, testLogger.LoggedMessages);
            VerifyGetSection((IConfigurationSection)result.Value, sectionName, childrenKeys);
        }

        public static IEnumerable<object[]> GetSectionTestData() =>
            new[]
            {
                new object[] { NonExisting, new List<string>() },
                new object[] { "Root", new List<string> { "RootGroup1", "RootGroup2", "SettingString0" } },
                new object[] { "Root:RootGroup2", new List<string> { "Setting1Url", "Setting2Url" } }
            };

        [Fact]
        public void GetSectionSettingsListNonExistingSectionReturnsEmptyList()
        {
            // arrange
            var testLogger = CreateTestLogger();
            var controller = CreateTestConfigurationController(_configuration, _stringLocalizer, testLogger);

            // act
            var objectResult = controller.GetSectionSettingsList(NonExisting);

            // assert
            var keyValueResult = ((IEnumerable<KeyValuePair<string, string>>)objectResult.Value).First();
            Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);
            Assert.Equal(new List<string> { testLogger.GetLogString(LogLevel.Information, nameof(ConfigurationController.GetSection), string.Format(CultureInfo.InvariantCulture, ReturningSectionSettingsMessage, NonExisting)) }, testLogger.LoggedMessages);
            Assert.Equal(NonExisting, keyValueResult.Key);
            Assert.Null(keyValueResult.Value);
        }

        [Theory]
        [InlineData("Root")]
        [InlineData("Root:RootGroup2")]
        public void GetSectionSettingsListReturnsSettings(string sectionName)
        {
            // arrange
            var testLogger = CreateTestLogger();
            var controller = CreateTestConfigurationController(_configuration, _stringLocalizer, testLogger);

            // act
            var result = controller.GetSectionSettingsList(sectionName);

            // assert
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            Assert.Equal(new List<string> { testLogger.GetLogString(LogLevel.Information, nameof(ConfigurationController.GetSection), string.Format(CultureInfo.InvariantCulture, ReturningSectionSettingsMessage, sectionName)) }, testLogger.LoggedMessages);
            Assert.Equal(result.Value, CreateAllConfigEnum().Where(s => s.Key.StartsWith(sectionName, StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public void GetNonExistingSettingReturnsNotFoundRequest()
        {
            // arrange
            var testLogger = CreateTestLogger();
            var controller = CreateTestConfigurationController(_configuration, _stringLocalizer, testLogger);

            // act
            var result = controller.GetSetting("NonExistingSetting");

            // assert
            VerifyCallResult(testLogger, result, StatusCodes.Status404NotFound, NonExistingSettingMessage, new List<string> { testLogger.GetLogString(LogLevel.Error, nameof(ConfigurationController.GetSetting), NonExistingSettingMessage) });
        }

        [Theory]
        [InlineData("Root:SettingString0", "Value for setting string 0")]
        [InlineData("Root:RootGroup1:Group1:Group1:SettingString1", "Value for setting string 1")]
        [InlineData("Root:RootGroup1:Group2:Group1:SettingString2", "Value for setting string 2")]
        public void GetSettingReturnsSettingValue(string settingName, string settingValue)
        {
            // arrange
            var testLogger = CreateTestLogger();
            var controller = CreateTestConfigurationController(_configuration, _stringLocalizer, testLogger);

            // act
            var result = controller.GetSetting(settingName);

            // assert
            VerifyCallResult(testLogger, result, StatusCodes.Status200OK, settingValue, new List<string> { testLogger.GetLogString(LogLevel.Information, nameof(ConfigurationController.GetSetting), string.Format(CultureInfo.InvariantCulture, ReturningSettingName, settingValue, settingName)) });
        }

        [Fact]
        public void GetEnvironmentNameReturnsEnvironmentName()
        {
            // arrange
            const string EnvironmentName = "Staging";
            var testLogger = CreateTestLogger();
            var controller = CreateTestConfigurationController(_configuration, _stringLocalizer, testLogger, EnvironmentName);

            // act
            var result = controller.GetEnvironment();

            // assert
            VerifyCallResult(testLogger, result, StatusCodes.Status200OK, EnvironmentName, new List<string> { testLogger.GetLogString(LogLevel.Information, nameof(ConfigurationController.GetEnvironment), string.Format(CultureInfo.InvariantCulture, ReturningEnvironmentMessage, EnvironmentName)) });
        }

        /// <summary>
        /// Verifies the expected child sections that should be returned for a particular configuration section 
        /// </summary>
        private static void VerifyGetSection(IConfigurationSection configSection, string sectionName, IEnumerable<string> expectedChildrenKeys)
        {
            VerifySectionKey(sectionName, configSection.Key);

            var actualChildrenKeys = new List<string>();
            foreach (var child in configSection.GetChildren())
            {
                actualChildrenKeys.Add(child.Key);
            }

            Assert.Equal(expectedChildrenKeys, actualChildrenKeys);
        }

        /// <summary>
        /// Verifies a call to one of the ConfigurationService methods
        /// </summary>
        private static void VerifyCallResult(TestLogger<ConfigurationController> testLogger, Microsoft.AspNetCore.Mvc.ObjectResult result, int statusCode, string expectedResultValue, IEnumerable<string> expectedLoggedMessages)
        {
            Assert.Equal(statusCode, result.StatusCode);
            Assert.Equal(expectedLoggedMessages, testLogger.LoggedMessages);
            Assert.Equal(expectedResultValue, result.Value);
        }

        /// <summary>
        /// Verifies a section key name
        /// </summary>
        private static void VerifySectionKey(string sectionName, string sectionKey)
        {
            var locationAfterLastColonIndex = sectionName.LastIndexOf(":", StringComparison.Ordinal) + 1;
            Assert.Equal(sectionKey, sectionName.Substring(locationAfterLastColonIndex, sectionName.Length - locationAfterLastColonIndex));
        }

        /// <summary>
        /// Returns a test service provider
        /// </summary>
        private static ServiceProvider CreateTestServiceProvider()
        {
            var services = new ServiceCollection();

            // Add resources services
            var resourceManager = new ResourceManager("PortabilityService.ConfigurationService.Resources.Controllers.ConfigurationController",
                      typeof(Startup).GetTypeInfo().Assembly);

            services.AddSingleton(typeof(IStringLocalizer<ConfigurationController>),
                                      new StringLocalizer<ConfigurationController>(new TestStringLocalizerFactory(resourceManager, CreateTestLogger())));

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Returns a test configuration provider
        /// </summary>
        private static IConfiguration CreateTestIConfiguration()
        {
            var assemblyLocation = typeof(ConfigurationControllerTests).Assembly.Location;
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(assemblyLocation.Substring(0, assemblyLocation.LastIndexOf(Path.DirectorySeparatorChar)))
                .AddJsonFile("TestConfigSettings.json");

            return configBuilder.Build();
        }

        /// <summary>
        /// Creates a test instance of the ConfigurationController
        /// </summary>
        private ConfigurationController CreateTestConfigurationController(IConfiguration configuration,
                                                                          IStringLocalizer<ConfigurationController> localizer,
                                                                          TestLogger<ConfigurationController> testLogger,
                                                                          string environmentName = "Development")
        {
            var controller = new ConfigurationController(configuration, new TestHostingEnvironment { EnvironmentName = environmentName }, localizer, testLogger, BaseConfigSectionName);

            // Set mock HTTP context (including DI service provider)
            controller.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                RequestServices = _serviceProvider
            };

            return controller;
        }

        /// <summary>
        /// Returns a TestLogger to be used by the tests
        /// </summary>
        private static TestLogger<ConfigurationController> CreateTestLogger() => new TestLogger<ConfigurationController>();

        /// <summary>
        /// Returns an IStringLocalizerFactory to be used in adding the mocked IStringLocalizer service
        /// </summary>
        private class TestStringLocalizerFactory : IStringLocalizerFactory
        {
            private readonly ResourceManager _resourceManager;
            private readonly string _resourcePath;
            private readonly ILogger _testLogger;

            public TestStringLocalizerFactory(ResourceManager resManager, ILogger testLogger, string resourcePath = "Resources")
            {
                _resourceManager = resManager;
                _resourcePath = resourcePath;
                _testLogger = testLogger;
            }

            public IStringLocalizer Create(Type resourceSource) => new ResourceManagerStringLocalizer(_resourceManager,
                                                                                                      resourceSource.GetTypeInfo().Assembly,
                                                                                                      _resourcePath,
                                                                                                      new ResourceNamesCache(),
                                                                                                      _testLogger);

            public IStringLocalizer Create(string baseName, string location) => throw new NotImplementedException("Not used");
        }
    }
}
