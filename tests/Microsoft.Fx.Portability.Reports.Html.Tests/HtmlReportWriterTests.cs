using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using Xunit;

namespace Microsoft.Fx.Portability.Reports.Html.Tests
{
    public class HtmlReportWriterTests
    {
        /// <summary>
        /// Tests that the templates are embedded into the assembly.
        /// </summary>
        /// <param name="templateName"></param>
        [InlineData("_PortabilityReport.cshtml")]
        [InlineData("ReportTemplate.cshtml")]
        [InlineData("_Scripts.cshtml")]
        [InlineData("_Styles.cshtml")]
        [InlineData("_BreakingChangesReport.cshtml")]
        [Theory]
        public void CanFindTemplates(string templateName)
        {
            var fullName = $"{typeof(HtmlReportWriter).Assembly.GetName().Name}.Resources.{templateName}";

            var stream = typeof(HtmlReportWriter).Assembly.GetManifestResourceStream(fullName);

            Assert.NotNull(stream);
        }

        [Fact]
        public void CreatesHtmlReport()
        {
            var mapper = Substitute.For<ITargetMapper>();
            var writer = new HtmlReportWriter(mapper);

            var response = new AnalyzeResponse
            {
                MissingDependencies = new List<MemberInfo>
                {
                    new MemberInfo { MemberDocId = "Type1.doc1", DefinedInAssemblyIdentity = "Assembly1", TypeDocId = "Type1" },
                    new MemberInfo { MemberDocId = "Type2.doc2", DefinedInAssemblyIdentity = "Assembly2", TypeDocId = "Type2" }
                },
                SubmissionId = Guid.NewGuid().ToString(),
                Targets = new List<FrameworkName> { new FrameworkName("target1", Version.Parse("1.0.0.0")) },
                UnresolvedUserAssemblies = new List<string> { "UnresolvedAssembly", "UnresolvedAssembly2", "UnresolvedAssembly3" },
                BreakingChangeSkippedAssemblies = new List<AssemblyInfo>(),
                BreakingChanges = new List<BreakingChangeDependency>(),
            };

            var reportingResult = new ReportingResult(response.Targets, response.MissingDependencies, response.SubmissionId, AnalyzeRequestFlags.NoTelemetry);
            response.ReportingResult = reportingResult;

            var tempFile = Path.GetTempFileName();

            try
            {
                using (var file = File.OpenWrite(tempFile))
                {
                    writer.WriteStream(file, response);
                }

                Assert.True(File.Exists(tempFile));

                var contents = File.ReadAllText(tempFile);

                Assert.True(!string.IsNullOrEmpty(contents));
                Assert.Contains(response.SubmissionId, contents);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
    }
}
