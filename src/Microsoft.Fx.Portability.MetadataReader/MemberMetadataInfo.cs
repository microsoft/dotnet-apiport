// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

namespace Microsoft.Fx.Portability.Analyzer
{
    internal class MemberMetadataInfo
    {
        public MemberMetadataInfo(string name)
        {
            Name = name;
            Kind = MemberKind.Type;
            Names = new List<string>();
        }

        public List<string> Names { get; set; }

        public MethodSignature<MemberMetadataInfo> MethodSignature { get; set; }

        public ModuleReferenceHandle Module { get; set; }

        public List<MemberMetadataInfo> GenericTypeArgs { get; set; }

        public bool IsArrayType { get; set; }

        public string ArrayTypeInfo { get; set; }

        public bool IsTypeDef { get; set; }

        public bool IsPrimitiveType { get; set; }

        public MemberKind Kind { get; set; }

        public bool IsGenericInstance { get; set; }

        public bool IsEnclosedType { get; set; }

        public string Name { get; set; }

        public string Namespace { get; set; }

        public MemberMetadataInfo ParentType { get; set; }

        public AssemblyReference DefinedInAssembly { get; set; }

        public bool IsAssemblySet { get; set; }

        public override string ToString()
        {
            if (Kind == MemberKind.Method || Kind == MemberKind.Field)
            {
                return GenerateMemberDocId();
            }
            else
            {
                return GenerateTypeDocId();
            }
        }

        private string GenerateTypeDocId()
        {
            var sb = new StringBuilder();

            if (Namespace != null)
            {
                sb.Append(Namespace);
                sb.Append(".");
            }

            var combinedNames = new List<string>(Names) { Name };
            var displayNames = IsGenericInstance && GenericTypeArgs != null && IsEnclosedType
                ? GetGenericDisplayNames(combinedNames) : combinedNames;

            sb.Append(string.Join(".", displayNames));

            return sb.ToString();
        }

        /// <summary>
        /// Add the type arguments for generic instances. Go through all type names, and if it is generic, such 
        /// as Hashtable`1, remove the `1. Look in the arguments list and put the list of arguments in between {}
        ///
        /// Example: Hashtable{`0}.KeyValuePair 
        /// </summary>
        private IEnumerable<string> GetGenericDisplayNames(IEnumerable<string> displayNames)
        {
            // The most outputs when run on mscorlib are under 50 bytes in length
            const int SB_CAPACITY = 50;

            int index = 0;
            foreach (var displayName in displayNames)
            {
                int pos = displayName.IndexOf('`');
                if (pos <= 0)
                {
                    yield return displayName;
                    continue;
                }

                int numGenericArgs;
                // TODO: Fix this to work with >9 generic arguments 
                if (!int.TryParse(displayName.Substring(pos + 1, 1), out numGenericArgs))
                {
                    yield return displayName;
                    continue;
                }

                Debug.Assert(index + numGenericArgs <= GenericTypeArgs.Count, "index + numGenericArgs is too large");

                // Start with the name up to the generic parameter token
                var sb = new StringBuilder(displayName, 0, pos, SB_CAPACITY);

                // Add the generic parameters
                sb.Append("{");
                sb.Append(string.Join(",", GenericTypeArgs.GetRange(index, numGenericArgs)));
                sb.Append("}");

                // Add any part that was after the generic entry
                if (displayName.Length > pos + 2)
                {
                    var start = pos + 2;
                    var length = displayName.Length - start;

                    sb.Append(displayName, start, length);
                }

                yield return sb.ToString();

                // Advance the index in the args list
                index += numGenericArgs;
            }
        }

        private string GenerateMemberDocId()
        {
            var sb = new StringBuilder();

            // Get the full name from the type
            sb.Append(ParentType.ToString());

            if (ParentType.IsArrayType)
            {
                sb.Append(ParentType.ArrayTypeInfo);
            }

            sb.Append(".");

            string name = Name.Replace("<", "{").Replace(">", "}");
            if (Kind == MemberKind.Method)
            {
                sb.Append(name.Replace(".", "#"));  // Expected output is '#ctor' instead of '.ctor'

                if (MethodSignature.GenericParameterCount > 0)
                {
                    sb.Append($"``{MethodSignature.GenericParameterCount}");
                }
            }
            else
            {
                sb.Append(name);
            }

            // Add the method signature 
            if (Kind == MemberKind.Method && MethodSignature.ParameterTypes.Count() > 0)
            {
                sb.Append("(");
                sb.Append(string.Join(",", MethodSignature.ParameterTypes));
                sb.Append(")");
            }

            return sb.ToString();
        }

        public void Join(MemberMetadataInfo info2)
        {
            Names.AddRange(info2.Names);
            Names.Add(info2.Name);
            if (info2.Namespace != null)
                Namespace = info2.Namespace;

            if (info2.IsAssemblySet)
            {
                DefinedInAssembly = info2.DefinedInAssembly;
                IsAssemblySet = true;
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
    }
}
