using Microsoft.Fx.Portability.Reporting.ObjectModel;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public sealed class AnalyzeRequest 
    {
        public const byte CurrentVersion = 2;

        public AnalyzeRequestFlags RequestFlags { get; set; }

        public IDictionary<MemberInfo, ICollection<AssemblyInfo>> Dependencies { get; set; }

        public ICollection<string> UnresolvedAssemblies { get; set; }

        /// <summary>
        /// TODO: Remove JsonIgnore when we get LCA approval on this.
        /// TODO: Find a way to remove this property since it contains user-defined assembly names.
        /// </summary>
        [JsonIgnore]
        public IDictionary<string, ICollection<string>> UnresolvedAssembliesDictionary { get; set; }

        public ICollection<AssemblyInfo> UserAssemblies { get; set; }
        
        public ICollection<string> AssembliesWithErrors { get; set; }
        
        public ICollection<string> Targets { get; set; }

        public string ApplicationName { get; set; }

        public byte Version { get; set; }
    }
}
