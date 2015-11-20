// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace Microsoft.Fx.Portability
{
    public interface IAssemblyFile
    {
        Stream OpenRead();

        string Name { get; }

        string Version { get; }

        bool Exists { get; }
    }
}
