// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    internal static class TestAssembly
    {
        public static IAssemblyFile Create(string source, bool allowUnsafe = false)
        {
            return Create(source, allowUnsafe, Array.Empty<string>());
        }

        public static IAssemblyFile Create(string source, bool allowUnsafe, IEnumerable<string> additionalReferences)

        {
        switch (Path.GetExtension(source).ToLowerInvariant())
            {
                case ".dll":
                    return new ResourceStreamAssemblyFile(source);
                case ".cs":
                    return new CSharpCompileAssemblyFile(source, allowUnsafe, additionalReferences);
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown extension");
            }
        }

        private class CSharpCompileAssemblyFile : IAssemblyFile
        {
            private static readonly Assembly s_assembly = typeof(CSharpCompileAssemblyFile).GetTypeInfo().Assembly;
            private static readonly IEnumerable<MetadataReference> s_references = new[] { typeof(object).GetTypeInfo().Assembly.Location, typeof(Uri).GetTypeInfo().Assembly.Location }
                                                                     .Select(r => MetadataReference.CreateFromFile(r))
                                                                     .ToList();

            private const string TFM = @"[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute("".NETFramework,Version=v4.5.1"", FrameworkDisplayName = "".NET Framework 4.5.1"")]";

            private readonly byte[] _data;

            public CSharpCompileAssemblyFile(string source, bool allowUnsafe)
            {
                _data = CreateRoslynAssemblyFile(source, allowUnsafe);
            }

            public CSharpCompileAssemblyFile(string source, bool allowUnsafe, IEnumerable<string> additionalReferences)
            {
                _data = CreateRoslynAssemblyFile(source, allowUnsafe, additionalReferences);
            }

            public bool Exists { get; }

            public string Name { get; }

            public string Version { get; }

            public Stream OpenRead() => new MemoryStream(_data);

            private static byte[] CreateRoslynAssemblyFile(string source, bool allowUnsafe)
            {
                return CreateRoslynAssemblyFile(source, allowUnsafe, Array.Empty<string>());
            }

            private static byte[] CreateRoslynAssemblyFile(string source, bool allowUnsafe, IEnumerable<string> additionalReferences)
            {
                var assemblyName = Path.GetFileNameWithoutExtension(source);
                var text = GetText(source);

                var tfm = CSharpSyntaxTree.ParseText(TFM);
                var tree = CSharpSyntaxTree.ParseText(text);
                var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: allowUnsafe);
                var references = additionalReferences.Select(x => MetadataReference.CreateFromFile(x)).Concat(s_references);
                var compilation = CSharpCompilation.Create(assemblyName, new[] { tree, tfm }, references, options);

                using (var stream = new MemoryStream())
                {
                    var result = compilation.Emit(stream);

                    Assert.True(result.Success, string.Join("\n", result.Diagnostics
                                                                .Where(d => d.Severity == DiagnosticSeverity.Error)
                                                                .Select(d => d.GetMessage())));
                    return stream.ToArray();
                }
            }

            private static string GetText(string fileName)
            {
                var name = s_assembly.GetManifestResourceNames().Single(n => n.EndsWith(fileName, StringComparison.Ordinal));

                using (var stream = s_assembly.GetManifestResourceStream(name))
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private class ResourceStreamAssemblyFile : IAssemblyFile
        {
            private static readonly Assembly s_assembly = typeof(ResourceStreamAssemblyFile).GetTypeInfo().Assembly;

            public ResourceStreamAssemblyFile(string fileName)
            {
                Name = s_assembly.GetManifestResourceNames().Single(n => n.EndsWith(fileName, StringComparison.Ordinal));
                Exists = Name != null;
            }

            public bool Exists { get; }

            public string Name { get; }

            public string Version { get; }

            public Stream OpenRead() => s_assembly.GetManifestResourceStream(Name);
        }
    }
}
