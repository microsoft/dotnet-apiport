using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class ApiInfoStorage
    {
        public string DocId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public IReadOnlyCollection<FrameworkName> Targets { get; set; }
        public IReadOnlyCollection<ApiMetadataStorage> Metadata { get; set; }
    }
}
