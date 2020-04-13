// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.Fx.Portability.Tests
{
    public class ReadWriteApiPortOptionsTests
    {
        [Fact]
        public static void AllPropertiesCopied()
        {
            var options = new TestOptions();
            var wrappedOptions = new ReadWriteApiPortOptions(options);

            foreach (var property in typeof(IApiPortOptions).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                Assert.Equal(property.GetValue(options), property.GetValue(wrappedOptions));
            }
        }

        public class TestOptions : IApiPortOptions
        {
            public IEnumerable<string> BreakingChangeSuppressions { get; } = GenerateRandomList(5);

            public string Description { get; } = GetRandomString();

            public IEnumerable<string> IgnoredAssemblyFiles { get; } = GenerateRandomList(5);

            public ImmutableDictionary<IAssemblyFile, bool> InputAssemblies { get; } = ImmutableDictionary<IAssemblyFile, bool>.Empty.Add(new TestAssemblyFile(), false);

            public IEnumerable<string> InvalidInputFiles { get; } = GenerateRandomList(5);

            public string OutputFileName { get; } = GetRandomString();

            public IEnumerable<string> OutputFormats { get; } = GenerateRandomList(5);

            public AnalyzeRequestFlags RequestFlags { get; } = AnalyzeRequestFlags.ShowBreakingChanges | AnalyzeRequestFlags.ShowNonPortableApis;

            public string ServiceEndpoint { get; } = GetRandomString();

            public IEnumerable<string> Targets { get; } = GenerateRandomList(5);

            public bool OverwriteOutputFile { get; }

            public IEnumerable<string> ReferencedNuGetPackages { get; }

            public string EntryPoint { get; } = GetRandomString();

            private static IEnumerable<string> GenerateRandomList(int length)
            {
                return Enumerable.Range(0, length)
                    .Select(_ => GetRandomString())
                    .ToList();
            }

            private static string GetRandomString() => Guid.NewGuid().ToString();

            private class TestAssemblyFile : IAssemblyFile
            {
                public bool Exists { get; } = true;

                public string Name { get; } = Path.GetTempFileName();

                public string Version { get; } = string.Empty;

                public Stream OpenRead() => File.OpenRead(Name);
            }
        }
    }
}
