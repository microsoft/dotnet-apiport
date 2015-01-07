using System.Collections.Generic;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class DotNetCatalog
    {
        public string BuiltBy { get; set; }
        public IReadOnlyCollection<ApiInfoStorage> Apis { get; set; }
        public IReadOnlyCollection<string> FrameworkAssemblyIdenties { get; set; }
        public IReadOnlyCollection<TargetInfo> SupportedTargets { get; set; }
    }
}
