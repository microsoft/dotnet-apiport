// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ApiPort
{
    internal class CommandLineOptions : ICommandLineOptions
    {
        public static ICommandLineOptions ParseCommandLineOptions(string[] args)
        {
            return new CommandLineOptions();
        }

        public AppCommands Command
        {
            get { return AppCommands.ListTargets; }
        }

        public string OutputFileName
        {
            get { return "ApiPort"; }
        }

        public IEnumerable<string> InvalidInputFiles
        {
            get
            {
                return Enumerable.Empty<string>();
            }
        }

        public string Description
        {
            get; set;
        }

        public IEnumerable<FileInfo> InputAssemblies
        {
            get
            {
                return Enumerable.Empty<FileInfo>();
            }
        }

        public IEnumerable<string> OutputFormats
        {
            get
            {
                return Enumerable.Empty<string>();
            }
        }

        public AnalyzeRequestFlags RequestFlags
        {
            get
            {
                return AnalyzeRequestFlags.None;
            }
        }

        public string ServiceEndpoint
        {
            get
            {
                return "https://portability.cloudapp.net/";
            }
        }

        public IEnumerable<string> Targets
        {
            get
            {
                return Enumerable.Empty<string>();
            }
        }
    }
}
