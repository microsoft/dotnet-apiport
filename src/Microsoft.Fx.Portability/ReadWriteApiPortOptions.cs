// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Fx.Portability
{
    /// <summary>
    /// Provides an implementation of IApiPortOptions that allows for reading and writing properties
    /// </summary>
    public class ReadWriteApiPortOptions : IApiPortOptions
    {
        public ReadWriteApiPortOptions() { }

        public ReadWriteApiPortOptions(IApiPortOptions other)
        {
            BreakingChangeSuppressions = other.BreakingChangeSuppressions;
            Description = other.Description;
            IgnoredAssemblyFiles = other.IgnoredAssemblyFiles;
            InputAssemblies = other.InputAssemblies;
            OutputFormats = other.OutputFormats;
            RequestFlags = other.RequestFlags;
            Targets = other.Targets;
            ServiceEndpoint = other.ServiceEndpoint;
            InvalidInputFiles = other.InvalidInputFiles;
            OutputFileName = other.OutputFileName;
        }

        public IEnumerable<string> BreakingChangeSuppressions { get; set; }

        public string Description { get; set; }

        public IEnumerable<string> IgnoredAssemblyFiles { get; set; }

        public IEnumerable<FileInfo> InputAssemblies { get; set; }

        public IEnumerable<string> InvalidInputFiles { get; set; }

        public string OutputFileName { get; set; }

        public IEnumerable<string> OutputFormats { get; set; }

        public AnalyzeRequestFlags RequestFlags { get; set; }

        public string ServiceEndpoint { get; set; }

        public IEnumerable<string> Targets { get; set; }
    }
}
