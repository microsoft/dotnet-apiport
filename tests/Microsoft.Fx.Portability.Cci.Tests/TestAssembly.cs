// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Fx.Portability.Cci.Tests
{
    internal class TestAssembly
    {
        private static readonly string s_mscorlib = typeof(object).Assembly.Location;
        private readonly string _path;
        private const string TFM = @"[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute("".NETFramework,Version=v4.5.1"", FrameworkDisplayName = "".NET Framework 4.5.1"")]";

        private TestAssembly(string assemblyName, string text, IEnumerable<string> referencePaths)
        {
            var path = new FileInfo(Path.Combine(System.IO.Path.GetTempPath(), assemblyName + ".exe"));
            _path = path.FullName;

            if (path.Exists)
            {
                // Don't regenerate if already created for this test run
                if (path.LastWriteTime < DateTime.Now.AddMinutes(-1))
                {
                    path.Delete();
                }
                else
                {
                    return;
                }
            }

            var references = referencePaths.Select(r => MetadataReference.CreateFromFile(r)).ToList();

            var tfm = CSharpSyntaxTree.ParseText(TFM);
            var tree = CSharpSyntaxTree.ParseText(text);
            var compilation = CSharpCompilation.Create(assemblyName, new[] { tree, tfm }, references);
            var result = compilation.Emit(path.FullName);

            Assert.True(result.Success);
        }

        public string path { get { return _path; } }

        public static string EmptyProject
        {
            get
            {
                var text = GetText("EmptyProject.cs");
                return new TestAssembly("EmptyProject", text, new[] { s_mscorlib }).path;
            }
        }

        public static string WithGenericsAndReference
        {
            get
            {
                var text = GetText("WithGenericsAndReference.cs");
                return new TestAssembly("WithGenericsAndReference", text, new[] { s_mscorlib, EmptyProject }).path;
            }
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
}
