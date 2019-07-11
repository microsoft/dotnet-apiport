// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Reporting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            _apiPortService = App.Resolve<IApiPortService>();
            _progressReport = App.Resolve<IProgressReporter>();
             _dependencyFinder = App.Resolve<IDependencyFinder>();
           
        }



        public void AnalyzeAssemblies(string exelocation, IApiPortService service)
        {
           FilePathAssemblyFile name = new FilePathAssemblyFile(exelocation);
           IAssemblyFile[] assemblies = new IAssemblyFile[] { name };
           var dependencyInfo = _dependencyFinder.FindDependencies(assemblies, _progressReport);
           // run it and get error b/c dependencyInfo is null - we think b/c progressReport is null ~Libba <3

           AnalyzeRequest request = GenerateRequest(dependencyInfo);

           var result =   service.SendAnalysisAsync(request).Result;
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
                Targets = new List<string> { ".NET Core, Version = 3.0" },
                Dependencies = dependencyInfo.Dependencies,

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
