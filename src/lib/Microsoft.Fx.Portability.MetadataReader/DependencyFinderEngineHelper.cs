// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace Microsoft.Fx.Portability.Analyzer
{
    internal class DependencyFinderEngineHelper
    {
        private readonly IDependencyFilter _assemblyFilter;
        private readonly MetadataReader _reader;
        private readonly SystemObjectFinder _objectFinder;
        private readonly AssemblyReferenceInformation _currentAssemblyInfo;
        private readonly string _currentAssemblyName;

        public DependencyFinderEngineHelper(IDependencyFilter assemblyFilter, MetadataReader metadataReader, IAssemblyFile file, SystemObjectFinder objectFinder)
        {
            _assemblyFilter = assemblyFilter;
            _reader = metadataReader;
            _objectFinder = objectFinder;

            MemberDependency = new List<MemberDependency>();
            CallingAssembly = new AssemblyInfo
            {
                Location = file.Name,
                AssemblyIdentity = metadataReader.FormatAssemblyInfo().ToString(),
                FileVersion = file.Version ?? string.Empty,
                TargetFrameworkMoniker = metadataReader.GetTargetFrameworkMoniker() ?? string.Empty,
                AssemblyReferences = ComputeAssemblyReferences(metadataReader)
            };

            // Get assembly info
            var assemblyDefinition = _reader.GetAssemblyDefinition();

            _currentAssemblyInfo = _reader.FormatAssemblyInfo(assemblyDefinition);
            _currentAssemblyName = _reader.GetString(assemblyDefinition.Name);
        }

        private IList<AssemblyReferenceInformation> ComputeAssemblyReferences(MetadataReader metadataReader)
        {
            var refs = new List<AssemblyReferenceInformation>();
            foreach (var handle in _reader.AssemblyReferences)
            {
                try
                {
                    var entry = _reader.GetAssemblyReference(handle);

                    refs.Add(metadataReader.FormatAssemblyInfo(entry));
                }
                catch (BadImageFormatException)
                {
                }
            }

            return refs;
        }

        public AssemblyInfo CallingAssembly { get; }

        public IList<MemberDependency> MemberDependency { get; }

        public void ComputeData()
        {
            var provider = new MemberMetadataInfoTypeProvider(_reader);

            // Get type references
            foreach (var handle in _reader.TypeReferences)
            {
                try
                {
                    var entry = _reader.GetTypeReference(handle);
                    var typeInfo = provider.GetFullName(entry);
                    var assembly = GetAssembly(typeInfo);
                    var typeReferenceMemberDependency = CreateMemberDependency(typeInfo, assembly);

                    if (typeReferenceMemberDependency != null)
                    {
                        MemberDependency.Add(typeReferenceMemberDependency);
                    }
                }
                catch (BadImageFormatException)
                {
                    // Some obfuscators will inject dead types that break decompilers
                    // (for example, types that serve as each others' scopes).
                    //
                    // For portability/compatibility analysis purposes, though,
                    // we can skip such malformed references and just analyze those
                    // that we can successfully decode.
                }
            }

            if (_reader.MemberReferences.Any())
            {
                // Primitives need to have their assembly set, so we search for a
                // reference to System.Object that is considered a possible
                // framework assembly and use that for any primitives that don't
                // have an assembly
                if (_objectFinder.TryGetSystemRuntimeAssemblyInformation(_reader, out var systemObjectAssembly))
                {
                    // Get member references
                    foreach (var handle in _reader.MemberReferences)
                    {
                        try
                        {
                            var entry = _reader.GetMemberReference(handle);

                            var memberReferenceMemberDependency = GetMemberReferenceMemberDependency(entry, systemObjectAssembly);
                            if (memberReferenceMemberDependency != null)
                            {
                                MemberDependency.Add(memberReferenceMemberDependency);
                            }
                        }
                        catch (BadImageFormatException)
                        {
                            // Some obfuscators will inject dead types that break decompilers
                            // (for example, types that serve as each others' scopes).
                            //
                            // For portability/compatibility analysis purposes, though,
                            // we can skip such malformed references and just analyze those
                            // that we can successfully decode.
                        }
                    }
                }
                else
                {
                    throw new SystemObjectNotFoundException();
                }
            }
        }

        private AssemblyReferenceInformation GetAssembly(MemberMetadataInfo type)
        {
            return type.DefinedInAssembly.HasValue ? _reader.FormatAssemblyInfo(type.DefinedInAssembly.Value) : _currentAssemblyInfo;
        }

        private MemberDependency CreateMemberDependency(MemberMetadataInfo type)
        {
            return CreateMemberDependency(type, GetAssembly(type));
        }

        private MemberDependency CreateMemberDependency(MemberMetadataInfo type, AssemblyReferenceInformation definedInAssembly)
        {
            // Apply heuristic to determine if API is most likely defined in a framework assembly
            if (!_assemblyFilter.IsFrameworkMember(definedInAssembly))
            {
                return null;
            }

            return new MemberDependency
            {
                CallingAssembly = CallingAssembly,
                MemberDocId = FormattableString.Invariant($"T:{type}"),
                DefinedInAssemblyIdentity = definedInAssembly
            };
        }

        private MemberDependency GetMemberReferenceMemberDependency(MemberReference memberReference, AssemblyReferenceInformation systemObjectAssembly)
        {
            var provider = new MemberMetadataInfoTypeProvider(_reader);
            var memberRefInfo = provider.GetMemberRefInfo(memberReference);

            AssemblyReferenceInformation definedInAssemblyIdentity = null;
            if (memberRefInfo.ParentType.DefinedInAssembly.HasValue)
            {
                definedInAssemblyIdentity = _reader.FormatAssemblyInfo(memberRefInfo.ParentType.DefinedInAssembly.Value);
            }
            else if (memberRefInfo.ParentType.IsPrimitiveType)
            {
                definedInAssemblyIdentity = systemObjectAssembly;
            }
            else
            {
                definedInAssemblyIdentity = _currentAssemblyInfo;
            }

            // Apply heuristic to determine if API is most likely defined in a framework assembly
            if (!_assemblyFilter.IsFrameworkMember(definedInAssemblyIdentity))
            {
                return null;
            }

            // Add the parent type to the types list (only needed when we want to report memberrefs defined in the current assembly)
            if (memberRefInfo.ParentType.IsTypeDef || (memberRefInfo.ParentType.IsPrimitiveType && _currentAssemblyName.Equals("mscorlib", StringComparison.OrdinalIgnoreCase)))
            {
                var memberDependency = CreateMemberDependency(memberRefInfo.ParentType);

                if (memberDependency != null)
                {
                    MemberDependency.Add(memberDependency);
                }
            }

            return new MemberDependency
            {
                CallingAssembly = CallingAssembly,
                MemberDocId = FormattableString.Invariant($"{GetPrefix(memberReference)}:{memberRefInfo}"),
                TypeDocId = FormattableString.Invariant($"T:{memberRefInfo.ParentType}"),
                IsPrimitive = memberRefInfo.ParentType.IsPrimitiveType,
                DefinedInAssemblyIdentity = definedInAssemblyIdentity
            };
        }

        private static string GetPrefix(MemberReference memberReference)
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
