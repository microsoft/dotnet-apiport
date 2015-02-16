// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text;

namespace Microsoft.Fx.Portability.Analyzer
{
	public class MemberMetadataInfo
	{
		private string _name;
		private string _nameSpace = null;
		private List<string> _names = new List<string>();
		private MethodSignature<MemberMetadataInfo> _methodSignature;
		private MemberMetadataInfo _parentType = null;  //for memberRefs, the type is from the parent
		private AssemblyReferenceHandle _assembly;	//assembly where it is defined
		private bool _assemblySet = false;
		private ModuleReferenceHandle _module;
		private bool _moduleSet = false;
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
				sb.Append(".");

				if (_kind == Kind.Method)
				{
					_name = _name.Replace(".", "#");	//expected output is #ctor instead of .ctor
				}
				sb.Append(_name);
				//To do: add method signature 
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
				memberRefInfo._parentType = parentType;

				switch (memberReference.GetKind())
				{
				case MemberReferenceKind.Field:
					memberRefInfo._kind = MemberMetadataInfo.Kind.Field;
					break;
				case MemberReferenceKind.Method:
					memberRefInfo._kind = MemberMetadataInfo.Kind.Method;
					//to do: get method signature
					break;
				default:
					memberRefInfo = null;
					break;
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

			public MemberMetadataInfo GetName(TypeDefinition type)
			{
				string name = Reader.GetString(type.Name);
				MemberMetadataInfo info = new MemberMetadataInfo(name);
				if (type.Namespace.IsNil)
				{
					return info;
				}
				info._nameSpace = Reader.GetString(type.Namespace);
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
				info._nameSpace = Reader.GetString(reference.Namespace);
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
					name._module = moduleReference;
					name._moduleSet = true;
					return name;

				case HandleKind.AssemblyReference:
					if (!name._assemblySet)
						name._assemblySet = true;
					name._assembly = (AssemblyReferenceHandle)(scope);
					return name;

				case HandleKind.TypeReference:
					return Join(name, GetFullName(_reader.GetTypeReference((TypeReferenceHandle)(scope))));

				default:
					// These cases are rare
					Debug.Assert(scope.IsNil || scope == Handle.ModuleDefinition);
					return name;
				}
			}
		}
	}
}
