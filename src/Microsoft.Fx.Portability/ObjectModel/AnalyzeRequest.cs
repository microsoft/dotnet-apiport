// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public sealed class AnalyzeRequest : IComparable
    {
        public const byte CurrentVersion = 2;

        public AnalyzeRequestFlags RequestFlags { get; set; }

        public IDictionary<MemberInfo, ICollection<AssemblyInfo>> Dependencies { get; set; }

        public ICollection<string> UnresolvedAssemblies { get; set; }

        [JsonIgnore]
        public IDictionary<string, ICollection<string>> UnresolvedAssembliesDictionary { get; set; }

        public ICollection<AssemblyInfo> UserAssemblies { get; set; }

        public ICollection<string> AssembliesWithErrors { get; set; }

        public ICollection<string> Targets { get; set; }

        public string ApplicationName { get; set; }

        public byte Version { get; set; }

        public IEnumerable<string> BreakingChangesToSuppress { get; set; }

        public IEnumerable<IgnoreAssemblyInfo> AssembliesToIgnore { get; set; }

        public IEnumerable<string> ReferencedNuGetPackages { get; set; }

        public int CompareTo(object obj)
        {
            var analyzeObject = obj as AnalyzeRequest;

            return string.Compare(ApplicationName, analyzeObject.ApplicationName, StringComparison.Ordinal);
        }
    }
}
