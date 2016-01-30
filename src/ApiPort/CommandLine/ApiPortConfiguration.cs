// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Framework.Configuration;
using Microsoft.Framework.OptionsModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ApiPort.CommandLine
{
    internal static class ApiPortConfiguration
    {
        internal static TOptions Parse<TOptions>(IEnumerable<string> args, IDictionary<string, string> switchMapping)
            where TOptions : class, new()
        {
            var booleanSwitches = GetConfigMapCounts(typeof(TOptions), t => t == typeof(bool), switchMapping);
            var arrays = GetConfigMapCounts(typeof(TOptions), t => (t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)) || t.IsArray, switchMapping);
            var updatedArgs = TransformArguments(args, arrays, booleanSwitches).ToArray();

            var cmd = new ConfigurationBuilder(Directory.GetCurrentDirectory())
                 .AddCommandLine(updatedArgs, switchMapping)
                 .Build();

            var manager = new ConfigureFromConfigurationOptions<TOptions>(cmd);

            var options = new TOptions();
            manager.Action(options);

            return options;
        }

        private static IEnumerable<ConfigMapCount> GetConfigMapCounts(Type type, Func<Type, bool> comparer, IDictionary<string, string> switchMapping)
        {
            var arrayNames = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(b => comparer(b.PropertyType))
                .Select(prop => prop.Name)
                .ToList();

            var mappings = switchMapping.GroupBy(map => map.Value);

            foreach (var name in arrayNames)
            {
                var mapping = mappings.FirstOrDefault(map => string.Equals(map.Key, name, StringComparison.OrdinalIgnoreCase))
                    ?.Select(map => map.Key) ?? Enumerable.Empty<string>();
                var key = $"--{name}";

                yield return new ConfigMapCount
                {
                    Tags = new HashSet<string>(mapping.Concat(new[] { key }), StringComparer.OrdinalIgnoreCase),
                    ExpectedTag = key,
                    Count = 0
                };
            }
        }

        private static IEnumerable<string> TransformArguments(IEnumerable<string> args, IEnumerable<ConfigMapCount> arrays, IEnumerable<ConfigMapCount> booleanSwitches)
        {
            var arrayTransformed = arrays.Aggregate(args, (acc, array) => acc.Select(arg => ArrayTransform(arg, array)));

            return booleanSwitches.Aggregate(arrayTransformed, (acc, booleanSwitch) => acc.SelectMany(arg => BooleanTransform(arg, booleanSwitch)));
        }

        private static IEnumerable<string> BooleanTransform(string arg, ConfigMapCount booleanSwitch)
        {
            foreach (var tag in booleanSwitch.Tags)
            {
                if (string.Equals(arg, tag, StringComparison.OrdinalIgnoreCase))
                {
                    return new[] { booleanSwitch.ExpectedTag, "True" };
                }
            }

            return new[] { arg };
        }

        private static string ArrayTransform(string arg, ConfigMapCount array)
        {
            foreach (var tag in array.Tags)
            {
                var newKey = string.Format(CultureInfo.InvariantCulture, "{0}:{1}", array.ExpectedTag, array.Count++);
                var tagFormatted = string.Format(CultureInfo.InvariantCulture, "{0}=", tag);

                if (arg.StartsWith(tagFormatted, StringComparison.OrdinalIgnoreCase))
                {
                    return arg.Replace(tagFormatted, string.Format(CultureInfo.InvariantCulture, "{0}=", newKey));
                }
                else if (string.Equals(arg, tag, StringComparison.OrdinalIgnoreCase))
                {
                    return newKey;
                }
            }

            return arg;
        }

        private class ConfigMapCount
        {
            public int Count { get; set; }

            public string ExpectedTag { get; set; }

            public ICollection<string> Tags { get; set; }
        }
    }
}
