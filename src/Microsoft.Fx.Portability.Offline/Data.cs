// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Microsoft.Fx.Portability
{
    internal static class Data
    {
        public static DotNetCatalog LoadCatalog()
        {
            using (var stream = OpenFileOrResource("catalog.bin"))
            {
                return stream.DecompressToObject<DotNetCatalog>();
            }
        }

        public static IEnumerable<BreakingChange> LoadBreakingChanges()
        {
            // Prefer a local 'BreakingChanges' directory to embedded breaking changes
            if (Directory.Exists(Path.Combine(GetCurrentDirectoryName(), "BreakingChanges")))
            {
                var breakingChanges = new List<BreakingChange>();
                foreach (var file in Directory.GetFiles(Path.Combine(GetCurrentDirectoryName(), "BreakingChanges"), "*", SearchOption.AllDirectories))
                {
                    using (var fs = File.Open(file, FileMode.Open, FileAccess.Read))
                    {
                        breakingChanges.AddRange(ParseBreakingChange(fs, Path.GetExtension(file)));
                    }
                }

                return breakingChanges;
            }
            // If no BreakingChanges folder exists, then we'll fall back to the old model of looking for
            // BreakingChanges.json either next to the assembly or, if not there, embedded as a resource
            else
            {
                using (var stream = OpenFileOrResource("BreakingChanges.json"))
                {
                    var breakingChanges = stream.Deserialize<IEnumerable<BreakingChange>>();

                    if (breakingChanges == null)
                    {
                        Trace.WriteLine("No data was found in 'BreakingChanges.json'");
                        return Enumerable.Empty<BreakingChange>();
                    }

                    return breakingChanges;
                }
            }
        }

        private static Stream OpenFileOrResource(string path)
        {
            var file = Path.Combine(GetCurrentDirectoryName(), path);

            if (File.Exists(file))
            {
                return File.OpenRead(file);
            }
            else
            {
                var stream = typeof(Data).Assembly.GetManifestResourceStream("Microsoft.Fx.Portability.data." + path);

                if (stream == null)
                {
                    throw new FileNotFoundException("Could not find data file next to or embedded in assembly.", path);
                }

                return stream;
            }
        }

        private static string GetCurrentDirectoryName()
        {
            return Path.GetDirectoryName(typeof(Data).Assembly.Location);
        }

        private static IEnumerable<BreakingChange> ParseBreakingChange(Stream stream, string extension)
        {
            if (string.Equals(".md", extension, StringComparison.OrdinalIgnoreCase))
            {
                return BreakingChangeParser.FromMarkdown(stream);
            }
            if (string.Equals(".json", extension, StringComparison.OrdinalIgnoreCase))
            {
                return stream.Deserialize<IEnumerable<BreakingChange>>();
            }
            else
            {
                return Enumerable.Empty<BreakingChange>();
            }
        }
    }
}
