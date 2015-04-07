// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Fx.Portability
{
    public interface IApiPortOptions
    {
        string Description { get; }
        IEnumerable<FileInfo> InputAssemblies { get; }
        AnalyzeRequestFlags RequestFlags { get; }
        IEnumerable<string> Targets { get; }
        IEnumerable<string> OutputFormats { get; }
    }
}