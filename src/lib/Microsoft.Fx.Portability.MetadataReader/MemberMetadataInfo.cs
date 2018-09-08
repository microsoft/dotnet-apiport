// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer.Resources;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;

namespace Microsoft.Fx.Portability.Analyzer
{
    internal class MemberMetadataInfo
    {
        public MemberMetadataInfo()
        {
            Kind = MemberKind.Type;
            Names = new List<string>();
            Modifiers = ImmutableStack.Create<MemberModifiedMetadata>();
        }

        public MemberMetadataInfo(MemberMetadataInfo other)
        {
            Names = other.Names;
            MethodSignature = other.MethodSignature;
            Module = other.Module;
            ArrayTypeInfo = other.ArrayTypeInfo;
            IsTypeDef = other.IsTypeDef;
            IsPrimitiveType = other.IsPrimitiveType;
            Kind = other.Kind;
            IsGenericInstance = other.IsGenericInstance;
            IsEnclosedType = other.IsEnclosedType;
            IsPointer = other.IsPointer;
            Modifiers = other.Modifiers;
            Name = other.Name;
            Namespace = other.Namespace;
            DefinedInAssembly = other.DefinedInAssembly;

            if (other.ParentType != null)
            {
                ParentType = new MemberMetadataInfo(other.ParentType);
            }

            if (other.GenericTypeArgs != null)
            {
                GenericTypeArgs = other.GenericTypeArgs.Select(o => new MemberMetadataInfo(o)).ToList();
            }
        }

        public MemberMetadataInfo(MemberMetadataInfo other1, MemberMetadataInfo other2)
            : this(other1)
        {
            Names.AddRange(other2.Names);
            Names.Add(other2.Name);

            if (other2.Namespace != null)
            {
                Namespace = other2.Namespace;
            }

            if (other2.DefinedInAssembly.HasValue)
            {
                DefinedInAssembly = other2.DefinedInAssembly;
            }
        }

        public List<string> Names { get; set; }

        public MethodSignature<MemberMetadataInfo> MethodSignature { get; set; }

        public ModuleReferenceHandle Module { get; set; }

        public List<MemberMetadataInfo> GenericTypeArgs { get; set; }

        public bool IsArrayType => !string.IsNullOrEmpty(ArrayTypeInfo);

        public string ArrayTypeInfo { get; set; }

        public bool IsTypeDef { get; set; }

        public bool IsPrimitiveType { get; set; }

        public MemberKind Kind { get; set; }

        public bool IsGenericInstance { get; set; }

        public bool IsEnclosedType { get; set; }

        public string Name { get; set; }

        public string Namespace { get; set; }

        public MemberMetadataInfo ParentType { get; set; }

        public AssemblyReference? DefinedInAssembly { get; set; }

        public IImmutableStack<MemberModifiedMetadata> Modifiers { get; set; }

        public bool IsPointer { get; set; }

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

            if (IsArrayType)
            {
                sb.Append(ArrayTypeInfo);
            }

            foreach (var modifier in Modifiers)
            {
                sb.Append(modifier.IsRequired ? " reqmod " : " optmod ");
                sb.Append(modifier.Metadata);
            }

            if (IsPointer)
            {
                sb.Append('*');
            }

            return sb.ToString();
        }

