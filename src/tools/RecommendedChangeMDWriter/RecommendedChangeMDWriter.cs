using Microsoft.Office.Interop.Excel;
using RecommendedChangeMDImporter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RecommendedChangeMDWriter.Resources;

namespace RecommendedChangeMDWriter
{
    class RecommendedChangeMDWriter
    {
        private class Constants
        {
            public const string NamespacePrefix = "N:";
            public const string TypePrefix = "T:";
            public const string MemberPrefix = "M:";
            public const string EventPrefix = "E:";
            public const string FieldPrefix = "F:";
            public const string PropertyPrefix = "P:";
        }
        //root directory where md files will write to
        string mdRootDir = null;
        public void WriteMarkDownFiles(string root, List<RecommendedChangeDoc> recommendedChangeDocs)
        {
            mdRootDir = root;
            List<RecommendedChangeDoc> sameDocs = new List<RecommendedChangeDoc>();
            if ( !Directory.Exists( root))
            {
                throw new ApplicationException("MarkDown file root directory {0} doesn't exist, please create the directory first before run this tool.");
            }

            string currentNameSpace = null;
            string previousNameSpace = null;
            foreach( RecommendedChangeDoc currentDoc in recommendedChangeDocs)
            {
                if (currentDoc != null )
                {
                    currentDoc.MdFileName = GetMDFileName(currentDoc.RecommendedChanges);
                    if (currentNameSpace != null)
                        previousNameSpace = currentNameSpace;
                    currentNameSpace = GetNameSpace((string)(currentDoc.AffectedAPIs[0]));
                    currentDoc.MdDir = Path.Combine(mdRootDir, currentNameSpace);
                    //check if the previous recommendedChangeDoc has the same recommended change content
                    if (sameDocs.Count > 0)
                    {
                        //if recommended change content is the same, can be writen to a same markdown file
                        //however, if different namespaces have the same recommended change content, we still want to separate them into different namespace files.
                        if ((String.Compare(sameDocs[sameDocs.Count - 1].RecommendedChanges, currentDoc.RecommendedChanges, true) == 0)
                            && (String.Compare(previousNameSpace, currentNameSpace) == 0))
                        {
                            sameDocs.Add(currentDoc);
                            continue;
                        }
                        else
                        {
                            WriteMarkDownfile(sameDocs);
                            //is call RemoveAll() better?
                            sameDocs = new List<RecommendedChangeDoc>();
                            sameDocs.Add(currentDoc);
                            continue;
                        }
                    }
                    else
                    {
                        sameDocs.Add(currentDoc);
                    }
                }
            }
            //write the remaining to the markdown file.
            WriteMarkDownfile(sameDocs);
        }

        public string GetNameSpace(string docId)
        {
            string prefix = docId.Substring(0, 2);
            string nameSpace = null;
            int suffixIndex = -1;

            switch (prefix)
            {
                case Constants.NamespacePrefix:
                    nameSpace = docId.Substring(2);
                    break;
                case Constants.TypePrefix:
                    suffixIndex = docId.LastIndexOf('.');
                    nameSpace = docId.Substring(2, suffixIndex - 2);
                    break;
                case Constants.EventPrefix:
                case Constants.FieldPrefix:
                case Constants.PropertyPrefix:
                case Constants.MemberPrefix:
                    //remove the parameter portion
                    int itemp = docId.IndexOf('(');
                    int startIdx = itemp == -1? docId.Length-1: itemp;

                    //remove the member and type
                    for (int i = 0; i < 2; i++)
                    {
                        suffixIndex = docId.LastIndexOf('.', startIdx);
                        if (suffixIndex > 0)
                            startIdx = suffixIndex - 1;
                        else
                            throw new InvalidDataException("unexpected DocId: " + docId);
                    }
                    nameSpace = docId.Substring(2, suffixIndex - 2);
                    break;
                default:
                    throw new NotSupportedException($"This xml docId type is not supported: {prefix}");
            }
            return nameSpace;
        }

        public string GetMDFileName (string recommendedChange)
        {
            string mdFileName = null;
            int idx = 0;
            int nextspace = 0;
            //take the first 5 word from recommendedChange as title of the MD file name with the assumption that it will be unique within the namespace (MD files are in namespace folder)
            for (int i = 0; i < 5; i++)
            {
                nextspace = recommendedChange.Substring(idx).IndexOf(' ');
                idx += nextspace;
                if (nextspace != -1)
                    idx += 1;
                else
                {
                    //recommendedChange has 5 or less word. Use the full content as the file name.
                    idx = recommendedChange.Length;
                    break;
                }
            }
            mdFileName = recommendedChange.Substring(0, idx).Trim() + ".md";
            //if file name string contains ":" or "\", which is drive or directory separator, not valid in filename, replace them with "-"
            if (mdFileName.IndexOf(@":") != -1 )
                mdFileName = mdFileName.Replace(@":", @"-");
            if (mdFileName.IndexOf(@"\") != -1)
                mdFileName = mdFileName.Replace(@"\", @"-");
            return mdFileName;
        }

        public void WriteMarkDownfile(List<RecommendedChangeDoc> sameDocs)
        {
            if (sameDocs == null)
                throw new ArgumentNullException("sameDocs shouldn't bee null");
            else if(sameDocs.Count <= 0)
                throw new ArgumentException("sameDocs should have one or more elements");
            try { 
                //all the elements in sameDocs should have the same filename and recommended change content. 
                string mdFileName = Path.GetFileName(sameDocs[0].MdFileName);
                string mdDir = sameDocs[0].MdDir;

                if (!Directory.Exists(mdDir))
                {
                    Directory.CreateDirectory(mdDir);
                }
                using (StreamWriter sw = File.CreateText(Path.Combine(mdDir, mdFileName)))
                {
                    sw.WriteLine("### " + LocalizedStrings.RecommendedChange);
                    sw.WriteLine(sameDocs[0].RecommendedChanges);
                    sw.WriteLine();

                    sw.WriteLine("### " + LocalizedStrings.AffectedAPIs);
                    foreach (RecommendedChangeDoc doc in sameDocs)
                    {
                        foreach (string docId in doc.AffectedAPIs)
                        {
                            sw.WriteLine("* `" + docId + "`");
                        }

                    }

                    if (!String.IsNullOrEmpty(sameDocs[0].ReplacementCode))
                    {
                        sw.WriteLine();
                        sw.WriteLine("### " + LocalizedStrings.ReplacementCode);
                        sw.WriteLine(sameDocs[0].ReplacementCode);
                    }
                }
            }catch( Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
