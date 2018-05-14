// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Models;
using ApiPortVS.Resources;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPortVS.ViewModels
{
    public sealed class OptionsViewModel : NotifyPropertyBase, IDisposable
    {
        private const string ExcelMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly object _lock = new object();
        private readonly IApiPortService _apiPortService;
        private readonly ITargetMapper _targetMapper;
        private readonly OptionsModel _optionsModel;
        private TargetPlatformVersion[] _currentVersions = new TargetPlatformVersion[0];

        private bool _disposed = false; // To detect redundant calls
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
            PropertyChanged += TargetPlatformAndResultFormatPropertyChanged;

            UpdateResultFormats(optionsModel.Formats);
            UpdateTargetPlatforms(optionsModel.Platforms);
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
                _currentVersions = value?.SelectMany(x => x.Versions).ToArray() ?? new TargetPlatformVersion[0];
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
            var response = await _apiPortService.GetResultFormatsAsync().ConfigureAwait(false);
            var current = new HashSet<string>(Formats.Where(f => f.IsSelected).Select(f => f.MimeType), StringComparer.OrdinalIgnoreCase);

            if (!current.Any())
            {
                var defaultResultFormat = await _apiPortService.GetDefaultResultFormatAsync().ConfigureAwait(false);
                current.Add(defaultResultFormat.MimeType);
            }

            var formats = response.Select(f => new SelectedResultFormat(f, current.Contains(f.MimeType))).ToList();

            UpdateResultFormats(formats);
        }

        /// <summary>
        /// Removes existing event handlers from current <see cref="Formats"/>
        /// and then adds event handlers to the new formats before setting them.
        /// </summary>
        private void UpdateResultFormats(IList<SelectedResultFormat> formats)
        {
            lock (_lock)
            {
                foreach (var format in Formats)
                {
                    format.PropertyChanged -= TargetPlatformAndResultFormatPropertyChanged;
                }

                Formats = formats;

                foreach (var format in Formats)
                {
                    format.PropertyChanged += TargetPlatformAndResultFormatPropertyChanged;
                }
            }
        }

        /// <summary>
        /// Matches targets with what is available on the server and updates the options model
        /// </summary>
        /// <returns>Targets that were removed</returns>
        private async Task UpdateTargetsAsync()
        {
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

            UpdateTargetPlatforms(canonicalPlatforms);
        }

        private void UpdateTargetPlatforms(IEnumerable<TargetPlatform> targetPlatforms)
        {
            lock (_lock)
            {
                // Remove any existing subscribed events
                foreach (var platform in _currentVersions)
                {
                    platform.PropertyChanged -= TargetPlatformAndResultFormatPropertyChanged;
                }

                var reconciledPlatforms = new List<TargetPlatform>();

                foreach (var canonicalPlatform in targetPlatforms)
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
                            if (String.Equals(platform.Name, name, StringComparison.Ordinal))
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

                foreach (var platform in _currentVersions)
                {
                    platform.PropertyChanged += TargetPlatformAndResultFormatPropertyChanged;
                }
            }
        }

        private void TargetPlatformAndResultFormatPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                // PropertyName is "" or null when the entire object has been updated
                case null:
                case "":
                case nameof(TargetPlatformVersion.IsSelected):
                case nameof(Targets):
                case nameof(Formats):
                    var containsExcel = Formats.Any(x => x.IsSelected && string.Equals(x.MimeType, ExcelMimeType, StringComparison.OrdinalIgnoreCase));

                    if (_currentVersions.Count(x => x.IsSelected) > ApiPortClient.MaxNumberOfTargets && containsExcel)
                    {
                        HasError = true;
                        ErrorMessage = string.Format(CultureInfo.CurrentCulture, LocalizedStrings.TooManyTargetsMessage, ApiPortClient.MaxNumberOfTargets);
                    }
                    else
                    {
                        HasError = false;
                        ErrorMessage = string.Empty;
                    }
                    break;
            }
        }

        private async Task<IEnumerable<AvailableTarget>> GetTargetsAsync()
        {
            var allTargetInfos = await _apiPortService.GetTargetsAsync().ConfigureAwait(false);

            // We don't want any grouped targets as that option only makes sense for command line
            var targetInfos = allTargetInfos.Where(t => !t.ExpandedTargets.Any());

            var platformNames = new HashSet<string>(targetInfos.Select(t => t.Name), StringComparer.OrdinalIgnoreCase);

            var uniqueFromTargetMap = _targetMapper.Aliases
                .Where(a => !_targetMapper.GetNames(a).Any(platformNames.Contains))
                .Select(a => new AvailableTarget { Name = a });

            return targetInfos.Concat(uniqueFromTargetMap).ToList();
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                PropertyChanged -= TargetPlatformAndResultFormatPropertyChanged;

                if (_currentVersions != null)
                {
                    foreach (var platform in _currentVersions)
                    {
                        platform.PropertyChanged -= TargetPlatformAndResultFormatPropertyChanged;
                    }
                }

                _currentVersions = null;

                if (_optionsModel.Formats != null)
                {
                    foreach (var format in _optionsModel.Formats)
                    {
                        format.PropertyChanged -= TargetPlatformAndResultFormatPropertyChanged;
                    }
                }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
