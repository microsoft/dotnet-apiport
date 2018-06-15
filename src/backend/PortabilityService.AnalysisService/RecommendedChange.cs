// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using CommonMark;
using CommonMark.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Fx.Portability
{
    public sealed class RecommendedChange
    {
        public string RecommendedAction { get; set; }

        public ICollection<string> AffectedApis { get; set; }

        public static RecommendedChange ParseFromMarkdown(string contents)
        {
            return MarkdownParser.Parse(contents);
        }

        private static class MarkdownParser
        {
            private static List<string> s_empty = new List<string>();
            private const string RecommendedAction = "Recommended Action";

            private const string AffectedApis = "Affected APIs";

            /// <summary>
            /// Parses markdown formatted as: https://github.com/Microsoft/dotnet-apiport/blob/master/docs/RecommendedChanges/!%20Template.md
            /// </summary>
            /// <returns>The corresponding RecommendedChange; returns null if unable to parse the markdown contents</returns>
            public static RecommendedChange Parse(string contents)
            {
                var root = CommonMarkConverter.Parse(contents);
                var currentBlock = root.FirstChild;

                string recommendedAction = null;
                IEnumerable<string> affectedApis = null;

                while (currentBlock != null)
                {
                    string header = null;

                    if (TryParseAtxHeading(currentBlock, out header))
                    {
                        if (header.Equals(RecommendedAction, StringComparison.Ordinal))
                        {
                            currentBlock = currentBlock.NextSibling;

                            if (!TryParseParagraph(currentBlock, out recommendedAction))
                            {
                                Trace.TraceError($"Could not parse recommended action.");
                                return null;
                            }
                        }
                        else if (header.Equals(AffectedApis, StringComparison.Ordinal))
                        {
                            if (!TryParseList(currentBlock.NextSibling, out affectedApis))
                            {
                                Trace.TraceError($"Could not parse the affected apis");
                                return null;
                            }
                        }
                        else
                        {
                            Trace.TraceError($"Could not parse the ATX header");
                            return null;
                        }
                    }

                    currentBlock = currentBlock.NextSibling;
                }

                return new RecommendedChange
                {
                    RecommendedAction = recommendedAction ?? string.Empty,
                    AffectedApis = affectedApis?.ToList() ?? s_empty
                };
            }

            private static bool TryParseParagraph(Block block, out string contents)
            {
                contents = null;

                if (block.Tag != BlockTag.Paragraph)
                    return false;

                contents = block.InlineContent.LiteralContent;

                return true;
            }

            private static bool TryParseAtxHeading(Block block, out string header)
            {
                header = null;

                if (block.Tag != BlockTag.AtxHeading)
                    return false;

                header = block.InlineContent?.LiteralContent;

                return !string.IsNullOrEmpty(header);
            }

            private static bool TryParseList(Block block, out IEnumerable<string> listContents)
            {
                listContents = null;

                if (block.Tag != BlockTag.List)
                    return false;

                List<string> apis = new List<string>();

                var listItem = block.FirstChild;

                while (listItem != null)
                {
                    string api = null;

                    if (TryParseListItem(listItem, out api))
                    {
                        apis.Add(api);
                    }
                    else
                    {
                        // log something about not being able to parse the list item and continue;
                    }

                    listItem = listItem.NextSibling;
                }

                listContents = apis;

                return true;
            }

            private static bool TryParseListItem(Block block, out string listItem)
            {
                listItem = null;
                var contents = block.FirstChild;

                if (block.Tag != BlockTag.ListItem || contents == null)
                    return false;

                return TryParseParagraph(contents, out listItem);
            }
        }
    }
}
