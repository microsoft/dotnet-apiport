// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability
{
    public interface ITargetNameParser
    {
        /// <summary>
        /// Maps the list of targets specified as strings to a list of supported target names.
        /// </summary>
        IEnumerable<FrameworkName> MapTargetsToExplicitVersions(IEnumerable<string> targets);

        IEnumerable<FrameworkName> DefaultTargets { get; }
    }
}
