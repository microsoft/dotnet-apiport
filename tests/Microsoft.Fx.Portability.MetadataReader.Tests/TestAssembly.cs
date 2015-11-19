// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    internal abstract class TestAssembly
    {
        public string AssemblyPath { get; } = Path.GetTempFileName();

        public static TestAssembly Create(string source, bool allowUnsafe = false)
        {
            switch (Path.GetExtension(source).ToLowerInvariant())
            {
                case ".dll":
                    return new BinaryTestAssembly(source);
                case ".cs":
                    return new CSharpSourceTestAssembly(source, allowUnsafe);
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown extension");
            }
        }

        private class CSharpSourceTestAssembly : TestAssembly
        {
            private static readonly IEnumerable<MetadataReference> s_references = new[] { typeof(object).Assembly.Location, typeof(Uri).Assembly.Location }
                                                                        .Select(r => MetadataReference.CreateFromFile(r))
                                                                        .ToList();
            private const string TFM = @"[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute("".NETFramework,Version=v4.5.1"", FrameworkDisplayName = "".NET Framework 4.5.1"")]";

            public CSharpSourceTestAssembly(string source, bool allowUnsafe)
            {
                var assemblyName = Path.GetFileNameWithoutExtension(source);
                var text = GetText(source);

                var tfm = CSharpSyntaxTree.ParseText(TFM);
                var tree = CSharpSyntaxTree.ParseText(text);
                var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: allowUnsafe);
                var compilation = CSharpCompilation.Create(assemblyName, new[] { tree, tfm }, s_references, options);
                var result = compilation.Emit(AssemblyPath);

                Assert.True(result.Success, string.Join("\n", result.Diagnostics
                                                            .Where(d => d.Severity == DiagnosticSeverity.Error)
                                                            .Select(d => d.GetMessage())));
            }

            private static string GetText(string fileName)
            {
                var name = typeof(TestAssembly).Assembly.GetManifestResourceNames().Single(n => n.EndsWith(fileName));

                using (var stream = typeof(TestAssembly).Assembly.GetManifestResourceStream(name))
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private class BinaryTestAssembly : TestAssembly
        {
            public BinaryTestAssembly(string assemblyPath)
            {
                var data = GetBytes(assemblyPath);

                File.WriteAllBytes(AssemblyPath, data);
            }

            private static byte[] GetBytes(string fileName)
            {
                var name = typeof(TestAssembly).Assembly.GetManifestResourceNames().Single(n => n.EndsWith(fileName));
                using (var stream = typeof(TestAssembly).Assembly.GetManifestResourceStream(name))
                {
                    var ret = new byte[stream.Length];
                    stream.Read(ret, 0, ret.Length);
                    return ret;
                }
            }
        }
    }
}
