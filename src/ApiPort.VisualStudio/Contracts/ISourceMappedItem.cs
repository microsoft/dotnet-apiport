using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace ApiPortVS.Contracts
{
    public interface ISourceMappedItem
    {
        MissingInfo Item { get; }

        IEnumerable<FrameworkName> UnsupportedTargets { get; }
        
        int Column { get; }
        
        int Line { get; }
        
        string Path { get; }
    }
}