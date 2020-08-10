// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Fx.Portability.Reports.Excel.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Fx.OpenXmlExtensions
{
    internal static class OpenXmlExtensions
    {
        public static Worksheet AddWorksheet(this SpreadsheetDocument spreadsheet, string name)
        {
            var sheets = spreadsheet.WorkbookPart.Workbook.GetFirstChild<Sheets>();
            if (sheets == null)
            {
                sheets = new Sheets();
                spreadsheet.WorkbookPart.Workbook.AppendChild(sheets);
            }

            var worksheetPart = spreadsheet.WorkbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet();

            worksheetPart.Worksheet.Save();

            // create the worksheet to workbook relation
            sheets.AppendChild(new Sheet()
            {
                Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = new DocumentFormat.OpenXml.UInt32Value((uint)sheets.Count() + 1),
                Name = name
            });

            // create sheet data
            return worksheetPart.Worksheet;
        }

        public static Table AddTable(this Worksheet worksheet, int rowStart, int rowCount, int columnStart, params string[] headers)
        {
            if (rowCount == 1)
            {
                rowCount++;
            }

            var range = ComputeRange(rowStart, rowCount, columnStart, headers.Length);

            var sheetViews = worksheet.GetFirstChild<SheetViews>();
            if (sheetViews == null)
            {
                sheetViews = worksheet.InsertAt(new SheetViews(), 0);
            }

            var sheetView = sheetViews.AppendChild(new SheetView());
            sheetView.WorkbookViewId = 0;

            var selection = sheetView.AppendChild(new Selection());
            selection.SequenceOfReferences = new ListValue<StringValue>() { InnerText = range };
            selection.ActiveCell = range.Substring(0, range.IndexOf(":", StringComparison.Ordinal));

            var tableDefPart = worksheet.WorksheetPart.AddNewPart<TableDefinitionPart>();

            var tp = new TablePart
            {
                Id = worksheet.WorksheetPart.GetIdOfPart(tableDefPart)
            };
            var tableParts = worksheet.GetFirstChild<TableParts>();
            if (tableParts == null)
            {
                tableParts = worksheet.AppendChild(new TableParts());
            }

            tableParts.AppendChild(tp);

            var tableID = GetNextTableId(worksheet);

            tableDefPart.Table = new Table()
            {
                Id = tableID,
                Name = tableID.ToString(CultureInfo.CurrentCulture),
                DisplayName = "Table" + tableID.ToString(CultureInfo.CurrentCulture)
            };
            tableDefPart.Table.Reference = range;

            var columnCount = (uint)headers.Length;
            var tc = tableDefPart.Table.AppendChild(new TableColumns() { Count = columnCount });
            for (uint i = 0; i < columnCount; i++)
            {
                tc.AppendChild(new TableColumn() { Id = i + 1, Name = headers[i] });
            }

            tableDefPart.Table.AutoFilter = new AutoFilter
            {
                Reference = range
            };

            var styleInfo = tableDefPart.Table.AppendChild(new TableStyleInfo());
            styleInfo.Name = "TableStyleMedium2";
            styleInfo.ShowFirstColumn = false;
            styleInfo.ShowRowStripes = true;
            styleInfo.ShowLastColumn = false;
            styleInfo.ShowColumnStripes = false;

            return tableDefPart.Table;
        }

        public static void AddConditionalFormatting(this Worksheet worksheet, int rowStart, int rowCount, int columnStart, int columnCount)
        {
            var range = ComputeRange(rowStart, rowCount, columnStart, columnCount);

            var conditionalFormatting1 = new ConditionalFormatting()
            {
                SequenceOfReferences = new ListValue<StringValue>() { InnerText = range }
            };

            var conditionalFormattingRule1 =
                new ConditionalFormattingRule() { Type = ConditionalFormatValues.ColorScale, Priority = 1 };

            var colorScale1 = new ColorScale();
            var conditionalFormatValueObject1 = new ConditionalFormatValueObject() { Type = ConditionalFormatValueObjectValues.Number, Val = "0" };
            var conditionalFormatValueObject2 = new ConditionalFormatValueObject() { Type = ConditionalFormatValueObjectValues.Percentile, Val = "50" };
            var conditionalFormatValueObject3 = new ConditionalFormatValueObject() { Type = ConditionalFormatValueObjectValues.Number, Val = "100" };
            var color1 = new Color() { Rgb = "FFF8696B" };
            var color2 = new Color() { Rgb = "FFFFEB84" };
            var color3 = new Color() { Rgb = "FF63BE7B" };

            colorScale1.Append(conditionalFormatValueObject1);
            colorScale1.Append(conditionalFormatValueObject2);
            colorScale1.Append(conditionalFormatValueObject3);
            colorScale1.Append(color1);
            colorScale1.Append(color2);
            colorScale1.Append(color3);

            conditionalFormattingRule1.Append(colorScale1);

            conditionalFormatting1.Append(conditionalFormattingRule1);

            // If we don't have this after SheetData, it corrupts the file if we have added hyperlinks before
            worksheet.InsertAfter(conditionalFormatting1, worksheet.Descendants<SheetData>().First());
        }

        public static void AddRow(this Worksheet ws, params object[] data)
        {
            var sd = ws.GetFirstChild<SheetData>();
            if (sd == null)
            {
                sd = ws.AppendChild(new SheetData());
            }

            var row = sd.AppendChild(new Row());

            foreach (var item in data)
            {
                if (item == null)
                {
                    row.AppendChild(new Cell());
                }
                else if (item is HyperlinkCell)
                {
                    var hyperlinkCell = item as HyperlinkCell;

                    var cell = CreateTextCell(hyperlinkCell.DisplayString);

                    if (hyperlinkCell.StyleIndex.HasValue)
                    {
                        cell.StyleIndex = (UInt32Value)hyperlinkCell.StyleIndex.Value;
                    }

                    row.AppendChild(cell);

                    var hlRelationship = ws.WorksheetPart.AddHyperlinkRelationship(hyperlinkCell.Url, true);

                    var hyperlink = new Hyperlink
                    {
                        Reference = GetCellRefence(sd, row),
                        Id = hlRelationship.Id
                    };

                    var hyperlinks = ws.Descendants<Hyperlinks>().FirstOrDefault() ?? ws.AppendChild(new Hyperlinks());

                    hyperlinks.Append(hyperlink);
                }
                else if (item is string)
                {
                    row.AppendChild(CreateTextCell(item as string));
                }
                else
                {
                    row.AppendChild(CreateNumberCell(item.ToString()));
                }
            }
        }

        /// <summary>
        /// Determines the cell reference for a row.  This assumes that the cell of interest
        /// is the next cell to be added to the row.
        /// </summary>
        private static string GetCellRefence(SheetData sd, Row row)
        {
            var rowCount = sd.Descendants<Row>().TakeWhile(r => r != row).Count() + 1;

            // Column needs to be 0-based for the GetColumnName method
            var columnCount = row.Descendants<Cell>().Count() - 1;

            return string.Format(CultureInfo.CurrentCulture, "{0}{1}", GetColumnName(columnCount), rowCount);
        }

        /// <summary>
        /// Excel column names are base-26 using letters A-Z.  This converts a base-10
        /// index into the corresponding Excel column name.
        /// </summary>
        private static string GetColumnName(int index)
        {
            const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var sb = new StringBuilder();

            if (index >= Alphabet.Length)
            {
                sb.Append(Alphabet[(index / Alphabet.Length) - 1]);
            }

            sb.Append(Alphabet[index % Alphabet.Length]);

            return sb.ToString();
        }

        public static void AddColumnWidth(this Worksheet ws, params double[] columnWidths)
        {
            AddColumnWidth(ws, (IEnumerable<double>)columnWidths);
        }

        public static void AddColumnWidth(this Worksheet ws, IEnumerable<double> columnWidths)
        {
            var columns = new Columns();

            uint pos = 1;
            foreach (var width in columnWidths)
            {
                var column = new Column()
                {
                    Min = (UInt32Value)pos,
                    Max = (UInt32Value)pos,
                    Width = width,
                    BestFit = true,
                    CustomWidth = true
                };
                columns.Append(column);

                pos++;
            }

            var sd = ws.GetFirstChild<SheetData>();

            if (sd != null)
            {
                // insert this before the sheetdata (if it exists possible).
                ws.InsertBefore<Columns>(columns, sd);
            }
            else
            {
                ws.Append(columns);
            }
        }

        public static Cell CreateTextCell(string text)
        {
            var cell = new Cell
            {
                DataType = CellValues.InlineString,
            };

            var inlineString = new InlineString
            {
                Text = new Text(text)
            };

            cell.AppendChild(inlineString);

            return cell;
        }

        public static Cell CreateNumberCell(string value)
        {
            return new Cell
            {
                CellValue = new CellValue(value)
            };
        }

        private static string ComputeRange(int rowStart, int rowCount, int columnStart, int columnCount)
        {
            // Only support up to 26 columns for now..
            if (columnStart + columnCount > 26)
            {
                throw new NotSupportedException(LocalizedStrings.TooManyColumns);
            }

            return string.Format(CultureInfo.CurrentCulture, "{0}{1}:{2}{3}", (char)(((uint)'A') + columnStart - 1), rowStart, (char)(((uint)'A') + columnStart + columnCount - 2), rowStart + rowCount - 1);
        }

        // Table ids must be unique for the whole document
        private static uint GetNextTableId(Worksheet w)
        {
            var result = 0L;
            var spreadsheet = (SpreadsheetDocument)w.WorksheetPart.OpenXmlPackage;

            foreach (var wp in spreadsheet.WorkbookPart.WorksheetParts)
            {
                foreach (var part in wp.GetPartsOfType<TableDefinitionPart>())
                {
                    var value = part.Table?.Id;

                    if (value != null && value.HasValue)
                    {
                        result = Math.Max(result, value.Value);
                    }
                }
            }

            return (uint)(result + 1);
        }
    }
}
