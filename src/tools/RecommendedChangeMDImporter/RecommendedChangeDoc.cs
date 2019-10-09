using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommendedChangeMDImporter
{
    public class RecommendedChangeDoc
    {
        string recommendedChanges = null;
        IList affectedAPIs = new List<string>();
        string replacementCode = null;
        string mdFileName;
        string mdDir;

        public string RecommendedChanges { get => recommendedChanges; set => recommendedChanges = value; }
        public IList AffectedAPIs { get => affectedAPIs; set => affectedAPIs = value; }

        public string ReplacementCode { get => replacementCode; set => replacementCode = value; }
        public string MdFileName { get => mdFileName; set => mdFileName = value; }
        public string MdDir { get => mdDir; set => mdDir = value; }

        public void AddAffectedAPIs(string affectedAPI)
        {
            affectedAPIs.Add(affectedAPI);
        }
   }
}
