// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    internal static class TestAssembly
    {
        private static readonly Assembly s_assembly = typeof(TestAssembly).GetTypeInfo().Assembly;

        public static IAssemblyFile Create(string source, ITestOutputHelper output, bool allowUnsafe = false)
        {
            switch (Path.GetExtension(source).ToLowerInvariant())
            {
                case ".dll":
                case ".exe":
                    return new ResourceStreamAssemblyFile(source);
                case ".cs":
                    return new CSharpCompileAssemblyFile(source, allowUnsafe, Enumerable.Empty<string>());
                case ".il":
                    return new ILStreamAssemblyFile(source, output);
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown extension");
            }
        }

        private class CSharpCompileAssemblyFile : IAssemblyFile
        {
            private static readonly IEnumerable<MetadataReference> s_references = new[] { typeof(object).GetTypeInfo().Assembly.Location, typeof(Uri).GetTypeInfo().Assembly.Location, typeof(Console).GetTypeInfo().Assembly.Location }
                                                                     .Distinct()
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
                var references = additionalReferences
                                    .Select(x => MetadataReference.CreateFromFile(x))
                                    .Concat(s_references);

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

        private class ILStreamAssemblyFile : IAssemblyFile
        {
            private static readonly string s_ilAsmPath = Path.Combine(Path.GetDirectoryName(s_assembly.Location), "ilasm.exe");

            private readonly ITestOutputHelper _output;

            public ILStreamAssemblyFile(string fileName, ITestOutputHelper output)
            {
                _output = output;

                Name = s_assembly.GetManifestResourceNames().Single(n => n.EndsWith(fileName, StringComparison.Ordinal));
                Exists = Name != null;
            }

            public string Name { get; }

            public string Version { get; }

            public bool Exists { get; }

            public Stream OpenRead()
            {
                if (!File.Exists(s_ilAsmPath))
                {
                    throw new FileNotFoundException("Could not find ilasm");
                }

                var tmp = Path.GetTempFileName();

                using (var fs = File.OpenWrite(tmp))
                using (var stream = s_assembly.GetManifestResourceStream(Name))
                {
                    stream.CopyTo(fs);
                }

                var psi = new ProcessStartInfo
                {
                    Arguments = $"{tmp} /dll",
                    FileName = s_ilAsmPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();

                    var stdout = process.StandardOutput.ReadToEnd();
                    var stderr = process.StandardError.ReadToEnd();

                    _output.WriteLine("ilasm stdout:");
                    _output.WriteLine(stdout);

                    _output.WriteLine("ilasm stderr:");
                    _output.WriteLine(stderr);

                    Assert.Equal(0, process.ExitCode);
                }

                File.Delete(tmp);
                var output = Path.ChangeExtension(tmp, ".dll");

                Assert.True(File.Exists(output));

                return File.OpenRead(output);
            }
        }
    }
}
