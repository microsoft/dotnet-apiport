// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Decoding;
using System.Security.Cryptography;

namespace Microsoft.Fx.Portability
{
    internal static class MetadataReaderExtensions
    {
        public static AssemblyReferenceInformation FormatAssemblyInfo(this MetadataReader metadataReader)
        {
            return metadataReader.FormatAssemblyInfo(metadataReader.GetAssemblyDefinition());
        }

        public static AssemblyReferenceInformation FormatAssemblyInfo(this MetadataReader metadataReader, AssemblyReference assemblyReference)
        {
            var name = metadataReader.GetString(assemblyReference.Name);

            return metadataReader.FormatAssemblyInfo(name, assemblyReference.Culture, assemblyReference.PublicKeyOrToken, assemblyReference.Version);
        }

        public static AssemblyReferenceInformation FormatAssemblyInfo(this MetadataReader metadataReader, AssemblyDefinition assemblyDefinition)
        {
            var name = metadataReader.GetString(assemblyDefinition.Name);

            return metadataReader.FormatAssemblyInfo(name, assemblyDefinition.Culture, assemblyDefinition.PublicKey, assemblyDefinition.Version);
        }

        public static string GetTargetFrameworkMoniker(this MetadataReader metadataReader)
        {
            var tfmAttributeHandle = metadataReader.CustomAttributes
                            .Cast<CustomAttributeHandle>()
                            .SingleOrDefault(metadataReader.IsTargetFrameworkMonikerAttribute);

            if (tfmAttributeHandle.IsNil)
            {
                return null;
            }

            var parameters = metadataReader.GetParameterValues(metadataReader.GetCustomAttribute(tfmAttributeHandle));

            return parameters.FirstOrDefault();
        }

        /// <summary>
        /// This method will return a list of parameter values for the custom attribute.
        /// </summary>
        /// <remarks>
        /// Currently, this only works for string values
        /// </remarks>
        public static ImmutableArray<string> GetParameterValues(this MetadataReader metadataReader, CustomAttribute customAttribute)
        {
            if (customAttribute.Constructor.Kind != HandleKind.MemberReference)
            {
                throw new InvalidOperationException();
            }

            var ctor = metadataReader.GetMemberReference((MemberReferenceHandle)customAttribute.Constructor);
            var provider = new StringParameterValueTypeProvider(metadataReader, customAttribute.Value);
            var signature = ctor.DecodeMethodSignature(provider);

            return signature.ParameterTypes;
        }

        private static AssemblyReferenceInformation FormatAssemblyInfo(this MetadataReader metadataReader, string name, StringHandle cultureHandle, BlobHandle publicKeyTokenHandle, Version version)
        {
            var culture = cultureHandle.IsNil
                ? "neutral"
                : metadataReader.GetString(cultureHandle);

            var publicKeyToken = publicKeyTokenHandle.IsNil
                ? "null"
                : metadataReader.FormatPublicKeyToken(publicKeyTokenHandle);

            return new AssemblyReferenceInformation(name, version, culture, publicKeyToken);
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

        private static bool IsTargetFrameworkMonikerAttribute(this MetadataReader metadataReader, CustomAttributeHandle handle)
        {
            if (handle.IsNil)
            {
                return false;
            }

            var customAttribute = metadataReader.GetCustomAttribute(handle);

            if (customAttribute.Constructor.Kind != HandleKind.MemberReference)
            {
                return false;
            }

            // Managed C++ can build modules which are later linked together into an assembly by the linker.
            // In such cases, there's no place to hang assembly-level attributes at compile-time, so the compiler
            // attaches the attributes to specific typerefs which exists solely for this purpose, and the linker
            // later picks them up from there and attaches them to the assembly. So, if we find assembly-level
            // attributes on a type reference, we can safely ignore them (since they will be
            // duplicated by what the linker propagates to the assembly level).
            if (customAttribute.Parent.Kind == HandleKind.TypeReference)
            {
                return false;
            }

            var constructorRef = metadataReader.GetMemberReference((MemberReferenceHandle)customAttribute.Constructor);

            if (constructorRef.Parent.Kind != HandleKind.TypeReference)
            {
                return false;
            }

            var typeRef = metadataReader.GetTypeReference((TypeReferenceHandle)constructorRef.Parent);

            var typeRefName = metadataReader.GetString(typeRef.Name);
            var typeRefNamespace = metadataReader.GetString(typeRef.Namespace);

            return string.Equals(typeRefName, "TargetFrameworkAttribute", StringComparison.Ordinal)
                && string.Equals(typeRefNamespace, "System.Runtime.Versioning", StringComparison.Ordinal);
        }
    }
}
