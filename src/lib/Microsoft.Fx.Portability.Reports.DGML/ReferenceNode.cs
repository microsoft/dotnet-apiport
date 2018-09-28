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

            // sum up the number of calls to available APIs and the ones for not available APIs for references.
            if (!TryGetAPICountFromReferences(target, out int availableApis, out int unavailableApis))
            {
                // Cycle detected
                return 1;
            }
            else
            {
                // remove the calls from the current node.
                availableApis -= UsageData[target].GetAvailableAPICalls();
                unavailableApis -= UsageData[target].GetUnavailableAPICalls();

                // prevent Div/0
                if (availableApis == 0 && unavailableApis == 0)
                    return 0;

                return availableApis / ((double)availableApis + unavailableApis);
            }
        }

        public bool TryGetAPICountFromReferences(int target, out int availAPIs, out int unavailAPIs)
        {
            availAPIs = UsageData[target].GetAvailableAPICalls();
            unavailAPIs = UsageData[target].GetUnavailableAPICalls();

            // We are going to use a flag on the object to detect if we have a reference cycle while computing the APIs for the references.
            if (_searchInGraph == true)
            {
                // Cycle!!!
                _searchInGraph = false; // Reset this flag
                return false;
            }
            else
            {
                _searchInGraph = true;
            }

            foreach (var item in Nodes)
            {
                if (!item.TryGetAPICountFromReferences(target, out int refCountAvail, out int refCountUnavail))
                {
                    // Cycle!
                    _searchInGraph = false; // Reset this flag

                    return false;
                }

                availAPIs += refCountAvail;
                unavailAPIs += refCountUnavail;
            }

            _searchInGraph = false;
            return true;
        }
    }
}
