// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Microsoft.Fx.Portability.Analyzer
{
    internal class DependencyFinderEngineHelper
    {
        private readonly MetadataReader _reader;
        private readonly string _assemblyLocation;

        private readonly string _currentAssemblyInfo;
        private readonly string _currentAssemblyName;
        
        public DependencyFinderEngineHelper(MetadataReader metadataReader, string assemblyPath)
        {
            _reader = metadataReader;
            _assemblyLocation = assemblyPath;

            MemberDependency = new List<MemberDependency>();
            CallingAssembly = _reader.GetAssemblyInfo(assemblyPath);

            // Get assembly info
            var assemblyDefinition = _reader.GetAssemblyDefinition();

            _currentAssemblyInfo = _reader.FormatAssemblyInfo(assemblyDefinition);
            _currentAssemblyName = _reader.GetString(assemblyDefinition.Name);
        }

        public AssemblyInfo CallingAssembly { get; }

        public IList<MemberDependency> MemberDependency { get; }

        public void ComputeData()
        {
            // Get type references
            foreach (var handle in _reader.TypeReferences)
            {
                var entry = _reader.GetTypeReference(handle);

                var typeReferenceMemberDependency = GetTypeReferenceMemberDependency(entry);
                if (typeReferenceMemberDependency != null)
                {
                    MemberDependency.Add(typeReferenceMemberDependency);
                }
            }

            // Get member references
            foreach (var handle in _reader.MemberReferences)
            {
                var entry = _reader.GetMemberReference(handle);

                var memberReferenceMemberDependency = GetMemberReferenceMemberDependency(entry);
                if (memberReferenceMemberDependency != null)
                {
                    this.MemberDependency.Add(memberReferenceMemberDependency);
                }
            }
        }

        private MemberDependency GetTypeReferenceMemberDependency(TypeReference typeReference)
        {
            var provider = new MemberMetadataInfoTypeProvider(_reader);
            var typeInfo = provider.GetFullName(typeReference);

            return CreateMemberDependency(typeInfo);
        }

        private MemberDependency CreateMemberDependency(MemberMetadataInfo type)
        {
            return new MemberDependency
            {
                CallingAssembly = CallingAssembly,
                MemberDocId = $"T:{type}",
                DefinedInAssemblyIdentity = type.IsAssemblySet ? _reader.FormatAssemblyInfo(type.DefinedInAssembly) : _currentAssemblyInfo
            };
        }

        private MemberDependency GetMemberReferenceMemberDependency(MemberReference memberReference)
        {
            var provider = new MemberMetadataInfoTypeProvider(_reader);
            var memberRefInfo = provider.GetMemberRefInfo(memberReference);

            // Add the parent type to the types list (only needed when we want to report memberrefs defined in the current assembly)
            if (memberRefInfo.ParentType.IsTypeDef || (memberRefInfo.ParentType.IsPrimitiveType && _currentAssemblyName.Equals("mscorlib", StringComparison.OrdinalIgnoreCase)))
            {
                MemberDependency.Add(CreateMemberDependency(memberRefInfo.ParentType));
            }

            var dep = new MemberDependency
            {
                CallingAssembly = CallingAssembly,
                MemberDocId = $"{GetPrefix(memberReference)}:{memberRefInfo}",
                TypeDocId = $"T:{memberRefInfo.ParentType}",
                IsPrimitive = memberRefInfo.ParentType.IsPrimitiveType
            };

            if (memberRefInfo.ParentType.IsAssemblySet)
            {
                dep.DefinedInAssemblyIdentity = _reader.FormatAssemblyInfo(memberRefInfo.ParentType.DefinedInAssembly);
            }
            // If no assembly is set, then the type is either a primitive type or it's in the current assembly.
            // Mscorlib is special-cased for testing purposes.
            else if (!memberRefInfo.ParentType.IsPrimitiveType || string.Equals(_currentAssemblyName, "mscorlib", StringComparison.OrdinalIgnoreCase))
            {
                dep.DefinedInAssemblyIdentity = _currentAssemblyInfo;
            }

            return dep;
        }

        private string GetPrefix(MemberReference memberReference)
        {
            switch (memberReference.GetKind())
            {
                case MemberReferenceKind.Field:
                    return "F";
                case MemberReferenceKind.Method:
                    return "M";
                default:
                    return memberReference.GetKind().ToString();
            }
        }
    }
}
