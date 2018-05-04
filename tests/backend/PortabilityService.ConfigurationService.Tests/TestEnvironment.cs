// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using System;

namespace PortabilityService.ConfigurationService.Tests
{
    public partial class ConfigurationControllerTests
    {
        public class TestHostingEnvironment : IHostingEnvironment
        {
            public string EnvironmentName { get; set; }

            public string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public string WebRootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public IFileProvider WebRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public string ContentRootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public IFileProvider ContentRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        }
    }
}
