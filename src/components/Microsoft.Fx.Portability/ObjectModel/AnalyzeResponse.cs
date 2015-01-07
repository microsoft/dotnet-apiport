using Microsoft.Fx.Portability.Reporting.ObjectModel;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public sealed class AnalyzeResponse
    {
        public AnalyzeResponse()
        {
            MissingDependencies = new List<MemberInfo>();
            UnresolvedUserAssemblies = new List<string>();
            Targets = new List<FrameworkName>();
        }

        public IList<MemberInfo> MissingDependencies { get; set; }

        public IList<string> UnresolvedUserAssemblies { get; set; }

        public IList<FrameworkName> Targets { get; set; }

        public string SubmissionId { get; set; }

        [JsonIgnore]
        public ReportingResult ReportingResult { get; set; }
    }
}
