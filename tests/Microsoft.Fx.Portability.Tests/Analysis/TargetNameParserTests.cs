// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analysis;
using Microsoft.Fx.Portability.TestData;
using System;
using System.Linq;
using System.Runtime.Versioning;
using Xunit;

namespace Microsoft.Fx.Portability.Tests
{
    public class TargetNameParserTests
    {
        [Fact]
        public void NoSpecifiedTargets()
        {
            var parser = new TargetNameParser(new TestCatalog(), "Target 1, version=1.0");
            var targets = parser.MapTargetsToExplicitVersions(null);

            // Tests if we actually filter out the public targets based on the default target list in the config
            // We should only have 1 target!
            Assert.Equal(1, targets.Count());
            Assert.Equal("Target 1,Version=v1.0", targets.First().ToString());
        }

        [Fact]
        public void NoSpecifiedTargets_2()
        {
            var parser = new TargetNameParser(new TestCatalog(), "Target 1, version=1.0");
            var targets = parser.MapTargetsToExplicitVersions(Enumerable.Empty<string>());

            // We should only have 1 target!
            Assert.Equal(1, targets.Count());
            Assert.Equal("Target 1,Version=v1.0", targets.First().ToString());
        }

        [Fact]
        public void CaseInsensitive()
        {
            var parser = new TargetNameParser(new TestCatalog(), String.Empty);
            var targets = parser.MapTargetsToExplicitVersions(new String[] { "target 1, version=1.0" });

            // We should only have 1 target!
            Assert.Equal(1, targets.Count());
            Assert.Equal("target 1,Version=v1.0", targets.First().ToString());
        }

        [Fact]
        public void NoSpecifiedDefaultTargets()
        {
            var parser = new TargetNameParser(new TestCatalog(), String.Empty);
            var targets = parser.MapTargetsToExplicitVersions(Enumerable.Empty<string>());

            // We should only have 0 target!
            Assert.Equal(0, targets.Count());
        }

        [Fact]
        public void UnReleasedTarget()
        {
            var parser = new TargetNameParser(new TestCatalog(), "Target 1, version=1.0");
            var targets = parser.MapTargetsToExplicitVersions(new string[] { "Target 3" });

            // We should only have 1 target!
            Assert.Equal(1, targets.Count());
            Assert.Equal("Target 3,Version=v2.0", targets.First().ToString());
        }

        [Fact]
        public void NonExistentSpecifiedTarget()
        {
            var parser = new TargetNameParser(new TestCatalog(), "Target 1, version=1.0");
            Assert.Throws<UnknownTargetException>(() => parser.MapTargetsToExplicitVersions(new string[] { "Foo" }));
        }

        [Fact]
        public void NonExistentSpecifiedVersionOnKnownTarget()
        {
            var parser = new TargetNameParser(new TestCatalog(), "Target 1, version=1.0");
            Assert.Throws<UnknownTargetException>(() => parser.MapTargetsToExplicitVersions(new string[] { "Target 1, version=2.0" }));
        }

        [Fact]
        public void NonExistentSpecifiedVersionOnKnownTargetWithAvailableTarget()
        {
            var parser = new TargetNameParser(new TestCatalog(), "Target 1, version=1.0");
            Assert.Throws<UnknownTargetException>(() => parser.MapTargetsToExplicitVersions(new string[] { "Target 1, version=2.0", "Target 1, version=1.0" }));
        }

        [Fact]
        public void NonExistentDefaultTarget()
        {
            var target1 = "Target 1, version=1.0";
            var target1Framework = new FrameworkName(target1);

            var parser = new TargetNameParser(new TestCatalog(), $"TargetNonExistent, version=4.0;{target1}");

            Assert.Equal(1, parser.DefaultTargets.Count());
            Assert.Equal(target1Framework, parser.DefaultTargets.Single());
        }
    }
}
