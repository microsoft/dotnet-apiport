// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            Console.WriteLine();
            Console.WriteLine("Enter a search query to find possible docids.");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  #q               Exit the REPL.");
            Console.WriteLine("  #count=[number]  Display [number] of search results.");
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

                if (string.Equals("#q", query, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                const string hashCount = "#count=";
                if (query.StartsWith(hashCount, StringComparison.OrdinalIgnoreCase))
                {
                    var trimmed = query.Replace(hashCount, "").Trim();
                    int updatedCount;

                    if (Int32.TryParse(trimmed, out updatedCount))
                    {
                        count = updatedCount;
                        WriteColorLine(string.Format("Updated count to {0}", count), ConsoleColor.Yellow);
                    }
                    else
                    {
                        WriteColorLine(string.Format("Invalid number: '{0}'", trimmed), ConsoleColor.Red);
                    }

                    continue;
                }

                var results = await _searcher.SearchAsync(query, count);

                if (results.Any())
                {
                    foreach (var result in results)
                    {
                        WriteColorLine(string.Format("\"{0}\",", result), ConsoleColor.Cyan);
                    }
                }
                else
                {
                    WriteColorLine(string.Format("Did not find anything for search '{0}'", query), ConsoleColor.Yellow);
                }
            }
        }

        static void ConsoleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private static void WriteColorLine(string message, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;

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
