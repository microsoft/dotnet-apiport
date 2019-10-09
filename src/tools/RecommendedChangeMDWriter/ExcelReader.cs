using Excel = Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Text;
using RecommendedChangeMDImporter;

namespace RecommendedChangeMDWriter
{
    class ExcelReader
    {
        List<RecommendedChangeDoc> recommendedChangeDocs = new List<RecommendedChangeDoc>();
        public ExcelReader(string excelFileName)
        {
            //Create COM Objects.
            Excel.Application excelApp = new Excel.Application();
            if (excelApp == null)
            {
                throw new ApplicationException ("Excel is not installed! Please install excel before run this app.");
            }

            Excel.Workbook excelBook = excelApp.Workbooks.Open(excelFileName);
            int countSheets = excelBook.Sheets.Count;
            Excel.Worksheet excelSheet = (Excel.Worksheet)excelBook.Sheets[1];
            string sheetName = excelSheet.Name;
            int r = excelSheet.Rows.Count;
            int c = excelSheet.Columns.Count;
            //Excel.Worksheet excelSheet = (Excel.Worksheet)excelBook.ActiveSheet;
            Excel.Range excelRange = excelSheet.UsedRange;
            int rows = excelRange.Rows.Count;
            int cols = excelRange.Columns.Count;
            int recommendedChangeColumn = -1;
            int affectedAPIsColumn = -1;
            int replacementCodeColumn = -1;
            RecommendedChangeDoc currentDoc = null;
            Excel.Range cellRange = null;
            string cellStr = null;

            //verify each column's header
            for (int j=1; j <= cols; j++)
            {
                cellRange = (Excel.Range)excelRange.Cells[1, j];
                cellStr = (string)cellRange.Value2;
                if (cellStr != null)
                {
                    switch (cellStr.Trim().ToLowerInvariant())
                    {
                        case "recommended action":
                            recommendedChangeColumn = j;
                            break;
                        case "affected apis":
                            affectedAPIsColumn = j;
                            break;
                        case "replacement code":
                            replacementCodeColumn = j;
                            break;
                        default:
                            //ignore all the other columns in the excel file.
                            break;
                    }
                }
            }

            for (int i = 2; i <= rows; i++)
            {
                currentDoc = new RecommendedChangeDoc();
                currentDoc.RecommendedChanges = excelRange.Cells[i, recommendedChangeColumn] == null? null: (string)(((Excel.Range)excelRange.Cells[i, recommendedChangeColumn]).Value2);
                currentDoc.AddAffectedAPIs( excelRange.Cells[i, affectedAPIsColumn] == null? null: (string)(((Excel.Range)excelRange.Cells[i, affectedAPIsColumn]).Value2) );
                //it's okay if replacementCode Column doesn't exist, just skip. 
                if(replacementCodeColumn > 1)
                    currentDoc.ReplacementCode = excelRange.Cells[i, replacementCodeColumn] == null ? null : (string)(((Excel.Range)excelRange.Cells[i, replacementCodeColumn]).Value2);
                recommendedChangeDocs.Add(currentDoc);
            }
            //after reading, release the excel project
            excelBook.Close();
            excelApp.Quit();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
        }

        public List<RecommendedChangeDoc> RecommendedChangeDocs { get => recommendedChangeDocs;}
    }
}
