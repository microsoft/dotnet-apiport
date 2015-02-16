// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Decoding;
using System.Text;

namespace Microsoft.Fx.Portability.Analyzer
{
    internal class MemberMetadataInfo
    {
        private string _name;
        private string _nameSpace = null;
        private List<string> _names = new List<string>();
        private MethodSignature<MemberMetadataInfo> _methodSignature;
        private MemberMetadataInfo _parentType = null;  //for memberRefs, the type is from the parent
        private AssemblyReferenceHandle _assembly;  //assembly where it is defined
        private bool _assemblySet = false;
        private ModuleReferenceHandle _module;
        private bool _isGenericInstance = false;
        private bool _isEnclosedType = false;
        private List<MemberMetadataInfo> _genericTypeArgs = null;
        private bool _isArrayType = false;
        private string _arrayTypeInfo;
        private bool _isTypeDef = false;
        private bool _isPrimitiveType = false;
        private Kind _kind = Kind.Type;
        public enum Kind
        {
            Type,
            Method,
            Field,
            Unknown
        }

        public MemberMetadataInfo ParentType
        {
            get
            {
                return _parentType;
            }
        }

        public AssemblyReferenceHandle DefinedInAssembly
        {
            get
            {
                return _assembly;
            }
        }

        public bool AssemblySet
        {
            get
            {
                return _assemblySet;
            }
        }

        public bool IsPrimitiveType
        {
            get
            {
                return _isPrimitiveType;
            }
        }

        public bool IsTypeDef
        {
            get
            {
                return _isTypeDef;
            }
        }

