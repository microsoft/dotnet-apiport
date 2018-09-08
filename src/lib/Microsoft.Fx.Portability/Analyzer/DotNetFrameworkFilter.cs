// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability.Analyzer
{
    public class DotNetFrameworkFilter : IDependencyFilter
    {
        /// <summary>
        /// These keys are a collection of public key tokens derived from all the reference assemblies in
        /// "%ProgramFiles%\Reference Assemblies\Microsoft" on a Windows 10 machine with VS 2015 installed
        /// </summary>
        private static readonly ICollection<string> s_microsoftKeys = new HashSet<string>(new[]
        {
            "b77a5c561934e089", // ECMA
            "b03f5f7f11d50a3a", // DEVDIV
            "7cec85d7bea7798e", // SLPLAT
            "31bf3856ad364e35", // SILVERLIGHT
            "24eec0d8c86cda1e", // PHONE
            "0738eb9f132ed756", // MONO
            "cc7b13ffcd2ddd51" // NetStandard
        }, StringComparer.OrdinalIgnoreCase);

        private static readonly IEnumerable<string> s_frameworkAssemblyNamePrefixes = new[]
        {
            "System.",
            "Microsoft.AspNet.",
            "Microsoft.AspNetCore.",
            "Microsoft.CSharp.",
            "Microsoft.EntityFrameworkCore.",
            "Microsoft.Win32.",
            "Microsoft.VisualBasic.",
            "Windows."
        };

        public bool IsFrameworkAssembly(AssemblyReferenceInformation assembly)
        {
            if (assembly == null)
            {
                // If we don't have the assembly, default to including the API
                return true;
            }

            if (s_microsoftKeys.Contains(assembly.PublicKeyToken))
            {
                return true;
            }

            if (s_frameworkAssemblyNamePrefixes.Any(p => assembly.Name.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            if (string.Equals(assembly.Name, "mscorlib", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
