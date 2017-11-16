// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;
using System.Reflection;

namespace Microsoft.Fx.Portability
{
    public class ProductInformation
    {
        public ProductInformation(string name)
        {
            var assembly = typeof(ProductInformation).GetTypeInfo().Assembly;
            var info = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            var version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

            if (!IsValid(name))
            {
                throw new ArgumentOutOfRangeException(nameof(name), LocalizedStrings.ProductInformationInvalidArgument);
            }

            if (!IsValid(version))
            {
                throw new ArgumentOutOfRangeException(nameof(version), LocalizedStrings.ProductInformationInvalidArgument);
            }

            InformationalVersion = info;
            Name = name;
            Version = version;
        }

        public string InformationalVersion { get; }

        public string Name { get; }

        public string Version { get; }

        /// <summary>
        /// Verify strings/versions only contain letters, digits, '.', or '_'.  Otherwise, the user agent string may be created incorrectly
        /// </summary>
        private static bool IsValid(string str)
        {
            foreach (var s in str.ToCharArray())
            {
                if (!char.IsLetterOrDigit(s) && s != '.' && s != '_')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
