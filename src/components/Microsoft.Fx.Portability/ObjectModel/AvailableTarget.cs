using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class AvailableTarget
    {
        public AvailableTarget()
        {
            ExpandedTargets = Enumerable.Empty<string>();
        }

        public string Name { get; set; }
        public Version Version { get; set; }
        public IEnumerable<string> ExpandedTargets { get; set; }
        public bool IsSet { get; set; }
    }
}
