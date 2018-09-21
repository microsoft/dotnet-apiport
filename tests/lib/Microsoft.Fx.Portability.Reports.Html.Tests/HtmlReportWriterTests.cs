// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        [InlineData("_PortabilityReport.cshtml")]
        [InlineData("ReportTemplate.cshtml")]
        [InlineData("_Scripts.cshtml")]
        [InlineData("_Styles.cshtml")]
        [InlineData("_BreakingChangesReport.cshtml")]
        [InlineData("_CompatibilityResults.cshtml")]
        [InlineData("_CompatibilitySummary.cshtml")]
        [Theory]
        public static void CanFindTemplates(string templateName)
        {
            var fullName = FormattableString.Invariant($"{typeof(HtmlReportWriter).Assembly.GetName().Name}.Resources.{templateName}");

            var stream = typeof(HtmlReportWriter).Assembly.GetManifestResourceStream(fullName);

            Assert.NotNull(stream);
        }

        [Fact]
        public static void CreatesHtmlReport()
        {
            var mapper = Substitute.For<ITargetMapper>();
            var writer = new HtmlReportWriter(mapper);

            var response = GetAnalyzeResponse();

            // setting all show... flags renders every section of the report
            var flags = AnalyzeRequestFlags.NoTelemetry
                      | AnalyzeRequestFlags.ShowBreakingChanges
                      | AnalyzeRequestFlags.ShowNonPortableApis
                      | AnalyzeRequestFlags.ShowRetargettingIssues;

            var reportingResult = new ReportingResult(response.Targets, response.MissingDependencies, response.SubmissionId, flags);
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
                Assert.Contains(response.SubmissionId, contents, StringComparison.Ordinal);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        // an AnalyzeResponse with data for every part of the report
        private static AnalyzeResponse GetAnalyzeResponse() => new AnalyzeResponse
        {
            SubmissionId = Guid.NewGuid().ToString(),
            CatalogLastUpdated = DateTime.Now,
            MissingDependencies = new List<MemberInfo>
            {
                new MemberInfo { MemberDocId = "MissingType1.Member()", DefinedInAssemblyIdentity = "Assembly1", TypeDocId = "MissingType1" },
            },
            Targets = new List<FrameworkName>
            {
                new FrameworkName("target1", Version.Parse("1.0.0.0"), "profile"),
            },
            UnresolvedUserAssemblies = new List<string> { "UnresolvedAssembly" },
            BreakingChangeSkippedAssemblies = new List<AssemblyInfo>
            {
                new AssemblyInfo
                {
                    AssemblyIdentity = "breaking change skipped assembly",
                    FileVersion = "42.42.42",
                    IsExplicitlySpecified = true,
                    Location = "C:/",
                    TargetFrameworkMoniker = "tfm"
                }
            },
            BreakingChanges = new List<BreakingChangeDependency>
            {
                new BreakingChangeDependency
                {
                    Break = new BreakingChange
                    {
                        ApplicableApis = new[] { "all of them" },
                        Categories = new[] { "categories" },
                        Details = "details",
                        Id = "42",
                        VersionBroken = Version.Parse("1.0"),
                        VersionFixed = Version.Parse("1.1"),
                        ImpactScope = BreakingChangeImpact.Edge
                    },
                    DependantAssembly = new AssemblyInfo
                    {
                        AssemblyIdentity = "DependentAssembly",
                        FileVersion = Version.Parse("42.42.42").ToString(),
                        IsExplicitlySpecified = true,
                        Location = "c:/foo/bar",
                        TargetFrameworkMoniker = "TFM"
                    },
                    Member = new MemberInfo
                    {
                        DefinedInAssemblyIdentity = "DependentAssembly",
                        IsSupportedAcrossTargets = false,
                        MemberDocId = "M:Foo.NotSupported",
                        RecommendedChanges = "don't use this",
                        TypeDocId = "T:Foo"
                    }
                }
            }
        };
    }
}
