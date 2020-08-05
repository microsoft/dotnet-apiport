// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Fx.Portability.Tests
{
    public class ApiPortClientTests
    {
        private static Task<ServiceResponse<T>> CreateResponse<T>(T result)
        {
            var response = new ServiceResponse<T>(result, EndpointStatus.Alive);

            return Task.FromResult(response);
        }

        [Fact]
        public async Task ListTargetsTest()
        {
            var targets = new List<AvailableTarget> { new AvailableTarget { Name = "Target1" }, new AvailableTarget { Name = "Target2" } };
            var progressReporter = Substitute.For<IProgressReporter>();
            var targetMapper = Substitute.For<ITargetMapper>();
            var dependencyFinder = Substitute.For<IDependencyFinder>();
            var reportGenerator = Substitute.For<IReportGenerator>();
            var ignoreAssemblyInfoList = Substitute.For<IEnumerable<IgnoreAssemblyInfo>>();
            var orderer = Substitute.For<IDependencyOrderer>();
            var writer = Substitute.For<IFileWriter>();

            var apiPortService = Substitute.For<IApiPortService>();
            apiPortService.GetTargetsAsync().Returns(CreateResponse<IEnumerable<AvailableTarget>>(targets.AsReadOnly()));

            var client = new ApiPortClient(apiPortService, progressReporter, targetMapper, dependencyFinder, reportGenerator, ignoreAssemblyInfoList, writer, orderer);

            var actualTargets = await client.GetTargetsAsync();

            Assert.Equal<AvailableTarget[]>(actualTargets.OrderBy(k => k.Name).ToArray(), targets.OrderBy(k => k.Name).ToArray());
        }

        [Fact]
        public static async Task WriteAnalysisReports_ThrowsOnInvalidOptions_TargetCount()
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
            options.Targets.Returns(Enumerable.Range(0, 16).Select(x => x.ToString(CultureInfo.CurrentCulture)));
            options.OutputFormats.Returns(new[] { "HTML", "Excel" });

            await Assert.ThrowsAsync<InvalidApiPortOptionsException>(() => client.WriteAnalysisReportsAsync(options));
            await Assert.ThrowsAsync<InvalidApiPortOptionsException>(() => client.WriteAnalysisReportsAsync(options, true));
        }

        [Fact]
        public static async Task SingleAssemblyFile()
        {
            var files = new[]
            {
                new AssemblyInfo
                {
                    AssemblyIdentity = "file1",
                    FileVersion = "1.0.0",
                    Location = "file1",
                    IsExplicitlySpecified = false
                }
            };

            await UserAssemblyTestsAsync(files);
        }

        [Fact]
        public static async Task TwoAssemblyFiles()
        {
            var files = new[]
            {
                new AssemblyInfo
                {
                    AssemblyIdentity = "file1",
                    FileVersion = "1.0.0",
                    Location = "file1",
                    IsExplicitlySpecified = false
                },
                new AssemblyInfo
                {
                    AssemblyIdentity = "file2",
                    FileVersion = "1.0.0",
                    Location = "file2",
                    IsExplicitlySpecified = true
                }
            };

            await UserAssemblyTestsAsync(files);
        }

        [Fact]
        public static async Task DuplicateAssemblyFile()
        {
            var files = new[]
            {
                new AssemblyInfo
                {
                    AssemblyIdentity = "file1",
                    FileVersion = "1.0.0",
                    Location = "file1",
                    IsExplicitlySpecified = false
                },
                new AssemblyInfo
                {
                    AssemblyIdentity = "file1",
                    FileVersion = "1.0.0",
                    Location = "file1",
                    IsExplicitlySpecified = true
                }
            };

            await UserAssemblyTestsAsync(files);
        }

        [Fact]
        public static async Task DuplicateNameDifferentVersion()
        {
            var files = new[]
            {
                new AssemblyInfo
                {
                    AssemblyIdentity = "file1",
                    FileVersion = "1.0.0",
                    Location = "file1",
                    IsExplicitlySpecified = true
                },
                new AssemblyInfo
                {
                    AssemblyIdentity = "file3",
                    FileVersion = "1.0.1",
                    Location = "file3",
                    IsExplicitlySpecified = false
                }
            };

            await UserAssemblyTestsAsync(files);
        }

        private static async Task UserAssemblyTestsAsync(IEnumerable<AssemblyInfo> assemblies)
        {
            var service = Substitute.For<IApiPortService>();
            var progressReporter = Substitute.For<IProgressReporter>();
            var targetMapper = Substitute.For<ITargetMapper>();
            var dependencyFinder = Substitute.For<IDependencyFinder>();
            var reportGenerator = Substitute.For<IReportGenerator>();
            var ignoreAssemblyInfoList = Substitute.For<IEnumerable<IgnoreAssemblyInfo>>();
            var orderer = Substitute.For<IDependencyOrderer>();
            var writer = Substitute.For<IFileWriter>();

            service.SendAnalysisAsync(Arg.Any<AnalyzeRequest>(), Arg.Any<IEnumerable<string>>()).Returns(
                ServiceResponse.Create(Enumerable.Empty<ReportingResultWithFormat>()));

            var client = new ApiPortClient(service, progressReporter, targetMapper, dependencyFinder, reportGenerator, ignoreAssemblyInfoList, writer, orderer);
            var options = Substitute.For<IApiPortOptions>();

            IAssemblyFile CreateAssemblyFile(AssemblyInfo assemblyInfo)
            {
                var file = Substitute.For<IAssemblyFile>();
                file.Name.Returns(assemblyInfo.AssemblyIdentity);
                file.Version.Returns(assemblyInfo.FileVersion);
                file.Exists.Returns(true);
                return file;
            }

            var assemblyFiles = assemblies.Where(a => a.IsExplicitlySpecified).ToImmutableDictionary(CreateAssemblyFile, _ => false);
            options.InputAssemblies.Returns(assemblyFiles);

            var info = Substitute.For<IDependencyInfo>();
            info.UserAssemblies.Returns(assemblies);

            dependencyFinder.FindDependencies(Arg.Any<IEnumerable<IAssemblyFile>>(), progressReporter)
                .Returns(info);

            await client.WriteAnalysisReportsAsync(options);

            Assert.All(assemblies, a =>
            {
                var expected = assemblies.First(t => string.Equals(t.Location, a.Location, StringComparison.OrdinalIgnoreCase)).IsExplicitlySpecified;
                Assert.Equal(expected, a.IsExplicitlySpecified);
            });
        }
    }
}
