// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

namespace Microsoft.Fx.Portability
{
    public interface IApiPortOptions
    {
        string Description { get; }
        IEnumerable<FileInfo> InputAssemblies { get; }
        bool NoTelemetry { get; }
        IEnumerable<string> Targets { get; }
        string OutputFormat { get; }
    }
}