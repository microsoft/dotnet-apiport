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

            if (!result.Success && path.Exists)
            {
                path.Delete();
            }

            Assert.True(result.Success, string.Join("\n", result.Diagnostics
                                                        .Where(d => d.Severity == DiagnosticSeverity.Error)
                                                        .Select(d => d.GetMessage())));
        }

        private TestAssembly(string assemblyName, byte[] assembly)
        {
            var path = new FileInfo(Path.Combine(System.IO.Path.GetTempPath(), assemblyName));
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

            using (var fs = path.OpenWrite())
            {
                fs.Write(assembly, 0, assembly.Length);
            }
        }

        public string AssemblyPath { get { return _path; } }

        public static string Arglist
        {
            get
            {
                var text = GetText("Arglist.cs");
                return new TestAssembly("Arglist", text, new[] { s_mscorlib }).AssemblyPath;
            }
        }

        public static string EmptyProject
        {
            get
            {
                var text = GetText("EmptyProject.cs");
                return new TestAssembly("EmptyProject", text, new[] { s_mscorlib }).AssemblyPath;
            }
        }

        public static string NestedGenericTypes
        {
            get
            {
                var text = GetText("NestedGenericTypes.cs");
                return new TestAssembly("NestedGenericTypes", text, new[] { s_mscorlib }).AssemblyPath;
            }
        }

        public static string NestedGenericTypesWithInvalidNames
        {
            get
            {
                var text = GetText("NestedGenericTypesWithInvalidNames.cs");
                return new TestAssembly("NestedGenericTypesWithInvalidNames", text, new[] { s_mscorlib }).AssemblyPath;
            }
        }

        public static string ModsFromIL
        {
            get
            {
                var bytes = GetBytes("modopt.dll");
                return new TestAssembly("ModOpt.dll", bytes).AssemblyPath;
            }
        }

        public static string NestedGenericTypesFromIL
        {
            get
            {
                var bytes = GetBytes("NestedGenericTypes.dll");
                return new TestAssembly("NestedGenericTypes.dll", bytes).AssemblyPath;
            }
        }

        public static string NonGenericTypesWithGenericParametersFromIL
        {
            get
            {
                var bytes = GetBytes("NonGenericTypesWithGenericParameters.dll");
                return new TestAssembly("NonGenericTypesWithGenericParameters.dll", bytes).AssemblyPath;
            }
        }

        public static string GenericClassWithGenericMethod
        {
            get
            {
                var text = GetText("GenericClassMemberWithDifferentGeneric.cs");
                return new TestAssembly("GenericClassMemberWithDifferentGeneric", text, new[] { s_mscorlib }).AssemblyPath;
            }
        }

        public static string WithGenericsAndReference
        {
            get
            {
                var text = GetText("WithGenericsAndReference.cs");
                return new TestAssembly("WithGenericsAndReference", text, new[] { s_mscorlib, EmptyProject }).AssemblyPath;
            }
        }

        public static string MoreThan9GenericParams
        {
            get
            {
                var text = GetText("10-generic-params.cs");
                return new TestAssembly("10-generic-params", text, new[] { s_mscorlib, EmptyProject }).AssemblyPath;
            }
        }

        public static string OpImplicit
        {
            get
            {
                var text = GetText("OpImplicit.cs");
                return new TestAssembly("OpImplicit", text, new[] { s_mscorlib, EmptyProject }).AssemblyPath;
            }
        }

        public static string OpImplicitMethod
        {
            get
            {
                var text = GetText("OpImplicitMethod.cs");
                return new TestAssembly("OpImplicitMethod", text, new[] { s_mscorlib }).AssemblyPath;
            }
        }

        public static string OpImplicitMethod2Parameter
        {
            get
            {
                var text = GetText("OpImplicitMethod2Parameter.cs");
                return new TestAssembly("OpImplicitMethod2Parameter", text, new[] { s_mscorlib }).AssemblyPath;
            }
        }

        public static string OpExplicit
        {
            get
            {
                var text = GetText("OpExplicit.cs");
                return new TestAssembly("OpExplicit", text, new[] { s_mscorlib, EmptyProject }).AssemblyPath;
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
