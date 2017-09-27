// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.Fx.Portability
{
    public interface IApiPortOptions
    {
        string Description { get; }

        /// <summary>
        /// Key: IAssemblyFile
        /// Value: true if the file was explicitly specified and false otherwise.
        /// </summary>
        ImmutableDictionary<IAssemblyFile, bool> InputAssemblies { get; }
        IEnumerable<string> IgnoredAssemblyFiles { get; }
        AnalyzeRequestFlags RequestFlags { get; }
        IEnumerable<string> Targets { get; }
        IEnumerable<string> OutputFormats { get; }
        IEnumerable<string> BreakingChangeSuppressions { get; }
        string ServiceEndpoint { get; }
        string OutputFileName { get; }
        IEnumerable<string> InvalidInputFiles { get; }
        bool OverwriteOutputFile { get; }
        IEnumerable<string> ReferencedNuGetPackages { get; }
    }
}