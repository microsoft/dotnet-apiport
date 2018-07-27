// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Xml.Linq;

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

        private DGMLManager dgml = new DGMLManager();

        public void WriteStream(Stream stream, AnalyzeResponse response, AnalyzeRequest request)
        {
            ReferenceGraph rg = ReferenceGraph.CreateGraph(response, request);

            ReportingResult analysisResult = response.ReportingResult;
            var targets = analysisResult.Targets;
            GenerateTargetContainers(targets);
            dgml.SetTitle(response.ApplicationName);

            //for each target, let's generate the assemblies
            foreach (var node in rg.Nodes.Keys)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    double portabilityIndex = 0;
                    string missingTypes = null;
                    if (node.UsageData != null)
                    {
                        TargetUsageInfo usageInfo = node.UsageData[i];
                        portabilityIndex = Math.Round(usageInfo.PortabilityIndex * 100.0, 2);

                        missingTypes = GenerateMissingTypes(node.Assembly, analysisResult, i);
                    }

                    // generate the node
                    string tfm = targets[i].FullName;
                    dgml.GetOrCreateGuid($"{node.Assembly},TFM:{tfm}", out Guid nodeGuid);

                    dgml.AddNode(nodeGuid, $"{node.SimpleName}, {portabilityIndex}%",
                        node.IsMissing ? "Unresolved" : GetCategory(portabilityIndex),
                        portabilityIndex,
                        group: missingTypes.Length == 0 ? null : "Collapsed");

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
                    dgml.GetOrCreateGuid($"{node.Assembly},TFM:{tfm}", out Guid nodeGuid);

                    foreach (var refNode in node.Nodes)
                    {
                        dgml.GetOrCreateGuid($"{refNode.Assembly},TFM:{tfm}", out Guid refNodeGuid);

                        dgml.AddLink(nodeGuid, refNodeGuid);
                    }
                }
            }

            dgml.Save(stream);

            return;
        }

        private static string GenerateMissingTypes(string assembly, ReportingResult response, int i)
        {
            // for a given assembly identity and a given target usage, display the missing types
            //TODO: this is very allocation heavy.
            IEnumerable<MissingTypeInfo> missingTypesForAssembly = response.GetMissingTypes().Where(mt => mt.UsedIn.Any(x => x.AssemblyIdentity == assembly) && mt.IsMissing);
            var missingTypesForFramework = missingTypesForAssembly.Where(mt => mt.TargetStatus.ToList()[i] == "Not supported" || (mt.TargetVersionStatus.ToList()[i] > response.Targets[i].Version)).Select(x => x.DocId).OrderBy(x => x);

            return string.Join("\n", missingTypesForFramework);
        }

        private void GenerateTargetContainers(IList<FrameworkName> targets)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                string targetFramework = targets[i].FullName;
                Guid nodeGuid = Guid.NewGuid();
                dgml.AddId(targetFramework, nodeGuid);
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
