// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        private const string LogMessageFormat = "{ActionName}: {ErrorMessage}";

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
            _logger.LogInformation(LogMessageFormat, nameof(Get), _localizer["ReturningAllConfigSettings"].Value);
            return Ok(_configuration.GetSection(_baseConfigSectionName).AsEnumerable());
        }

        [HttpGet("section/{sectionName}")]
        public ObjectResult GetSection(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
            {
                var errorMessage = _localizer["SectionNameNullOrEmpty"].Value;
                _logger.LogError(LogMessageFormat, nameof(GetSection), errorMessage);
                return BadRequest(errorMessage);
            }

            if (SectionNameDoesNotStartsWithBaseSectionName(sectionName))
            {
                var errorMessage = _localizer["SectionNameMustStartWithBaseSectionName", _baseConfigSectionName].Value;
                _logger.LogError(LogMessageFormat, nameof(GetSection), errorMessage);
                return BadRequest(errorMessage);
            }

            _logger.LogInformation(LogMessageFormat, nameof(GetSection), _localizer["ReturningSectionSettings", sectionName].Value);
            return Ok(_configuration.GetSection(sectionName));
        }

        [HttpGet("sectionsettingslist/{sectionName}")]
        public ObjectResult GetSectionSettingsList(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
            {
                var errorMessage = _localizer["SectionNameNullOrEmpty"].Value;
                _logger.LogError(LogMessageFormat, nameof(GetSectionSettingsList), errorMessage);
                return BadRequest(errorMessage);
            }

            if (SectionNameDoesNotStartsWithBaseSectionName(sectionName))
            {
                var errorMessage = _localizer["SectionNameMustStartWithBaseSectionName", _baseConfigSectionName].Value;
                _logger.LogError(LogMessageFormat, nameof(GetSectionSettingsList), errorMessage);
                return BadRequest(errorMessage);
            }

            _logger.LogInformation(LogMessageFormat, nameof(GetSectionSettingsList), _localizer["ReturningSectionSettings", sectionName].Value);
            return Ok(_configuration.GetSection(sectionName).AsEnumerable());
        }

        [HttpGet("setting/{settingName}")]
        public ObjectResult GetSetting(string settingName)
        {
            if (string.IsNullOrEmpty(settingName))
            {
                var errorMessage = _localizer["SettingNameNull"].Value;
                _logger.LogError(LogMessageFormat, nameof(GetSetting), errorMessage);
                return BadRequest(errorMessage);
            }

            var settingValue = _configuration.GetValue<string>(settingName);
            if (settingValue == null)
            {
                var errorMessage = _localizer["SettingNameNotValid", settingName].Value;
                _logger.LogError(LogMessageFormat, nameof(GetSetting), errorMessage);
                return NotFound(errorMessage);
            }

            _logger.LogInformation(LogMessageFormat, nameof(GetSetting), _localizer["ReturningSettingName", settingValue, settingName].Value);
            return Ok(settingValue);
        }

        [HttpGet("environment")]
        public ObjectResult GetEnvironment()
        {
            _logger.LogInformation(LogMessageFormat, nameof(GetEnvironment), _localizer["ReturningEnvironment", _hostingEnvironment.EnvironmentName].Value);
            return Ok(_hostingEnvironment.EnvironmentName);
        }

        /// <summary>
        /// Returns whether the section name starts with the base section name that was used when constructing the ConfigurationController
        /// </summary>
        private bool SectionNameDoesNotStartsWithBaseSectionName(string sectionName) => !sectionName.StartsWith(_baseConfigSectionName, StringComparison.OrdinalIgnoreCase);
    }
}
