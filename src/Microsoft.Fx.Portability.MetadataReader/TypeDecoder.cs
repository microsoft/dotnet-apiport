// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Decoding;
using System.Text;

namespace Microsoft.Fx.Portability.Analyzer
{
    internal class TypeDecoder : ISignatureTypeProvider<MemberMetadataInfo>
    {
        public MetadataReader Reader { get; }

        public TypeDecoder(MetadataReader reader)
        {
            Reader = reader;
        }

        public MemberMetadataInfo GetMemberRefInfo(MemberReference memberReference)
        {
            string name = Reader.GetString(memberReference.Name);

            MemberMetadataInfo memberRefInfo = new MemberMetadataInfo(name);

            MemberMetadataInfo parentType = GetMemberParentInfo(memberReference);
            if (parentType == null)
                return null;
            memberRefInfo.ParentType = parentType;

            switch (memberReference.GetKind())
            {
                case MemberReferenceKind.Field:
                    memberRefInfo.Kind = MemberKind.Field;
                    break;
                case MemberReferenceKind.Method:
                    memberRefInfo.Kind = MemberKind.Method;
                    //get method signature
                    memberRefInfo.MethodSignature = SignatureDecoder.DecodeMethodSignature<MemberMetadataInfo>(memberReference.Signature, this);
                    foreach (MemberMetadataInfo mInfo in memberRefInfo.MethodSignature.ParameterTypes)
                    {
                        mInfo.IsEnclosedType = true;
                    }
                    break;
                default:
                    memberRefInfo = null;
                    break;
            }

            return memberRefInfo;
        }

        public MemberMetadataInfo GetMemberParentInfo(MemberReference memberReference)
        {
            Handle parent = memberReference.Parent;
            switch (parent.Kind)
            {
                case HandleKind.TypeReference: //get the typeref parent of memberRef
                    return GetFullName((TypeReferenceHandle)parent);
                case HandleKind.TypeDefinition: //get the typedef parent of memberRef
                    MemberMetadataInfo typeDefInfo = GetFullName((TypeDefinitionHandle)parent);
                    typeDefInfo.IsTypeDef = true;
                    return typeDefInfo;
                case HandleKind.TypeSpecification:   //get the typeref parent of memberRef
                    return SignatureDecoder.DecodeType(parent, this);
                default:
                    //Console.Error.WriteLine("MemberReference parent is of kind " + memberReference.Parent.Kind);
                    return null;
            }
        }

        private MemberMetadataInfo GetName(TypeDefinition type)
        {
            string name = Reader.GetString(type.Name);
            MemberMetadataInfo info = new MemberMetadataInfo(name);
            info.IsTypeDef = true;
            if (!type.Namespace.IsNil)
            {
                info.Namespace = Reader.GetString(type.Namespace);
            }
            return info;
        }

        private MemberMetadataInfo GetName(TypeReference reference)
        {
            string name = Reader.GetString(reference.Name);
            MemberMetadataInfo info = new MemberMetadataInfo(name);
            if (!reference.Namespace.IsNil)
            {
                info.Namespace = Reader.GetString(reference.Namespace);
            }
            return info;
        }

        private MemberMetadataInfo GetFullName(TypeDefinitionHandle handle)
        {
            TypeDefinition definition = Reader.GetTypeDefinition(handle);
            return GetFullName(definition);
        }

        private MemberMetadataInfo GetFullName(TypeDefinition type)
        {
            TypeDefinitionHandle declaringTypeHandle = type.GetDeclaringType();

            if (declaringTypeHandle.IsNil)
            {
                return GetName(type);
            }
            MemberMetadataInfo info1 = GetName(type);
            MemberMetadataInfo info2 = GetFullName(declaringTypeHandle);
            info1.Join(info2);
            return info1;
        }

        private MemberMetadataInfo GetFullName(TypeReferenceHandle handle)
        {
            var reference = Reader.GetTypeReference(handle);
            return GetFullName(reference);
        }

        public MemberMetadataInfo GetFullName(TypeReference reference)
        {
            Handle scope = reference.ResolutionScope;
            MemberMetadataInfo name = GetName(reference);

            switch (scope.Kind)
            {
                case HandleKind.ModuleReference:
                    var moduleReference = (ModuleReferenceHandle)scope;
                    name.Module = moduleReference;
                    return name;

                case HandleKind.AssemblyReference:
                    if (!name.IsAssemblySet)
                    {
                        name.IsAssemblySet = true;
                        name.DefinedInAssembly = (AssemblyReferenceHandle)(scope);
                    }
                    return name;

                case HandleKind.TypeReference:
                    MemberMetadataInfo info2 = GetFullName(Reader.GetTypeReference((TypeReferenceHandle)(scope)));
                    name.Join(info2);
                    return name;

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
                    throw new ArgumentOutOfRangeException("typeCode");
            }
            MemberMetadataInfo mInfo = new MemberMetadataInfo(name);
            mInfo.IsPrimitiveType = true;
            return mInfo;
        }

        public MemberMetadataInfo GetTypeFromDefinition(TypeDefinitionHandle handle)
        {
            return GetFullName(handle);
        }

        public MemberMetadataInfo GetTypeFromReference(TypeReferenceHandle handle)
        {
            return GetFullName(handle);
        }

        public MemberMetadataInfo GetSZArrayType(MemberMetadataInfo elementType)
        {
            elementType.Name += "[]";
            return elementType;
        }

        public MemberMetadataInfo GetPointerType(MemberMetadataInfo elementType)
        {
            elementType.Name += "*";
            return elementType;
        }


        public MemberMetadataInfo GetByReferenceType(MemberMetadataInfo elementType)
        {
            elementType.Name += "@";
            return elementType;
        }

        public MemberMetadataInfo GetGenericMethodParameter(int index)
        {
            // generic arguments on methods are prefixed with ``
            // type generic arguments are prefixed with '
            MemberMetadataInfo info = new MemberMetadataInfo("``" + index.ToString());
            info.IsGenericInstance = true;
            return info;
        }

        public MemberMetadataInfo GetGenericTypeParameter(int index)
        {
            MemberMetadataInfo info = new MemberMetadataInfo("`" + index.ToString());
            info.IsGenericInstance = true;
            return info;
        }

        public MemberMetadataInfo GetGenericInstance(MemberMetadataInfo genericType, ImmutableArray<MemberMetadataInfo> typeArguments)
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
            //not sure of the format of the output here
            //_isFunctionPointer = true;
            //return "method " + signature.ReturnType + "*" + GetParameterList(signature) + ")";
            throw new NotImplementedException("Function pointer");
        }

        public MemberMetadataInfo GetPinnedType(MemberMetadataInfo elementType)
        {
            elementType.Name += " pinned";
            return elementType;
        }

        public MemberMetadataInfo GetArrayType(MemberMetadataInfo elementType, ArrayShape shape)
        {
            elementType.IsArrayType = true;
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

        public MemberMetadataInfo GetModifiedType(MemberMetadataInfo unmodifiedType, ImmutableArray<CustomModifier<MemberMetadataInfo>> modifiers)
        {
            var builder = new StringBuilder();
            builder.Append('~');
            builder.Append(unmodifiedType.Name);

            foreach (var modifier in modifiers)
            {
                builder.Append(modifier.IsRequired ? "modreq(" : "modopt(");
                builder.Append(modifier.Type);
                builder.Append(')');
            }

            unmodifiedType.Name = builder.ToString();
            return unmodifiedType;
        }
    }
}
