// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;

namespace Microsoft.Fx.Portability
{
    public readonly struct PublicKeyToken : IEquatable<PublicKeyToken>
    {
        private readonly ImmutableArray<byte> _token;

        public PublicKeyToken(ImmutableArray<byte> bytes)
        {
            _token = bytes;
        }

        public bool IsEmpty => _token.IsDefaultOrEmpty;

        public ImmutableArray<byte> Token => _token.IsDefault ? ImmutableArray<byte>.Empty : _token;

        public bool Equals(PublicKeyToken other)
        {
            if (Token.Length != other.Token.Length)
            {
                return false;
            }

            for (int i = 0; i < Token.Length; i++)
            {
                if (Token[i] != other.Token[i])
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object obj) => obj is PublicKeyToken other ? Equals(other) : false;

        public override int GetHashCode()
        {
            int hash = 19;
            unchecked
            {
                foreach (var item in Token)
                {
                    hash = (hash * 31) + item;
                }
            }

            return hash;
        }

        public override string ToString()
        {
            if (Token.IsEmpty)
            {
                return "null";
            }

            var hex = new StringBuilder(Token.Length * 2);

            foreach (byte b in Token)
            {
                hex.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);
            }

            return hex.ToString();
        }

        public static PublicKeyToken Parse(string input) => new PublicKeyToken(ParseString(input));

        public static bool operator ==(PublicKeyToken left, PublicKeyToken right) => left.Equals(right);

        public static bool operator !=(PublicKeyToken left, PublicKeyToken right) => !(left == right);

        private static ImmutableArray<byte> ParseString(string hex)
        {
            if (hex.Length % 2 != 0)
            {
                throw new PortabilityAnalyzerException(string.Format(CultureInfo.InvariantCulture, LocalizedStrings.InvalidPublicKeyToken, hex));
            }

            try
            {
                var bytes = new byte[hex.Length / 2];

                for (int i = 0; i < hex.Length; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                }

                return ImmutableArray.Create(bytes);
            }
            catch (FormatException e)
            {
                throw new PortabilityAnalyzerException(string.Format(CultureInfo.InvariantCulture, LocalizedStrings.InvalidPublicKeyToken, hex), e);
            }
        }
    }
}
