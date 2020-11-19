// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Fx.Portability.Utils
{
    /// <summary>
    /// Some utility functions for creating hashes useful for providing a usefully-strong content identity for assemblies.
    /// Collisions are unlikely, but acceptable.
    /// </summary>
    public static class Hasher
    {
        public static string GetBinHash(string path)
        {
            using var stream = File.OpenRead(path);
            return GetBinHash(stream);
        }

        // calculate and assign binhash
        public static string GetBinHash(Stream input)
        {
            // NOTE: we are using MD5 as a "stronger than assembly name" identity.
            // This is not a security feature, and has no security implications at all.
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
            using var md5Hash = MD5.Create();
#pragma warning restore CA5351 // Do Not Use Broken Cryptographic Algorithms
            byte[] hash = md5Hash.ComputeHash(input);
            input.Seek(0, SeekOrigin.Begin);
            return hash.ToHexString();
        }

        private static string ToHexString(this byte[] bytes)
        {
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }
    }
}
