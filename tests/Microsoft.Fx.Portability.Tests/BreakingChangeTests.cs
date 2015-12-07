// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace Microsoft.Fx.Portability.Tests
{
    public class BreakingChangeTests
    {
        [Fact]
        public void EqualityTests()
        {
            var breakingChangeSame1 = new BreakingChange { Id = "id1" };
            var breakingChangeSame2 = new BreakingChange { Id = "id1" };
            var breakingChangeDifferent1 = new BreakingChange { Id = "ID1" };
            var breakingChangeDifferent2 = new BreakingChange { Id = "id2" };

            Assert.Equal(breakingChangeSame1.GetHashCode(), breakingChangeSame2.GetHashCode());
            Assert.NotEqual(breakingChangeSame1.GetHashCode(), breakingChangeDifferent1.GetHashCode());
            Assert.NotEqual(breakingChangeSame2.GetHashCode(), breakingChangeDifferent1.GetHashCode());
            Assert.NotEqual(breakingChangeSame1.GetHashCode(), breakingChangeDifferent2.GetHashCode());

            Assert.False(breakingChangeSame1.Equals(null));
            Assert.False(breakingChangeSame1.Equals(new BreakingChange()));
            Assert.False(breakingChangeSame1.Equals("other type"));
            Assert.True(breakingChangeSame1.Equals(breakingChangeSame1));
            Assert.True(breakingChangeSame1.Equals(breakingChangeSame2));
            Assert.False(breakingChangeSame1.Equals(breakingChangeDifferent1));
            Assert.False(breakingChangeSame2.Equals(breakingChangeDifferent1));
            Assert.False(breakingChangeSame1.Equals(breakingChangeDifferent2));
        }
    }
}
