// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability.Reports.DGML
{
    class ReferenceGraph
    {
        public static ReferenceGraph CreateGraph(AnalyzeResponse response, AnalyzeRequest request)
        {
            ReferenceGraph rg = new ReferenceGraph();

            // get the list of assemblies that have some data reported for them.
            var assembliesWithData = response.ReportingResult.GetAssemblyUsageInfo().ToDictionary(x => x.SourceAssembly.AssemblyIdentity, x => x.UsageData);

            var unresolvedAssemblies = response.ReportingResult.GetUnresolvedAssemblies().Select(x => x.Key).ToList();

            // Add every user specified assembly to the graph
            foreach (var userAsem in request.UserAssemblies)
            {
                var node = rg.GetOrAddNodeForAssembly(new ReferenceNode(userAsem.AssemblyIdentity));

                //for this node, make sure we capture the data, if we have it.
                if (assembliesWithData.ContainsKey(node.Assembly))
                {
                    node.UsageData = assembliesWithData[node.Assembly];
                }

                // create nodes for all the references, if non platform.
                foreach (var reference in userAsem.AssemblyReferences)
                {
                    if (!(assembliesWithData.ContainsKey(reference.ToString()) || unresolvedAssemblies.Contains(reference.ToString())))
                    {
                        // platform reference (not in the user specified asssemblies and not an unresolved assembly.
                        continue;
                    }

                    var refNode = rg.GetOrAddNodeForAssembly(new ReferenceNode(reference.ToString()));

                    // if the reference is missing, flag it as such.
                    if (unresolvedAssemblies.Contains(reference.ToString()))
                    {
                        refNode.IsMissing = true;
                    }

                    node.AddReferenceToNode(refNode);
                }
            }

            if (rg.HasCycles())
            {
                // do nothing as we don't support this scenario.
                return rg;
            }

            rg.ComputeNewPortabilityIndex();

            return rg;
        }

        private void ComputeNewPortabilityIndex()
        {
            // TODO: update the index for the assemblies based on their references.
        }

        private bool HasCycles()
        {
            //TODO: implement
            return false;
        }

        public Dictionary<ReferenceNode, ReferenceNode> Nodes { get; set; }

        public ReferenceGraph()
        {
            Nodes = new Dictionary<ReferenceNode, ReferenceNode>(new ReferenceNodeComparer());
        }

        public ReferenceNode GetOrAddNodeForAssembly(ReferenceNode node)
        {
            if (Nodes.ContainsKey(node))
                return Nodes[node];

            Nodes.Add(node, node);
            return node;
        }
    }
}
