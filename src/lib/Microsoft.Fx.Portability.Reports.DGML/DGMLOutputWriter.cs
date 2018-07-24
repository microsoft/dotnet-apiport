// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public void WriteStream(Stream stream, AnalyzeResponse response, AnalyzeRequest request)
        {
            XDocument file = XDocument.Parse(_template);
            XElement root = file.Root;
            root.SetAttributeValue("Title", request.ApplicationName);

            nodes = root.Element(_nameSpace + "Nodes");
            links = root.Element(_nameSpace + "Links");

            ReportingResult analysisResult = response.ReportingResult;

            if (analysisResult.GetAssemblyUsageInfo().Any())
            {
                var targets = analysisResult.Targets;
                for (int i = 0; i < targets.Count; i++)
                {
                    string targetFramework = targets[i].FullName;
                    Guid nodeGuid = Guid.NewGuid();
                    _nodesDictionary.Add(targetFramework, nodeGuid);
                    AddNode(nodeGuid, targetFramework, "Target", group: "Expanded");
                }

                List<AssemblyUsageInfo> assemblyUsageInfo = analysisResult.GetAssemblyUsageInfo().OrderBy(a => a.SourceAssembly.AssemblyIdentity).ToList();
                IDictionary<string, ICollection<string>> unresolvedAssemblies = analysisResult.GetUnresolvedAssemblies();
                foreach (var item in assemblyUsageInfo)
                {
                    string assemblyName = analysisResult.GetNameForAssemblyInfo(item.SourceAssembly);
                    IEnumerable<MissingTypeInfo> missingTypesForAssembly = analysisResult.GetMissingTypes().Where(mt => mt.UsedIn.Contains(item.SourceAssembly) && mt.IsMissing);
                    for (int i = 0; i < item.UsageData.Count; i++)
                    {
                        TargetUsageInfo usageInfo = item.UsageData[i];
                        var portabilityIndex = Math.Round(usageInfo.PortabilityIndex * 100.0, 2);
                        string framework = targets[i].FullName;
                        var missingTypesForFramework = missingTypesForAssembly.Where(mt => mt.TargetStatus.ToList()[i] == "Not supported" || (mt.TargetVersionStatus.ToList()[i] > targets[i].Version)).ToList();

                        GetOrCreateGuid($"{assemblyName},TFM:{framework}", out Guid nodeGuid);
                        AddNode(nodeGuid, $"{assemblyName} {portabilityIndex}%", GetCategory(portabilityIndex), $"{portabilityIndex}%", missingTypesForFramework.Count > 0 ? "Collapsed" : null);

                        if (_nodesDictionary.TryGetValue(framework, out Guid frameworkGuid))
                        {
                            AddLink(frameworkGuid, nodeGuid, "Contains");
                        }

                        StringBuilder sb = new StringBuilder();
                        for (int j = 0; j < missingTypesForFramework.Count; j++)
                        {
                            if (j > 0)
                                sb.Append("\n");

                            sb.Append(missingTypesForFramework[j].DocId);
                        }

                        if (sb.Length > 0)
                        {
                            Guid commentGuid = Guid.NewGuid();
                            AddNode(commentGuid, sb.ToString(), "Comment");
                            AddLink(nodeGuid, commentGuid, "Contains");
                        }
                    }

                    IList<AssemblyReferenceInformation> references = item.SourceAssembly.AssemblyReferences;
                    for (int i = 0; i < targets.Count; i++)
                    {
                        string framework = targets[i].FullName;
                        _nodesDictionary.TryGetValue($"{assemblyName},TFM:{framework}", out Guid myGuid);
                        for (int j = 0; j < references.Count; j++)
                        {
                            AssemblyReferenceInformation reference = references[j];
                            bool isUnResolvedAssembly = unresolvedAssemblies.TryGetValue(reference.Name, out var _);
                            bool isResolvedAssembly = assemblyUsageInfo.Exists(aui => analysisResult.GetNameForAssemblyInfo(aui.SourceAssembly) == reference.Name);

                            if (!(isUnResolvedAssembly || isResolvedAssembly))
                            {
                                continue;
                            }

                            bool nodeExists = GetOrCreateGuid($"{reference.Name},TFM:{framework}", out Guid referenceGuid);
                            if (isUnResolvedAssembly)
                            {
                                if (!nodeExists)
                                {
                                    AddNode(referenceGuid, $"Unresolved: {reference.Name}", "Unresolved");

                                    if (_nodesDictionary.TryGetValue(framework, out Guid frameworkGuid))
                                    {
                                        AddLink(frameworkGuid, referenceGuid, "Contains");
                                    }
                                }
                            }

                            AddLink(myGuid, referenceGuid);
                        }
                    }
                }
            }

            using (var ms = new MemoryStream())
            {
                file.Save(ms);
                ms.Position = 0;
                ms.CopyTo(stream);
            }
        }

        private bool GetOrCreateGuid(string nodeLabel, out Guid guid)
        {
            if (!_nodesDictionary.TryGetValue(nodeLabel, out guid))
            {
                guid = Guid.NewGuid();
                _nodesDictionary.Add(nodeLabel, guid);
                return false;
            }

            return true;
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

        private void AddLink(Guid source, Guid target, string category = null)
        {
            var element = new XElement(_nameSpace + "Link",
                new XAttribute("Source", source),
                new XAttribute("Target", target));

            if (category != null)
                element.SetAttributeValue("Category", category);

            links.Add(element);
        }

        private void AddNode(Guid id, string label, string category, string portabilityIndex = null, string group = null)
        {
            var element = new XElement(_nameSpace + "Node",
                new XAttribute("Id", id),
                new XAttribute("Label", label),
                new XAttribute("Category", category));

            if (portabilityIndex != null)
                element.SetAttributeValue("PortabilityIndex", portabilityIndex);
            if (group != null)
                element.SetAttributeValue("Group", group);

            nodes.Add(element);
        }

        private XElement nodes;

        private XElement links;

        private readonly Dictionary<string, Guid> _nodesDictionary = new Dictionary<string, Guid>();

        private readonly XNamespace _nameSpace = "http://schemas.microsoft.com/vs/2009/dgml";

        private readonly string _template =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
            <DirectedGraph xmlns=""http://schemas.microsoft.com/vs/2009/dgml"" Background=""grey"">
            <Nodes>
            </Nodes>
            <Links>
            </Links>
            <Categories>
                <Category Id=""VeryHigh"" Background=""#009933"" />
                <Category Id=""High"" Background=""#ffff66"" />
                <Category Id=""Medium"" Background=""#ff9900"" />
                <Category Id=""MediumLow"" Background=""#ff3300"" />
                <Category Id=""Low"" Background=""#990000"" />
                <Category Id=""Target"" Background=""white"" />
                <Category Id=""Unresolved"" Background=""red"" />
                <Category Id=""Comment"" Label=""Comment"" Description=""Represents a user defined comment on the diagram"" NavigationActionLabel=""Comments"" />
            </Categories>
            <Properties>
                <Property Id=""PortabilityIndex"" Label=""Portability Index"" DataType=""System.String"" />
            </Properties>
            <Styles>
                <Style TargetType=""Node"" GroupLabel=""Comment"" ValueLabel=""Has comment"">
                  <Condition Expression = ""HasCategory('Comment')"" />
                  <Setter Property=""Background"" Value=""#FFFFFACD"" />
                  <Setter Property=""Stroke"" Value=""#FFE5C365"" />
                  <Setter Property=""StrokeThickness"" Value=""1"" />
                  <Setter Property=""NodeRadius"" Value=""2"" />
                  <Setter Property=""MaxWidth"" Value=""250"" />
                </Style>
              </Styles>W
            </DirectedGraph>";

    }
}
