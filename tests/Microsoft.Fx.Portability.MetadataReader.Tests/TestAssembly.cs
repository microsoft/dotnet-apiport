using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    class TestAssembly
    {
        private static readonly string mscorlib = typeof(object).Assembly.Location;
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

        public string AssemblyPath { get { return _path; } }

        public static string EmptyProject
        {
            get
            {
                var text = GetText("EmptyProject.cs");
                return new TestAssembly("EmptyProject", text, new[] { mscorlib }).AssemblyPath;
            }
        }

        public static string GenericClassWithGenericMethod
        {
            get
            {
                var text = GetText("GenericClassMemberWithDifferentGeneric.cs");
                return new TestAssembly("GenericClassMemberWithDifferentGeneric", text, new[] { mscorlib }).AssemblyPath;
            }
        }

        public static string WithGenericsAndReference
        {
            get
            {
                var text = GetText("WithGenericsAndReference.cs");
                return new TestAssembly("WithGenericsAndReference", text, new[] { mscorlib, EmptyProject }).AssemblyPath;
            }
        }

        public static string MoreThan9GenericParams
        {
            get
            {
                var text = GetText("10-generic-params.cs");
                return new TestAssembly("10-generic-params", text, new[] { mscorlib, EmptyProject }).AssemblyPath;
            }
        }

        public static string OpImplicit
        {
            get
            {
                var text = GetText("OpImplicit.cs");
                return new TestAssembly("OpImplicit", text, new[] { mscorlib, EmptyProject }).AssemblyPath;
            }
        }

        public static string OpExplicit
        {
            get
            {
                var text = GetText("OpExplicit.cs");
                return new TestAssembly("OpExplicit", text, new[] { mscorlib, EmptyProject }).AssemblyPath;
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
