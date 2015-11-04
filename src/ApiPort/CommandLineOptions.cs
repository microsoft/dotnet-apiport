// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ApiPort
{
    internal abstract class CommandLineOptions
    {
        public abstract string Name { get; }

        public abstract string HelpMessage { get; }

        public abstract ICommandLineOptions Parse(IEnumerable<string> args);

        private static IDictionary<string, CommandLineOptions> s_possibleCommands =
            new CommandLineOptions[] { new AnalyzeOptions(), new ListTargetsOptions(), new ListOutputFormatOptions(), new DocIdSearchOptions() }
            .ToDictionary(o => o.Name, o => o, StringComparer.OrdinalIgnoreCase);

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
                var output = option.Value.Parse(args.Skip(1));

                if (output.Command == AppCommands.Help)
                {
                    ShowHelp(inputCommand);

                    return CommonCommands.Exit;
                }

                return output;
            }
            catch (FormatException)
            {
                return ShowHelp(inputCommand, true);
            }
            catch (InvalidOperationException)
            {
                return ShowHelp(inputCommand, true);
            }
        }

        private static ICommandLineOptions ShowHelp(string suppliedCommand = null, bool error = false)
        {
            var command = s_possibleCommands.Select(c => c.Value).FirstOrDefault(c => string.Equals(c.Name, suppliedCommand, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(suppliedCommand) && command == null)
            {
                Console.WriteLine();
                Program.WriteColorLine($"Unknown command: {suppliedCommand}", ConsoleColor.Red);
            }
            else if (error)
            {
                Console.WriteLine();
                // TODO: Get invalid parameter (Microsoft.Framework.Configuration currently does not surface this)
                Program.WriteColorLine($"Invalid parameter passed to {suppliedCommand}", ConsoleColor.Red);
            }

            var codebase = new Uri(typeof(CommandLineOptions).GetTypeInfo().Assembly.CodeBase);
            var path = Path.GetFileName(codebase.AbsolutePath);

            var displayCommands = command == null ? s_possibleCommands.Select(c => c.Value) : new[] { command };
            foreach (var displayCommand in displayCommands)
            {
                Console.WriteLine();
                Console.WriteLine(new string('=', Math.Min(Console.WindowWidth, 100)));
                Program.WriteColorLine($"{path} {displayCommand.Name} [options]", ConsoleColor.Yellow);
                Console.WriteLine();
                Console.WriteLine(displayCommand.HelpMessage);
            }

            return CommonCommands.Exit;
        }
    }
}
