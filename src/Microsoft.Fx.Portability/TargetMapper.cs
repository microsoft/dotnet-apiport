// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Fx.Portability.Resources;

#if FEATURE_XML_SCHEMA
using System.Xml.Schema;
#endif

namespace Microsoft.Fx.Portability
{
    /// <summary>
    /// Provides a mapping from one target to another via aliases defined in a config xml file.
    /// </summary>
    public class TargetMapper : ITargetMapper
    {
        private readonly IDictionary<string, ICollection<string>> _map = new Dictionary<string, ICollection<string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Loads a config file into the target mapper
        /// </summary>
        /// <param name="path">Path to XML config.  If null, a config file is expected next to the assembly with the name "TargetMap.xml"</param>
        public bool LoadFromConfig(string path = null)
        {
            var configPath = GetPossibleFileLocations(path)
                .Where(p => IsValidPath(p) && File.Exists(p))
                .FirstOrDefault();

            if (configPath == null)
            {
                return false;
            }

            using (var fs = File.OpenRead(configPath))
            {
                Load(fs, configPath);

                return true;
            }
        }

        private bool IsValidPath(string path)
        {
            try
            {
                // This will throw if a path is invalid
                return path != null && Path.GetFullPath(path) != null;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private IEnumerable<string> GetPossibleFileLocations(string path)
        {
            yield return path;

#if FEATURE_ASSEMBLY_LOCATION
            const string DefaultFileName = "TargetMap.xml";

            var location = typeof(TargetMapper).Assembly.Location;

            if (IsValidPath(location))
            {
                yield return Path.Combine(Path.GetDirectoryName(location), DefaultFileName);
            }

            yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), DefaultFileName);
#endif
        }

        public void Load(Stream stream)
        {
            Load(stream, null);
        }

        private void Load(Stream stream, string path)
        {
            try
            {
                var doc = XDocument.Load(XmlReader.Create(stream));

#if FEATURE_XML_SCHEMA
                // Validate against schema
                var schemas = new XmlSchemaSet();
                schemas.Add(null, XmlReader.Create(typeof(TargetMapper).Assembly.GetManifestResourceStream("Microsoft.Fx.Portability.Targets.xsd")));
                doc.Validate(schemas, (s, e) => { throw new TargetMapperException(e.Message, e.Exception); });
#endif

                foreach (var item in doc.Descendants("Target"))
                {
                    var alias = (string)item.Attribute("Alias");
                    var name = (string)item.Attribute("Name");

#if !FEATURE_XML_SCHEMA
                    // We must manually check this now that schema validation is not available
                    if (alias == null || name == null)
                    {
                        throw new TargetMapperException(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.MalformedMap, path));
                    }
#endif

                    AddAlias(alias, name);
                }
            }
            catch (XmlException e)
            {
                var message = string.Format(CultureInfo.CurrentCulture, LocalizedStrings.MalformedMap, e.Message);

                if (!string.IsNullOrEmpty(path))
                {
                    message = string.Format(CultureInfo.CurrentCulture, "{0} [{1}]", message, path);
                }

                throw new TargetMapperException(message, e);
            }
        }

        /// <summary>
        /// Performs alias to name mapping
        /// </summary>
        /// <param name="aliasName">target alias</param>
        /// <returns>target name</returns>
        public ICollection<string> GetNames(string aliasName)
        {
            ICollection<string> result;

            if (_map.TryGetValue(aliasName, out result))
            {
                return new ReadOnlyCollection<string>(result.ToList());
            }
            else
            {
                return new ReadOnlyCollection<string>(new List<string> { aliasName });
            }
        }

        public ICollection<string> Aliases
        {
            get { return new ReadOnlyCollection<string>(_map.Keys.ToList()); }
        }

        /// <summary>
        /// Returns the identifies for the target names. If multiple targets have the same name, keep the version as well
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetTargetNames(IEnumerable<FrameworkName> targets, bool includeVersion)
        {
            foreach (var group in targets.GroupBy(target => target.Identifier))
            {
                /*If you specify Windows/8.0 and Windows/8.1 we would need to keep both name and the platform. 
                Windows 8.0, Windows 8.1

                However if you specify Windows/8.0 and Silverlight/5.0 we would only want to keep the name:
                Windows, Silverlight

                So, if the number of elements in the group is 1 it means that we only have 1 platform, so we can just keep the Identifier (which is group.Key).

                Otherwise, we want to iterate over all the elements in the group and return the FullName (Identifier + version).
                 */
                if (group.Count() == 1)
                {
                    if (includeVersion)
                        yield return group.Single().FullName;
                    else
                        yield return group.Key;
                }
                else
                {
                    foreach (var target in group)
                    {
                        // We need to reverse map the identifier from the service (if we have a map defined).
                        yield return new FrameworkName(target.Identifier, target.Version).FullName;
                    }
                }
            }
        }

        /// <summary>
        /// Performs name to grouped-target (alias) mapping
        /// </summary>
        /// <example>
        /// If there are Grouped Targets, like:
        /// Available Grouped Targets:
        /// - Mobile (Windows, Windows Phone, Xamarin.Android, Xamarin.iOS)
        /// 
        /// Then:
        /// GetAlias(".NET Framework") will return ".NET Framework"
        /// GetAlias("Windows") will return "Mobile"
        /// GetAlias("Windows Phone") will return "Mobile"
        /// 
        /// </example>
        /// <param name="targetName">Official target name</param>
        public string GetAlias(string targetName)
        {
            var pair = _map.FirstOrDefault(i => i.Value.Contains(targetName));

            return pair.Key == null ? targetName : pair.Key;
        }

        public void AddAlias(string alias, string name)
        {
            // Verify aliases do not equal any defined target names
            if (_map.Keys.Contains(name))
            {
                throw new TargetMapperException(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.AliasCanotBeEqualToTargetNameError, name));
            }

            // Create entry if it does not exist
            if (!_map.ContainsKey(alias))
            {
                _map.Add(alias, new SortedSet<string>(StringComparer.OrdinalIgnoreCase));
            }

            _map[alias].Add(name);
        }

        /// <summary>
        /// Parses a JSON-like string for aliases and targets
        /// </summary>
        /// <param name="aliasString">
        /// Expected input similar to the following:
        /// 
        /// alias1: target1, target2; alias2: target1, target2, target3
        /// </param>
        /// <param name="validate">if true, and exception will be thrown if format is not correct</param>
        public void ParseAliasString(string aliasString, bool validate = false)
        {
            const char GroupSeparator = ';';
            const char AliasTargetSeparator = ':';
            const char TargetSeparator = ',';

            if (string.IsNullOrEmpty(aliasString))
            {
                return;
            }

            var groups = aliasString.Split(GroupSeparator).Select(group => group.Split(AliasTargetSeparator)).ToList();

            if (validate && groups.Any(g => g.Length != 2))
            {
                throw new ArgumentOutOfRangeException("aliasString", aliasString, string.Format("An alias should be separated from names by '{0}'", AliasTargetSeparator));
            }

            foreach (var group in groups)
            {
                if (group.Length == 2)
                {
                    var alias = group[0];
                    var names = group[1].Split(TargetSeparator);

                    foreach (var name in names)
                    {
                        AddAlias(alias.Trim(), name.Trim());
                    }
                }
            }
        }

        public void VerifySingleAlias()
        {
            var invalidNames = Aliases.Where(alias => GetNames(alias).Count != 1).ToList();

            if (invalidNames.Any())
            {
                throw new AliasMappedToMultipleNamesException(invalidNames);
            }
        }
    }
}
