// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection.Metadata;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Microsoft.Fx.Portability.Analyzer
{
	public class MemberMetadataInfo
	{
		public string name;
		public string nameSpace = null;
		public List<string> names = new List<string>();
		public bool definedInCallingAssembly = false;
		public MemberMetadataInfo parentType = null;  //for memberRefs, the type is from the parent
		public AssemblyReferenceHandle assembly;   //assembly where it is defined
		public bool assemblySet = false;
		public ModuleReferenceHandle module;
		public bool moduleSet = false;
		public bool isGenericTypeParameter = false;
		public bool isGenericMethodParameter = false;
		public bool isGenericInstance = false;
		public bool isEnclosedType = false;
		public List<MemberMetadataInfo> genericTypeArgs = null;
		public bool isArrayType = false;
		public bool isSzArrayType = false;
		public string arrayTypeInfo;
		public bool IsTypeDef = false;
		public Kind kind = Kind.Type;
		public enum Kind
		{
			Type,
			Method,
			Field,
			Unknown
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			if (kind == Kind.Method || kind == Kind.Field)
			{
				name = name.Replace("<", "{").Replace(">", "}");
				//get the full name from the type
				sb.Append(parentType.ToString());
				if (parentType.isArrayType)
				{
					sb.Append(parentType.arrayTypeInfo);
				}
				sb.Append(".");

				if (kind == Kind.Method)
				{
					name = name.Replace(".", "#");	//expected output is #ctor instead of .ctor
				}
				sb.Append(name);
				//To do: add method signature 
			}
			else
			{
				if (nameSpace != null)
				{
					sb.Append(nameSpace);
					sb.Append(".");
				}

				List<string> displayNames = new List<string>(names);
				displayNames.Add(name);

				//add the type arguments for generic instances
				//example output: Hashtable{`0}.KeyValuePair
				//Go through all type names, if it is generic such as Hashtable`1 remove the '1 , look in the arguments list 
				//and put the list of arguments in between {}
				if (isGenericInstance && genericTypeArgs != null && isEnclosedType)
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
								if (index + numArgs <= genericTypeArgs.Count)
								{
									List<MemberMetadataInfo> args = genericTypeArgs.GetRange(index, numArgs);
									string argsList = "{" + String.Join(",", args) + "}";
									displayNames[i] += argsList;
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
			this.name = name;
		}

		public void Join(MemberMetadataInfo info2)
		{
			names.AddRange(info2.names);
			names.Add(info2.name);
			if (info2.nameSpace != null)
				nameSpace = info2.nameSpace;

			if (info2.assemblySet)
			{
				assembly = info2.assembly;
				assemblySet = true;
			}
		}
	}
	public class TypeDecoder
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

		public MemberMetadataInfo Join(MemberMetadataInfo info1, MemberMetadataInfo info2)
		{
			//info1 name should be last
			info1.Join(info2);
			return info1;
		}

		public MemberMetadataInfo GetMemberRefInfo(MemberReference memberReference)
		{
			string name = _reader.GetString(memberReference.Name);

			MemberMetadataInfo memberRefInfo = new MemberMetadataInfo(name);

			MemberMetadataInfo parentType = GetMemberParentInfo(memberReference);
			if (parentType == null)
				return null;
			memberRefInfo.parentType = parentType;

			if (memberReference.GetKind() == MemberReferenceKind.Field)
				memberRefInfo.kind = MemberMetadataInfo.Kind.Field;
			else if (memberReference.GetKind() == MemberReferenceKind.Method)
				memberRefInfo.kind = MemberMetadataInfo.Kind.Method;
			else
			{
				//Console.Error.WriteLine("Member reference is of kind: " + memberReference.GetKind().ToString());
				return null;
			}

			if (memberReference.GetKind() == MemberReferenceKind.Method)
			{
				//to do: get method signature
			}
			return memberRefInfo;
		}

		private MemberMetadataInfo GetMemberParentInfo(MemberReference memberReference)
		{
			Handle parent = memberReference.Parent;
			if (parent.Kind.CompareTo(HandleKind.TypeReference) == 0)	//get the typeref parent of memberRef
			{
				return GetFullName((TypeReferenceHandle)parent);
			}
			else if (parent.Kind.CompareTo(HandleKind.TypeDefinition) == 0)	  //get the typedef parent of memberRef
			{
				MemberMetadataInfo typeDefInfo = GetFullName((TypeDefinitionHandle)parent);
				typeDefInfo.IsTypeDef = true;
				return typeDefInfo;
			}
			else if (parent.Kind.CompareTo(HandleKind.TypeSpecification) == 0)	 //get the typeref parent of memberRef
			{
				//decoding signature blob not implemented
				return null;
			}
			else
			{
				//Console.Error.WriteLine("MemberReference parent is of kind " + memberReference.Parent.Kind);
				return null;
			}
		}

		public MemberMetadataInfo GetName(TypeDefinitionHandle handle)
		{
			TypeDefinition type = Reader.GetTypeDefinition(handle);
			return GetName(type);
		}

		public MemberMetadataInfo GetName(TypeDefinition type)
		{
			string name = Reader.GetString(type.Name);
			MemberMetadataInfo info = new MemberMetadataInfo(name);
			info.IsTypeDef = true;
			if (type.Namespace.IsNil)
			{
				return info;
			}
			info.nameSpace = Reader.GetString(type.Namespace);
			return info;
		}

		public MemberMetadataInfo GetName(TypeReferenceHandle handle)
		{
			TypeReference reference = Reader.GetTypeReference(handle);
			return GetName(reference);
		}

		public MemberMetadataInfo GetName(TypeReference reference)
		{
			string name = Reader.GetString(reference.Name);
			if (reference.Namespace.IsNil)
			{
				return new MemberMetadataInfo(name);
			}
			MemberMetadataInfo info = new MemberMetadataInfo(name);
			info.nameSpace = Reader.GetString(reference.Namespace);
			return info;
		}

		public MemberMetadataInfo GetFullName(TypeDefinitionHandle handle)
		{
			TypeDefinition definition = Reader.GetTypeDefinition(handle);
			return GetFullName(definition);
		}

		public MemberMetadataInfo GetFullName(TypeDefinition type)
		{
			TypeDefinitionHandle declaringTypeHandle = type.GetDeclaringType();

			if (declaringTypeHandle.IsNil)
			{
				return GetName(type);
			}

			return Join(GetName(type), GetFullName(declaringTypeHandle));
		}

		public MemberMetadataInfo GetFullName(TypeReferenceHandle handle)
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
				name.module = moduleReference;
				name.moduleSet = true;
				return name;

			case HandleKind.AssemblyReference:
				if (!name.assemblySet)
					name.assemblySet = true;
				name.assembly = (AssemblyReferenceHandle)(scope);
				return name;

			case HandleKind.TypeReference:
				return Join(name, GetFullName(_reader.GetTypeReference((TypeReferenceHandle)(scope))));

			default:
				// These cases are rare
				Debug.Assert(scope.IsNil || scope == Handle.ModuleDefinition);
				return name;
			}
		}

		public MemberMetadataInfo GetTypeFromDefinition(TypeDefinitionHandle handle)
		{
			return GetFullName(handle);
		}

		public MemberMetadataInfo GetTypeFromReference(TypeReferenceHandle handle)
		{
			return GetFullName(handle);
		}

		public MemberMetadataInfo GetSZArrayType(string elementType)
		{
			return new MemberMetadataInfo(elementType + "[]");
		}

		public MemberMetadataInfo GetSZArrayType(MemberMetadataInfo elementType)
		{
			elementType.isSzArrayType = true;
			elementType.name += "[]";
			return elementType;
		}

		public MemberMetadataInfo GetPointerType(MemberMetadataInfo elementType)
		{
			elementType.name += "*";
			return elementType;
		}

		public MemberMetadataInfo GetByReferenceType(MemberMetadataInfo elementType)
		{
			elementType.name += "@";
			return elementType;
		}

		public MemberMetadataInfo GetGenericMethodParameter(int index)
		{
			MemberMetadataInfo info = new MemberMetadataInfo("``" + index.ToString());
			info.isGenericInstance = true;
			info.isGenericMethodParameter = true;
			return info;
		}

		public MemberMetadataInfo GetGenericTypeParameter(int index)
		{
			MemberMetadataInfo info = new MemberMetadataInfo("`" + index.ToString());
			info.isGenericInstance = true;
			info.isGenericTypeParameter = true;
			return info;
		}

		public MemberMetadataInfo GetGenericInstance(MemberMetadataInfo genericType, ImmutableArray<MemberMetadataInfo> typeArguments)
		{
			genericType.isGenericInstance = true;
			genericType.genericTypeArgs = new List<MemberMetadataInfo>(typeArguments);
			foreach (MemberMetadataInfo mInfo in genericType.genericTypeArgs)
			{
				mInfo.isEnclosedType = true;
			}
			return genericType;
		}
	}
}
