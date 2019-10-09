using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonMark;

namespace RecommendedChangeMDImporter
{ 
    class RecommendedChangeMarkDownReader
    {
        RecommendedChangeDoc recommendedChangeDoc;
        public RecommendedChangeMarkDownReader()
        {
            recommendedChangeDoc = new RecommendedChangeDoc();
        }

        public RecommendedChangeDoc MDReader(string currentFile)
        {
            bool atRecommendedAction = false;
            StringBuilder sRecommendedAction = null;
            bool atReplacementCode = false;
            StringBuilder sReplacementCode = null;
            bool atAffectedAPIs = false;
            CommonMark.Syntax.Block syntaxBlock = null;
            recommendedChangeDoc.MdFileName = currentFile;

            using (StreamReader reader = File.OpenText(currentFile))
            {
                syntaxBlock = CommonMarkConverter.Parse(reader);
                syntaxBlock = syntaxBlock.FirstChild;
                while (syntaxBlock != null)
                {
                    if (syntaxBlock.Heading.Level == 3)
                    {
                        if (String.Compare(syntaxBlock.InlineContent.LiteralContent, "Recommended Action", true) == 0)
                        {
                            atAffectedAPIs = atReplacementCode = false;
                            atRecommendedAction = true;
                            sRecommendedAction = new StringBuilder();
                        }
                        else if (String.Compare(syntaxBlock.InlineContent.LiteralContent, "Affected APIs", true) == 0)
                        {
                            atRecommendedAction = atReplacementCode = false;
                            atAffectedAPIs = true;
                        }
                        else if (String.Compare(syntaxBlock.InlineContent.LiteralContent, "Replacement Code", true) == 0)
                        {
                            atReplacementCode = atAffectedAPIs = false;
                            atReplacementCode = true;
                            sReplacementCode = new StringBuilder();
                        }
                        else
                        {
                            throw new InvalidDataException("unexpected 3rd level in md file: " + syntaxBlock.InlineContent.LiteralContent);
                        }
                    }
                    else if (syntaxBlock.Heading.Level == 0)
                    {
                        if (atRecommendedAction)
                        {
                            sRecommendedAction.Append(this.ReadParagraphBlock(syntaxBlock));
                        }
                        else if (atReplacementCode)
                        {
                            sReplacementCode.Append(this.ReadParagraphBlock(syntaxBlock));
                        }
                        else if (atAffectedAPIs)
                        {
                            //since there could be more than one affected APIs in a MD doc. Each API should have its own row in CSV file.
                            recommendedChangeDoc.AffectedAPIs = this.ReadListBlock(syntaxBlock);
                        }
                    }
                    else
                    {
                        throw new InvalidDataException("unexpected section in md file: " + syntaxBlock.InlineContent.LiteralContent);
                    }

                    syntaxBlock = syntaxBlock.NextSibling;
                }
            }
            recommendedChangeDoc.RecommendedChanges = sRecommendedAction == null ? null : sRecommendedAction.ToString();
            recommendedChangeDoc.ReplacementCode = sReplacementCode == null ? null : sReplacementCode.ToString();
            return recommendedChangeDoc;
        }

        private List<string> ReadListBlock(CommonMark.Syntax.Block syntaxBlock)
        {
            List<string> affectedAPIs = new List<string>();
            CommonMark.Syntax.Block listBlock = syntaxBlock.FirstChild;
            while (listBlock != null)
            {
                if ((listBlock.Tag is CommonMark.Syntax.BlockTag.ListItem))
                {
                    if ((listBlock.FirstChild != null) && (listBlock.FirstChild.Tag is CommonMark.Syntax.BlockTag.Paragraph))
                    {
                        affectedAPIs.Add(this.ReadParagraphBlock(listBlock.FirstChild));
                    }
                    listBlock = listBlock.NextSibling;
                }
                else
                {
                    throw new InvalidDataException("Unexpected Tag: " + listBlock.Tag);
                }
            }
            return affectedAPIs;
        }
        private string ReadParagraphBlock(CommonMark.Syntax.Block syntaxBlock)
        {
            if (syntaxBlock.InlineContent == null) return null;

            StringBuilder paragraphBuilder = new StringBuilder();
            paragraphBuilder.Append(syntaxBlock.InlineContent.LiteralContent);

            CommonMark.Syntax.Inline paragraphBlock = syntaxBlock.InlineContent.NextSibling;

            while (paragraphBlock != null)
            {
                if (paragraphBlock.LiteralContent != null)
                {
                    paragraphBuilder.Append(paragraphBlock.LiteralContent);
                }
                paragraphBlock = paragraphBlock.NextSibling;
            }
            return paragraphBuilder.ToString();
        }

    }
}
