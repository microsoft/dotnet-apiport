// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ApiPort.CommandLine
{
    /// <summary>
    /// Provides an IApiPortOptions instance that is seeded with the console defaults. Also, only sets
    /// values if they are not null (so nothing gets overwritten)
    /// </summary>
    internal class ConsoleDefaultApiPortOptions : ReadWriteApiPortOptions
    {
        public ConsoleDefaultApiPortOptions()
        {
            ServiceEndpoint = "http://portability.cloudapp.net";
            Description = string.Empty;
            OutputFileName = "ApiPortAnalysis";
            RequestFlags = AnalyzeRequestFlags.None;
            Targets = Enumerable.Empty<string>();
        }

        public override string ServiceEndpoint
        {
            get { return base.ServiceEndpoint; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    base.ServiceEndpoint = value;
                }
            }
        }

        public override IEnumerable<string> BreakingChangeSuppressions
        {
            get { return base.BreakingChangeSuppressions; }
            set
            {
                if (value != null)
                {
                    base.BreakingChangeSuppressions = value;
                }
            }
        }

        public override string Description
        {
            get { return base.Description; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    base.Description = value;
                }
            }
        }

        public override IEnumerable<string> IgnoredAssemblyFiles
        {
            get { return base.IgnoredAssemblyFiles; }
            set
            {
                if (value != null)
                {
                    base.IgnoredAssemblyFiles = value;
                }
            }
        }

        public override IEnumerable<FileInfo> InputAssemblies
        {
            get { return base.InputAssemblies; }
            set
            {
                if (value != null)
                {
                    base.InputAssemblies = value;
                }
            }
        }

        public override IEnumerable<string> InvalidInputFiles
        {
            get { return base.InvalidInputFiles; }
            set
            {
                if (value != null)
                {
                    base.InvalidInputFiles = value;
                }
            }
        }

        public override string OutputFileName
        {
            get { return base.OutputFileName; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    base.OutputFileName = value;
                }
            }
        }

        public override IEnumerable<string> OutputFormats
        {
            get { return base.OutputFormats; }
            set
            {
                if (value != null)
                {
                    base.OutputFormats = value;
                }
            }
        }

        public override IEnumerable<string> Targets
        {
            get { return base.Targets; }
            set
            {
                if (value != null)
                {
                    base.Targets = value;
                }
            }
        }
    }
}
