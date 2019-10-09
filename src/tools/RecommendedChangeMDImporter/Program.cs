using System;
using System.IO;

namespace RecommendedChangeMDImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length <=0 && Directory.Exists(args[0]))
                Console.WriteLine("Usage: RecommendedChangeMDImporter <RootDirectory>");
            String rootDir = args[0];
            RecommendedChangeDoc currentDoc = null;
            ExcelWriter excelWriter = new ExcelWriter();
            RecommendedChangeMarkDownReader recommendedChangeMDReader = new RecommendedChangeMarkDownReader();
            try { 
                var txtFiles = Directory.EnumerateFiles(rootDir, "*.md", SearchOption.AllDirectories);

                foreach (string currentFile in txtFiles)
                {
                    currentDoc = recommendedChangeMDReader.MDReader(currentFile);
                    excelWriter.WriteMetadataDoc(currentDoc);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}