        /// <summary>
        /// Add the type arguments for generic instances. Go through all type names, and if it is generic, such
        /// as Hashtable`1, remove the `1. Look in the arguments list and put the list of arguments in between {}
        ///
        /// Example: Hashtable{`0}.KeyValuePair
        ///
        /// There are some interesting corner cases involving nested types -
        /// First, generic argument indexes are counted over all types in the nested type hierarchy.
        /// Therefore, OuterClass`2.InnerClass`2 should resolve as OuterClass{`0,`1}.InnerClass{`2,`3}.
        ///
        /// Secondly, it is not required that all generic arguments be made concrete.
        /// For example, it's possible in IL to define nested generic types OuterClass`2.InnerClass`2 and
        /// then pass only two generic types in GenericTypeArgs. In such cases, the type should resolve
        /// as OuterClass{`0,`1}.InnerClass`2.
        /// </summary>
        private IEnumerable<string> GetGenericDisplayNames(IList<string> displayNames)
        {
            // The most outputs when run on mscorlib are under 50 bytes in length
            const int SB_CAPACITY = 50;

            // Index goes outside the for loop because it increments over all type names, not just a specific type name
            int index = 0;
            for (int i = 0; i < displayNames.Count; i++)
            {
                var displayName = displayNames[i];

                int pos = displayName.IndexOf('`');
                if (pos <= 0)
                {
                    StringBuilder returnName = new StringBuilder(displayName, SB_CAPACITY);

                    if (i == displayNames.Count - 1 && GenericTypeArgs.Count > index)
                    {
                        // Even though this is not a generic type, if it's the last type in the displayNames list and
                        // there are left-over generic arguments, we should append them to this name.
                        // This is because it's possible (from IL) to define a non-generic type that accepts generic arguments.
                        // This construction is used by some obfuscators.
                        returnName.Append("{");
                        returnName.Append(string.Join(",", GenericTypeArgs.GetRange(index, GenericTypeArgs.Count - index)));
                        returnName.Append("}");
                        index = GenericTypeArgs.Count;
                    }

                    yield return returnName.ToString();
                    continue;
                }

                // Separate the name from the generic marker and the string after.
                // The format is: 'StringBefore`10StringAfter'
                int numGenericArgs;
                int offsetStringAfterGenericMarker;

                // This calculates the number of generic arguments in the specific displayName we're looking at in
                // this iteration of the foreach loop.
                if (!CalculateGenericArgsOffset(displayName, pos, out numGenericArgs, out offsetStringAfterGenericMarker))
                {
                    yield return displayName;
                    continue;
                }

                // It is possible (from IL) to only pass generic type arguments for the outer class in the case of nested generic classes.
                // In such cases, once we run out of generic arguments, we just return the generic type names without resolving generic arguments.
                if (index + numGenericArgs > GenericTypeArgs.Count)
                {
                    yield return displayName;
                    continue;
                }

                // Start with the name up to the generic parameter token
                var sb = new StringBuilder(displayName, 0, pos, SB_CAPACITY);

                // Add the generic parameters
                sb.Append("{");
                sb.Append(string.Join(",", GenericTypeArgs.GetRange(index, numGenericArgs)));
                sb.Append("}");

                // Add any part that was after the generic entry
                if (displayName.Length > offsetStringAfterGenericMarker)
                {
                    var length = displayName.Length - offsetStringAfterGenericMarker;

                    sb.Append(displayName, offsetStringAfterGenericMarker, length);
                }

                yield return sb.ToString();

                // Advance the index in the args list
                index += numGenericArgs;
            }
        }

        /// <summary>
        /// Replace the non-safe characters (<>.,) in a method name with safe ones ({}#@)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetDocIdSafeMemberName(string name)
        {
            char[] newName = new char[name.Length];
            for (int i = 0; i < name.Length; i++)
            {
                if (name[i] == '<') newName[i] = '{';
                else if (name[i] == '>') newName[i] = '}';
                else if (name[i] == '.') newName[i] = '#';
                else if (name[i] == ',') newName[i] = '@';
                else newName[i] = name[i];
            }
            return new string(newName);
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

            // Make sure that the member name is safe by replacing the unwanted characters.
            sb.Append(GetDocIdSafeMemberName(Name));

            if (Kind == MemberKind.Method)
            {
                if (MethodSignature.GenericParameterCount > 0)
                {
                    sb.Append(FormattableString.Invariant($"``{MethodSignature.GenericParameterCount}"));
                }
            }

            // Add the method signature
            if (Kind == MemberKind.Method && MethodSignature.ParameterTypes.Count() > 0)
            {
                sb.Append("(");

                // Only required parameters should be listed explicitly
                Debug.Assert(MethodSignature.ParameterTypes.Count() >= MethodSignature.RequiredParameterCount, LocalizedStrings.MoreParametersWereRequired);
                sb.Append(string.Join(",", MethodSignature.ParameterTypes.Select(m => m.ToString()).ToArray(), 0, MethodSignature.RequiredParameterCount));

                // If the method is a varargs method, it should add an '__arglist' parameter
                if (MethodSignature.Header.CallingConvention.HasFlag(SignatureCallingConvention.VarArgs))
                {
                    sb.Append(",__arglist");
                }

                sb.Append(")");
            }
            else if (Kind == MemberKind.Method && MethodSignature.Header.CallingConvention.HasFlag(SignatureCallingConvention.VarArgs))
            {
                // It's possible to have an __arglist without anything being passed to it
                sb.Append("(__arglist)");
            }

            // Technically, we want to verify that these are marked as a special name along with the names op_Implicit or op_Explicit.  However,
            // since we are just using member references, we don't have enought information to know if it is.  For now, we will assume that it is
            // a special name if it only has one input parameter
            if (Kind == MemberKind.Method && MethodSignature.ParameterTypes.Length == 1 &&
                (string.Equals(Name, "op_Implicit", StringComparison.Ordinal) || string.Equals(Name, "op_Explicit", StringComparison.Ordinal)))
            {
                sb.Append("~");
                sb.Append(MethodSignature.ReturnType);
            }

            return sb.ToString();
        }

        private static bool CalculateGenericArgsOffset(string displayName, int pos, out int numGenericArgs, out int offsetStringAfterGenericMarker)
        {
            Debug.Assert(displayName[pos] == '`');

            offsetStringAfterGenericMarker = ++pos;

            while (offsetStringAfterGenericMarker < displayName.Length && char.IsDigit(displayName[offsetStringAfterGenericMarker]))
            {
                offsetStringAfterGenericMarker++;
            }

            return int.TryParse(displayName.Substring(pos, offsetStringAfterGenericMarker - pos), out numGenericArgs);
        }
    }
}
