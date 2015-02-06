// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Fx.Csv
{
    public static class CsvFile
    {
        public static CsvTextReader Read(string fileName)
        {
            return Read(fileName, CsvSettings.Default);
        }

        public static CsvTextReader Read(string fileName, CsvSettings settings)
        {
            var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var streamReader = new StreamReader(fileStream, settings.Encoding);
            return new CsvTextReader(streamReader, settings);
        }

        public static CsvTextWriter Create(string fileName)
        {
            return Create(fileName, CsvSettings.Default);
        }

        public static CsvTextWriter Create(string fileName, CsvSettings settings)
        {
            var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            var streamWriter = new StreamWriter(fileStream, settings.Encoding);
            return new CsvTextWriter(streamWriter, settings);
        }

        public static CsvTextWriter Append(string fileName)
        {
            return Append(fileName, CsvSettings.Default);
        }

        public static CsvTextWriter Append(string fileName, CsvSettings settings)
        {
            var fileStream = new FileStream(fileName, FileMode.Append, FileAccess.Write, FileShare.None);
            var streamWriter = new StreamWriter(fileStream, settings.Encoding);
            return new CsvTextWriter(streamWriter, settings);
        }

        public static IEnumerable<IEnumerable<string>> ReadLines(string fileName)
        {
            return ReadLines(fileName, CsvSettings.Default);
        }

        public static IEnumerable<IEnumerable<string>> ReadLines(string fileName, CsvSettings settings)
        {
            using (var csvReader = Read(fileName, settings))
            {
                var line = csvReader.Read();
                while (line != null)
                {
                    yield return line;
                    line = csvReader.Read();
                }
            }
        }

        public static void WriteLines(string fileName, IEnumerable<IEnumerable<string>> lines)
        {
            WriteLines(fileName, lines, CsvSettings.Default);
        }

        public static void WriteLines(string fileName, IEnumerable<string> header, IEnumerable<IEnumerable<string>> lines)
        {
            WriteLines(fileName, header, lines, CsvSettings.Default);
        }

        public static void WriteLines(string fileName, IEnumerable<string> header, IEnumerable<IEnumerable<string>> lines, CsvSettings settings)
        {
            var headerLine = new[] { header };
            var allLines = headerLine.Concat(lines);
            WriteLines(fileName, allLines, settings);
        }

        public static void WriteLines(string fileName, IEnumerable<IEnumerable<string>> lines, CsvSettings settings)
        {
            using (var csvWriter = Create(fileName, settings))
            {
                foreach (var line in lines)
                    csvWriter.WriteLine(line);
            }
        }

        public static void AppendLines(string fileName, IEnumerable<IEnumerable<string>> lines)
        {
            AppendLines(fileName, lines, CsvSettings.Default);
        }

        public static void AppendLines(string fileName, IEnumerable<string> header, IEnumerable<IEnumerable<string>> lines)
        {
            AppendLines(fileName, header, lines, CsvSettings.Default);
        }

        public static void AppendLines(string fileName, IEnumerable<string> header, IEnumerable<IEnumerable<string>> lines, CsvSettings settings)
        {
            var headerLine = new[] { header };
            var allLines = headerLine.Concat(lines);
            AppendLines(fileName, allLines, settings);
        }

        public static void AppendLines(string fileName, IEnumerable<IEnumerable<string>> lines, CsvSettings settings)
        {
            using (var csvWriter = Append(fileName, settings))
            {
                foreach (var line in lines)
                    csvWriter.WriteLine(line);
            }
        }
    }
}