        public Kind ReferenceKind
        {
            get
            {
                return _kind;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (_kind == Kind.Method || _kind == Kind.Field)
            {
                _name = _name.Replace("<", "{").Replace(">", "}");
                //get the full name from the type
                sb.Append(_parentType.ToString());
                if (_parentType._isArrayType)
                {
                    sb.Append(_parentType._arrayTypeInfo);
                }
                sb.Append(".");

                if (_kind == Kind.Method)
                {
                    _name = _name.Replace(".", "#");  //expected output is #ctor instead of .ctor
                    if (_methodSignature.Header.IsGeneric)
                    {
                        if (_methodSignature.GenericParameterCount > 0)
                        {
                            _name += "``" + _methodSignature.GenericParameterCount;
                        }
                    }
                }
                sb.Append(_name);
                //add method signature 
                if (_kind == Kind.Method)
                {
                    if (_methodSignature.ParameterTypes.Count() > 0)
                    {
                        sb.Append("(");
                        _methodSignature.ParameterTypes[0]._isEnclosedType = true;
                        sb.Append(_methodSignature.ParameterTypes[0].ToString());

                        for (int i = 1; i < _methodSignature.ParameterTypes.Count(); i++)
                        {
                            sb.Append(",");
                            _methodSignature.ParameterTypes[i]._isEnclosedType = true;
                            sb.Append(_methodSignature.ParameterTypes[i].ToString());
                        }
                        sb.Append(")");
                    }
                }
            }
            else
            {
                if (_nameSpace != null)
                {
                    sb.Append(_nameSpace);
                    sb.Append(".");
                }

                List<string> displayNames = new List<string>(_names);
                displayNames.Add(_name);

                //add the type arguments for generic instances
                //example output: Hashtable{`0}.KeyValuePair
                //Go through all type names, if it is generic such as Hashtable`1 remove the '1 , look in the arguments list 
                //and put the list of arguments in between {}
                if (_isGenericInstance && _genericTypeArgs != null && _isEnclosedType)
                {
                    int index = 0;
                    for (int i = 0; i < displayNames.Count; i++)
                    {
                        int pos = displayNames[i].IndexOf('`');
                        if (pos > 0)
                        {
                            int numArgs;
                            bool success = Int32.TryParse(displayNames[i].Substring(pos + 1, 1), out numArgs);

                            if (success)
                            {
                                string substr1 = displayNames[i].Substring(0, pos);
                                string substr2 = "";
                                if (displayNames[i].Length > pos + 2)
                                    substr2 = displayNames[i].Substring(pos + 2);

                                displayNames[i] = substr1;
                                if (index + numArgs <= _genericTypeArgs.Count)
                                {
                                    List<MemberMetadataInfo> args = _genericTypeArgs.GetRange(index, numArgs);
                                    string argsList = "{" + String.Join(",", args) + "}";
                                    displayNames[i] += argsList;
                                }
                                else
                                {
                                    Console.WriteLine("error");
                                }
                                displayNames[i] += substr2;

                                //advance the index in the args list
                                index += numArgs;
                            }
                        }
                    }
                }


                for (int i = 0; i < displayNames.Count; i++)
                {
                    if (i > 0)
                        sb.Append(".");
                    sb.Append(displayNames[i]);
                }
            }
            return sb.ToString();
        }
        public MemberMetadataInfo(string name)
        {
            _name = name;
        }

        public void Join(MemberMetadataInfo info2)
        {
            _names.AddRange(info2._names);
            _names.Add(info2._name);
            if (info2._nameSpace != null)
                _nameSpace = info2._nameSpace;

            if (info2._assemblySet)
            {
                _assembly = info2._assembly;
                _assemblySet = true;
            }
        }

        public static MemberMetadataInfo GetFullName(TypeReference typeReference, MetadataReader reader)
        {
            TypeDecoder provider = new TypeDecoder(reader);
            return provider.GetFullName(typeReference);
        }

        public static MemberMetadataInfo GetMemberRefInfo(MemberReference memberReference, MetadataReader reader)
        {
            TypeDecoder provider = new TypeDecoder(reader);
            return provider.GetMemberRefInfo(memberReference);
        }

        private class TypeDecoder : ISignatureTypeProvider<MemberMetadataInfo>
        {
            private readonly MetadataReader _reader;
            public MetadataReader Reader
            {
                get { return _reader; }
            }

            public TypeDecoder(MetadataReader reader)
            {
                _reader = reader;
            }

            public MemberMetadataInfo GetMemberRefInfo(MemberReference memberReference)
            {
                string name = _reader.GetString(memberReference.Name);

                MemberMetadataInfo memberRefInfo = new MemberMetadataInfo(name);

                MemberMetadataInfo parentType = GetMemberParentInfo(memberReference);
                if (parentType == null)
                    return null;
                memberRefInfo._parentType = parentType;

                switch (memberReference.GetKind())
                {
                case MemberReferenceKind.Field:
                    memberRefInfo._kind = MemberMetadataInfo.Kind.Field;
                    break;
                case MemberReferenceKind.Method:
                    memberRefInfo._kind = MemberMetadataInfo.Kind.Method;
                    //get method signature
                    memberRefInfo._methodSignature = SignatureDecoder.DecodeMethodSignature<MemberMetadataInfo>(memberReference.Signature, this);
                    foreach (MemberMetadataInfo mInfo in memberRefInfo._methodSignature.ParameterTypes)
                    {
                        mInfo._isEnclosedType = true;
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
                    typeDefInfo._isTypeDef = true;
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
                info._isTypeDef = true;
                if (!type.Namespace.IsNil)
                {
                    info._nameSpace = Reader.GetString(type.Namespace);
                }
                return info;
            }

            private MemberMetadataInfo GetName(TypeReference reference)
            {
                string name = Reader.GetString(reference.Name);
                MemberMetadataInfo info = new MemberMetadataInfo(name);
                if (!reference.Namespace.IsNil)
                {
                    info._nameSpace = Reader.GetString(reference.Namespace);
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
                    name._module = moduleReference;
                    return name;

                case HandleKind.AssemblyReference:
                    if (!name._assemblySet)
                    {
                        name._assemblySet = true;
                        name._assembly = (AssemblyReferenceHandle)(scope);
                    }
                    return name;

                case HandleKind.TypeReference:
                    MemberMetadataInfo info2 = GetFullName(_reader.GetTypeReference((TypeReferenceHandle)(scope)));
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
                mInfo._isPrimitiveType = true;
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
                elementType._name += "[]";
                return elementType;
            }

            public MemberMetadataInfo GetPointerType(MemberMetadataInfo elementType)
            {
                elementType._name += "*";
                return elementType;
            }


            public MemberMetadataInfo GetByReferenceType(MemberMetadataInfo elementType)
            {
                elementType._name += "@";
                return elementType;
            }

            public MemberMetadataInfo GetGenericMethodParameter(int index)
            {
                // generic arguments on methods are prefixed with ``
                // type generic arguments are prefixed with '
                MemberMetadataInfo info = new MemberMetadataInfo("``" + index.ToString());
                info._isGenericInstance = true;
                return info;
            }

            public MemberMetadataInfo GetGenericTypeParameter(int index)
            {
                MemberMetadataInfo info = new MemberMetadataInfo("`" + index.ToString());
                info._isGenericInstance = true;
                return info;
            }

            public MemberMetadataInfo GetGenericInstance(MemberMetadataInfo genericType, ImmutableArray<MemberMetadataInfo> typeArguments)
            {
                genericType._isGenericInstance = true;
                genericType._genericTypeArgs = new List<MemberMetadataInfo>(typeArguments);
                foreach (MemberMetadataInfo mInfo in genericType._genericTypeArgs)
                {
                    mInfo._isEnclosedType = true;
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
                elementType._name += " pinned";
                return elementType;
            }

            public MemberMetadataInfo GetArrayType(MemberMetadataInfo elementType, ArrayShape shape)
            {
                elementType._isArrayType = true;
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
                elementType._arrayTypeInfo = builder.ToString();
                return elementType;
            }

            public MemberMetadataInfo GetModifiedType(MemberMetadataInfo unmodifiedType, ImmutableArray<CustomModifier<MemberMetadataInfo>> modifiers)
            {
                var builder = new StringBuilder();
                builder.Append('~');
                builder.Append(unmodifiedType._name);

                foreach (var modifier in modifiers)
                {
                    builder.Append(modifier.IsRequired ? "modreq(" : "modopt(");
                    builder.Append(modifier.Type);
                    builder.Append(')');
                }

                unmodifiedType._name = builder.ToString();
                return unmodifiedType;
            }
        }
    }
}
