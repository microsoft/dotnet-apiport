// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Models;
using ApiPortVS.ViewModels;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Xunit;

using static System.FormattableString;

namespace ApiPortVS.Tests
{
    public class OptionsViewModelTest
    {
        /// <summary>
        /// Assert that the error only shows up when the excel format is selected
        /// and there are > 15 targets.
        /// </summary>
        [Fact]
        public void OptionsViewModel_TargetCount_Excel()
        {
            var service = Substitute.For<IApiPortService>();
            var progressReporter = Substitute.For<IProgressReporter>();
            var targetMapper = Substitute.For<ITargetMapper>();
            var dependencyFinder = Substitute.For<IDependencyFinder>();
            var reportGenerator = Substitute.For<IReportGenerator>();
            var ignoreAssemblyInfoList = Substitute.For<IEnumerable<IgnoreAssemblyInfo>>();
            var orderer = Substitute.For<IDependencyOrderer>();
            var writer = Substitute.For<IFileWriter>();

            var client = new ApiPortClient(service, progressReporter, targetMapper, dependencyFinder, reportGenerator, ignoreAssemblyInfoList, writer, orderer);
            var options = Substitute.For<IApiPortOptions>();

            var resultFormats = Enumerable.Range(0, 5)
                .Select(x => new SelectedResultFormat
                {
                    DisplayName = Invariant($"{nameof(SelectedResultFormat.DisplayName)} {x}"),
                    FileExtension = Invariant($".{x}"),
                    MimeType = Invariant($"type-{x}")
                })
                .ToList();

            var platforms = Enumerable.Range(0, 20).Select(platform =>
            {
                var name = Invariant($"Platform {platform}");
                var versions = Enumerable.Range(1, 5)
                    .Select(version => new TargetPlatformVersion
                    {
                        PlatformName = name,
                        Version = new Version(version, 0)
                    })
                    .ToList();

                return new TargetPlatform
                {
                    Name = name,
                    Versions = versions
                };
            }).ToList();

            // Select the Excel format and another one.
            resultFormats.Add(new SelectedResultFormat
            {
                DisplayName = "Excel",
                IsSelected = true,
                FileExtension = ".xlsx",
                MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            });
            resultFormats.First().IsSelected = true;

            var model = new OptionsModel
            {
                Formats = resultFormats,
                Platforms = platforms,
                OutputDirectory = Path.GetTempPath()
            };

            using (var viewModel = new OptionsViewModel(service, targetMapper, model))
            {
                var allPlatforms = platforms.SelectMany(x => x.Versions).ToArray();
                int count = 1;

                while (count < (ApiPortClient.MaxNumberOfTargets + 2))
                {
                    allPlatforms[count].IsSelected = true;

                    if (count > ApiPortClient.MaxNumberOfTargets)
                    {
                        Assert.True(viewModel.HasError);
                        Assert.True(!string.IsNullOrEmpty(viewModel.ErrorMessage), "There should be an error message set after too many targets selected and the Excel format is selected.");
                    }
                    else
                    {
                        Assert.False(viewModel.HasError);
                        Assert.True(string.IsNullOrEmpty(viewModel.ErrorMessage));
                    }

                    count++;
                }
            }
        }

