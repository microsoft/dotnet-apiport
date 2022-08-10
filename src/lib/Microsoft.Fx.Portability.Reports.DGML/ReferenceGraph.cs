// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability.Reports.DGML
{
    internal class ReferenceGraph
    {
        public Dictionary<ReferenceNode, ReferenceNode> Nodes { get; }

        public static ReferenceGraph CreateGraph(AnalyzeResponse response)
        {
            ReferenceGraph rg = new ReferenceGraph();
            AnalyzeRequest request = response.Request;

            // get the list of assemblies that have some data reported for them.
            var assembliesWithData = response.ReportingResult.GetAssemblyUsageInfo().ToDictionary(x => x.SourceAssembly.AssemblyIdentity, x => x.UsageData);

            var unresolvedAssemblies = response.ReportingResult.GetUnresolvedAssemblies().Select(x => x.Key).ToList();

            // Add every user specified assembly to the graph
            foreach (var userAsem in request.UserAssemblies)
            {
                var node = rg.GetOrAddNodeForAssembly(new ReferenceNode(userAsem.AssemblyIdentity));

                // For this node, make sure we capture the data, if we have it.
                if (assembliesWithData.ContainsKey(node.Assembly))
                {
                    node.UsageData = assembliesWithData[node.Assembly];
                }

                // create nodes for all the references, if non platform.
                if (userAsem.AssemblyReferences != null)
                {
                    foreach (var reference in userAsem.AssemblyReferences)
                    {
                        if (!(assembliesWithData.ContainsKey(reference.ToString()) || unresolvedAssemblies.Contains(reference.ToString())))
                        {
                            // platform reference (not in the user specified asssemblies and not an unresolved assembly.
                            continue;
                        }

                        var refNode = rg.GetOrAddNodeForAssembly(new ReferenceNode(reference.ToString()));

                        // if the reference is missing, flag it as such.
                        refNode.IsMissing = unresolvedAssemblies.Contains(reference.ToString());

                        node.AddReferenceToNode(refNode);
                    }
                }
            }

            return rg;
        }

        public ReferenceGraph()
        {
            Nodes = new Dictionary<ReferenceNode, ReferenceNode>(ReferenceNodeComparer.Instance);
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
