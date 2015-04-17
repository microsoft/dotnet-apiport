// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiPort
{
    internal class CommandLineOptions
    {
        private static IDictionary<string, Func<string, CommandLineOptionSet>> s_possibleCommands = new Dictionary<string, Func<string, CommandLineOptionSet>>(StringComparer.OrdinalIgnoreCase)
        {
            {"analyze", name => new AnalyzeOptionSet(name) },
            {"listOutputFormats", name => new  ServiceEndpointOptionSet(name, AppCommands.ListOutputFormats) },
            {"listTargets", name => new  ServiceEndpointOptionSet(name, AppCommands.ListTargets) },
        };

        public static ICommandLineOptions ParseCommandLineOptions(string[] args)
        {
            if (args.Length == 0)
            {
                return ShowHelp();
            }

            var inputCommand = args[0];

            try
            {
                var option = s_possibleCommands.Single(c => c.Key.StartsWith(inputCommand, StringComparison.OrdinalIgnoreCase));
                var parser = option.Value(option.Key);

                return parser.Parse(args.Skip(1));
            }
            catch (InvalidOperationException)
            {
                return ShowHelp(args[0]);
            }
        }

        private static ICommandLineOptions ShowHelp(string suppliedCommand = null)
        {
            if (!string.IsNullOrEmpty(suppliedCommand))
            {
                Program.WriteColorLine($"Unknown command: {suppliedCommand}", ConsoleColor.Yellow);
                Console.WriteLine();
            }

            Console.WriteLine("Available Commands:");

            foreach (var command in s_possibleCommands)
            {
                Console.WriteLine($"- {command.Key}");
            }

            return CommandLineOptionSet.ExitCommandLineOption;
        }
    }
}
