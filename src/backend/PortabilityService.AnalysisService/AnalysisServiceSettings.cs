// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using System;

namespace PortabilityService.AnalysisService
{
    internal class AnalysisServiceSettings : IServiceSettings
    {
        private const string CatalogStorageConnectionKeyName = "CatalogStorageConnection";
        private readonly IConfiguration _configuration;

        public AnalysisServiceSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TimeSpan UpdateFrequency
        {
            get
            {
                var updateFrequencyInMinutes = _configuration["UpdateFrequencyInMinutes"];
                var updateFrequency = TimeSpan.MinValue;

                if (TimeSpan.TryParse(updateFrequencyInMinutes, out updateFrequency))
                {
                    return updateFrequency;
                }

                return TimeSpan.FromMinutes(15);
            }
        }

        public string DefaultResultFormat => _configuration[nameof(DefaultResultFormat)];

        public string DefaultTargets => _configuration[nameof(DefaultVersions)];

        public string DefaultVersions => _configuration[nameof(DefaultVersions)];

        public CloudStorageAccount StorageAccount
        {
            get
            {
                var storageConnectionString = _configuration.GetConnectionString(CatalogStorageConnectionKeyName);

                return CloudStorageAccount.Parse(storageConnectionString);
            }
        }

        public string TargetGroups => _configuration[nameof(TargetGroups)];

        public bool UnionAspNetWithNetCore =>
            bool.TryParse(_configuration[nameof(UnionAspNetWithNetCore)], out var settingValue) ? settingValue : false;

        public string BreakingChangesPath => _configuration[nameof(BreakingChangesPath)];

        public string RecommendedChangesPath => _configuration[nameof(RecommendedChangesPath)];
    }
}
