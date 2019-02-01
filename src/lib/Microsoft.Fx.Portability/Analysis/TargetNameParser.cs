// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.Analysis
{
    public class TargetNameParser : ITargetNameParser
    {
        private readonly IApiCatalogLookup _catalog;

        public TargetNameParser(IApiCatalogLookup catalog, string defaultTargets)
        {
            _catalog = catalog;
            DefaultTargets = GetDefaultTargets(defaultTargets).ToList();
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
        ///  - If just the target name is specified and not the version, use the latest version for that target
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

            // Determine if any of the user specified targets are not known to the system.
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
        /// Returns list of all public targets with their latest version.
        /// </summary>
        private IEnumerable<FrameworkName> GetDefaultTargets(string defaultTargets)
        {
            // The default targets are kept in a ';' separated string.
            // Trimming target names because "; .NET Core" is interpreted as
            // " .NET Core", which does not exist, but ".NET Core" does.
            var defaultTargetsSplit = defaultTargets?.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                ?? Array.Empty<string>();

            // Create a hashset of all the targets specified in the configuration setting.
            var parsedDefaultTargets = ParseTargets(defaultTargetsSplit, skipNonExistent: true);

            // Verify that any default targets returned are part of the public interface.
            var publicTargets = new HashSet<FrameworkName>(_catalog.GetPublicTargets());

            return parsedDefaultTargets.Where(x => publicTargets.Contains(x));
        }

        /// <summary>
        /// Parse a string containing target names into FrameworkNames.
        ///
        /// Try the following in order:
        ///   1. Check if the target specified uses a FrameworkName (ie. .NET Core, Version=1.0)
        ///   2. Check if the target specified uses the 'simple' name (i.e. Windows, .NET Framework)
        ///      then get the latest version for it.
        /// </summary>
        /// <param name="skipNonExistent">true to suppress <see cref="UnknownTargetException"/>
        /// when a target is not found. false, will not throw and skip that target instead.</param>
        /// <exception cref="UnknownTargetException">Thrown when a target is unknown</exception>
        private ICollection<FrameworkName> ParseTargets(IEnumerable<string> targets, bool skipNonExistent)
        {
            bool TryParseFrameworkName(string targetName, out FrameworkName frameworkName)
            {
                try
                {
                    frameworkName = new FrameworkName(targetName);
                    return true;
                }
                catch (ArgumentException)
                {
                    // Catch ArgumentException because FrameworkName does not have a TryParse method
                    frameworkName = null;
                    return false;
                }
            }

            var list = new List<FrameworkName>();

            foreach (var target in targets.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var framework = TryParseFrameworkName(target, out var name)
                    ? name
                    : _catalog.GetLatestVersion(target);

                if (framework != null)
                {
                    list.Add(framework);
                }
                else if (!skipNonExistent)
                {
                    throw new UnknownTargetException(target);
                }
                else
                {
                    // Trace.TraceWarning("Could not resolve target: {0}", target);
                }
            }

            return list;
        }

        private ICollection<FrameworkName> ParseTargets(IEnumerable<string> targets)
        {
            return ParseTargets(targets, skipNonExistent: false);
        }
    }
}
