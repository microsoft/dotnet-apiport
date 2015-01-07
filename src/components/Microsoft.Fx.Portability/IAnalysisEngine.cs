using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.Versioning;
using Microsoft.Fx.Portability.ObjectModel;

namespace Microsoft.Fx.Portability
{
    public interface IAnalysisEngine
    {
        IEnumerable<string> FindUnreferencedAssemblies(IEnumerable<string> unreferencedAssemblies, IEnumerable<AssemblyInfo> specifiedUserAssemblies);
        IList<MemberInfo> FindMembersNotInTargets(IEnumerable<FrameworkName> targets, IDictionary<MemberInfo, ICollection<AssemblyInfo>> dependencies);
    }

}
