// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;
using System.Collections.Immutable;
using System.Globalization;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class AssemblyReferenceInformation
    {
        private static readonly StringComparer DefaultComparer = StringComparer.OrdinalIgnoreCase;

        private readonly string _string;

        public AssemblyReferenceInformation(AssemblyInfo info)
        {
            var name = info.GetAssemblyName();

            Name = name.Name;
            Version = name.Version;
            Culture = name.CultureName;

            var publicKey = name.GetPublicKeyToken();

            if (publicKey != null)
            {
                PublicKeyToken = new PublicKeyToken(publicKey.ToImmutableArray());
            }

            _string = string.Format(CultureInfo.CurrentCulture, LocalizedStrings.AssemblyReferenceInformation, Name, Version, Culture, PublicKeyToken);
        }

        public AssemblyReferenceInformation(string name, Version version, string culture, PublicKeyToken publicKeyToken)
        {
            Name = name;
            Version = version;
            Culture = culture;
            PublicKeyToken = publicKeyToken;

            _string = string.Format(CultureInfo.CurrentCulture, LocalizedStrings.AssemblyReferenceInformation, Name, Version, Culture, PublicKeyToken);
        }

        public string Name { get; }

        public string Culture { get; }

        public Version Version { get; }

        public PublicKeyToken PublicKeyToken { get; }

        public override string ToString() => _string;

        public override bool Equals(object obj) => DefaultComparer.Equals(_string, (obj as AssemblyReferenceInformation)?._string);

        public override int GetHashCode() => DefaultComparer.GetHashCode(_string);
    }
}
