// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Reports.DGML
{
    public class DGMLOutputWriter : IReportWriter
    {
        public ResultFormatInformation Format => new ResultFormatInformation()
        {
            DisplayName = "DGML",
            MimeType = "application/xml",
            FileExtension = ".dgml"
        };

        private readonly DGMLManager dgml = new DGMLManager();

        public Task WriteStreamAsync(Stream stream, AnalyzeResponse response)
        {
            ReferenceGraph rg = ReferenceGraph.CreateGraph(response);

            ReportingResult analysisResult = response.ReportingResult;
            var targets = analysisResult.Targets;
            GenerateTargetContainers(targets);
            dgml.SetTitle(response.ApplicationName);

            // For each target, let's generate the assemblies
            foreach (var node in rg.Nodes.Keys)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    double portabilityIndex = 0, portabilityIndexRefs = 0;
                    string missingTypes = null;
                    if (node.UsageData != null)
                    {
                        TargetUsageInfo usageInfo = node.UsageData[i];
                        portabilityIndex = node.GetPortabilityIndex(i);
                        portabilityIndexRefs = node.GetPortabilityIndexForReferences(i);

                        missingTypes = GenerateMissingTypes(node.Assembly, analysisResult, i);
                    }

                    // generate the node
                    string tfm = targets[i].FullName;
                    Guid nodeGuid = dgml.GetOrCreateGuid($"{node.Assembly},TFM:{tfm}");
                    string nodeTitle = $"{node.SimpleName}: {Math.Round(portabilityIndex * 100, 2)}%, References: {Math.Round(portabilityIndexRefs * 100, 2)}%";
                    string nodeCategory = node.IsMissing ? "Unresolved" : GetCategory(Math.Round(portabilityIndex * portabilityIndexRefs * 100, 2));

                    dgml.AddNode(nodeGuid, nodeTitle,
                        nodeCategory,
                        portabilityIndex,
                        group: string.IsNullOrEmpty(missingTypes) ? null : "Collapsed");

                    if (dgml.TryGetId(tfm, out Guid frameworkGuid))
                    {
                        dgml.AddLink(frameworkGuid, nodeGuid, "Contains");
                    }

                    if (!string.IsNullOrEmpty(missingTypes))
                    {
                        Guid commentGuid = Guid.NewGuid();
                        dgml.AddNode(commentGuid, missingTypes, "Comment");
                        dgml.AddLink(nodeGuid, commentGuid, "Contains");
                    }
                }
            }

            // generate the references.
            foreach (var node in rg.Nodes.Keys)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    // generate the node
                    string tfm = targets[i].FullName;
                    Guid nodeGuid = dgml.GetOrCreateGuid($"{node.Assembly},TFM:{tfm}");

                    foreach (var refNode in node.Nodes)
                    {
                        Guid refNodeGuid = dgml.GetOrCreateGuid($"{refNode.Assembly},TFM:{tfm}");
                        dgml.AddLink(nodeGuid, refNodeGuid);
                    }
                }
            }

            dgml.Save(stream);

            return Task.CompletedTask;
        }

        private static string GenerateMissingTypes(string assembly, ReportingResult response, int i)
        {
            // for a given assembly identity and a given target usage, display the missing types
            IEnumerable<MissingTypeInfo> missingTypesForAssembly = response.GetMissingTypes()
                                                                    .Where(mt => mt.UsedIn.Any(x => x.AssemblyIdentity == assembly) && mt.IsMissing);

            var missingTypesForFramework = missingTypesForAssembly
                                            .Where(mt => mt.TargetStatus.ToList()[i] == "Not supported" || (mt.TargetVersionStatus.ToList()[i] > response.Targets[i].Version))
                                            .Select(x => x.DocId).OrderBy(x => x);

            return string.Join("\n", missingTypesForFramework);
        }

        private void GenerateTargetContainers(IList<FrameworkName> targets)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                string targetFramework = targets[i].FullName;
                Guid nodeGuid = dgml.GetOrCreateGuid(targetFramework);
                dgml.AddNode(nodeGuid, targetFramework, "Target", null, group: "Expanded");
            }
        }

        private static string GetCategory(double probabilityIndex)
        {
            if (probabilityIndex == 100.0)
                return "VeryHigh";
            if (probabilityIndex >= 75.0)
                return "High";
            if (probabilityIndex >= 50.0)
                return "Medium";
            if (probabilityIndex >= 30.0)
                return "MediumLow";

            return "Low";
        }
    }
}
