// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.Resources;
using Microsoft.Fx.Portability;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.Fx.Portability.Utils.FormattableStringHelper;

namespace ApiPort
{
    public class DocIdSearchRepl
    {
        private readonly ISearcher<string> _searcher;
        public DocIdSearchRepl(ISearcher<string> searcher)
        {
            _searcher = searcher;
        }

        public async Task DocIdSearchAsync()
        {
            var countOption = ToCurrentCulture($"{LocalizedStrings.ReplOptionCount}[{LocalizedStrings.Number}]");
            var optionColumnWidth = Math.Max(countOption.Length, LocalizedStrings.ReplOptionExit.Length);

            Console.WriteLine();
            Console.WriteLine(LocalizedStrings.ReplEnterQuery);
            Console.WriteLine();
            Console.WriteLine(LocalizedStrings.ReplOptionsHeader);
            Console.WriteLine(ToCurrentCulture($"  {LocalizedStrings.ReplOptionExit.PadRight(optionColumnWidth)}\t{LocalizedStrings.ReplOptionExit_Text}"));
            Console.WriteLine(ToCurrentCulture($"  {countOption.PadRight(optionColumnWidth)}\t{LocalizedStrings.ReplOptionCount_Text}"));
            Console.WriteLine();

            Console.CancelKeyPress += ConsoleCancelKeyPress;

            try
            {
                await ReplLoopAsync();
            }
            finally
            {
                Console.CancelKeyPress -= ConsoleCancelKeyPress;
            }
        }

        private async Task ReplLoopAsync()
        {
            var count = 10;

            while (true)
            {
                Console.Write("> ");
                var rawQuery = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(rawQuery))
                {
                    Console.WriteLine();
                    continue;
                }

                var query = rawQuery.Trim();

                if (string.Equals(LocalizedStrings.ReplOptionExit, query, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (query.StartsWith(LocalizedStrings.ReplOptionCount, StringComparison.OrdinalIgnoreCase))
                {
                    var trimmed = query
#if NETCOREAPP2_1
                        .Replace(LocalizedStrings.ReplOptionCount, string.Empty, StringComparison.OrdinalIgnoreCase)
#else
                        .Replace(LocalizedStrings.ReplOptionCount, string.Empty)
#endif
                        .Trim();

                    if (Int32.TryParse(trimmed, out int updatedCount))
                    {
                        count = updatedCount;
                        WriteColorLine(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.ReplUpdatedCount, count), ConsoleColor.Yellow);
                    }
                    else
                    {
                        WriteColorLine(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.ReplInvalidNumber, trimmed), ConsoleColor.Red);
                    }

                    continue;
                }

                var results = await _searcher.SearchAsync(query, count);

                if (results.Any())
                {
                    foreach (var result in results)
                    {
                        WriteColorLine(ToCurrentCulture($"\"{result}\","), ConsoleColor.Cyan);
                    }
                }
                else
                {
                    WriteColorLine(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.ReplNoResultsFound, query), ConsoleColor.Yellow);
                }
            }
        }

        private static void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private static void WriteColorLine(string message, ConsoleColor color)
        {
            var previousColor =
#if LINUX
                // Console.get_ForegroundColor is unsupported by the Linux PAL
                ConsoleColor.White;
#else // LINUX
                Console.ForegroundColor;
#endif // LINUX

            try
            {
                Console.ForegroundColor = color;
                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = previousColor;
            }
        }
    }
}
