// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.Fx.Portability.Analyzer.Resources;

namespace Microsoft.Fx.Portability.Analyzer
{
    internal class MemberMetadataInfoTypeProvider : ISignatureTypeProvider<MemberMetadataInfo, object>
    {
        private readonly SignatureDecoder<MemberMetadataInfo, object> _methodDecoder;

        public MetadataReader Reader { get; }

        public MemberMetadataInfoTypeProvider(MetadataReader reader)
        {
            Reader = reader;
            _methodDecoder = new SignatureDecoder<MemberMetadataInfo, object>(this, reader, null);
        }

        public MemberMetadataInfo GetMemberRefInfo(MemberReference memberReference)
        {
            var parentType = GetMemberParentInfo(memberReference);

            if (parentType == null)
            {
                return null;
            }

            switch (memberReference.GetKind())
            {
                case MemberReferenceKind.Field:
                    return new MemberMetadataInfo
                    {
                        Name = Reader.GetString(memberReference.Name),
                        ParentType = parentType,
                        Kind = MemberKind.Field
                    };
                case MemberReferenceKind.Method:
                    return new MemberMetadataInfo
                    {
                        Name = Reader.GetString(memberReference.Name),
                        ParentType = parentType,
                        Kind = MemberKind.Method,
                        MethodSignature = memberReference.DecodeMethodSignature(this, null).MakeEnclosedType()
                    };
                default:
                    return null;
            }
        }

        public MemberMetadataInfo GetMemberParentInfo(MemberReference memberReference)
        {
            Handle parent = memberReference.Parent;
            switch (parent.Kind)
            {
                case HandleKind.TypeReference:
                    return GetFullName((TypeReferenceHandle)parent);
                case HandleKind.TypeDefinition:
                    return new MemberMetadataInfo(GetFullName((TypeDefinitionHandle)parent))
                    {
                        IsTypeDef = true
                    };
                case HandleKind.TypeSpecification:
                    var type = Reader.GetTypeSpecification((TypeSpecificationHandle)parent);
                    return type.DecodeSignature(this, null);
                case HandleKind.MethodDefinition:
                    var method = Reader.GetMethodDefinition((MethodDefinitionHandle)parent);
                    return new MemberMetadataInfo(GetFullName(method.GetDeclaringType()));
                default:
                    return null;
            }
        }

        private MemberMetadataInfo GetName(TypeDefinition type)
        {
            return new MemberMetadataInfo
            {
                Name = Reader.GetString(type.Name),
                IsTypeDef = true,
                Namespace = !type.Namespace.IsNil ? Reader.GetString(type.Namespace) : null
            };
        }

        private MemberMetadataInfo GetName(TypeReference reference)
        {
            return new MemberMetadataInfo
            {
                Name = Reader.GetString(reference.Name),
                Namespace = !reference.Namespace.IsNil ? Reader.GetString(reference.Namespace) : null
            };
        }

        private MemberMetadataInfo GetFullName(TypeDefinitionHandle handle)
        {
            TypeDefinition definition = Reader.GetTypeDefinition(handle);
            return GetFullName(definition);
        }

        private MemberMetadataInfo GetFullName(TypeDefinition type)
        {
            var declaringTypeHandle = type.GetDeclaringType();

            if (declaringTypeHandle.IsNil)
            {
                return GetName(type);
            }

            MemberMetadataInfo info1 = GetName(type);
            MemberMetadataInfo info2 = GetFullName(declaringTypeHandle);

            return new MemberMetadataInfo(info1, info2);
        }

        private MemberMetadataInfo GetFullName(TypeReferenceHandle handle)
        {
            var reference = Reader.GetTypeReference(handle);
            return GetFullName(reference);
        }

