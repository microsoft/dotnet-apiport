// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using System.Collections.Generic;

namespace ApiPort
{
    public interface ICommandLineOptions : IApiPortOptions
    {
        string ServiceEndpoint { get; }
        string OutputFileName { get; }
        AppCommands Command { get; }
        IEnumerable<string> InvalidInputFiles { get; }
    }
}
