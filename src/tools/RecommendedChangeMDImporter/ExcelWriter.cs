using Excel = Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RecommendedChangeMDImporter
{
    public class ExcelWriter
    {
        Excel.Application RecommendedChangesMDApp;
        Excel.Workbook MDWorkbook;
        Excel.Worksheet MDWorksheet;
        string RecommendedChangeMDFile;
        int rowNumber = 1;
        public ExcelWriter()
        {
            RecommendedChangesMDApp = new Excel.Application();
            //RecommendedChangesMDApp.Visible = false;
            //MDWorkbooks = RecommendedChangesMDApp.Workbooks.Open(Path.Combine(Directory.GetCurrentDirectory(), "RecommendedChangesMetaData.csv");
            MDWorkbook = RecommendedChangesMDApp.Workbooks.Add(); // Explicit cast is not required here
            MDWorksheet = (Microsoft.Office.Interop.Excel.Worksheet)MDWorkbook.ActiveSheet;
            MDWorksheet.Name = "RecommendedChangeMetaData";
            //lastRow = MySheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell).Row;
            RecommendedChangeMDFile = Path.Combine(Directory.GetCurrentDirectory(), "RecommendedChangesMetaData");
            MDWorksheet.Cells[rowNumber, 1] = "Affected APIs";
            MDWorksheet.Cells[rowNumber, 2] = "Recommended Action";
            MDWorksheet.Cells[rowNumber, 3] = "Replacement Code";
            MDWorksheet.Cells[rowNumber, 4] = "Metadata Filename";
            //string header = "Recommended Action,Affected APIs,Replacement Code" + System.Environment.NewLine;
            //File.WriteAllText(RecommendedChangeMDFile, header);

        }

        public void WriteMetadataDoc(RecommendedChangeDoc metadataDoc)
        {
            foreach (string affectedAPI in metadataDoc.AffectedAPIs)
            {
                rowNumber += 1;
                MDWorksheet.Cells[rowNumber, 1] = affectedAPI;
                MDWorksheet.Cells[rowNumber, 2] = metadataDoc.RecommendedChanges;
                MDWorksheet.Cells[rowNumber, 3] = metadataDoc.ReplacementCode;
                MDWorksheet.Cells[rowNumber, 4] = metadataDoc.MdFileName;
            }
            //File.AppendAllText(RecommendedChangeMDFile, (metadataDoc.ToString()+ System.Environment.NewLine));
        }

        ~ExcelWriter()
        {
                MDWorkbook.SaveAs(RecommendedChangeMDFile);
                MDWorkbook.Close();
                RecommendedChangesMDApp.Quit();
                MDWorksheet = null;
                MDWorkbook = null;
        }
    }
}
