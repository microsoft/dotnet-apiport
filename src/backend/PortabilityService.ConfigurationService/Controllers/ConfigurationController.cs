// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ConfigurationControllerResources = PortabilityService.ConfigurationService.Resources.Controllers.ConfigurationController;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;

namespace PortabilityService.ConfigurationService.Controllers
{
    [Route("api/[controller]")]
    public class ConfigurationController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IStringLocalizer<ConfigurationController> _localizer;
        private readonly ILogger _logger;
        private readonly string _baseConfigSectionName;

        public ConfigurationController(IConfiguration configuration,
                                       IHostingEnvironment hostingEnvironment,
                                       IStringLocalizer<ConfigurationController> localizer,
                                       ILogger<ConfigurationController> logger = null,
                                       string baseConfigSectionName = "PortabilityServiceSettings")
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _logger = logger ?? new NullLogger<ConfigurationController>();

            if (string.IsNullOrEmpty(baseConfigSectionName))
            {
                throw new ArgumentNullException(nameof(baseConfigSectionName));
            }
            else
            {
                _baseConfigSectionName = baseConfigSectionName;
            }
        }

        [HttpGet]
        public ObjectResult Get()
        {
            _logger.LogInformation(BuildLogMessage(nameof(Get), _localizer[nameof(ConfigurationControllerResources.ReturningAllConfigSettings)].Value));
            return Ok(_configuration.GetSection(_baseConfigSectionName).AsEnumerable());
        }

        [HttpGet("section/{sectionName}")]
        public ObjectResult GetSection(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
            {
                var errorMessage = _localizer[nameof(ConfigurationControllerResources.SectionNameNullOrEmpty)].Value;
                _logger.LogError(BuildLogMessage(nameof(GetSection), errorMessage));
                return BadRequest(errorMessage);
            }

            _logger.LogInformation(BuildLogMessage(nameof(GetSection), _localizer[nameof(ConfigurationControllerResources.ReturningSectionSettings), sectionName].Value));
            return Ok(_configuration.GetSection(sectionName));
        }

        [HttpGet("sectionsettingslist/{sectionName}")]
        public ObjectResult GetSectionSettingsList(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
            {
                var errorMessage = _localizer[nameof(ConfigurationControllerResources.SectionNameNullOrEmpty)].Value;
                _logger.LogError(BuildLogMessage(nameof(GetSectionSettingsList), errorMessage));
                return BadRequest(errorMessage);
            }

            _logger.LogInformation(BuildLogMessage(nameof(GetSection), _localizer[nameof(ConfigurationControllerResources.ReturningSectionSettings), sectionName].Value));
            return Ok(_configuration.GetSection(sectionName).AsEnumerable());
        }

        [HttpGet("setting/{settingName}")]
        public ObjectResult GetSetting(string settingName)
        {
            if (string.IsNullOrEmpty(settingName))
            {
                var errorMessage = _localizer[nameof(ConfigurationControllerResources.SettingNameNull)].Value;
                _logger.LogError(BuildLogMessage(nameof(GetSetting), errorMessage));
                return BadRequest(errorMessage);
            }

            var settingValue = _configuration.GetValue<string>(settingName);
            if (settingValue == null)
            {
                var errorMessage = _localizer[nameof(ConfigurationControllerResources.SettingNameNotValid), settingName].Value;
                _logger.LogError(BuildLogMessage(nameof(GetSetting), errorMessage));
                return NotFound(errorMessage);
            }

            _logger.LogInformation(BuildLogMessage(nameof(GetSetting), _localizer[nameof(ConfigurationControllerResources.ReturningSettingName), settingValue, settingName].Value));
            return Ok(settingValue);
        }

        [HttpGet("environment")]
        public ObjectResult GetEnvironment()
        {
            _logger.LogInformation(BuildLogMessage(nameof(GetEnvironment), _localizer[nameof(ConfigurationControllerResources.ReturningEnvironment), _hostingEnvironment.EnvironmentName].Value));
            return Ok(_hostingEnvironment.EnvironmentName);
        }

        /// <summary>
        /// Builds up a message that will be returned with an http result
        /// </summary>
        private static string BuildLogMessage(string methodName, string message) => $"{methodName}: {message}";
    }
}
