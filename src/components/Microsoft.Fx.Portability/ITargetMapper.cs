using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability
{
    public interface ITargetMapper
    {
        string GetAlias(string targetName);
        ICollection<string> GetNames(string aliasName);
        ICollection<string> Aliases { get; }
        IEnumerable<string> GetTargetNames(IEnumerable<FrameworkName> targets, bool alwaysIncludeVersion = false);
    }
}