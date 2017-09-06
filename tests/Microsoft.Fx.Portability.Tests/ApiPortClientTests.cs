// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Fx.Portability.Tests
{
    public class ApiPortClientTests
    {
        private Task<ServiceResponse<T>> CreateResponse<T>(T result)
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
            var writer = Substitute.For<IFileWriter>();

            var apiPortService = Substitute.For<IApiPortService>();
            apiPortService.GetTargetsAsync().Returns(CreateResponse<IEnumerable<AvailableTarget>>(targets.AsReadOnly()));

            var client = new ApiPortClient(apiPortService, progressReporter, targetMapper, dependencyFinder, reportGenerator, ignoreAssemblyInfoList, writer);

            var actualTargets = await client.GetTargetsAsync();

            Assert.Equal<AvailableTarget[]>(actualTargets.OrderBy(k => k.Name).ToArray(), targets.OrderBy(k => k.Name).ToArray());
        }

        [Fact]
        public async Task AnalyzeTest()
        {
            var dependencyResult = Enumerable.Range(0, 10).ToDictionary(
                o => new MemberInfo { MemberDocId = "type" + o },
                o => Enumerable.Range(0, o).Select(count => new AssemblyInfo { AssemblyIdentity = "dependency" + count }).ToList() as ICollection<AssemblyInfo>);
            var expectedResult = Enumerable.Range(0, 10).Select(o => Tuple.Create("type" + o, o)).ToList();

            var apiPortService = Substitute.For<IApiPortService>();

            apiPortService.SendAnalysisAsync(Arg.Any<AnalyzeRequest>()).Returns(r =>
            {
                var a = r.Arg<AnalyzeRequest>();

                var foundDocIds = a.Dependencies.Select(o => Tuple.Create(o.Key.MemberDocId, o.Value.Count)).ToList();

                Assert.Equal<IEnumerable<Tuple<string, int>>>(expectedResult.OrderBy(k => k.Item1), foundDocIds.OrderBy(k => k.Item1));
                return CreateResponse(new AnalyzeResponse());
            });

            var progressReporter = Substitute.For<IProgressReporter>();
            var targetMapper = Substitute.For<ITargetMapper>();
            var reportGenerator = Substitute.For<IReportGenerator>();
            var writer = Substitute.For<IFileWriter>();

            var dependencyFinder = Substitute.For<IDependencyFinder>();

            dependencyFinder.FindDependencies(Arg.Any<IEnumerable<IAssemblyFile>>(), Arg.Any<IProgressReporter>()).Returns(r =>
            {
                var shared = r.Arg<IProgressReporter>();

                var dependencies = Substitute.For<IDependencyInfo>();

                dependencies.Dependencies.Returns(dependencyResult);
                dependencies.UnresolvedAssemblies.Returns(new Dictionary<string, ICollection<string>>());
                dependencies.UserAssemblies.Returns(Enumerable.Empty<AssemblyInfo>());
                dependencies.AssembliesWithErrors.Returns(Enumerable.Empty<string>());

                return dependencies;
            });

            var ignoreAssemblyInfoList = Substitute.For<IEnumerable<IgnoreAssemblyInfo>>();

            var client = new ApiPortClient(apiPortService, progressReporter, targetMapper, dependencyFinder, reportGenerator, ignoreAssemblyInfoList, writer);

            var options = Substitute.For<IApiPortOptions>();

            options.Targets.Returns(Enumerable.Empty<string>());
            options.InputAssemblies.Returns(ImmutableDictionary<IAssemblyFile, bool>.Empty);

            var result = await client.AnalyzeAssembliesAsync(options);
        }

        [Fact]
        public async Task AnalyzeAssemblies_ThrowsOnInvalidOptions_TargetCount()
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
            options.Targets.Returns(Enumerable.Range(0, 16).Select(x => x.ToString()));
            options.OutputFormats.Returns(new[] { "HTML", "Excel" });

            await Assert.ThrowsAsync<InvalidApiPortOptionsException>(() => client.AnalyzeAssembliesAsync(options));
        }

        [Fact]
        public async Task AnalyzeAssemblies_DoesNotThrowsOnInvalidOptions_TargetCount()
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
            options.Targets.Returns(Enumerable.Range(0, 16).Select(x => x.ToString()));
            options.OutputFormats.Returns(new[] { "HTML" });

            var item = await client.AnalyzeAssembliesAsync(options);
        }

        [Fact]
        public async Task WriteAnalysisReports_ThrowsOnInvalidOptions_TargetCount()
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
            options.Targets.Returns(Enumerable.Range(0, 16).Select(x => x.ToString()));
            options.OutputFormats.Returns(new[] { "HTML", "Excel" });

            await Assert.ThrowsAsync<InvalidApiPortOptionsException>(() => client.WriteAnalysisReportsAsync(options));
            await Assert.ThrowsAsync<InvalidApiPortOptionsException>(() => client.WriteAnalysisReportsAsync(options, true));
        }
    }
}
