// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace ApiPort.CommandLine
{
    internal class ServiceEndpointOptionSet : CommandLineOptionSet
    {
        public ServiceEndpointOptionSet(string name, AppCommands appCommand, string summaryMessage)
            : base(name, summaryMessage)
        {
            Command = appCommand;

#if DEBUG
            Add("e|endpoint=", "Service endpoint", e => ServiceEndpoint = e);
#endif
        }

        public override AppCommands Command { get; }
    }
}
