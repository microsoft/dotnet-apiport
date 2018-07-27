// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Fx.Portability.Reports.DGML
{
    class ReferenceNode
    {
        public string SimpleName
        {
            get
            {
                if (!IsMissing)
                    return new AssemblyName(Assembly).Name;

                return "Unresolved: " + new AssemblyName(Assembly).Name;
            }
        }
        public ReferenceNode(string AssemblyName, bool unresolved = false)
        {
            Assembly = AssemblyName;
            this.Unresolved = unresolved;
            Nodes = new HashSet<ReferenceNode>();
        }
        public override int GetHashCode()
        {
            return Assembly.GetHashCode();
        }

        public void AddReferenceToNode(ReferenceNode node)
        {
            Nodes.Add(node);
        }

        public override string ToString()
        {
            return Assembly;
        }

        public List<TargetUsageInfo> UsageData { get; set; }

        public string Assembly { get; set; }

        public bool Unresolved { get; set; }

        public HashSet<ReferenceNode> Nodes { get; set; }
        public bool IsMissing { get; internal set; }
    }
}
