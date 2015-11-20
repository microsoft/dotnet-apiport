// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Xunit;

namespace Microsoft.Fx.Portability.Tests
{
    public class ApiPortServiceTests
    {
        [Fact]
        public void VerifyParameterChecks()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ApiPortService(null, new ProductInformation("")));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ApiPortService(string.Empty, new ProductInformation("")));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ApiPortService(" \t", new ProductInformation("")));
        }
    }
}
