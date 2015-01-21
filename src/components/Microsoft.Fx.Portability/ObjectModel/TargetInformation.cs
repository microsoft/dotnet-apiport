// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class TargetInformation
    {
        private static readonly ISet<string> s_EmptyTargets = new HashSet<string>();
        private static readonly ICollection<TargetVersion> s_EmptyVersions = new List<TargetVersion>();
        private static readonly string s_ListSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";

        private ISet<string> _expandedTargets;
        private ICollection<TargetVersion> _versions;

        public TargetInformation()
        {
            _expandedTargets = s_EmptyTargets;
            _versions = s_EmptyVersions;
        }

        public string Name { get; set; }

        public IEnumerable<string> ExpandedTargets
        {
            get { return _expandedTargets; }
            set
            {
                _expandedTargets = new HashSet<string>(value, StringComparer.OrdinalIgnoreCase);
            }
        }

        public IEnumerable<TargetVersion> AvailableVersions
        {
            get { return _versions; }
            set
            {
                if (value == null)
                {
                    _versions = s_EmptyVersions;
                }
                else
                {
                    _versions = value.OrderBy(v => v.ToVersion()).ToList();
                }
            }
        }

        public override string ToString()
        {
            if (ExpandedTargets.Any())
            {
                return String.Format(LocalizedStrings.TargetInformationGroups, Name, String.Join(s_ListSeparator, ExpandedTargets));
            }
            else
            {
                return Name;
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as TargetInformation;

            if (other == null) return false;

            return String.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
                && _expandedTargets.SetEquals(other._expandedTargets);
        }

        public override int GetHashCode()
        {
            const int HashSeed = 17;
            const int HashMultiplier = 23;

            int hash = HashSeed;
            hash = hash * HashMultiplier + (Name ?? String.Empty).GetHashCode();

            foreach (var target in ExpandedTargets)
            {
                hash = hash * HashMultiplier + target.GetHashCode();
            }

            return hash;
        }
    }
}
