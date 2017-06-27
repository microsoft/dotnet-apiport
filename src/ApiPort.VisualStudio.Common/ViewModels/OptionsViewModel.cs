﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Models;
using ApiPortVS.Resources;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPortVS.ViewModels
{
    public sealed class OptionsViewModel : NotifyPropertyBase, IDisposable
    {
        private readonly IApiPortService _apiPortService;
        private readonly ITargetMapper _targetMapper;
        private readonly OptionsModel _optionsModel;
        private TargetPlatformVersion[] _currentVersions = new TargetPlatformVersion[0];

        private bool _disposed  = false; // To detect redundant calls
        private bool _hasError;
        private bool _updating;
        private bool _saveMetadata;

        public OptionsViewModel(IApiPortService apiPort, ITargetMapper targetMapper, OptionsModel optionsModel)
        {
            _apiPortService = apiPort;
            _targetMapper = targetMapper;
            _optionsModel = optionsModel;

            InvalidTargets = Array.Empty<TargetPlatform>();

#if DEBUG
            SaveMetadata = false; // ensures telemetry from debug builds doesn't skew our data
#else
            SaveMetadata = true;
#endif
        }

        public IList<SelectedResultFormat> Formats
        {
            get { return _optionsModel.Formats; }
            set
            {
                _optionsModel.Formats = value;
                OnPropertyUpdated();
            }
        }

        public IList<TargetPlatform> Targets
        {
            get { return _optionsModel.Platforms; }
            set
            {
                _optionsModel.Platforms = value;
                OnPropertyUpdated();
            }
        }

        public bool SaveMetadata
        {
            get { return _saveMetadata; }
            set { UpdateProperty(ref _saveMetadata, value); }
        }

        public bool UpdatingPlatforms
        {
            get { return _updating; }
            set { UpdateProperty(ref _updating, value); }
        }

        public string OutputDirectory
        {
            get { return _optionsModel.OutputDirectory; }
            set
            {
                _optionsModel.OutputDirectory = value;
                OnPropertyUpdated();
            }
        }

        public string DefaultOutputName
        {
            get { return _optionsModel.DefaultOutputName; }
            set
            {
                _optionsModel.DefaultOutputName = value;
                OnPropertyUpdated();
            }
        }

        public bool HasError
        {
            get { return _hasError; }
            set { UpdateProperty(ref _hasError, value); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { UpdateProperty(ref _errorMessage, value); }
        }


        public void Save() => _optionsModel.Save();

        public IList<TargetPlatform> InvalidTargets { get; set; }

        public async Task UpdateAsync(bool force = false)
        {
            if (!force && _optionsModel.LastUpdate.AddDays(1) > DateTimeOffset.Now)
            {
                return;
            }

            UpdatingPlatforms = true;
            HasError = false;

            try
            {
                await UpdateTargetsAsync().ConfigureAwait(false);
                await UpdateResultsAsync().ConfigureAwait(false);

                _optionsModel.LastUpdate = DateTimeOffset.Now;

                _optionsModel.Save();
            }
            catch (PortabilityAnalyzerException)
            {
                HasError = true;
                ErrorMessage = LocalizedStrings.UnableToContactService;
            }
            finally
            {
                UpdatingPlatforms = false;
            }
        }

        private async Task UpdateResultsAsync()
        {
            var formats = await _apiPortService.GetResultFormatsAsync().ConfigureAwait(false);
            var current = new HashSet<string>(Formats.Where(f => f.IsSelected).Select(f => f.MimeType), StringComparer.OrdinalIgnoreCase);

            if (current.Count == 0)
            {
                const string DefaultMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                current.Add(DefaultMimeType);
            }

            Formats = formats.Response.Select(f => new SelectedResultFormat
            {
                DisplayName = f.DisplayName,
                FileExtension = f.FileExtension,
                MimeType = f.MimeType,
                IsSelected = current.Contains(f.MimeType)
            }).ToList();
        }

        /// <summary>
        /// Matches targets with what is available on the server and updates the options model
        /// </summary>
        /// <returns>Targets that were removed</returns>
        private async Task UpdateTargetsAsync()
        {
            // Remove any existing subscribed events
            foreach (var platform in _currentVersions)
            {
                platform.PropertyChanged -= TargetPlatformVersionChanged;
            }

            var targets = await GetTargetsAsync().ConfigureAwait(false);
            var canonicalPlatforms = targets.GroupBy(t => t.Name).Select(t =>
            {
                return new TargetPlatform
                {
                    Name = t.Key,

                    Versions = t.Select(v => new TargetPlatformVersion
                    {
                        PlatformName = t.Key,
                        Version = v.Version,
                        IsSelected = v.IsSet
                    })
                    .OrderBy(v => v.Version)
                    .ToList()
                };
            });

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

            InvalidTargets = Targets.Where(p => !reconciledPlatforms.Contains(p)).ToList();
            Targets = reconciledPlatforms;
            _currentVersions = Targets.SelectMany(x => x.Versions).ToArray();

            foreach (var platform in _currentVersions)
            {
                platform.PropertyChanged += TargetPlatformVersionChanged;
            }
        }

        private void TargetPlatformVersionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!string.Equals(nameof(TargetPlatformVersion.IsSelected), e.PropertyName))
            {
                return;
            }

            if (_currentVersions.Count(x => x.IsSelected) > ApiPortClient.MaxNumberOfTargets)
            {
                HasError = true;
                ErrorMessage = string.Format(Microsoft.Fx.Portability.Resources.LocalizedStrings.TooManyTargetsMessage, ApiPortClient.MaxNumberOfTargets);
            }
            else
            {
                HasError = false;
            }
        }

        private async Task<IEnumerable<AvailableTarget>> GetTargetsAsync()
        {
            var allTargetInfos = await _apiPortService.GetTargetsAsync().ConfigureAwait(false);

            // We don't want any grouped targets as that option only makes sense for command line
            var targetInfos = allTargetInfos.Response.Where(t => !t.ExpandedTargets.Any());

            var platformNames = new HashSet<string>(targetInfos.Select(t => t.Name), StringComparer.OrdinalIgnoreCase);

            var uniqueFromTargetMap = _targetMapper.Aliases
                .Where(a => !_targetMapper.GetNames(a).Any(platformNames.Contains))
                .Select(a => new AvailableTarget { Name = a });

            return targetInfos.Concat(uniqueFromTargetMap).ToList();
        }

        private void Dispose(bool disposing)
        {
            if (_disposed )
            {
                return;
            }

            if (disposing)
            {
                if (_currentVersions != null)
                {
                    foreach (var platform in _currentVersions)
                    {
                        platform.PropertyChanged -= TargetPlatformVersionChanged;
                    }
                }

                _currentVersions = null;
            }

            _disposed  = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
