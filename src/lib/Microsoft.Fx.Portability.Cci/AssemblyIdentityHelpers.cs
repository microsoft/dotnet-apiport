// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Linq;

namespace Microsoft.Cci.Extensions
{
    public static class AssemblyIdentityHelpers
    {
        public static string Format(this AssemblyIdentity assemblyIdentity)
        {
            var name = new System.Reflection.AssemblyName();
            var cultureInfo = new CultureInfo(assemblyIdentity.Culture);

            name.Name = assemblyIdentity.Name.Value;
#if FEATURE_ASSEMBLYNAME_CULTUREINFO
            name.CultureInfo = cultureInfo;
#else
            name.CultureName = cultureInfo.Name;
#endif
            name.Version = assemblyIdentity.Version;
            name.SetPublicKeyToken(assemblyIdentity.PublicKeyToken.ToArray());
#if FEATURE_ASSEMBLYNAME_CODEBASE
            name.CodeBase = assemblyIdentity.Location;
#endif
            return name.ToString();
        }

        public static AssemblyIdentity Parse(INameTable nameTable, string formattedName)
        {
            var name = new System.Reflection.AssemblyName(formattedName);
            return new AssemblyIdentity(nameTable.GetNameFor(name.Name),
#if FEATURE_ASSEMBLYNAME_CULTUREINFO
                                        name.CultureInfo.Name,
#else
                                        name.CultureName,
#endif
                                        name.Version,
                                        name.GetPublicKeyToken(),
#if FEATURE_ASSEMBLYNAME_CODEBASE
                                        name.CodeBase);
#else
                                        string.Empty);
#endif
        }
    }
}
