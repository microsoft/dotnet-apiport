// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;

namespace ApiPort
{
    public interface ICommandLineOptions : IApiPortOptions
    {
        AppCommand Command { get; }

        string TargetMapFile { get; }
    }
}
