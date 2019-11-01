// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Fx.Portability.Reports.DGML
{
    internal class ReferenceNode
    {
        public List<TargetUsageInfo> UsageData { get; set; }

        public string Assembly { get; }

        public bool Unresolved { get; set; }

        public HashSet<ReferenceNode> Nodes { get; }

        public bool IsMissing { get; internal set; }

        public string SimpleName
        {
            get
            {
                if (!IsMissing)
                    return new AssemblyName(Assembly).Name;

                return "Unresolved: " + new AssemblyName(Assembly).Name;
            }
        }

        public ReferenceNode(string assemblyName, bool unresolved = false)
        {
            Assembly = assemblyName;
            Unresolved = unresolved;
            Nodes = new HashSet<ReferenceNode>(ReferenceNodeComparer.Instance);
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

        public double GetPortabilityIndex(int target)
        {
            return UsageData[target].PortabilityIndex;
        }

        public double GetPortabilityIndexForReferences(int target)
        {
            // if we don't have any outgoing references, it is a good sign!
            if (Nodes.Count == 0)
                return 1;

            // HashSet keep track of the node went through to prevent duplicated counting in the reference graph and circular reference
            HashSet<ReferenceNode> searchedNodes = new HashSet<ReferenceNode>();

            // sum up the number of calls to available APIs and the ones for not available APIs for references.
            GetAPICountFromReferences(target, searchedNodes, out int availableApis, out int unavailableApis);

            // prevent Div/0. When availableApis is 0 and unavailableApis is 0, it likely means that it's references are all not resulted assemblies, return 0.
            if (availableApis == 0 && unavailableApis == 0)
                return 0;

            return availableApis / ((double)availableApis + unavailableApis);
        }

        private void GetAPICountFromReferences(int target, HashSet<ReferenceNode> searchedNodes, out int availAPIs, out int unavailAPIs)
        {
            availAPIs = 0;
            unavailAPIs = 0;

            foreach (var item in Nodes)
            {
                // searchedNodes.Add(item) return false if the same node has been added before, which means its ApiCounts has been counted as well, we should keep the node,
                // because we want to only count a reference node once if it is referenced in the graph more than once. This also avoids cycular reference as well.
                if (searchedNodes.Add(item))
                {
                    if (item.UsageData != null)
                    {
                        try
                        {
                            availAPIs += item.UsageData[target].GetAvailableAPICalls();
                            unavailAPIs += item.UsageData[target].GetUnavailableAPICalls();
                        }
                        catch (Exception)
                        {
                            // for any exception like item.UsageData[target] is null or item.UsageData[target] throws IndexOutOfRangeException. Ignore it and continue.
                        }
                    }

                    item.GetAPICountFromReferences(target, searchedNodes, out int refCountAvail, out int refCountUnavail);

                    availAPIs += refCountAvail;
                    unavailAPIs += refCountUnavail;
                }
            }
        }
    }
}
