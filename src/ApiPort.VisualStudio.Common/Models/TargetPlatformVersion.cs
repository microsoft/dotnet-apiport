// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace ApiPortVS
{
    public class TargetPlatformVersion : NotifyPropertyBase
    {
        private string _platformName;
        public string PlatformName
        {
            get { return _platformName; }
            set { UpdateProperty(ref _platformName, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { UpdateProperty(ref _isSelected, value); }
        }

        private Version _version;
        public Version Version
        {
            get { return _version; }
            set { UpdateProperty(ref _version, value); }
        }

        public override string ToString()
        {
            if (Version == null)
            {
                return PlatformName;
            }
            else
            {
                return String.Format("{0}, Version={1}", PlatformName, Version);
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as TargetPlatformVersion;

            if (other == null)
            {
                return false;
            }

            return String.Equals(PlatformName, other.PlatformName, StringComparison.OrdinalIgnoreCase)
                && Version == other.Version;
        }

        public override int GetHashCode()
        {
            const int HashMultipler = 31;

            unchecked
            {
                int hash = 17;

                hash = hash * HashMultipler + PlatformName.GetHashCode();
                hash = hash * HashMultipler + Version.GetHashCode();

                return hash;
            }
        }
    }
}
