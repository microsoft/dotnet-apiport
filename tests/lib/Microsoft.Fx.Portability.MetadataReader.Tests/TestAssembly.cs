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

        public static IAssemblyFile Create(string source, ITestOutputHelper output, bool allowUnsafe = false, IEnumerable<string> additionalReferences = null)
        {
            switch (Path.GetExtension(source).ToLowerInvariant())
            {
                case ".dll":
                case ".exe":
                    return new ResourceStreamAssemblyFile(source, output);
                case ".cs":
                    return new CSharpCompileAssemblyFile(source, allowUnsafe, additionalReferences ?? Enumerable.Empty<string>(), output);
                case ".il":
                    return new ILStreamAssemblyFile(source, output);
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown extension");
            }
        }

        private class CSharpCompileAssemblyFile : ResourceStreamAssemblyFile
        {
            private static readonly IEnumerable<MetadataReference> s_references = new[] { typeof(object).GetTypeInfo().Assembly.Location, typeof(Uri).GetTypeInfo().Assembly.Location, typeof(Console).GetTypeInfo().Assembly.Location }
                                                                     .Distinct()
                                                                     .Select(r => MetadataReference.CreateFromFile(r))
                                                                     .ToList();

            private const string TFM = @"[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute("".NETFramework,Version=v4.5.1"", FrameworkDisplayName = "".NET Framework 4.5.1"")]";

            private readonly bool _allowUnsafe;
            private readonly IEnumerable<string> _additionalReferences;

            public CSharpCompileAssemblyFile(string source, bool allowUnsafe, IEnumerable<string> additionalReferences, ITestOutputHelper output)
                : base(source, output)
            {
                _allowUnsafe = allowUnsafe;
                _additionalReferences = additionalReferences;
            }

            public override Stream OpenRead()
            {
                var assemblyName = Path.GetFileNameWithoutExtension(Name);
                var text = GetText();

                var tfm = CSharpSyntaxTree.ParseText(TFM);
                var tree = CSharpSyntaxTree.ParseText(text);
                var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: _allowUnsafe);
                var references = _additionalReferences
                                    .Select(x => MetadataReference.CreateFromFile(x))
                                    .Concat(s_references);

                var compilation = CSharpCompilation.Create(assemblyName, new[] { tree, tfm }, references, options);

                var stream = new MemoryStream();
                var result = compilation.Emit(stream);
                var resultMessages = string.Join("\n", result.Diagnostics
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .Select(d => d.GetMessage()));

                Output.WriteLine(resultMessages);

                Assert.True(result.Success);

                stream.Position = 0;

                return stream;
            }

            private string GetText()
            {
                using (var stream = base.OpenRead())
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private class ResourceStreamAssemblyFile : IAssemblyFile
        {
            public ResourceStreamAssemblyFile(string fileName, ITestOutputHelper output)
            {
                Name = fileName;
                Output = output;
            }

            protected ITestOutputHelper Output { get; }

            public bool Exists => true;

            public string Name { get; }

            public string Version { get; }

            public virtual Stream OpenRead()
            {
                var names = s_assembly.GetManifestResourceNames();
                var name = names.Single(n => n.EndsWith(Name, StringComparison.Ordinal));

                if (name == null)
                {
                    Output.WriteLine($"'{Name}' not found. Available names:");

                    foreach (var available in names)
                    {
                        Output.WriteLine(available);
                    }
                }

                Assert.NotNull(name);

                return s_assembly.GetManifestResourceStream(name);
            }
        }

        private class ILStreamAssemblyFile : ResourceStreamAssemblyFile
        {
            private static readonly string s_ilAsmPath = Path.Combine(Path.GetDirectoryName(s_assembly.Location), "ilasm.exe");

            public ILStreamAssemblyFile(string fileName, ITestOutputHelper output)
                : base(fileName, output)
            {
            }

            public override Stream OpenRead()
            {
                if (!File.Exists(s_ilAsmPath))
                {
                    throw new FileNotFoundException("Could not find ilasm");
                }

                var ilPath = Path.GetTempFileName();

                using (var fs = File.OpenWrite(ilPath))
                using (var stream = base.OpenRead())
                {
                    stream.CopyTo(fs);
                }

                var psi = new ProcessStartInfo
                {
                    Arguments = $"{ilPath} /dll",
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

                    Output.WriteLine("ilasm stdout:");
                    Output.WriteLine(stdout);

                    Output.WriteLine("ilasm stderr:");
                    Output.WriteLine(stderr);

                    Assert.Equal(0, process.ExitCode);
                }

                File.Delete(ilPath);
                var dllPath = Path.ChangeExtension(ilPath, ".dll");

                Assert.True(File.Exists(dllPath));

                return File.OpenRead(dllPath);
            }
        }
    }
}
