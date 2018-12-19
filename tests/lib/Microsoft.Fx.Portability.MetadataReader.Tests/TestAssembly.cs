// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    internal static class TestAssembly
    {
        private static readonly Assembly Assembly = typeof(TestAssembly).GetTypeInfo().Assembly;

        public static IAssemblyFile CreateFromIL(string il, string name, ITestOutputHelper output)
        {
            return new ILStreamAssemblyFile(new StringAssemblyFile(name, il), output);
        }

        public static IAssemblyFile Create(string source, ITestOutputHelper output, bool allowUnsafe = false, IEnumerable<string> additionalReferences = null)
        {
            switch (Path.GetExtension(source).ToLowerInvariant())
            {
                case ".dll":
                case ".exe":
                    return new ResourceStreamAssemblyFile(source, output);
                case ".cs":
                    return new CSharpCompileAssemblyFile(source, allowUnsafe, additionalReferences, output);
                case ".il":
                    return new ILStreamAssemblyFile(new ResourceStreamAssemblyFile(source, output), output);
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown extension");
            }
        }

        private class StringAssemblyFile : IAssemblyFile
        {
            private readonly byte[] _contents;

            public StringAssemblyFile(string name, string contents)
            {
                Name = name;
                _contents = Encoding.UTF8.GetBytes(contents);
            }

            public string Name { get; }

            public string Version => string.Empty;

            public bool Exists => true;

            public Stream OpenRead() => new MemoryStream(_contents);
        }

        private class CSharpCompileAssemblyFile : ResourceStreamAssemblyFile
        {
            private const string TFM = @"[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute("".NETFramework,Version=v4.5.1"", FrameworkDisplayName = "".NET Framework 4.5.1"")]";
            private static readonly IEnumerable<MetadataReference> References = new[]
            {
                typeof(object).GetTypeInfo().Assembly.Location,
                typeof(Uri).GetTypeInfo().Assembly.Location,
                typeof(Console).GetTypeInfo().Assembly.Location,

                // Reference to System.Runtime.dll rather than System.Private.CoreLib on .NET Core
                typeof(RuntimeReflectionExtensions).Assembly.Location
            }
            .Distinct()
            .Select(r => MetadataReference.CreateFromFile(r))
            .ToList();

            private readonly bool _allowUnsafe;
            private readonly IEnumerable<string> _additionalReferences;

            public CSharpCompileAssemblyFile(string source, bool allowUnsafe, IEnumerable<string> additionalReferences, ITestOutputHelper output)
                : base(source, output)
            {
                _allowUnsafe = allowUnsafe;
                _additionalReferences = additionalReferences ?? Enumerable.Empty<string>();
            }

            public override Stream OpenRead()
            {
                var assemblyName = Path.GetFileNameWithoutExtension(Name);
                var text = GetText();

                var tfm = CSharpSyntaxTree.ParseText(TFM);
                var tree = CSharpSyntaxTree.ParseText(GetText());
                var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: _allowUnsafe);
                var references = _additionalReferences
                    .Select(x => MetadataReference.CreateFromFile(x))
                    .Concat(References);

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

            private SourceText GetText()
            {
                using (var stream = base.OpenRead())
                {
                    return SourceText.From(stream);
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
                var names = Assembly.GetManifestResourceNames();
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

                return Assembly.GetManifestResourceStream(name);
            }
        }

        private class ILStreamAssemblyFile : IAssemblyFile
        {
            private static readonly string ILAssemblyPath = Path.Combine(Path.GetDirectoryName(Assembly.Location), "ilasm.exe");

            private readonly IAssemblyFile _other;
            private readonly ITestOutputHelper _output;

            public ILStreamAssemblyFile(IAssemblyFile other, ITestOutputHelper output)
            {
                _other = other;
                _output = output;
            }

            public string Name => _other.Name;

            public string Version => _other.Version;

            public bool Exists => _other.Exists;

            public Stream OpenRead()
            {
                if (!File.Exists(ILAssemblyPath))
                {
                    throw new FileNotFoundException("Could not find ilasm");
                }

                var ilPath = Path.GetTempFileName();

                using (var fs = File.OpenWrite(ilPath))
                {
                    using (var stream = _other.OpenRead())
                    {
                        stream.CopyTo(fs);
                    }
                }

                var psi = new ProcessStartInfo
                {
                    Arguments = $"{ilPath} /dll",
                    FileName = ILAssemblyPath,
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

                File.Delete(ilPath);
                var dllPath = Path.ChangeExtension(ilPath, ".dll");

                Assert.True(File.Exists(dllPath));

                return File.OpenRead(dllPath);
            }
        }
    }
}
