// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Reporting.ObjectModel;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace ApiPortVS.Contracts
{
    public interface ISourceMappedItem
    {
        string Assembly { get; }

        MissingInfo Item { get; }

        IEnumerable<FrameworkName> UnsupportedTargets { get; }

        int Column { get; }

        int Line { get; }

        string Path { get; }
    }
}