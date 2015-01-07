using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability
{
    public interface ITargetNameParser
    {
        /// <summary>
        /// Maps the list of targets specified as strings to a list of supported target names.
        /// </summary>
        IEnumerable<FrameworkName> MapTargetsToExplicitVersions(IEnumerable<string> targets);

        IEnumerable<FrameworkName> DefaultTargets { get; }
    }
}