        public MemberMetadataInfo GetFullName(TypeReference reference, TypeReference? child = null)
        {
            Handle scope = reference.ResolutionScope;
            MemberMetadataInfo name = GetName(reference);

            switch (scope.Kind)
            {
                case HandleKind.ModuleReference:
                    var moduleReference = (ModuleReferenceHandle)scope;

                    return new MemberMetadataInfo(name)
                    {
                        Module = moduleReference
                    };

                case HandleKind.AssemblyReference:
                    if (!name.DefinedInAssembly.HasValue)
                    {
                        return new MemberMetadataInfo(name)
                        {
                            DefinedInAssembly = Reader.GetAssemblyReference((AssemblyReferenceHandle)(scope))
                        };
                    }
                    else
                    {
                        return name;
                    }

                case HandleKind.TypeReference:
                    if (child != null)
                    {
                        // Some obfuscators will inject impossible types that are each others' scopes
                        // in order to foil decompilers. Check for that case so that we can fail reasonably
                        // instead of stack overflowing.
                        if (Reader.GetTypeReference((TypeReferenceHandle)(scope)).Equals(child.Value))
                        {
                            throw new BadImageFormatException(LocalizedStrings.InfiniteTypeParentingRecursion);
                        }
                    }
                    MemberMetadataInfo info2 = GetFullName(Reader.GetTypeReference((TypeReferenceHandle)(scope)), reference);
                    return new MemberMetadataInfo(name, info2);

                default:
                    // These cases are rare. According to spec, nil means look 
                    // in ExportedTypes, and Handle.ModuleDefinition means
                    // "reference" to type defined in the current module. The
                    // syntax generated here may be wrong. In the module definition
                    // case, ilasm would use a typedef directly, which should be
                    // equivalent if my understanding is correct. I don't know
                    // if it will resolve ExportedTypes from the same syntax
                    // however.
                    Debug.Assert(scope.IsNil || scope == Handle.ModuleDefinition);
                    return name;
            }
        }

        public MemberMetadataInfo GetPrimitiveType(PrimitiveTypeCode typeCode)
        {
            string name;
            switch (typeCode)
            {
                case PrimitiveTypeCode.Boolean:
                    name = "System.Boolean";
                    break;

                case PrimitiveTypeCode.Byte:
                    name = "System.Byte";
                    break;

                case PrimitiveTypeCode.Char:
                    name = "System.Char";
                    break;

                case PrimitiveTypeCode.Double:
                    name = "System.Double";
                    break;

                case PrimitiveTypeCode.Int16:
                    name = "System.Int16";
                    break;

                case PrimitiveTypeCode.Int32:
                    name = "System.Int32";
                    break;

                case PrimitiveTypeCode.Int64:
                    name = "System.Int64";
                    break;

                case PrimitiveTypeCode.IntPtr:
                    name = "System.IntPtr";
                    break;

                case PrimitiveTypeCode.Object:
                    name = "System.Object";
                    break;

                case PrimitiveTypeCode.SByte:
                    name = "System.SByte";
                    break;

                case PrimitiveTypeCode.Single:
                    name = "System.Single";
                    break;

                case PrimitiveTypeCode.String:
                    name = "System.String";
                    break;

                case PrimitiveTypeCode.TypedReference:
                    name = "System.TypedReference";
                    break;

                case PrimitiveTypeCode.UInt16:
                    name = "System.UInt16";
                    break;

                case PrimitiveTypeCode.UInt32:
                    name = "System.UInt32";
                    break;

                case PrimitiveTypeCode.UInt64:
                    name = "System.UInt64";
                    break;

                case PrimitiveTypeCode.UIntPtr:
                    name = "System.UIntPtr";
                    break;

                case PrimitiveTypeCode.Void:
                    name = "System.Void";
                    break;

                default:
                    Debug.Assert(false);
                    throw new ArgumentOutOfRangeException(nameof(typeCode));
            }

            return new MemberMetadataInfo
            {
                Name = name,
                IsPrimitiveType = true
            };
        }

        public MemberMetadataInfo GetTypeFromDefinition(TypeDefinitionHandle handle)
        {
            return GetFullName(handle);
        }

        public MemberMetadataInfo GetTypeFromDefinition(TypeDefinitionHandle handle, bool? isValueType)
        {
            return GetFullName(handle);
        }

        public MemberMetadataInfo GetSZArrayType(MemberMetadataInfo elementType)
        {
            return new MemberMetadataInfo(elementType)
            {
                Name = $"{elementType.Name}[]"
            };
        }

