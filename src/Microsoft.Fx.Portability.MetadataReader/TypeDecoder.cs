// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reflection.Metadata;

namespace Microsoft.Fx.Portability.Analyzer
{
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

			switch (memberReference.GetKind())
			{
			case MemberReferenceKind.Field:
				memberRefInfo.kind = MemberMetadataInfo.Kind.Field;
				break;
			case MemberReferenceKind.Method:
				memberRefInfo.kind = MemberMetadataInfo.Kind.Method;
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
	}
}
