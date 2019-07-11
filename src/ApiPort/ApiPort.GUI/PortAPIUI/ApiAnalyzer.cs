// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PortAPIUI
{
    internal class ApiAnalyzer
    {
        private readonly IApiPortService _apiPortService;
        private readonly IProgressReporter _progressReport;
        private readonly ITargetMapper _targetMapper;
        private readonly IDependencyFinder _dependencyFinder;
        private readonly IReportGenerator _reportGenerator;
        private readonly IEnumerable<IgnoreAssemblyInfo> _assembliesToIgnore;
        private readonly IFileWriter _writer;

        public ApiAnalyzer()
        {
        }

        public ApiAnalyzer(IApiPortService apiPortService, IProgressReporter progressReport, ITargetMapper targetMapper, IDependencyFinder dependencyFinder, IReportGenerator reportGenerator, IEnumerable<IgnoreAssemblyInfo> assembliesToIgnore, IFileWriter writer)
        {
            _apiPortService = apiPortService;
            _progressReport = progressReport;
            _targetMapper = targetMapper;
            _dependencyFinder = dependencyFinder;
            _reportGenerator = reportGenerator;
            _assembliesToIgnore = assembliesToIgnore;
            _writer = writer;
        }

        public void AnalyzeAssemblies(string exelocation, IApiPortService service)
        {
           FilePathAssemblyFile name = new FilePathAssemblyFile(exelocation);
           IAssemblyFile[] assemblies = new IAssemblyFile[] { name };
           var dependencyInfo = _dependencyFinder.FindDependencies(assemblies, _progressReport);
           // run it and get error b/c dependencyInfo is null - we think b/c progressReport is null ~Libba <3

           AnalyzeRequest request = GenerateRequest(dependencyInfo);

           service.SendAnalysisAsync(request);
        }

        private AnalyzeRequest GenerateRequest(IDependencyInfo dependencyInfo)
        {
            // Match the dependencyInfo for each user assembly to the given
            // input assemblies to see whether or not the assembly was explicitly
            // specified.
            foreach (var assembly in dependencyInfo.UserAssemblies)
            {
                // Windows's file paths are case-insensitive
                // var matchingAssembly = options.InputAssemblies.FirstOrDefault(x => x.Key.Name.Equals(assembly.Location, StringComparison.OrdinalIgnoreCase));

                // AssemblyInfo is explicitly specified if we found a matching
                // assembly location in the input dictionary AND the value is
                // true.
                //  assembly.IsExplicitlySpecified = matchingAssembly.Key != default(IAssemblyFile)
                //     && matchingAssembly.Value;
            }

            return new AnalyzeRequest
            {
                // Targets = options.Targets.SelectMany(_targetMapper.GetNames).ToList(),
                Dependencies = dependencyInfo.Dependencies,

                // We pass along assemblies to ignore instead of filtering them from Dependencies at this point
                // because breaking change analysis and portability analysis will likely want to filter dependencies
                // in different ways for ignored assemblies.
                // For breaking changes, we should show breaking changes for
                // an assembly if it is un-ignored on any of the user-specified targets and we should hide breaking changes
                // for an assembly if it ignored on all user-specified targets.
                // For portability analysis, on the other hand, we will want to show portability for precisely those targets
                // that a user specifies that are not on the ignore list. In this case, some of the assembly's dependency
                // information will be needed.
                AssembliesToIgnore = _assembliesToIgnore,
                UnresolvedAssemblies = dependencyInfo.UnresolvedAssemblies.Keys.ToList(),
                UnresolvedAssembliesDictionary = dependencyInfo.UnresolvedAssemblies,
                UserAssemblies = dependencyInfo.UserAssemblies.ToList(),
                AssembliesWithErrors = dependencyInfo.AssembliesWithErrors.ToList(),
                //ApplicationName = options.Description,
                Version = AnalyzeRequest.CurrentVersion,
                //RequestFlags = options.RequestFlags,
                // BreakingChangesToSuppress = options.BreakingChangeSuppressions,
                // ReferencedNuGetPackages = options.ReferencedNuGetPackages
            };
        }


    }
}
