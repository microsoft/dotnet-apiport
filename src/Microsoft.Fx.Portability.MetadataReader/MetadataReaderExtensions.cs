// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Security.Cryptography;

namespace Microsoft.Fx.Portability
{
    internal static class MetadataReaderExtensions
    {
        public static AssemblyInfo GetAssemblyInfo(this MetadataReader metadataReader, string filePath)
        {
            var fileInfo = FileVersionInfo.GetVersionInfo(filePath);

            // TODO: Find TFM of assembly
            return new AssemblyInfo
            {
                AssemblyIdentity = metadataReader.FormatAssemblyInfo(metadataReader.GetAssemblyDefinition()),
                FileVersion = fileInfo.FileVersion ?? string.Empty,
                TargetFrameworkMoniker = string.Empty
            };
        }

        public static string FormatAssemblyInfo(this MetadataReader metadataReader, AssemblyReference assemblyReference)
        {
            var name = metadataReader.GetString(assemblyReference.Name);

            return metadataReader.FormatAssemblyInfo(name, assemblyReference.Culture, assemblyReference.PublicKeyOrToken, assemblyReference.Version);
        }

        public static string FormatAssemblyInfo(this MetadataReader metadataReader, AssemblyDefinition assemblyDefinition)
        {
            var name = metadataReader.GetString(assemblyDefinition.Name);

            return metadataReader.FormatAssemblyInfo(name, assemblyDefinition.Culture, assemblyDefinition.PublicKey, assemblyDefinition.Version);
        }

        private static string FormatAssemblyInfo(this MetadataReader metadataReader, string name, StringHandle cultureHandle, BlobHandle publicKeyTokenHandle, Version version)
        {
            var culture = cultureHandle.IsNil
                ? "neutral"
                : metadataReader.GetString(cultureHandle);

            var publicKeyToken = publicKeyTokenHandle.IsNil
                ? "null"
                : metadataReader.FormatPublicKeyToken(publicKeyTokenHandle);

            return $"{name}, Version={version}, Culture={culture}, PublicKeyToken={publicKeyToken}";
        }

        /// <summary>
        /// Convert a blob referencing a public key token from a PE file into a human-readable string.
        /// 
        /// If there are no bytes, the return will be 'null'
        /// If the length is greater than 8, it is a strong name signed assembly
        /// Otherwise, the key is the byte sequence
        /// </summary>
        /// <param name="metadataReader"></param>
        /// <param name="handle"></param>
        /// <returns></returns>
        private static string FormatPublicKeyToken(this MetadataReader metadataReader, BlobHandle handle)
        {
            byte[] bytes = metadataReader.GetBlobBytes(handle);

            if (bytes == null || bytes.Length <= 0)
            {
                return "null";
            }

            if (bytes.Length > 8)  // Strong named assembly
            {
                // Get the public key token, which is the last 8 bytes of the SHA-1 hash of the public key 
                using (var sha1 = SHA1.Create())
                {
                    var token = sha1.ComputeHash(bytes);

                    bytes = new byte[8];
                    int count = 0;
                    for (int i = token.Length - 1; i >= token.Length - 8; i--)
                    {
                        bytes[count] = token[i];
                        count++;
                    }
                }
            }

            // Convert bytes to string, but we don't want the '-' characters and need it to be lower case
            return BitConverter.ToString(bytes)
                .Replace("-", "")
                .ToLowerInvariant();
        }
    }
}
