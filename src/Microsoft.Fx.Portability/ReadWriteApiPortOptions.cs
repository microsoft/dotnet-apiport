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
            OverwriteOutputFile = other.OverwriteOutputFile;
        }

        public virtual IEnumerable<string> BreakingChangeSuppressions { get; set; }

        public virtual string Description { get; set; }

        public virtual IEnumerable<string> IgnoredAssemblyFiles { get; set; }

        public virtual IEnumerable<IAssemblyFile> InputAssemblies { get; set; }

        public virtual IEnumerable<string> InvalidInputFiles { get; set; }

        public virtual string OutputFileName { get; set; }

        public virtual IEnumerable<string> OutputFormats { get; set; }

        public virtual AnalyzeRequestFlags RequestFlags { get; set; }

        public virtual string ServiceEndpoint { get; set; }

        public virtual IEnumerable<string> Targets { get; set; }

        public virtual bool OverwriteOutputFile { get; set; }
    }
}
