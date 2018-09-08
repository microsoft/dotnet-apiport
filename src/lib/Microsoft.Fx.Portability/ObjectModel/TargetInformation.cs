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
        private static readonly ISet<string> EmptyTargets = new HashSet<string>();
        private static readonly ICollection<TargetVersion> EmptyVersions = new List<TargetVersion>();
        private static readonly string ListSeparator = CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ";

        private ISet<string> _expandedTargets;
        private ICollection<TargetVersion> _versions;

        public TargetInformation()
        {
            _expandedTargets = EmptyTargets;
            _versions = EmptyVersions;
        }

        public string Name { get; set; }

        public IEnumerable<string> ExpandedTargets
        {
            get { return _expandedTargets; }
            set { _expandedTargets = new HashSet<string>(value, StringComparer.OrdinalIgnoreCase); }
        }

        public IEnumerable<TargetVersion> AvailableVersions
        {
            get
            {
                return _versions;
            }

            set
            {
                if (value == null)
                {
                    _versions = EmptyVersions;
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
                return string.Format(CultureInfo.CurrentCulture, LocalizedStrings.TargetInformationGroups, Name, string.Join(ListSeparator, ExpandedTargets));
            }
            else
            {
                return Name;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TargetInformation other))
            {
                return false;
            }

            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
                && _expandedTargets.SetEquals(other._expandedTargets);
        }

        public override int GetHashCode()
        {
            const int HashSeed = 17;
            const int HashMultiplier = 23;

            int hash = HashSeed;
            hash = (hash * HashMultiplier) + (Name ?? string.Empty).GetHashCode();

            foreach (var target in ExpandedTargets)
            {
                hash = (hash * HashMultiplier) + target.GetHashCode();
            }

            return hash;
        }
    }
}
