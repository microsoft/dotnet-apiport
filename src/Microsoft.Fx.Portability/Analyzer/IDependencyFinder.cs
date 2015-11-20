// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.Fx.Portability.Analyzer
{
    public interface IDependencyFinder
    {
        IDependencyInfo FindDependencies(IEnumerable<IAssemblyFile> files, IProgressReporter progressReport);
    }
}
