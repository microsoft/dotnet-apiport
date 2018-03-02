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
        public static void NoSpecifiedTargets()
        {
            var parser = new TargetNameParser(new TestCatalog(), "Target 1, version=1.0");
            var targets = parser.MapTargetsToExplicitVersions(null);

            // Tests if we actually filter out the public targets based on the default target list in the config
            // We should only have 1 target!
            Assert.Single(targets);
            Assert.Equal(TestCatalog.Target1.FullName, targets.First().ToString());
        }

        [Fact]
        public static void NoSpecifiedTargets_2()
        {
            var parser = new TargetNameParser(new TestCatalog(), "Target 1, version=1.0");
            var targets = parser.MapTargetsToExplicitVersions(Enumerable.Empty<string>());

            // We should only have 1 target!
            Assert.Single(targets);
            Assert.Equal(TestCatalog.Target1.FullName, targets.First().ToString());
        }

        [Fact]
        public static void CaseInsensitive()
        {
            var parser = new TargetNameParser(new TestCatalog(), String.Empty);
            var targets = parser.MapTargetsToExplicitVersions(new String[] { "target 1, version=1.0" });

            // We should only have 1 target!
            Assert.Single(targets);
            Assert.Equal("target 1,Version=v1.0", targets.First().ToString());
        }

        [Fact]
        public static void NoSpecifiedDefaultTargets()
        {
            var parser = new TargetNameParser(new TestCatalog(), String.Empty);
            var targets = parser.MapTargetsToExplicitVersions(Enumerable.Empty<string>());

            // We should only have 0 target!
            Assert.Empty(targets);
        }

        [Fact]
        public static void UnReleasedTarget()
        {
            var parser = new TargetNameParser(new TestCatalog(), "Target 1, version=1.0");
            var targets = parser.MapTargetsToExplicitVersions(new string[] { "Target 3" });

            // We should only have 1 target!
            Assert.Single(targets);
            Assert.Equal(TestCatalog.Target3.FullName, targets.First().ToString());
        }

        [Fact]
        public static void NonExistentSpecifiedTarget()
        {
            var parser = new TargetNameParser(new TestCatalog(), "Target 1, version=1.0");
            Assert.Throws<UnknownTargetException>(() => parser.MapTargetsToExplicitVersions(new string[] { "Foo" }));
        }

        [Fact]
        public static void NonExistentSpecifiedVersionOnKnownTarget()
        {
            var parser = new TargetNameParser(new TestCatalog(), "Target 1, version=1.0");
            Assert.Throws<UnknownTargetException>(() => parser.MapTargetsToExplicitVersions(new string[] { "Target 1, version=2.0" }));
        }

        [Fact]
        public static void NonExistentSpecifiedVersionOnKnownTargetWithAvailableTarget()
        {
            var parser = new TargetNameParser(new TestCatalog(), "Target 1, version=1.0");
            Assert.Throws<UnknownTargetException>(() => parser.MapTargetsToExplicitVersions(new string[] { "Target 1, version=2.0", "Target 1, version=1.0" }));
        }

        [Fact]
        public static void NonExistentDefaultTarget()
        {
            var target1 = "Target 1, version=1.0";
            var target1Framework = new FrameworkName(target1);

            var parser = new TargetNameParser(new TestCatalog(), $"TargetNonExistent, version=4.0;{target1}");

            Assert.Single(parser.DefaultTargets);
            Assert.Equal(target1Framework, parser.DefaultTargets.Single());
        }
    }
}
