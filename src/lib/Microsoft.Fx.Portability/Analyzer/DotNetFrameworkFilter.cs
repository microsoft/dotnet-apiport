// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Fx.Portability.Analyzer
{
    public class DotNetFrameworkFilter : IDependencyFilter
    {
        /// <summary>
        /// These keys are a collection of public key tokens derived from all the reference assemblies in
        /// "%ProgramFiles%\Reference Assemblies\Microsoft" on a Windows 10 machine with VS 2015 installed.
        /// </summary>
        private static readonly HashSet<PublicKeyToken> MicrosoftKeys = new HashSet<PublicKeyToken>(new[]
        {
            PublicKeyToken.Parse("b77a5c561934e089"), // ECMA
            PublicKeyToken.Parse("b03f5f7f11d50a3a"), // DEVDIV
            PublicKeyToken.Parse("7cec85d7bea7798e"), // SLPLAT
            PublicKeyToken.Parse("31bf3856ad364e35"), // SILVERLIGHT
            PublicKeyToken.Parse("24eec0d8c86cda1e"), // PHONE
            PublicKeyToken.Parse("0738eb9f132ed756"), // MONO
            PublicKeyToken.Parse("cc7b13ffcd2ddd51") // NetStandard
        });

        private static readonly string[] FrameworkAssemblyNamePrefixes = new[]
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

        public virtual bool IsFrameworkMember(string name, PublicKeyToken publicKeyToken)
            => IsFrameworkAssembly(name, publicKeyToken);

        public virtual bool IsFrameworkAssembly(string name, PublicKeyToken publicKeyToken)
            => IsKnownPublicKeyToken(publicKeyToken) || IsKnownName(name);

        private static bool IsKnownPublicKeyToken(PublicKeyToken publicKeyToken)
            => MicrosoftKeys.Contains(publicKeyToken);

        private static bool IsKnownName(string name)
        {
            // Name is null, default to submitting the API
            if (name is null)
            {
                return true;
            }

            if (string.Equals(name, "mscorlib", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (FrameworkAssemblyNamePrefixes.Any(p => name.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }
    }
}
