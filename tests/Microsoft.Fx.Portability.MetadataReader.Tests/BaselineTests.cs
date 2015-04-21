using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    public class BaselineTests
    {
        [Fact]
        public void MscorlibTest()
        {
            var mscorlib = typeof(object).Assembly.Location;

            var baseline = GetBaseline(mscorlib);
            var dependencyFinder = new ReflectionMetadataDependencyFinder();
            var path = new FileInfo(mscorlib);
            var progressReporter = Substitute.For<IProgressReporter>();

            var dependencies = dependencyFinder.FindDependencies(new[] { path }, progressReporter);

            var result = dependencies.Dependencies
                .Select(d => d.Key)
                .OrderBy(d => d);

            Assert.Equal(baseline, result);
        }

        private IEnumerable<MemberInfo> GetBaseline(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            var version = FileVersionInfo.GetVersionInfo(path);
            var version_file = $"{fileName}_{version.ProductVersion}.json";

            using (var data = typeof(ManagedMetadataReaderTests).Assembly.GetManifestResourceStream(typeof(ManagedMetadataReaderTests), $"Data.{version_file}"))
            {
                if (data == null)
                {
                    Assert.True(false, $"Could not find baseline file for {fileName} version={version.ProductVersion}");
                }

                return data.Deserialize<IEnumerable<MemberInfo>>();
            }
        }

    }
}
