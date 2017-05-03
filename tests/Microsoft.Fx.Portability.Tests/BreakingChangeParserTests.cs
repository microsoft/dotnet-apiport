// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            ValidateParse(GetBreakingChangeMarkdown("005- ListT.ForEach.md"), ListTBC);
            ValidateParse(GetBreakingChangeMarkdown("006- System.Uri.md"), UriBC);
        }

        [Fact]
        public void MultiChangeParses()
        {
            ValidateParse(GetBreakingChangeMarkdown("MultiBreak.md"), ListTBC, UriBC);
        }

        [Fact]
        public void DuplicateSections()
        {
            BreakingChange bc = UriBC.DeepCopy();
            bc.VersionFixed = new Version(1, 0);
            bc.Id = ListTBC.Id;
            bc.Title = ListTBC.Title;
            bc.Details = ListTBC.Details + "\n\n\n" + UriBC.Details;
            bc.Suggestion = ListTBC.Suggestion + "\n\n" + UriBC.Suggestion;
            bc.ApplicableApis = ListTBC.ApplicableApis.Concat(UriBC.ApplicableApis).ToList();
            ValidateParse(GetBreakingChangeMarkdown("DupSections.md"), bc);
        }

        [Fact]
        public void MissingData()
        {
            BreakingChange bc = ListTBC.DeepCopy();
            bc.ImpactScope = BreakingChangeImpact.Unknown;
            bc.SourceAnalyzerStatus = BreakingChangeAnalyzerStatus.Unknown;
            bc.VersionBroken = null;
            bc.ApplicableApis = null;
            ValidateParse(GetBreakingChangeMarkdown("MissingData.md"), bc);
        }

        [Fact]
        public void MissingApis()
        {
            BreakingChange bc = UriBC.DeepCopy();
            bc.ApplicableApis = null;
            ValidateParse(GetBreakingChangeMarkdown("MissingApis.md"), bc);
        }

        [Fact]
        public void CorruptData()
        {
            BreakingChange bc = UriBC.DeepCopy();
            bc.VersionBroken = null;
            bc.ImpactScope = BreakingChangeImpact.Unknown;
            bc.SourceAnalyzerStatus = BreakingChangeAnalyzerStatus.Unknown;
            bc.IsQuirked = false;
            bc.ApplicableApis = bc.ApplicableApis.Concat(new[] { "##" }).ToList();
            bc.Suggestion = "\\0\0\0\0\0" + bc.Suggestion + "\u0001\u0002";
            ValidateParse(GetBreakingChangeMarkdown("CorruptData.md"), bc);
        }

        [Fact]
        public void IdInComments()
        {
            BreakingChange bc = new BreakingChange
            {
                Id = "144",
                Title = "Application.FilterMessage no longer throws for re-entrant implementations of IMessageFilter.PreFilterMessage",
                VersionBroken = Version.Parse("4.6.1"),
                ImpactScope = BreakingChangeImpact.Edge,
                SourceAnalyzerStatus = BreakingChangeAnalyzerStatus.Planned,
                Details = "Prior to the .NET Framework 4.6.1, calling Application.FilterMessage with an IMessageFilter.PreFilterMessage which called AddMessageFilter or RemoveMessageFilter (while also calling Application.DoEvents) would cause an IndexOutOfRangeException."
                + "\n\n"
                + "Beginning with applications targeting the .NET Framework 4.6.1, this exception is no longer thrown, and re-entrant filters as described above may be used.",
                IsQuirked = true,
                Suggestion = "Be aware that Application.FilterMessage will no longer throw for the re-entrant IMessageFilter.PreFilterMessage behavior described above. This only affects applications targeting the .NET Framework 4.6.1.",
                Categories = new List<string> { "Windows Forms" },
                Link = "https://msdn.microsoft.com/en-us/library/mt620031%28v=vs.110%29.aspx#WinForms",
                ApplicableApis = new List<string> {
                    "M:System.Windows.Forms.Application.FilterMessage(System.Windows.Forms.Message@)"
                },
                Notes = "It's unclear if this one will be better analyzed by Application.FilterMessage callers (who would have seen the exception previously)"
                + "\n" + "or the IMessageFilter.PreFilterMessage implementers (who caused the exception previously). Unfortunately, the analyzer on the caller is probably"
                + "\n" + "more useful, even though it would be easier to be 'precise' if we analyzed the interface implementer."
            };

            ValidateParse(GetBreakingChangeMarkdown("Application.FilterMessage.md"), bc);
        }

        [Fact]
        public void PartialData()
        {
            BreakingChange bc = new BreakingChange
            {
                Id = ListTBC.Id,
                Title = ListTBC.Title,
                ImpactScope = ListTBC.ImpactScope,
                VersionBroken = ListTBC.VersionBroken,
                Details = ListTBC.Details
            };

            ValidateParse(GetBreakingChangeMarkdown("PartialData.md"), bc);
        }

        [Fact]
        public void Empty()
        {
            ValidateParse(GetBreakingChangeMarkdown("Empty.md"), new BreakingChange[0]);
        }

        [Fact]
        public void RandomText()
        {
            BreakingChange bc = new BreakingChange { Title = "Chapter 2. The Mail" };
            ValidateParse(GetBreakingChangeMarkdown("RandomText.md"), bc);

            ValidateParse(GetBreakingChangeMarkdown("RandomText2.md"), new BreakingChange[0]);
        }

        #endregion


        #region Negative Test Cases
        // This is intentionally empty as the breaking change parser is never expected
        // to throw. In the case of invalid/corrupt inputs, it will do its best to return
        // partially correct breaking changes or, in the worst case, an empty set of breaks.

        #endregion


        #region Helper Methods
        private void ValidateParse(Stream markdown, params BreakingChange[] expected)
        {
            BreakingChange[] actual = BreakingChangeParser.FromMarkdown(markdown).ToArray();
            markdown.Dispose();

            Assert.Equal(expected.Length, actual.Length);

            for (int i = 0; i < expected.Length; i++)
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
            Assert.Equal(expected.SourceAnalyzerStatus, actual.SourceAnalyzerStatus);
            Assert.Equal(expected.ImpactScope, actual.ImpactScope);
        }

        private Stream GetBreakingChangeMarkdown(string resourceName)
        {
            var name = typeof(BreakingChangeParserTests).GetTypeInfo().Assembly.GetManifestResourceNames().Single(n => n.EndsWith(resourceName));
            return typeof(BreakingChangeParserTests).GetTypeInfo().Assembly.GetManifestResourceStream(name);
        }

        #endregion


        #region Expected Breaking Changes
        public static BreakingChange TemplateBC = new BreakingChange
        {
            Id = "100",
            Title = "Breaking Change Title",
            ImpactScope = BreakingChangeImpact.Major,
            VersionBroken = new Version(4, 5),
            VersionFixed = new Version(4, 6),
            Details = "Description goes here.",
            IsQuirked = false,
            IsBuildTime = false,
            SourceAnalyzerStatus = BreakingChangeAnalyzerStatus.NotPlanned,
            Suggestion = "Suggested steps if user is affected (such as work arounds or code fixes) go here.",
            ApplicableApis = new string[] { },
            Link = "LinkForMoreInformation",
            BugLink = "Bug link goes here",
            Notes = "Source analyzer status: Not usefully detectable with an analyzer"
        };

        public static BreakingChange ListTBC = new BreakingChange
        {
            Id = "5",
            Title = "List<T>.ForEach",
            ImpactScope = BreakingChangeImpact.Minor,
            VersionBroken = new Version(4, 5),
            Details = "Beginning in .NET 4.5, a List&lt;T&gt;.ForEach enumerator will throw an InvalidOperationException exception if an element in the calling collection is modified. Previously, this would not throw an exception but could lead to race conditions.",
            IsQuirked = true,
            IsBuildTime = false,
            SourceAnalyzerStatus = BreakingChangeAnalyzerStatus.Available,
            Suggestion = "Ideally, code should be fixed such that Lists are not modifed while enumerating their elements, as that is never a safe operation. To revert to the previous behavior, though, an app may target .NET 4.0.",
            ApplicableApis = new[] { "M:System.Collections.Generic.List`1.ForEach(System.Action{`0})" },
            Link = "https://msdn.microsoft.com/en-us/library/hh367887(v=vs.110).aspx#core",
            Notes = "This introduces an exception, but requires retargeting\nSource analyzer status: Pri 1, source and binary done (MikeRou)"
        };

        public static BreakingChange UriBC = new BreakingChange
        {
            Id = "6",
            Title = "System.Uri",
            ImpactScope = BreakingChangeImpact.Major,
            VersionBroken = new Version(4, 5),
            Details = "URI parsing has changed in several ways in .NET 4.5. Note, however, that these changes only affect code targeting .NET 4.5. If a binary targets .NET 4.0, the old behavior will be observed.\nChanges to URI parsing in .NET 4.5 include:<ul><li>URI parsing will perform normalization and character checking according to the latest IRI rules in RFC 3987</li><li>Unicode normalization form C will only be performed on the host portion of the URI</li><li>Invalid mailto: URIs will now cause an exception</li><li>Trailing dots at the end of a path segment are now preserved</li><li>file:// URIs do not escape the '?' character</li><li>Unicode control characters U+0080 through U+009F are not supported</li><li>Comma characters (',' %2c) are not automatically unescaped</li></ul>",
            IsQuirked = true,
            IsBuildTime = false,
            SourceAnalyzerStatus = BreakingChangeAnalyzerStatus.Available,
            Suggestion = "If the old .NET 4.0 URI parsing semantics are necessary (they often aren't), they can be used by targeting .NET 4.0. This can be accomplished by using a TargetFrameworkAttribute on the assembly, or through Visual Studio's project system UI in the 'project properties' page.",
            ApplicableApis = new[] {
                "M:System.Uri.#ctor(System.String)",
                "M:System.Uri.#ctor(System.String,System.Boolean)",
                "M:System.Uri.#ctor(System.String,System.UriKind)",
                "M:System.Uri.#ctor(System.Uri,System.String)",
                "M:System.Uri.TryCreate(System.String,System.UriKind,System.Uri@)",
                "M:System.Uri.TryCreate(System.Uri,System.String,System.Uri@)",
                "M:System.Uri.TryCreate(System.Uri,System.Uri,System.Uri@)"
            },
            Link = "https://msdn.microsoft.com/en-us/library/hh367887(v=vs.110).aspx#core",
            Notes = "Changes IRI parsing, requires access to parameters to detect\nSource analyzer status: Pri 1, source done (AlPopa)"
        };

        #endregion
    }
}
