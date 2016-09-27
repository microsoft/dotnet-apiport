// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPortVS.Resources;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace ApiPortVS
{
    public class AnalysisOptions : IApiPortOptions
    {
        internal const string DefaultReportFilename = "PortabilityAnalysis.html";

        public string Description { get; private set; }
        public IEnumerable<IAssemblyFile> InputAssemblies { get; private set; }
        public AnalyzeRequestFlags RequestFlags { get; private set; }
        public IEnumerable<string> Targets { get; private set; }
        public IEnumerable<string> OutputFormats { get; private set; }
        public string OutputFileName { get; private set; }
        public IEnumerable<string> IgnoredAssemblyFiles { get { return Enumerable.Empty<string>(); } }

        public AnalysisOptions(string description, IEnumerable<string> inputAssemblies, IEnumerable<string> targets, string format, bool discardMetadata, string outputFileName)
        {
            Description = description;
            InputAssemblies = inputAssemblies.Select(target => new AssemblyFile(target)).ToList();
            Targets = targets.ToList();
            OutputFormats = new[] { format };
            OutputFileName = outputFileName;

            RequestFlags |= AnalyzeRequestFlags.ShowNonPortableApis;

            if (discardMetadata)
            {
                RequestFlags |= AnalyzeRequestFlags.NoTelemetry;
            }
        }

        public IEnumerable<string> BreakingChangeSuppressions { get; private set; }

        public IEnumerable<string> InvalidInputFiles
        {
            get { return Enumerable.Empty<string>(); }
        }

        public string ServiceEndpoint
        {
            get { throw new NotImplementedException(); }
        }
    }
}
