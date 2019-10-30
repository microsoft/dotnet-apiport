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
        private bool _searchInGraph = false;

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

            // reset all the nodes in the reference graph as not be searched.
            ResetReferenceSearchGraph();

            // sum up the number of calls to available APIs and the ones for not available APIs for references.
            if (!TryGetAPICountFromReferences(target, out int availableApis, out int unavailableApis))
            {
                // Cycle detected
                return 1;
            }
            else
            {
                // prevent Div/0
                if (availableApis == 0 && unavailableApis == 0)
                    return 0;

                return availableApis / ((double)availableApis + unavailableApis);
            }
        }

        public bool TryGetAPICountFromReferences(int target, out int availAPIs, out int unavailAPIs)
        {
            availAPIs = 0;
            unavailAPIs = 0;

            foreach (var item in Nodes)
            {
                // We are going to use the flag on the object to detect if we have run into this node in the reference graph while computing the APIs for the references,
                // because we want to only count a reference node once if it is referenced in the graph more than once.
                if (item._searchInGraph == true)
                {
                    return false;
                }
                else
                {
                    item._searchInGraph = true;
                }

                if (item.UsageData != null)
                {
                    availAPIs += item.UsageData[target].GetAvailableAPICalls();
                    unavailAPIs += item.UsageData[target].GetUnavailableAPICalls();
                }

                if (!item.TryGetAPICountFromReferences(target, out int refCountAvail, out int refCountUnavail))
                {
                    // Skip the current node's references APIcounts since we have counted the references of this item before in the reference graph, continue counting the remaining nodes in the reference Nodes.
                    continue;
                }

                availAPIs += refCountAvail;
                unavailAPIs += refCountUnavail;
            }

            return true;
        }

        /// <summary>
        /// reset all the nodes in the reference graph as not be searched.
        /// </summary>
        private void ResetReferenceSearchGraph()
        {
            // if we don't have any outgoing references, done reset with this node
            if (Nodes.Count == 0)
                return;
            foreach (var item in Nodes)
            {
                item._searchInGraph = false;
                item.ResetReferenceSearchGraph();
            }

            return;
        }
    }
}
