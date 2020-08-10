// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer;
using NSubstitute;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    public class ReflectionMetadataInfoTests
    {
        private readonly ITestOutputHelper _log;

        public ReflectionMetadataInfoTests(ITestOutputHelper log)
        {
            _log = log;
        }

        [Fact]
        public void UnresolvedAssemblyTest()
        {
            var path = this.GetType().GetTypeInfo().Assembly.Location;
            var referencedAssemblies = this.GetType().GetTypeInfo().Assembly.GetReferencedAssemblies()
                .Select(a => a.ToString())
                .OrderBy(a => a)
                .ToList();
            var testInfo = new FilePathAssemblyFile(path);

            var filter = new AssemblyFileFrameworkFilter(testInfo);
            var finder = new ReflectionMetadataDependencyFinder(filter, new SystemObjectFinder(filter));
            var progressReport = Substitute.For<IProgressReporter>();

            var dependencies = finder.FindDependencies(new[] { testInfo }, progressReport);
            var actual = dependencies.UnresolvedAssemblies
                            .Select(u => u.Key)
                            .OrderBy(u => u);

            _log.WriteLine("Actual unresolved assemblies:");
            foreach (var assembly in actual)
            {
                _log.WriteLine(assembly);
            }

            Assert.Equal(referencedAssemblies.Count(), actual.Count());

            // Use this instead of Assert.Equal so it will output the missing item
            foreach (var items in actual.Zip(referencedAssemblies, Tuple.Create))
            {
                Assert.Equal(items.Item1, items.Item2);
            }

            Assert.True(dependencies.UnresolvedAssemblies.All(o => o.Value.Count == 1));

            // Make sure no issues were found
            progressReport.Received(0).ReportIssue(Arg.Any<string>());
        }

        private class FilePathAssemblyFile : IAssemblyFile
        {
            private readonly string _path;

            public FilePathAssemblyFile(string path)
            {
                _path = path;
                Name = Path.GetFileNameWithoutExtension(path);
            }

            public string Name { get; }

            public bool Exists => File.Exists(_path);

            public string Version => FileVersionInfo.GetVersionInfo(_path).FileVersion;

            public Stream OpenRead() => File.OpenRead(_path);
        }
    }
}
