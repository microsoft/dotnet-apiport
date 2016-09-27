using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiPortVS.Contracts;
using ApiPortVS.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace ApiPortVS.Tests
{
    [TestClass]
    public class OptionsViewModelTests
    {
        private class TestableOptionsViewModel : OptionsViewModel
        {
            public TestableOptionsViewModel(IApiPort apiPort, IEnumerable<TargetPlatform> platforms)
                : base(apiPort)
            {
                this.platforms = platforms.ToList();
            }
        }

        [TestMethod]
        public async Task OptionsViewModel_ReconcilePlatformsWithBackendAsync_DiscardsInvalidPlatforms()
        {
            var serverPlatformNames = new string[] { "valid1", "valid2" };
            var apiPort = ApiPortWhichReturnsPlatforms(serverPlatformNames);

            var localPlatforms = from n in Enumerable.Range(1, 6)
                                 select new TargetPlatform { Name = string.Format("invalid{0}", n), Selected = true };
            
            var viewModel = new TestableOptionsViewModel(apiPort, localPlatforms.ToList());

            await viewModel.ReconcilePlatformsWithBackendAsync();

            Assert.IsFalse(viewModel.Platforms.Any(x => x.Name.Contains("invalid")));
        }

        [TestMethod]
        public async Task OptionsViewModel_ReconcilePlatformsWithBackendAsync_ReturnsInvalidPlatforms()
        {
            int numberOfPlatforms = 6;
            var serverPlatformNames = new string[] { "valid1", "valid2" };
            var apiPort = ApiPortWhichReturnsPlatforms(serverPlatformNames);

            var invalidLocalPlatforms = (from n in Enumerable.Range(1, numberOfPlatforms)
                                 select new TargetPlatform { Name = string.Format("invalid{0}", n), Selected = n % 2 == 0 })
                                 .ToList();

            var viewModel = new TestableOptionsViewModel(apiPort, invalidLocalPlatforms);

            var result = await viewModel.ReconcilePlatformsWithBackendAsync();

            var localHash = new HashSet<TargetPlatform>(invalidLocalPlatforms);
            var resultHash = new HashSet<TargetPlatform>(result);

            Assert.IsTrue(localHash.Intersect(resultHash).Count() == localHash.Count);
        }

        [TestMethod]
        public async Task OptionsViewModel_ReconcilePlatformsWithBackendAsync_KeepsPlatformSelections()
        {
            int numberOfPlatforms = 6;
            var serverPlatformNames = (from n in Enumerable.Range(1, numberOfPlatforms)
                                       select string.Format("valid{0}", n)).ToList();

            var apiPort = ApiPortWhichReturnsPlatforms(serverPlatformNames);

            // create a list of TargetPlatforms corresponding to the server's, half selected
            var localPlatforms = (from n in Enumerable.Range(0, numberOfPlatforms - 1)
                                  select new TargetPlatform
                                  {
                                      Name = serverPlatformNames[n],
                                      Selected = n % 2 == 0
                                  })
                                  .ToList();

            var viewModel = new TestableOptionsViewModel(apiPort, localPlatforms);

            await viewModel.ReconcilePlatformsWithBackendAsync();

            // assert the same platforms remain and selections have not changed
            for (int i = 0; i < viewModel.Platforms.Count; ++i)
            {
                var platform = viewModel.Platforms[i];
                Assert.IsTrue(serverPlatformNames.Contains(platform.Name) && platform.Selected == (i % 2 == 0));
            }
        }

        private IApiPort ApiPortWhichReturnsPlatforms(IEnumerable<string> platformNames)
        {
            var apiPort = Substitute.For<IApiPort>();
            apiPort.GetTargetPlatformsAsync().Returns(Task.FromResult(platformNames));

            return apiPort;
        }
    }
}
