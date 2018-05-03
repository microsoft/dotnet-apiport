// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Fx.Portability.ObjectModel;
using NSubstitute;
using PortabilityService.AnalysisEngine.Controllers;
using System.Threading.Tasks;
using Xunit;

namespace PortabilityService.AnalysisEngine.Tests
{
    public class AnalyzeControllerTests
    {
        [Fact]
        public static async Task ShouldReturnNotFoundForInvalidRequest()
        {
            // Arrange
            var storage = Substitute.For<IStorage>();
            var configuration = Substitute.For<IConfiguration>();
            var logger = Substitute.For<ILogger<AnalyzeController>>();
            var controller = new AnalyzeController(configuration, storage, logger);

            // Act
            var result = await controller.Analyze("any-id");

            // Assert
            Assert.True(result is NotFoundResult);
        }

        [Fact]
        public static async Task ShouldReturnOkForValidSubmission()
        {
            // Arrange
            var storage = Substitute.For<IStorage>();
            var request = new AnalyzeRequest();
            storage.RetrieveRequestAsync("id").Returns(Task.FromResult(request));
            var result = new AnalyzeResponse();
            storage.RetrieveResultFromBlobAsync("id").Returns(Task.FromResult(result));

            var configuration = Substitute.For<IConfiguration>();
            var logger = Substitute.For<ILogger<AnalyzeController>>();
            var controller = new AnalyzeController(configuration, storage, logger);

            // Act
            var actionResult = await controller.Analyze("id");

            // Assert
            Assert.True(actionResult is OkResult);
        }
    }
}
