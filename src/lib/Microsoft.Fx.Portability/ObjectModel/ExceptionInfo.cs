using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class ExceptionInfo
    {
        public string DefinedInAssemblyIdentity { get; set; }

        public string MemberDocId { get; set; }

        public List<ApiException> ExceptionsThrown { get; set; }

        [JsonIgnore]
        public bool IsSupportedAcrossTargets { get; set; }
    }
}
