// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.Resources;
using System.Collections.Generic;

namespace ApiPort.CommandLine
{
    internal class ListTargetsOptions : ServiceEndpointOptions
    {
        public ListTargetsOptions() :
            base("listTargets", LocalizedStrings.CmdListTargets, AppCommands.ListTargets)
        { }
    }

    internal class ListOutputFormatOptions : ServiceEndpointOptions
    {
        public ListOutputFormatOptions() :
            base("listOutputFormats", LocalizedStrings.CmdListOutputFormats, AppCommands.ListOutputFormats)
        { }
    }

    internal class ServiceEndpointOptions : CommandLineOptions
    {
        private readonly AppCommands _command;

        public ServiceEndpointOptions(string name, string helpMessage, AppCommands command)
        {
            Name = name;
            HelpMessage = helpMessage;
            _command = command;
        }

        public override string Name { get; }

        public override string HelpMessage { get; }

        public override ICommandLineOptions Parse(IEnumerable<string> args)
        {
            var mappings = new Dictionary<string, string>
            {
                { "-f", "file" },
                { "-h", "help" },
                { "-?", "help" },
                { "-e", "endpoint" }
            };

            var options = ApiPortConfiguration.Parse<Options>(args, mappings);

            return options.Help ? CommonCommands.Help : new ServiceEndpointCommandLineOptions(options, _command);
        }

        private class Options
        {
            public string Endpoint { get; set; }
            public bool Help { get; set; }
        }

        private class ServiceEndpointCommandLineOptions : ConsoleDefaultApiPortOptions, ICommandLineOptions
        {
            public AppCommands Command { get; }

            public ServiceEndpointCommandLineOptions(Options options, AppCommands command)
            {
                Command = command;
                ServiceEndpoint = options.Endpoint;
            }
        }
    }
}
