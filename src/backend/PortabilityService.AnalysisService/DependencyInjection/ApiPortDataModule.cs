// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Autofac;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PortabilityService.AnalysisService
{
    public class ApiPortDataModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(LoadApiPortData).SingleInstance();
            builder.RegisterType<GitCatalogRecommendations>().As<IApiRecommendations>().SingleInstance();
        }

        private ApiPortData LoadApiPortData(IComponentContext context)
        {
            var settings = context.Resolve<IServiceSettings>();

            var breakingChanges = Directory.EnumerateFiles(settings.BreakingChangesPath, "*.md")
                .Where(path =>
                {
                    var filename = Path.GetFileNameWithoutExtension(path);
                    return !(filename.StartsWith('!') || filename.Equals("README", StringComparison.Ordinal));
                })
                .Select(File.ReadAllBytes)
                .SelectMany(bytes =>
                {
                    using (var stream = new MemoryStream(bytes))
                    {
                        return BreakingChangeParser.FromMarkdown(stream);
                    }
                });

            var recommendedChanges = Directory.EnumerateFiles(settings.RecommendedChangesPath, "*.md", SearchOption.AllDirectories)
                .Select(File.ReadAllText)
                .Select(RecommendedChange.ParseFromMarkdown)
                .Where(change => change != null);

            return new ApiPortData
            {
                BreakingChangesDictionary = ToDictionary(breakingChanges, change => change.ApplicableApis),
                RecommendedChanges = ToDictionary(recommendedChanges, change => change.AffectedApis, change => change.RecommendedAction)
            };
        }

        private IDictionary<string, Value> ToDictionary<Type, Value>(IEnumerable<Type> items, Func<Type, IEnumerable<string>> valueToKeysFactory, Func<Type, Value> dictionaryValueFactory)
        {
            var dictionary = new Dictionary<string, Value>();

            foreach (var item in items)
            {
                foreach (var key in valueToKeysFactory(item))
                {
                    var value = dictionaryValueFactory(item);

                    if (!dictionary.ContainsKey(key))
                    {
                        dictionary.Add(key, value);
                    }
                }
            }

            return dictionary;
        }

        private IDictionary<string, ICollection<T>> ToDictionary<T>(IEnumerable<T> items, Func<T, IEnumerable<string>> valueToKeysFactory)
        {
            var dictionary = new Dictionary<string, ICollection<T>>();

            foreach (var item in items)
            {
                var keys = valueToKeysFactory(item) ?? Enumerable.Empty<string>();

                foreach (var key in keys)
                {
                    if (dictionary.ContainsKey(key))
                    {
                        dictionary[key].Add(item);
                    }
                    else
                    {
                        dictionary.Add(key, new List<T> { item });
                    }
                }
            }

            return dictionary;
        }
    }
}
