// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Models;
using ApiPortVS.Resources;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPortVS.ViewModels
{
    public sealed class OptionsViewModel : NotifyPropertyBase
    {
        private readonly IApiPortService _apiPortService;
        private readonly ITargetMapper _targetMapper;
        private readonly OptionsModel _optionsModel;

        private bool _canRefresh;
        private bool _updating;
        private bool _saveMetadata;
        private string _status;

        public OptionsViewModel(IApiPortService apiPort, ITargetMapper targetMapper, OptionsModel optionsModel)
        {
            _apiPortService = apiPort;
            _targetMapper = targetMapper;
            _optionsModel = optionsModel;

#if DEBUG
            SaveMetadata = false; // ensures telemetry from debug builds doesn't skew our data
#else
            SaveMetadata = true;
#endif
        }

        public bool CanRefresh
        {
            get
            {
                return _canRefresh;
            }
            private set
            {
                UpdateProperty(ref _canRefresh, value);
            }
        }

        public IList<TargetPlatform> Targets
        {
            get
            {
                return _optionsModel.Platforms;
            }
            set
            {
                _optionsModel.Platforms = value;
                OnPropertyUpdated();
                OnPropertyUpdated("UpdatingPlatforms");
            }
        }

        public bool SaveMetadata
        {
            get
            {
                return _saveMetadata;
            }
            set
            {
                UpdateProperty(ref _saveMetadata, value);
            }
        }

        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                UpdateProperty(ref _status, value);
            }
        }

        public bool UpdatingPlatforms
        {
            get
            {
                return _updating;
            }
            set
            {
                UpdateProperty(ref _updating, value);
            }
        }

        public void SaveModel()
        {
            _optionsModel.Save();
        }

        /// <summary>
        /// Matches targets with what is available on the server and updates the options model
        /// </summary>
        /// <returns>Targets that were removed</returns>
        public async Task<IEnumerable<TargetPlatform>> UpdateTargets()
        {
            UpdatingPlatforms = true;
            CanRefresh = false;
            Status = LocalizedStrings.RefreshingPlatforms;

            try
            {
                var targets = await GetTargets();
                var canonicalPlatforms = targets.GroupBy(t => t.Name).Select(t => new TargetPlatform(t));
                var reconciledPlatforms = new List<TargetPlatform>();

                foreach (var canonicalPlatform in canonicalPlatforms)
                {
                    var existingTargetPlatform = _optionsModel.Platforms
                        .FirstOrDefault(t => StringComparer.OrdinalIgnoreCase.Equals(t.Name, canonicalPlatform.Name));

                    var platform = (existingTargetPlatform?.Equals(canonicalPlatform) ?? false)
                                        ? existingTargetPlatform
                                        : canonicalPlatform;

                    foreach (var alias in _targetMapper.Aliases)
                    {
                        foreach (var name in _targetMapper.GetNames(alias))
                        {
                            if (String.Equals(platform.Name, name))
                            {
                                platform.AlternativeNames.Add(alias);
                            }
                        }
                    }

                    reconciledPlatforms.Add(platform);
                }

                // This will sort the platforms on the 'Name' property 
                reconciledPlatforms.Sort();

                var invalidPlatforms = Targets.Where(p => !reconciledPlatforms.Contains(p)).ToList();

                Targets = reconciledPlatforms;

                _optionsModel.Save();

                return invalidPlatforms;
            }
            catch (PortabilityAnalyzerException)
            {
                Status = LocalizedStrings.UnableToContactService;
                CanRefresh = true;

                return Enumerable.Empty<TargetPlatform>();
            }
            finally
            {
                UpdatingPlatforms = false;
            }
        }

        private async Task<IEnumerable<AvailableTarget>> GetTargets()
        {
            var allTargetInfos = await _apiPortService.GetTargetsAsync();

            // We don't want any grouped targets as that option only makes sense for command line
            var targetInfos = allTargetInfos.Response.Where(t => !t.ExpandedTargets.Any());

            var platformNames = new HashSet<string>(targetInfos.Select(t => t.Name), StringComparer.OrdinalIgnoreCase);

            var uniqueFromTargetMap = _targetMapper.Aliases
                .Where(a => !_targetMapper.GetNames(a).Any(platformNames.Contains))
                .Select(a => new AvailableTarget { Name = a });

            return targetInfos.Concat(uniqueFromTargetMap).ToList();
        }
    }
}
