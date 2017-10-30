using Microsoft.Fx.Portability.Analyzer.Resources;
using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.Analyzer.Exceptions
{
    /// <summary>
    /// Exception thrown when assembly containing <see cref="System.Object"/>
    /// cannot be found.
    /// </summary>
    public class SystemObjectNotFoundException : PortabilityAnalyzerException
    {
        public SystemObjectNotFoundException(IEnumerable<AssemblyReferenceInformation> assemblies)
            : base(string.Format(LocalizedStrings.MissingSystemObjectAssembly,
                string.Join(", ", assemblies)))
        { }
    }
}
