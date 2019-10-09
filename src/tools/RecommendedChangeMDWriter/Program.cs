using System;
using System.IO;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;

namespace RecommendedChangeMDWriter
{
    class Program
    {
        /// <summary>
        /// Convert Recommended Changes from excel spreadsheet to markdown files
        /// Usage: RecommendedChangeMDWriter --inputExcelFile [excelfilename] --outputMDRootDir [rootDirofGeneratedMDFiles]
        /// </summary>
        /// <param name="inputExcelFile">The excel spreadsheet file name with path containing recommended chanages</param>
        /// <param name="outputMDRootDir">The root directory where the generated markdown files will be saved to</param>
        static void Main(string inputExcelFile, string outputMDRootDir)
        {
            var option1 = new Option("--inputExcelFile")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ExactlyOne
                }
            };
            //var result = parser.Parse("-x");

            //result.Errors
            //      .Should()
            //      .Contain(e => e.Message == "Required argument missing for option: -x");

            option1.AddAlias("-i");
            var option2 = new Option("--outputMDRootDir")
            {
                Argument = new Argument
                {
                    Arity = ArgumentArity.ExactlyOne
                }
            };
            option2.AddAlias("-o");
            Console.WriteLine("Read from {0}", inputExcelFile);

            ExcelReader excelReader = new ExcelReader(inputExcelFile);
            RecommendedChangeMDWriter mdWriter = new RecommendedChangeMDWriter();
            mdWriter.WriteMarkDownFiles(outputMDRootDir, excelReader.RecommendedChangeDocs);
        }
    }
}
