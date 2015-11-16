// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;

namespace ApiPort.CommandLine
{
    /// <summary>
    /// Represents some common commands that are used in parsing options
    /// </summary>
    internal class CommonCommands : ReadWriteApiPortOptions, ICommandLineOptions
    {
        public static ICommandLineOptions Exit { get; } = new CommonCommands(AppCommands.Exit);

        public static ICommandLineOptions Help { get; } = new CommonCommands(AppCommands.Help);

        private CommonCommands(AppCommands command)
        {
            Command = command;
        }

        public AppCommands Command { get; }

        public string TargetMapFile { get; }
    }
}
