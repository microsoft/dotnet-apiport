// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Fx.Portability.ObjectModel
{
    public struct NuGetPackageId
    {
        public readonly string PackageId;
        public readonly string Version;
        public readonly string Hyperlink;

        public NuGetPackageId(string packageId, string version, string hyperlink)
        {
            PackageId = packageId;
            Version = version;
            Hyperlink = hyperlink;
        }
    }
}
