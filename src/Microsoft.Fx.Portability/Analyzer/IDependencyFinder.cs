// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;

namespace Microsoft.Fx.Portability.Analyzer
{
    public interface IDependencyFinder
    {
        IDependencyInfo FindDependencies(IEnumerable<FileInfo> inputAssemblyPaths, IProgressReporter progressReport);
        IDependencyInfo FindDependencies(byte[] file, IProgressReporter progressReport);
    }
}
