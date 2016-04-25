// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Decoding;

namespace Microsoft.Fx.Portability
{
    internal class StringParameterValueTypeProvider : ISignatureTypeProvider<string>
    {
        private readonly BlobReader _valueReader;

        public StringParameterValueTypeProvider(MetadataReader reader, BlobHandle value)
        {
            Reader = reader;
            _valueReader = reader.GetBlobReader(value);

            var prolog = _valueReader.ReadUInt16();
            if (prolog != 1)
            {
                throw new BadImageFormatException("Invalid custom attribute prolog.");
            }
        }

        public MetadataReader Reader { get; }

        public string GetArrayType(string elementType, ArrayShape shape)
        {
            return string.Empty;
        }

        public string GetByReferenceType(string elementType)
        {
            return string.Empty;
        }

        public string GetFunctionPointerType(MethodSignature<string> signature)
        {
            return string.Empty;
        }

        public string GetGenericInstance(string genericType, ImmutableArray<string> typestrings)
        {
            return string.Empty;
        }

        public string GetGenericMethodParameter(int index)
        {
            return string.Empty;
        }

        public string GetGenericTypeParameter(int index)
        {
            return string.Empty;
        }

        public string GetModifiedType(MetadataReader reader, bool isRequired, string modifier, string unmodifiedType)
        {
            return string.Empty;
        }

        public string GetPinnedType(string elementType)
        {
            return string.Empty;
        }

        public string GetPointerType(string elementType)
        {
            return string.Empty;
        }

        public string GetPrimitiveType(PrimitiveTypeCode typeCode)
        {
            if (typeCode == PrimitiveTypeCode.String)
            {
                return _valueReader.ReadSerializedString();
            }
            else
            {
                return string.Empty;
            }
        }

        public string GetSZArrayType(string elementType)
        {
            return string.Empty;
        }

        public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, SignatureTypeHandleCode code)
        {
            return string.Empty;
        }

        public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, SignatureTypeHandleCode code)
        {
            return string.Empty;
        }

        public string GetTypeFromSpecification(MetadataReader reader, TypeSpecificationHandle handle, SignatureTypeHandleCode code)
        {
            return string.Empty;
        }
    }
}
