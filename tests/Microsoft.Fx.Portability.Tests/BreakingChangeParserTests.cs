using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Microsoft.Fx.Portability.Tests
{
    public class BreakingChangeParserTests
    {
        #region Positive Test Cases
        [Fact]
        public void VanillaParses()
        {
            ValidateParse(GetBreakingChangeMarkdown("Template.md"), TemplateBC);
        }

        #endregion


        #region Helper Methods
        private void ValidateParse(Stream markdown, params BreakingChange[] expected)
        {
            BreakingChange[] actual = BreakingChangeParser.FromMarkdown(markdown).ToArray();
            markdown.Close();

            Assert.Equal(expected.Length, actual.Length);

            for(int i = 0; i < expected.Length; i++)
            {
                TestEquality(expected[i], actual[i]);
            }
        }

        private void TestEquality(BreakingChange expected, BreakingChange actual)
        {
            if (expected == null)
            {
                Assert.Null(actual);
                return;
            }
            else
            {
                Assert.NotNull(actual);
            }

            Assert.Equal(expected.Id, actual.Id, StringComparer.Ordinal);
            Assert.Equal(expected.Title, actual.Title, StringComparer.Ordinal);
            Assert.Equal(expected.Details, actual.Details, StringComparer.Ordinal);
            Assert.Equal(expected.Suggestion, actual.Suggestion, StringComparer.Ordinal);
            Assert.Equal(expected.Link, actual.Link, StringComparer.Ordinal);
            Assert.Equal(expected.BugLink, actual.BugLink, StringComparer.Ordinal);
            Assert.Equal(expected.Notes, actual.Notes, StringComparer.Ordinal);
            Assert.Equal(expected.Markdown, actual.Markdown, StringComparer.Ordinal);
            Assert.Equal(expected.Related, actual.Related, StringComparer.Ordinal);
            Assert.Equal(expected.ApplicableApis, actual.ApplicableApis, StringComparer.Ordinal);
            Assert.Equal(expected.VersionBroken, actual.VersionBroken);
            Assert.Equal(expected.VersionFixed, actual.VersionFixed);
            Assert.Equal(expected.IsBuildTime, actual.IsBuildTime);
            Assert.Equal(expected.IsQuirked, actual.IsQuirked);
            Assert.Equal(expected.IsSourceAnalyzerAvailable, actual.IsSourceAnalyzerAvailable);
            Assert.Equal(expected.ImpactScope, actual.ImpactScope);
        }

        private Stream GetBreakingChangeMarkdown(string resourceName)
        {
            var name = typeof(BreakingChangeParserTests).Assembly.GetManifestResourceNames().Single(n => n.EndsWith(resourceName));
            return typeof(BreakingChangeParserTests).Assembly.GetManifestResourceStream(name);
        }

        #endregion


        #region Expected Breaking Changes
        public static BreakingChange TemplateBC = new BreakingChange()
        {
            Id = "ID",
            Title = "Breaking Change Title",
            ImpactScope = BreakingChangeImpact.Major,
            VersionBroken = new Version(4, 5),
            VersionFixed = new Version(4, 6),
            Details = "Description goes here.",
            IsQuirked = false,
            IsBuildTime = false,
            IsSourceAnalyzerAvailable = false,
            Suggestion = "Suggested steps if user is affected (such as work arounds or code fixes) go here.",
            ApplicableApis = new string[] { "Not detectable via API analysis" },
            Link = "LinkForMoreInformation",
            BugLink = "Bug link goes here",
            Notes = "Source analyzer status: Not usefully detectable with an analyzer"
        };

        #endregion
    }
}
