using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Fx.Portability.ObjectModel
{
    /// <summary>
    /// Model used to pass exception data through the Analyze response.
    /// Includes the Apis assembly, DocId, and the list of exceptions that it throws.
    /// </summary>
    public class ExceptionInfo
    {
        public string DefinedInAssemblyIdentity { get; set; }

        public string MemberDocId { get; set; }

        public List<ApiException> ExceptionsThrown { get; set; }

        [JsonIgnore]
        public bool IsSupportedAcrossTargets { get; set; }
    }
}
