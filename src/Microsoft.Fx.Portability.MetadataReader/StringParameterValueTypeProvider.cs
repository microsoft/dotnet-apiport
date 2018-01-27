// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer.Resources;
using System;
using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace Microsoft.Fx.Portability
{
    internal class StringParameterValueTypeProvider : ISignatureTypeProvider<string, object>
    {
        private readonly BlobReader _valueReader;

        public StringParameterValueTypeProvider(MetadataReader reader, BlobHandle value)
        {
            Reader = reader;
            _valueReader = reader.GetBlobReader(value);

            var prolog = _valueReader.ReadUInt16();
            if (prolog != 1)
            {
                throw new BadImageFormatException(LocalizedStrings.InvalidAttributeProlog);
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

        public static string GetGenericInstance(string genericType, ImmutableArray<string> typestrings)
        {
            return string.Empty;
        }

        public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments) => throw new NotImplementedException();

        public static string GetGenericMethodParameter(int index)
        {
            return string.Empty;
        }

        public string GetGenericMethodParameter(object genericContext, int index) => throw new NotImplementedException();

        public static string GetGenericTypeParameter(int index)
        {
            return string.Empty;
        }

        public string GetGenericTypeParameter(object genericContext, int index) => throw new NotImplementedException();

        public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired)
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

        public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            return string.Empty;
        }

        public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            return string.Empty;
        }

        public string GetTypeFromSpecification(MetadataReader reader, object genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
        {
            return string.Empty;
        }
    }
}
