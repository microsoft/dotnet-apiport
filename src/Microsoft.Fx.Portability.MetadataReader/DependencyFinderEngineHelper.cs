// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Reflection.Metadata;

namespace Microsoft.Fx.Portability.Analyzer
{
	internal class DependencyFinderEngineHelper
	{
		private MetadataReader _reader;
		public AssemblyInfo callingAssembly;
		public List<MemberDependency> memberDependency = new List<MemberDependency>();
		private string _currentAssemblyInfo;
		private string _currentAssemblyname;
		private string _assemblyLocation;
		public DependencyFinderEngineHelper(MetadataReader metadatareader, string assemblyPath)
		{
			_reader = metadatareader;
			_assemblyLocation = assemblyPath;
		}

		public void ComputeData()
		{
			if (_reader != null)
			{
				//get assembly info
				callingAssembly = GetAssemblyInfo();
				_currentAssemblyInfo = GetCurrentAssemblyInfo();

				//get type references
				foreach (var handle in _reader.TypeReferences)
				{
					var entry = _reader.GetTypeReference(handle);

					MemberDependency dep = GetTypeReferenceMemberDependency(entry);
					if (dep != null)
						memberDependency.Add(dep);
				}


				//get member references
				foreach (var handle in _reader.MemberReferences)
				{
					var entry = _reader.GetMemberReference(handle);

					MemberDependency dep = GetMemberReferenceMemberDependency(entry);
					if (dep != null)
						memberDependency.Add(dep);
				}
			}
		}

		private AssemblyInfo GetAssemblyInfo()
		{
			AssemblyInfo assemblyInfo = new AssemblyInfo();
			System.Reflection.Assembly assembly = System.Reflection.Assembly.ReflectionOnlyLoadFrom(_assemblyLocation);

			foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
			{
				try
				{
					System.Reflection.Assembly.ReflectionOnlyLoad(referencedAssemblyName.FullName);
				}
				catch
				{
					System.Reflection.Assembly.ReflectionOnlyLoadFrom(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(_assemblyLocation), referencedAssemblyName.Name + ".dll"));
				}
			}

			System.Reflection.AssemblyName assemblyName = assembly.GetName();

			//get file version
			FileVersionInfo fileInfo = FileVersionInfo.GetVersionInfo(_assemblyLocation);
			assemblyInfo.FileVersion = fileInfo.FileVersion;
			assemblyInfo.AssemblyIdentity = assemblyName.ToString();

			//get target framework from custom attributes
			var customAttributes = assembly.CustomAttributes;
			string targetFramework = "";
			foreach (System.Reflection.CustomAttributeData attribute in customAttributes)
			{
				string name = attribute.AttributeType.Name;
				if (attribute.AttributeType.Name.Contains("TargetFrameworkAttribute"))
				{
					for (int i = 0; i < attribute.ConstructorArguments.Count; i++)
					{
						if (i > 0)
							targetFramework += ", ";
						targetFramework += attribute.ConstructorArguments[i].ToString().Trim(new char[] { '\"' });
					}
					break;
				}
			}
			assemblyInfo.TargetFrameworkMoniker = targetFramework;
			return assemblyInfo;
		}

		private MemberDependency GetTypeReferenceMemberDependency(TypeReference typeReference)
		{
			TypeDecoder provider = new TypeDecoder(_reader);
			MemberMetadataInfo typeRefinfo = provider.GetFullName(typeReference);
			return CreateMemberDependency(typeRefinfo);
		}

		private MemberDependency CreateMemberDependency(MemberMetadataInfo type)
		{
			MemberDependency dep = new MemberDependency();
			dep.CallingAssembly = callingAssembly;

			dep.MemberDocId = "T:" + type.ToString(); ;

			if (type.assemblySet)
				dep.DefinedInAssemblyIdentity = GetAssemblyInfoFromHandle(type.assembly);
			else
				dep.DefinedInAssemblyIdentity = _currentAssemblyInfo;

			return dep;
		}

