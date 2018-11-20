// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using Microsoft.Fx.Portability.Offline.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

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
                var allowedCategories = GetLocalAllowedCategories();
                foreach (var file in Directory.GetFiles(Path.Combine(GetCurrentDirectoryName(), "BreakingChanges"), "*", SearchOption.AllDirectories))
                {
                    using (var fs = File.Open(file, FileMode.Open, FileAccess.Read))
                    {
                        breakingChanges.AddRange(ParseBreakingChange(fs, Path.GetExtension(file), allowedCategories));
                    }
                }

                return breakingChanges;
            }

            // If no BreakingChanges folder exists, then we'll fall back to loading embedded breaking changes
            else
            {
                var breakingChanges = new List<BreakingChange>();

                // Breaking changes will be serialized as either md or (less commonly now) json files
                foreach (var file in typeof(Data).GetTypeInfo().Assembly.GetManifestResourceNames().Where(s => s.EndsWith(".md", StringComparison.OrdinalIgnoreCase) || s.EndsWith(".json", StringComparison.OrdinalIgnoreCase)))
                {
                    using (var stream = typeof(Data).GetTypeInfo().Assembly.GetManifestResourceStream(file))
                    {
                        var fileBreakingChanges = ParseBreakingChange(stream, Path.GetExtension(file), null);

                        if (fileBreakingChanges == null)
                        {
                            // Trace.WriteLine("No data was found in '" + file + "'");
                        }
                        else
                        {
                            breakingChanges.AddRange(fileBreakingChanges);
                        }
                    }
                }

                return breakingChanges;
            }
        }

        private static IEnumerable<string> GetLocalAllowedCategories()
        {
            // Check to see if a file defining valid categories exists
            var categoriesPath = Path.Combine(GetCurrentDirectoryName(), "BreakingChanges", "BreakingChangeCategories.json");
            if (File.Exists(categoriesPath))
            {
                using (var categoriesFile = File.Open(categoriesPath, FileMode.Open, FileAccess.Read))
                {
                    return categoriesFile.Deserialize<string[]>();
                }
            }

            return null;
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
                var stream = typeof(Data).GetTypeInfo().Assembly.GetManifestResourceStream("Microsoft.Fx.Portability.Offline.data." + path);

                if (stream == null)
                {
                    throw new PortabilityAnalyzerException(string.Format(CultureInfo.CurrentUICulture, LocalizedStrings.DataFileNotFound, path));
                }

                return stream;
            }
        }

        private static string GetCurrentDirectoryName()
        {
#if NET45
            return Path.GetDirectoryName(typeof(Data).Assembly.Location);
#else
            return Path.GetDirectoryName(AppContext.BaseDirectory);
#endif
        }

        private static IEnumerable<BreakingChange> ParseBreakingChange(Stream stream, string extension, IEnumerable<string> allowedCategories)
        {
            if (string.Equals(".md", extension, StringComparison.OrdinalIgnoreCase))
            {
                return BreakingChangeParser.FromMarkdown(stream, allowedCategories);
            }

            if (string.Equals(".json", extension, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    return stream.Deserialize<IEnumerable<BreakingChange>>();
                }
                catch (Exception)
                {
                    // An invalid json file will throw an exception when deserialized. Simply ignore such files.
                }
            }

            return Enumerable.Empty<BreakingChange>();
        }
    }
}
