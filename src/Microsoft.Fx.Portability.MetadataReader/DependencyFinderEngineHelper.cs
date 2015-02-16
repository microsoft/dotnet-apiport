// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;

namespace Microsoft.Fx.Portability.Analyzer
{
	internal class DependencyFinderEngineHelper
	{
		private readonly MetadataReader _reader;
		public AssemblyInfo CallingAssembly { get; private set; }
		public List<MemberDependency> memberDependency = new List<MemberDependency>();
		private string _currentAssemblyInfo;
		private string _currentAssemblyName;
		private readonly string _assemblyLocation;
		public DependencyFinderEngineHelper(MetadataReader metadataReader, string assemblyPath)
		{
			_reader = metadataReader;
			_assemblyLocation = assemblyPath;
		}

		public void ComputeData()
		{
			if (_reader != null)
			{
				//get assembly info
				CallingAssembly = GetAssemblyInfo();
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
			Assembly assembly = Assembly.ReflectionOnlyLoadFrom(_assemblyLocation);

			foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
			{
				try
				{
					Assembly.ReflectionOnlyLoad(referencedAssemblyName.FullName);
				}
				catch
				{
					Assembly.ReflectionOnlyLoadFrom(Path.Combine(Path.GetDirectoryName(_assemblyLocation), referencedAssemblyName.Name + ".dll"));
				}
			}

			AssemblyName assemblyName = assembly.GetName();

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
			MemberMetadataInfo typeRefinfo = MemberMetadataInfo.GetFullName(typeReference, _reader);
			return CreateMemberDependency(typeRefinfo);
		}

		private MemberDependency CreateMemberDependency(MemberMetadataInfo type)
		{
			MemberDependency dep = new MemberDependency();
			dep.CallingAssembly = CallingAssembly;

			dep.MemberDocId = "T:" + type.ToString(); ;

			if (type.AssemblySet)
				dep.DefinedInAssemblyIdentity = GetAssemblyInfoFromHandle(type.DefinedInAssembly);
			else
				dep.DefinedInAssemblyIdentity = _currentAssemblyInfo;

			return dep;
		}

		private MemberDependency GetMemberReferenceMemberDependency(MemberReference memberReference)
		{
			MemberDependency dep = new MemberDependency();
			MemberMetadataInfo memberRefInfo = MemberMetadataInfo.GetMemberRefInfo(memberReference, _reader);

			if (memberRefInfo == null)
				return null;

			dep.CallingAssembly = CallingAssembly;

			//MemberReferenceKind can be Method or Field
			string kind;
			if (memberReference.GetKind() == MemberReferenceKind.Field)
				kind = "F:";
			else if (memberReference.GetKind() == MemberReferenceKind.Method)
				kind = "M:";
			else
				kind = memberReference.GetKind().ToString();

			dep.MemberDocId = kind + memberRefInfo.ToString();
			dep.TypeDocId = "T:" + memberRefInfo.ParentType.ToString();

			if (memberRefInfo.ParentType.AssemblySet)
				dep.DefinedInAssemblyIdentity = GetAssemblyInfoFromHandle(memberRefInfo.ParentType.DefinedInAssembly);
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
			_currentAssemblyName = _reader.GetString(entry.Name);
			return _currentAssemblyName + ", Version=" + entry.Version.Major + "." + entry.Version.Minor + "." + entry.Version.Build + "." + entry.Version.Revision + ", Culture=" + culture + ", PublicKeyToken=" + publickeytoken;
		}

		private string Literal_PublicKeyToken(BlobHandle handle)
		{
			byte[] bytes = _reader.GetBlobBytes(handle);
			if (bytes.Length > 8)  //strong named assembly
			{
				//get the public key token, which is the last 8 bytes of the SHA-1 hash of the public key 
				SHA1 sha1 = SHA1.Create();
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
			string removeDashesLowerCase = value.Replace("-", "").ToLowerInvariant();

			return string.Format("{0:x}", removeDashesLowerCase);
		}
	}
}