		private MemberDependency GetMemberReferenceMemberDependency(MemberReference memberReference)
		{
			MemberDependency dep = new MemberDependency();
			TypeDecoder provider = new TypeDecoder(_reader);
			MemberMetadataInfo memberRefInfo = provider.GetMemberRefInfo(memberReference);

			if (memberRefInfo == null)
				return null;

			dep.CallingAssembly = callingAssembly;

			//MemberReferenceKind can be Method or Field
			string kind;
			if (memberReference.GetKind() == MemberReferenceKind.Field)
				kind = "F:";
			else if (memberReference.GetKind() == MemberReferenceKind.Method)
				kind = "M:";
			else
				kind = memberReference.GetKind().ToString();

			dep.MemberDocId = kind + memberRefInfo.ToString();
			dep.TypeDocId = "T:" + memberRefInfo.parentType.ToString();

			if (memberRefInfo.parentType.assemblySet)
				dep.DefinedInAssemblyIdentity = GetAssemblyInfoFromHandle(memberRefInfo.parentType.assembly);
			else
				dep.DefinedInAssemblyIdentity = _currentAssemblyInfo;

			return dep;
		}

		private string GetAssemblyInfoFromHandle(AssemblyReferenceHandle assemblyHandle)
		{
			AssemblyReference entry = _reader.GetAssemblyReference(assemblyHandle);
			string culture = "neutral";
			if (!entry.Culture.IsNil)
				culture = _reader.GetString(entry.Culture);

			string publickeytoken = "null";
			if (!entry.PublicKeyOrToken.IsNil)
				publickeytoken = Literal_PublicKeyToken(entry.PublicKeyOrToken);
			return _reader.GetString(entry.Name) + ", Version=" + entry.Version.Major + "." + entry.Version.Minor + "." + entry.Version.Build + "." + entry.Version.Revision + ", Culture=" + culture + ", PublicKeyToken=" + publickeytoken;
		}

		private string GetCurrentAssemblyInfo()
		{
			AssemblyDefinition entry = _reader.GetAssemblyDefinition();
			string culture = "neutral";
			if (!entry.Culture.IsNil)
				culture = _reader.GetString(entry.Culture);

			string publickeytoken = "null";
			if (!entry.PublicKey.IsNil)
				publickeytoken = Literal_PublicKeyToken(entry.PublicKey);
			_currentAssemblyname = _reader.GetString(entry.Name);
			return _currentAssemblyname + ", Version=" + entry.Version.Major + "." + entry.Version.Minor + "." + entry.Version.Build + "." + entry.Version.Revision + ", Culture=" + culture + ", PublicKeyToken=" + publickeytoken;
		}

		private string Literal_PublicKeyToken(BlobHandle handle)
		{
			byte[] bytes = _reader.GetBlobBytes(handle);
			if (bytes.Length > 8)  //strong named assembly
			{
				//get the public key token, which is the last 8 bytes of the SHA-1 hash of the public key 
				System.Security.Cryptography.SHA1 sha1 = System.Security.Cryptography.SHA1.Create();
				byte[] token = sha1.ComputeHash(bytes);
				bytes = new byte[8];
				int count = 0;
				for (int i = token.Length - 1; i >= token.Length - 8; i--)
				{
					bytes[count] = token[i];
					count++;
				}
			}
			return FormatPublicKeyToken(bytes);
		}

		private string FormatPublicKeyToken(byte[] bytes)
		{
			if (bytes == null || bytes.Length <= 0)
				return "null";

			string value = BitConverter.ToString(bytes);

			//remove dashes
			string[] split = value.Split(new char[] { '-' });
			string newValue = "";
			foreach (string s in split)
			{
				newValue += s.ToLower();
			}
			return string.Format("{0:x}", newValue);
		}
	}
}
