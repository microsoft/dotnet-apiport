// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace PortabilityService.AnalysisService
{
    public class CloudTargetNameParser : ITargetNameParser
    {
        private readonly IApiCatalogLookup _catalog;
        private readonly Dictionary<string, FrameworkName> _defaultTargetVersions;

        public CloudTargetNameParser(IApiCatalogLookup catalog, IServiceSettings settings)
        {
            _catalog = catalog;
            _defaultTargetVersions = settings.DefaultVersions.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(tfm => new FrameworkName(tfm))
                .ToDictionary(name => name.Identifier, name => name);

            DefaultTargets = GetDefaultTargets(settings.DefaultTargets).ToList();
        }

        public IEnumerable<FrameworkName> DefaultTargets { get; private set; }

        /// <summary>
        /// Maps the list of targets specified as strings to a list of supported target names.
        /// </summary>
        /// <remarks>
        /// If no targets are specified, return the list of all public targets with their latest version.
        ///  - From the list of public targets, filter out the ones that are NOT in the configuration setting described by 'DefaultTargets'
        ///    Note: This allows us to support Mono targets without having them automatically show up in the default target list.
        /// If targets are specified, parse them like this:
        ///  - If just the target name is specified and not the version, use the configured default or latest version for that target
        ///  - if both target name and version are specified, use them
        /// If an unknown target is found, throw an UnknownTargetException.
        /// </remarks>
        public IEnumerable<FrameworkName> MapTargetsToExplicitVersions(IEnumerable<string> targets)
        {
            // If no targets were specified, return the default platfoms.
            if (targets == null || !targets.Any())
            {
                return DefaultTargets;
            }

            // Compute a set of all the targets that are supported by the service.
            var allTargets = new HashSet<string>(_catalog.GetAllTargets().Select(plat => plat.DisplayName.FullName), StringComparer.OrdinalIgnoreCase);

            // Parse the targets and make sure a version was specified.
            var userSpecifiedTargets = ParseTargets(targets);

            // Determine if any of the user specified targets are not known to the catalog.
            foreach (var target in userSpecifiedTargets)
            {
                if (!allTargets.Contains(target.FullName))
                {
                    throw new UnknownTargetException(target.FullName);
                }
            }

            return userSpecifiedTargets;
        }

        /// <summary>
        /// Returns list of all public targets with the configured default or latest version.
        /// </summary>
        private IEnumerable<FrameworkName> GetDefaultTargets(string defaultTargets)
        {
            // The default targets are kept in a ';' separated string.
            string[] defaultTargetsSplit = defaultTargets.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            // Create a hashset of all the targets specified in the configuration setting.
            HashSet<FrameworkName> parsedDefaultTargets = new HashSet<FrameworkName>(ParseTargets(defaultTargetsSplit, skipNonExistent: true));

            //return all the public targets (their latest versions) as long as they also show up the default targets set.
            return _catalog.GetPublicTargets()
                .Select(plat => plat.Identifier)
                .Distinct()
                .Select(GetDefaultOrLatestVersion)
                .Where(plat => plat != null && parsedDefaultTargets.Contains(plat));
        }

        /// <summary>
        /// Parse a string containing target names into FrameworkNames.
        ///
        /// Try the following in order:
        ///   1. Check if the target specified uses the 'simple' name (i.e. Windows, .NET Framework) then get the default or latest version of it
        ///   2. Try to parse it as a target name. If the target was not a valid FrameworkName, an exception will be thrown
        /// <exception cref="UnknownTargetException">Thrown when a target is unknown</exception>
        /// </summary>
        /// <param name="skipNonExistent">true to suppress <see cref="UnknownTargetException"/>
        /// when a target is not found. false, will not throw and skip that target instead.</param>
        private ICollection<FrameworkName> ParseTargets(IEnumerable<string> targets, bool skipNonExistent)
        {
            var list = new List<FrameworkName>();

            foreach (var target in targets.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    // this expects the catalog to return null if target is a full TFM, e.g. ".NET Core,Version=1.1"
                    list.Add(GetDefaultOrLatestVersion(target) ?? new FrameworkName(target));
                }
                catch (ArgumentException) // thrown by FrameworkName constructor when target is invalid
                {
                    if (!skipNonExistent)
                    {
                        throw new UnknownTargetException(target);
                    }
                }
            }

            return list;
        }

        private ICollection<FrameworkName> ParseTargets(IEnumerable<string> targets)
        {
            return ParseTargets(targets, skipNonExistent: false);
        }

        private FrameworkName GetDefaultOrLatestVersion(string targetIdentifier)
        {
            FrameworkName name;
            if (_defaultTargetVersions.TryGetValue(targetIdentifier, out name))
            {
                return name;
            }
            else
            {
                return _catalog.GetLatestVersion(targetIdentifier);
            }
        }
    }
}
