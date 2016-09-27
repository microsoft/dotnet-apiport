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
            name.Name = assemblyIdentity.Name.Value;
#if !COREFX
            name.CultureInfo = new CultureInfo(assemblyIdentity.Culture);
#endif
            name.Version = assemblyIdentity.Version;
            name.SetPublicKeyToken(assemblyIdentity.PublicKeyToken.ToArray());
#if !COREFX
            name.CodeBase = assemblyIdentity.Location;
#endif
            return name.ToString();
        }

        public static AssemblyIdentity Parse(INameTable nameTable, string formattedName)
        {
            var name = new System.Reflection.AssemblyName(formattedName);
            return new AssemblyIdentity(nameTable.GetNameFor(name.Name),
#if COREFX
                                        name.CultureName,
#else
                                        name.CultureInfo.Name,
#endif
                                        name.Version,
                                        name.GetPublicKeyToken(),
#if COREFX
                                        "");
#else
                                        name.CodeBase);
#endif
        }
    }
}