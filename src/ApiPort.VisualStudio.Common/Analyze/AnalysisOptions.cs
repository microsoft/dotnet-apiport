// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace ApiPortVS
{
    public class AnalysisOptions : IApiPortOptions
    {
        public AnalysisOptions(string description, IEnumerable<string> inputAssemblies, IEnumerable<string> targets, IEnumerable<string> formats, bool discardMetadata, string outputFileName)
        {
            Description = description;
            InputAssemblies = inputAssemblies.Select(target => new AssemblyFile(target)).ToList();
            Targets = targets.ToList();
            OutputFormats = formats.ToList();
            OutputFileName = outputFileName;

            RequestFlags |= AnalyzeRequestFlags.ShowNonPortableApis;

            if (discardMetadata)
            {
                RequestFlags |= AnalyzeRequestFlags.NoTelemetry;
            }
        }

        public string Description { get; }

        public IEnumerable<IAssemblyFile> InputAssemblies { get; }

        public AnalyzeRequestFlags RequestFlags { get; }

        public IEnumerable<string> Targets { get; }

        public IEnumerable<string> OutputFormats { get; }

        public string OutputFileName { get; }

        public IEnumerable<string> IgnoredAssemblyFiles { get; } = Enumerable.Empty<string>();

        public IEnumerable<string> BreakingChangeSuppressions { get; } = Enumerable.Empty<string>();

        public IEnumerable<string> InvalidInputFiles { get; } = Enumerable.Empty<string>();

        public string ServiceEndpoint { get; } = string.Empty;

        public bool OverwriteOutputFile { get; }
    }
}