        /// <summary>
        /// Assert that the error only shows up when the excel format is selected
        /// and there are > 15 targets.  In this case, we don't expect any errors.
        /// </summary>
        [Fact]
        public void OptionsViewModel_TargetCount_NoExcel()
        {
            var service = Substitute.For<IApiPortService>();
            var progressReporter = Substitute.For<IProgressReporter>();
            var targetMapper = Substitute.For<ITargetMapper>();
            var dependencyFinder = Substitute.For<IDependencyFinder>();
            var reportGenerator = Substitute.For<IReportGenerator>();
            var ignoreAssemblyInfoList = Substitute.For<IEnumerable<IgnoreAssemblyInfo>>();
            var orderer = Substitute.For<IDependencyOrderer>();
            var writer = Substitute.For<IFileWriter>();

            var client = new ApiPortClient(service, progressReporter, targetMapper, dependencyFinder, reportGenerator, ignoreAssemblyInfoList, writer, orderer);
            var options = Substitute.For<IApiPortOptions>();

            var resultFormats = Enumerable.Range(0, 5)
                .Select(x => new SelectedResultFormat
                {
                    DisplayName = Invariant($"{nameof(SelectedResultFormat.DisplayName)} {x}"),
                    FileExtension = Invariant($".{x}"),
                    MimeType = Invariant($"type-{x}")
                })
                .ToList();

            var platforms = Enumerable.Range(0, 20).Select(platform =>
            {
                var name = Invariant($"Platform {platform}");
                var versions = Enumerable.Range(1, 5)
                    .Select(version => new TargetPlatformVersion
                    {
                        PlatformName = name,
                        Version = new Version(version, 0)
                    })
                    .ToList();

                return new TargetPlatform
                {
                    Name = name,
                    Versions = versions
                };
            }).ToList();

            // Select the Excel format and another one.
            foreach (var format in resultFormats.Take(3))
            {
                format.IsSelected = true;
            }

            var model = new OptionsModel
            {
                Formats = resultFormats,
                Platforms = platforms,
                OutputDirectory = Path.GetTempPath()
            };

            using (var viewModel = new OptionsViewModel(service, targetMapper, model))
            {
                var allPlatforms = platforms.SelectMany(x => x.Versions).ToArray();
                int count = 1;

                while (count < (ApiPortClient.MaxNumberOfTargets + 2))
                {
                    allPlatforms[count].IsSelected = true;

                    Assert.False(viewModel.HasError);
                    Assert.True(string.IsNullOrEmpty(viewModel.ErrorMessage));

                    count++;
                }
            }
        }

        /// <summary>
        /// Assert that the error only shows up when the excel format is selected
        /// and there are > 15 targets.  In this case, we don't expect any errors.
        /// </summary>
        [Fact]
        public void OptionsViewModel_TargetCount_WhenSelectedAndNot()
        {
            var service = Substitute.For<IApiPortService>();
            var progressReporter = Substitute.For<IProgressReporter>();
            var targetMapper = Substitute.For<ITargetMapper>();
            var dependencyFinder = Substitute.For<IDependencyFinder>();
            var reportGenerator = Substitute.For<IReportGenerator>();
            var ignoreAssemblyInfoList = Substitute.For<IEnumerable<IgnoreAssemblyInfo>>();
            var orderer = Substitute.For<IDependencyOrderer>();
            var writer = Substitute.For<IFileWriter>();

            var client = new ApiPortClient(service, progressReporter, targetMapper, dependencyFinder, reportGenerator, ignoreAssemblyInfoList, writer, orderer);
            var options = Substitute.For<IApiPortOptions>();

            var platforms = Enumerable.Range(0, 20).Select(platform =>
            {
                var name = Invariant($"Platform {platform}");
                var versions = Enumerable.Range(1, 5)
                    .Select(version => new TargetPlatformVersion
                    {
                        PlatformName = name,
                        Version = new Version(version, 0),
                        IsSelected = true
                    })
                    .ToList();

                return new TargetPlatform
                {
                    Name = name,
                    Versions = versions
                };
            }).ToList();

            var excel = new SelectedResultFormat
            {
                DisplayName = "Excel",
                IsSelected = true,
                FileExtension = ".xlsx",
                MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
            var resultFormats = Enumerable.Range(0, 5)
                .Select(x => new SelectedResultFormat
                {
                    DisplayName = Invariant($"{nameof(SelectedResultFormat.DisplayName)} {x}"),
                    FileExtension = Invariant($".{x}"),
                    MimeType = Invariant($"type-{x}")
                })
                .ToList();

            // Select the Excel format and another one.
            resultFormats.Add(excel);
            foreach (var format in resultFormats.Take(3))
            {
                format.IsSelected = true;
            }

            var model = new OptionsModel
            {
                Formats = resultFormats,
                Platforms = platforms,
                OutputDirectory = Path.GetTempPath()
            };

            using (var viewModel = new OptionsViewModel(service, targetMapper, model))
            {
                // Check that when we select > MaxTargets and Excel is selected, it is in an error state.
                Assert.True(viewModel.HasError);
                Assert.True(!string.IsNullOrEmpty(viewModel.ErrorMessage));

                // Uncheck the excel format
                excel.IsSelected = false;

                // Assert that the viewModel no longer has an error.
                Assert.False(viewModel.HasError);
                Assert.True(string.IsNullOrEmpty(viewModel.ErrorMessage));
            }
        }
    }
}
