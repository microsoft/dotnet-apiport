// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Fx.OpenXmlExtensions;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using Microsoft.Fx.Portability.Reports.Excel.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Microsoft.Fx.Portability.Reports
{
    internal class ExcelOpenXmlOutputWriter
    {
        internal static class ColumnWidths
        {
            internal const double Targets = 15;

            internal static class SummaryPage
            {
                internal const double AssemblyName = 40;
                internal const double TFM = 30;
            }

            internal static class DetailsPage
            {
                public const double TargetType = 40;
                public const double TargetMember = 40;
                public const double AssemblyName = 30;
                public const double RecommendedChanges = 50;
            }
        }

        // This is the second item added in AddStylesheet.  Update accordingly
        private const int _hyperlinkFontIndex = 1;
        private readonly ITargetMapper _mapper;
        private readonly string _description;
        private readonly ReportingResult _analysisReport;
        private readonly IEnumerable<BreakingChangeDependency> _breakingChanges;
        private readonly DateTimeOffset _catalogLastUpdated;

        public ExcelOpenXmlOutputWriter(
            ITargetMapper mapper,
            ReportingResult analysisReport,
            IEnumerable<BreakingChangeDependency> breakingChanges,
            DateTimeOffset catalogLastUpdated,
            string description = null)
        {
            _mapper = mapper;
            _analysisReport = analysisReport;
            _breakingChanges = breakingChanges;
            _description = description ?? string.Empty;
            _catalogLastUpdated = catalogLastUpdated;
        }

        public void WriteTo(Stream outputStream)
        {
            // Writing directly to the stream can cause problems if it is a BufferedStream (as seen when writing a multipart response)
            // This will write the spreadsheet to a temporary stream, and then copy it to the expected stream afterward
            using (var ms = new MemoryStream())
            {
                using (var spreadsheet = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook))
                {
                    spreadsheet.AddWorkbookPart();
                    spreadsheet.WorkbookPart.Workbook = new Workbook();

                    AddStylesheet(spreadsheet.WorkbookPart);

                    GenerateSummaryPage(spreadsheet.AddWorksheet(LocalizedStrings.PortabilitySummaryPageTitle), _analysisReport);
                    GenerateDetailsPage(spreadsheet.AddWorksheet(LocalizedStrings.DetailsPageTitle), _analysisReport);

                    if (_analysisReport.GetUnresolvedAssemblies().Any())
                    {
                        GenerateUnreferencedAssembliesPage(spreadsheet.AddWorksheet(LocalizedStrings.MissingAssembliesPageTitle), _analysisReport);
                    }

                    if (_breakingChanges.Any())
                    {
                        GenerateBreakingChangesPage(spreadsheet.AddWorksheet(LocalizedStrings.BreakingChanges), _breakingChanges);
                    }

                    if (_analysisReport.NuGetPackages?.Any() ?? false)
                    {
                        GenerateNuGetInfoPage(spreadsheet.AddWorksheet(LocalizedStrings.SupportedPackages), _analysisReport);
                    }
                }

                ms.Position = 0;
                ms.CopyTo(outputStream);
            }
        }

        private void AddStylesheet(WorkbookPart wb)
        {
            var cellstyle = new CellStyle { Name = "Normal", FormatId = 0U, BuiltinId = 0U };
            var border = new Border(new LeftBorder(), new RightBorder(), new TopBorder(), new BottomBorder(), new DiagonalBorder());

            var fill1 = new Fill(new PatternFill { PatternType = PatternValues.None });
            var fill2 = new Fill(new PatternFill { PatternType = PatternValues.Gray125 });

            var format1 = new CellFormat { FontId = 0U };
            var format2 = new CellFormat { FontId = 1U, ApplyFont = true };

            var textFont = new Font(
                new FontSize { Val = 11D },
                new Color { Theme = 1U },
                new FontName { Val = "Calibri" },
                new FontFamilyNumbering { Val = 2 },
                new FontScheme { Val = FontSchemeValues.Minor });

            var hyperlinkFont = new Font(
                new Underline(),
                new FontSize { Val = 11D },
                new Color { Theme = 10U },
                new FontName { Val = "Calibri" },
                new FontFamilyNumbering { Val = 2 },
                new FontScheme { Val = FontSchemeValues.Minor });

            var stylesheet = new Stylesheet
            {
                Fonts = new Fonts(textFont, hyperlinkFont),
                CellFormats = new CellFormats(format1, format2),
                Fills = new Fills(fill1, fill2),
                CellStyles = new CellStyles(cellstyle),
                Borders = new Borders(border),
            };

            wb.AddNewPart<WorkbookStylesPart>();
            wb.WorkbookStylesPart.Stylesheet = stylesheet;
        }

        private void GenerateSummaryPage(Worksheet summaryPage, ReportingResult analysisResult)
        {
            var targetNames = _mapper.GetTargetNames(analysisResult.Targets, alwaysIncludeVersion: true);

            // This is the submission id
            summaryPage.AddRow(LocalizedStrings.SubmissionId, AddSubmissionLink(analysisResult.SubmissionId));

            // This is the description of the app
            summaryPage.AddRow(LocalizedStrings.Description, _description);

            // This is the target list that was submitted to the service.
            summaryPage.AddRow(LocalizedStrings.Targets, string.Join(",", targetNames));

            // Add an empty row.
            summaryPage.AddRow();

            if (analysisResult.GetAssemblyUsageInfo().Any())
            {
                var assemblyInfoHeader = new List<string> { LocalizedStrings.AssemblyHeader, "Target Framework" };
                assemblyInfoHeader.AddRange(targetNames);
                int tableRowCount = 0;

                summaryPage.AddRow(assemblyInfoHeader.ToArray());
                tableRowCount++;

                foreach (var item in analysisResult.GetAssemblyUsageInfo().OrderBy(a => a.SourceAssembly.AssemblyIdentity))
                {
                    var summaryData = new List<object>() { analysisResult.GetNameForAssemblyInfo(item.SourceAssembly), item.SourceAssembly.TargetFrameworkMoniker ?? string.Empty };

                    // TODO: figure out how to add formatting to cells to show percentages.
                    summaryData.AddRange(item.UsageData.Select(pui => (object)Math.Round(pui.PortabilityIndex * 100.0, 2)));
                    summaryPage.AddRow(summaryData.ToArray());
                    tableRowCount++;
                }

                summaryPage.AddConditionalFormatting(6, analysisResult.GetAssemblyUsageInfo().Count(), 3, analysisResult.Targets.Count);
                summaryPage.AddTable(5, tableRowCount, 1, assemblyInfoHeader.ToArray());

                var columnWidths = new List<double>
                {
                    ColumnWidths.SummaryPage.AssemblyName,
                    ColumnWidths.SummaryPage.TFM
                };

                columnWidths.AddRange(Enumerable.Repeat(ColumnWidths.Targets, analysisResult.Targets.Count)); // Targets

                summaryPage.AddColumnWidth(columnWidths);
            }

            summaryPage.AddRow();
            summaryPage.AddRow(LocalizedStrings.CatalogLastUpdated, _catalogLastUpdated.ToString("D", CultureInfo.CurrentCulture));
            summaryPage.AddRow(LocalizedStrings.HowToReadTheExcelTable);
        }

        private static void GenerateUnreferencedAssembliesPage(Worksheet missingAssembliesPage, ReportingResult analysisResult)
        {
            List<string> missingAssembliesPageHeader = new List<string>() { LocalizedStrings.AssemblyHeader, LocalizedStrings.UsedBy, LocalizedStrings.MissingAssemblyStatus };
            int detailsRows = 0;
            missingAssembliesPage.AddRow(missingAssembliesPageHeader.ToArray());
            detailsRows++;

            var unresolvedAssembliesMap = analysisResult.GetUnresolvedAssemblies();

            foreach (var unresolvedAssemblyPair in unresolvedAssembliesMap.OrderBy(asm => asm.Key))
            {
                if (unresolvedAssemblyPair.Value.Any())
                {
                    foreach (var usedIn in unresolvedAssemblyPair.Value)
                    {
                        missingAssembliesPage.AddRow(unresolvedAssemblyPair.Key, usedIn, LocalizedStrings.UnresolvedUsedAssembly);
                        detailsRows++;
                    }
                }
                else
                {
                    missingAssembliesPage.AddRow(unresolvedAssemblyPair.Key, string.Empty, LocalizedStrings.UnresolvedUsedAssembly);
                }
            }

            // Generate the pretty table
            missingAssembliesPage.AddTable(1, detailsRows, 1, missingAssembliesPageHeader.ToArray());
            missingAssembliesPage.AddColumnWidth(40, 40, 30);
        }

        private void GenerateDetailsPage(Worksheet detailsPage, ReportingResult analysisResult)
        {
            var showAssemblyColumn = analysisResult.GetAssemblyUsageInfo().Any();

            List<string> detailsPageHeader = new List<string>() { LocalizedStrings.TargetTypeHeader, LocalizedStrings.TargetMemberHeader };

            if (showAssemblyColumn)
            {
                detailsPageHeader.Add(LocalizedStrings.AssemblyHeader);
            }

            detailsPageHeader.AddRange(_mapper.GetTargetNames(analysisResult.Targets, alwaysIncludeVersion: true));
            detailsPageHeader.Add(LocalizedStrings.RecommendedChanges);

            int detailsRows = 0;
            detailsPage.AddRow(detailsPageHeader.ToArray());
            detailsRows++;

            // Dump out all the types that were identified as missing from the target
            foreach (var item in analysisResult.GetMissingTypes().OrderByDescending(n => n.IsMissing))
            {
                if (item.IsMissing)
                {
                    if (!showAssemblyColumn)
                    {
                        // for a missing type we are going to dump the type name for both the target type and target member columns
                        var rowContent = new List<object> { AddLink(item.TypeName), AddLink(item.TypeName) };

                        rowContent.AddRange(item.TargetStatus);
                        rowContent.Add(item.RecommendedChanges);
                        detailsPage.AddRow(rowContent.ToArray());
                        detailsRows++;
                    }
                    else
                    {
                        foreach (var assemblies in item.UsedIn)
                        {
                            string assemblyName = analysisResult.GetNameForAssemblyInfo(assemblies);

                            // for a missing type we are going to dump the type name for both the target type and target member columns
                            List<object> rowContent = new List<object> { AddLink(item.TypeName), AddLink(item.TypeName), assemblyName };
                            rowContent.AddRange(item.TargetStatus);
                            rowContent.Add(item.RecommendedChanges);
                            detailsPage.AddRow(rowContent.ToArray());
                            detailsRows++;
                        }
                    }
                }

                foreach (var member in item.MissingMembers.OrderBy(type => type.MemberName))
                {
                    if (showAssemblyColumn)
                    {
                        foreach (var assem in member.UsedIn.OrderBy(asm => asm.AssemblyIdentity))
                        {
                            string assemblyName = analysisResult.GetNameForAssemblyInfo(assem);
                            var rowContent = new List<object> { AddLink(item.TypeName), AddLink(member.MemberName), assemblyName };

                            rowContent.AddRange(member.TargetStatus);
                            rowContent.Add(member.RecommendedChanges);
                            detailsPage.AddRow(rowContent.ToArray());
                            detailsRows++;
                        }
                    }
                    else
                    {
                        var rowContent = new List<object> { AddLink(item.TypeName), AddLink(member.MemberName) };

                        rowContent.AddRange(member.TargetStatus);
                        rowContent.Add(member.RecommendedChanges);
                        detailsPage.AddRow(rowContent.ToArray());
                        detailsRows++;
                    }
                }
            }

            // Generate the pretty tables
            detailsPage.AddTable(1, detailsRows, 1, detailsPageHeader.ToArray());

            // Generate the columns
            var columnWidths = new List<double>
            {
                ColumnWidths.DetailsPage.TargetType, // Target type
                ColumnWidths.DetailsPage.TargetMember, // Target member
                ColumnWidths.DetailsPage.AssemblyName // Assembly name
            };
            columnWidths.AddRange(Enumerable.Repeat(ColumnWidths.Targets, analysisResult.Targets.Count)); // Targets
            columnWidths.Add(ColumnWidths.DetailsPage.RecommendedChanges); // Recommended changes

            detailsPage.AddColumnWidth(columnWidths);
        }

        private void GenerateBreakingChangesPage(Worksheet worksheet, IEnumerable<BreakingChangeDependency> breakingChanges)
        {
            var row = 1;

            var header = new[]
            {
                "Break ID",
                "API",
                "Assembly",
                "Title",
                "Scope",
                "Quirked",
                "Requires Retargeting",
                "Build Time",
                "VersionBroken",
                "Version Fixed",
                "Details",
                "Suggestion",
                "Analyzer Status",
                "Link",
                "Investigated"
            };

            worksheet.AddRow(header);

            foreach (var breakingChange in breakingChanges)
            {
                var rowContent = new object[]
                {
                    breakingChange.Break.Id,
                    breakingChange.Member.MemberDocId,
                    breakingChange.DependantAssembly.ToString(),
                    breakingChange.Break.Title,
                    breakingChange.Break.ImpactScope.ToString(),
                    breakingChange.Break.IsQuirked.ToString(),
                    breakingChange.Break.IsRetargeting.ToString(),
                    breakingChange.Break.IsBuildTime.ToString(),
                    breakingChange.Break.VersionBroken.ToString(),
                    breakingChange.Break.VersionFixed?.ToString() ?? string.Empty,
                    breakingChange.Break.Details,
                    breakingChange.Break.Suggestion,
                    breakingChange.Break.SourceAnalyzerStatus.ToString(),
                    string.IsNullOrWhiteSpace(breakingChange.Break.Link) ? "No link" : CreateHyperlink("Link", breakingChange.Break.Link),
                    string.Empty
                };

                worksheet.AddRow(rowContent);
                row++;
            }

            worksheet.AddTable(1, row, 1, header);
            worksheet.AddColumnWidth(10, 10, 30, 30, 20, 10, 10, 10, 10, 30, 30, 10, 10, 10);
        }

        private void GenerateNuGetInfoPage(Worksheet page, ReportingResult analysisResult)
        {
            bool showAssemblyName = analysisResult.NuGetPackages.Any(p => !string.IsNullOrEmpty(p.AssemblyInfo));

            var headerList = new List<string>() { LocalizedStrings.PackageIdHeader };

            headerList.AddRange(_mapper.GetTargetNames(analysisResult.Targets));

            if (showAssemblyName)
            {
                headerList.Add(LocalizedStrings.AssemblyHeader);
            }

            var header = headerList.ToArray();
            page.AddRow(header);

            int rowCount = 1;

            foreach (var nugetInfo in analysisResult.NuGetPackages)
            {
                var rowContent = new List<string>() { nugetInfo.PackageId };

                foreach (var target in analysisResult.Targets)
                {
                    var supported = nugetInfo.SupportedVersions.TryGetValue(target, out var version) ? version : LocalizedStrings.NotSupported;
                    rowContent.Add(supported);
                }

                if (showAssemblyName && nugetInfo.AssemblyInfo != null)
                {
                    rowContent.Add(nugetInfo.AssemblyInfo);
                }

                page.AddRow(rowContent.ToArray());
                rowCount++;
            }

            page.AddTable(1, rowCount, 1, header.ToArray());
            page.AddColumnWidth(70, 40, 30, 30);
        }

        private object CreateHyperlink(string displayString, string link)
        {
            return new HyperlinkCell
            {
                DisplayString = displayString,
                Url = new Uri(link),
                StyleIndex = _hyperlinkFontIndex
            };
        }

        private static object AddSubmissionLink(string submissionId)
        {
            return submissionId;

            // TODO: Add back in logic to create URIs when finished abstracting IReportWriter.
#if FALSE
            var headers = _analysisReport.Headers;
            // If no website is provided, do not create hyperlink
            if (String.IsNullOrWhiteSpace(headers.WebsiteEndpoint) || String.IsNullOrWhiteSpace(headers.SubmissionUrl))
            {
                return submissionId;
            }

            return new HyperlinkCell
            {
                DisplayString = submissionId,
                Url = headers.GetSubmissionUrl(submissionId),
                StyleIndex = _hyperlinkFontIndex
            };
#endif
        }

        private static object AddLink(string docId)
        {
            return docId;

            // TODO: Add back in logic to create URIs when finished abstracting IReportWriter.
#if FALSE
            var headers = _analysisReport.Headers;
            // If no website is provided, do not create hyperlink
            if (String.IsNullOrWhiteSpace(headers.WebsiteEndpoint) || String.IsNullOrWhiteSpace(headers.ApiInfoUrl))
            {
                return docId;
            }

            return new HyperlinkCell
            {
                DisplayString = docId,
                Url = headers.GetDocIdUrl(docId),
                StyleIndex = _hyperlinkFontIndex
            };
#endif
        }
    }
}
