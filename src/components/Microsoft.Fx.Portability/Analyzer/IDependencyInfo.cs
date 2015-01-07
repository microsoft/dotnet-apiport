using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.Analyzer
{
    public interface IDependencyInfo
    {
        IDictionary<MemberInfo, ICollection<AssemblyInfo>> Dependencies { get; }
        IEnumerable<string> AssembliesWithErrors { get; }
        IDictionary<string, ICollection<string>> UnresolvedAssemblies { get; }
        IEnumerable<AssemblyInfo> UserAssemblies { get; }
    }
}
