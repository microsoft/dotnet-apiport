using Microsoft.Fx.Portability.Resources;
using System;

namespace Microsoft.Fx.Portability.Reporting.ObjectModel
{
    public class MissingInfo 
    {
        public string DocId { get; set; }
        public string RecommendedChanges { get; protected set; }


        public MissingInfo(string docId)
        {
            DocId = docId;
        }

        public override int GetHashCode()
        {
            return DocId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            MissingInfo other = obj as MissingInfo;
            return other != null && StringComparer.Ordinal.Equals(other.DocId, DocId);
        }

        protected string GenerateTargetStatusMessage(Version version)
        {
            if (version == null)
            {
                return LocalizedStrings.NotSupported;
            }
            else
            {
                return String.Format(LocalizedStrings.SupportedOn, version);
            }
        }
    }
}
