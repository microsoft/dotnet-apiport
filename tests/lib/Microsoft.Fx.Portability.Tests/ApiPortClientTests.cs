// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using NSubstitute;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Fx.Portability.Tests
{
    public class ApiPortClientTests
    {
        [Fact]
        public async Task ListTargetsTest()
        {
            var targets = new List<AvailableTarget> { new AvailableTarget { Name = "Target1" }, new AvailableTarget { Name = "Target2" } };
            var progressReporter = Substitute.For<IProgressReporter>();
            var targetMapper = Substitute.For<ITargetMapper>();
            var dependencyFinder = Substitute.For<IDependencyFinder>();
            var reportGenerator = Substitute.For<IReportGenerator>();
            var ignoreAssemblyInfoList = Substitute.For<IEnumerable<IgnoreAssemblyInfo>>();
            var writer = Substitute.For<IFileWriter>();

            var apiPortService = Substitute.For<IApiPortService>();
            apiPortService.GetTargetsAsync().Returns(targets.AsReadOnly());

            var client = new ApiPortClient(apiPortService, progressReporter, targetMapper, dependencyFinder, reportGenerator, ignoreAssemblyInfoList, writer);

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
            var writer = Substitute.For<IFileWriter>();

            var client = new ApiPortClient(service, progressReporter, targetMapper, dependencyFinder, reportGenerator, ignoreAssemblyInfoList, writer);
            var options = Substitute.For<IApiPortOptions>();
            options.Targets.Returns(Enumerable.Range(0, 16).Select(x => x.ToString(CultureInfo.CurrentCulture)));
            options.OutputFormats.Returns(new[] { "HTML", "Excel" });

            await Assert.ThrowsAsync<InvalidApiPortOptionsException>(() => client.WriteAnalysisReportsAsync(options));
            await Assert.ThrowsAsync<InvalidApiPortOptionsException>(() => client.WriteAnalysisReportsAsync(options, true));
        }

        [Fact]
        public static async Task WriteAnalysisReportsThrowsForUnknownOutputFormat()
        {
            var service = Substitute.For<IApiPortService>();
            service.GetResultFormatsAsync().Returns(new[] { new ResultFormatInformation { DisplayName = "foo" } });
            var progressReporter = Substitute.For<IProgressReporter>();
            var targetMapper = Substitute.For<ITargetMapper>();
            var dependencyFinder = Substitute.For<IDependencyFinder>();
            var reportGenerator = Substitute.For<IReportGenerator>();
            var ignoreAssemblyInfoList = Substitute.For<IEnumerable<IgnoreAssemblyInfo>>();
            var writer = Substitute.For<IFileWriter>();

            var client = new ApiPortClient(service, progressReporter, targetMapper, dependencyFinder, reportGenerator, ignoreAssemblyInfoList, writer);
            var options = Substitute.For<IApiPortOptions>();
            options.OutputFormats.Returns(new[] { "bar" });

            await Assert.ThrowsAsync<UnknownReportFormatException>(async () => await client.WriteAnalysisReportsAsync(options));
        }
    }
}
