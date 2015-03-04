using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Xunit;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

            var apiPortService = Substitute.For<IApiPortService>();
            apiPortService.GetTargetsAsync().Returns(CreateResponse<IEnumerable<AvailableTarget>>(targets.AsReadOnly()));

            var client = new ApiPortClient(apiPortService, progressReporter, targetMapper, dependencyFinder);

            var actualTargets = await client.ListTargets();

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

            var dependencyFinder = Substitute.For<IDependencyFinder>();

            dependencyFinder.FindDependencies(Arg.Any<IEnumerable<FileInfo>>(), Arg.Any<IProgressReporter>()).Returns(r =>
            {
                var list = r.Arg<IEnumerable<FileInfo>>();
                var shared = r.Arg<IProgressReporter>();

                var dependencies = Substitute.For<IDependencyInfo>();

                dependencies.Dependencies.Returns(dependencyResult);
                dependencies.UnresolvedAssemblies.Returns(new Dictionary<string, ICollection<string>>());
                dependencies.UserAssemblies.Returns(Enumerable.Empty<AssemblyInfo>());
                dependencies.AssembliesWithErrors.Returns(Enumerable.Empty<string>());

                return dependencies;
            });

            var client = new ApiPortClient(apiPortService, progressReporter, targetMapper, dependencyFinder);

            var options = Substitute.For<IApiPortOptions>();

            options.Targets.Returns(Enumerable.Empty<string>());
            options.InputAssemblies.Returns(Enumerable.Empty<FileInfo>());

            var result = await client.AnalyzeAssemblies(options);
        }
    }
}
