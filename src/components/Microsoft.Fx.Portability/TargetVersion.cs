// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Fx.Portability
{
    public class TargetVersion
    {
        public string Version { get; set; }

        public Version ToVersion()
        {
            return new Version(Version);
        }

        public static TargetVersion FromVersion(Version version)
        {
            return new TargetVersion { Version = version.ToString() };
        }

        public override int GetHashCode()
        {
            return Version.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as TargetVersion;

            if (other == null)
            {
                return false;
            }

            return String.Equals(Version, other.Version, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return Version;
        }
    }
}