        public MemberMetadataInfo GetPointerType(MemberMetadataInfo elementType)
        {
            return new MemberMetadataInfo(elementType)
            {
                IsPointer = true
            };
        }

        public MemberMetadataInfo GetByReferenceType(MemberMetadataInfo elementType)
        {
            return new MemberMetadataInfo(elementType)
            {
                Name = $"{elementType.Name}@"
            };
        }

        public MemberMetadataInfo GetGenericMethodParameter(object genericContext, int index)
        {
            // Generic arguments on methods are prefixed with ``
            // Type generic arguments are prefixed with `
            return new MemberMetadataInfo
            {
                Name = $"``{index}",
                IsGenericInstance = true
            };
        }

        public MemberMetadataInfo GetGenericTypeParameter(object genericContext, int index)
        {
            // Type generic arguments are prefixed with `
            return new MemberMetadataInfo
            {
                Name = $"`{index}",
                IsGenericInstance = true
            };
        }

        public MemberMetadataInfo GetGenericInstantiation(MemberMetadataInfo genericType, ImmutableArray<MemberMetadataInfo> typeArguments)
        {
            genericType.IsGenericInstance = true;
            genericType.GenericTypeArgs = new List<MemberMetadataInfo>(typeArguments);
            foreach (MemberMetadataInfo mInfo in genericType.GenericTypeArgs)
            {
                mInfo.IsEnclosedType = true;
            }
            return genericType;
        }

        public MemberMetadataInfo GetFunctionPointerType(MethodSignature<MemberMetadataInfo> signature)
        {
            var pointerName = new StringBuilder("function ")
                .Append(signature.ReturnType)
                .Append(" (")
                .Append(string.Join(",", signature.ParameterTypes))
                .Append(")")
                .ToString();

            return new MemberMetadataInfo()
            {
                Name = pointerName,
                MethodSignature = signature,
                Kind = MemberKind.Type
            };
        }

        public MemberMetadataInfo GetPinnedType(MemberMetadataInfo elementType)
        {
            elementType.Name += " pinned";
            return elementType;
        }

        public MemberMetadataInfo GetArrayType(MemberMetadataInfo elementType, ArrayShape shape)
        {
            var builder = new StringBuilder("[");

            for (int i = 0; i < shape.Rank; i++)
            {
                int lowerBound = 0;

                if (i < shape.LowerBounds.Length)
                {
                    lowerBound = shape.LowerBounds[i];
                    builder.Append(lowerBound);
                    builder.Append(":");
                }

                if (i < shape.Sizes.Length)
                {
                    builder.Append(lowerBound + shape.Sizes[i] - 1);
                }

                if (i < shape.Rank - 1)
                {
                    builder.Append(',');
                }
            }

            builder.Append(']');
            elementType.ArrayTypeInfo = builder.ToString();
            return elementType;
        }

        public MemberMetadataInfo GetTypeFromSpecification(MetadataReader reader, object genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
        {
            var entityHandle = (EntityHandle)handle;

            switch (entityHandle.Kind)
            {
                case HandleKind.TypeDefinition:
                    return GetTypeFromDefinition((TypeDefinitionHandle)entityHandle);
                case HandleKind.TypeReference:
                    return GetFullName((TypeReferenceHandle)entityHandle);
                case HandleKind.TypeSpecification:
                    var specification = reader.GetTypeSpecification((TypeSpecificationHandle)entityHandle);

                    return specification.DecodeSignature(this, null);
                default:
                    throw new NotSupportedException("This kind is not supported!");
            }
        }

        public MemberMetadataInfo GetModifiedType(MemberMetadataInfo modifier, MemberMetadataInfo unmodifiedType, bool isRequired)
        {
            return new MemberMetadataInfo(unmodifiedType)
            {
                Modifiers = unmodifiedType.Modifiers.Push(new MemberModifiedMetadata(isRequired, modifier))
            };
        }

        public MemberMetadataInfo GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        {
            return GetTypeFromDefinition(handle);
        }

        public MemberMetadataInfo GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        {
            return GetFullName(handle);
        }
    }
}