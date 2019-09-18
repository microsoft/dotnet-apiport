// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Cci.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;

namespace Microsoft.Cci.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsVisibleOutsideAssembly(this ITypeDefinition type)
        {
            return TypeHelper.IsVisibleOutsideAssembly(type);
        }

        public static bool IsVisibleOutsideAssembly(this INestedTypeDefinition type)
        {
            return IsVisibleOutsideAssembly((ITypeDefinitionMember)type);
        }

        public static bool IsVisibleOutsideAssembly(this ITypeDefinitionMember member)
        {
            return MemberHelper.IsVisibleOutsideAssembly(member);
        }

        public static TypeMemberVisibility GetVisibility(this ITypeDefinition type)
        {
            return TypeHelper.TypeVisibilityAsTypeMemberVisibility(type);
        }

        public static bool IsDummy(this IDefinition def)
        {
            return def is Dummy;
        }

        public static ITypeDefinition GetDefinitionOrNull(this ITypeReference type)
        {
            if (type == null)
            {
                return null;
            }

            ITypeDefinition typeDef = type.ResolvedType;

            if (typeDef.IsDummy())
            {
                return null;
            }

            return typeDef;
        }

        public static IEnumerable<ITypeDefinition> GetBaseTypesAndInterfaces(this ITypeDefinition type)
        {
            foreach (var bc in type.BaseClasses)
            {
                ITypeDefinition baseType = bc.GetDefinitionOrNull();

                if (baseType != null)
                {
                    yield return baseType;
                }
            }

            foreach (var iface in type.Interfaces)
            {
                ITypeDefinition baseType = iface.GetDefinitionOrNull();

                if (baseType != null)
                {
                    yield return baseType;
                }
            }
        }

        public static IEnumerable<ITypeDefinition> GetAllBaseTypes(this ITypeDefinition type)
        {
            var types = type.IsInterface ? type.Interfaces : type.BaseClasses;

            foreach (ITypeReference baseTypeRef in types)
            {
                ITypeDefinition baseType = baseTypeRef.GetDefinitionOrNull();

                if (baseType != null)
                {
                    yield return baseType;

                    foreach (var nextBaseType in GetAllBaseTypes(baseType))
                    {
                        yield return nextBaseType;
                    }
                }
            }
        }

        public static IEnumerable<ITypeDefinition> GetAllInterfaces(this ITypeDefinition type)
        {
            foreach (var ifaceRef in type.Interfaces)
            {
                ITypeDefinition iface = ifaceRef.GetDefinitionOrNull();

                if (iface != null)
                {
                    yield return iface;

                    // Get all the base types of the interface.
                    foreach (ITypeDefinition nextIfaceRef in GetAllBaseTypes(iface))
                    {
                        yield return nextIfaceRef;
                    }
                }
            }

            foreach (var baseType in GetAllBaseTypes(type))
            {
                foreach (var iface in GetAllInterfaces(baseType))
                {
                    yield return iface;
                }
            }
        }

        public static IAssembly GetAssembly(this ITypeDefinition type)
        {
            IUnit unit = TypeHelper.GetDefiningUnit(type);

            if (unit is IAssembly assembly)
            {
                return assembly;
            }

            if (unit is IModule module)
            {
                return module.ContainingAssembly;
            }

            return null;
        }

        public static IAssemblyReference GetAssemblyReference(this ITypeReference type)
        {
            IUnitReference unit = TypeHelper.GetDefiningUnitReference(type);

            if (unit is IAssemblyReference assembly)
            {
                return assembly;
            }

            if (unit is IModuleReference module)
            {
                return module.ContainingAssembly;
            }

            return null;
        }

        public static IAssemblyReference GetAssemblyReference(this IReference reference)
        {
            Contract.Requires(reference != null);
            Contract.Requires(!(reference is Dummy));

            if (reference is IAssemblyReference assembly)
            {
                return assembly;
            }

            if (reference is ITypeReference type)
            {
                return type.GetAssemblyReference();
            }

            if (reference is ITypeMemberReference member)
            {
                return member.ContainingType.GetAssemblyReference();
            }

            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.UnknownIReference, reference.GetType().FullName));
        }

        public static bool IsGenericParameter(this ITypeReference type)
        {
            return type is IGenericParameterReference || type is IGenericParameter;
        }

        public static bool IsGenericInstance(this ITypeReference type)
        {
            return type is IGenericTypeInstanceReference;
        }

        public static bool IsGenericInstance(this IMethodReference method)
        {
            return method is IGenericMethodInstanceReference;
        }

        public static bool IsWindowsRuntimeAssembly(this IAssemblyReference assembly)
        {
            if (assembly == null)
            {
                return false;
            }

            // ContainsForeignTypes == AssemblyFlag 0x200 == windowsruntime bit
            return assembly.ContainsForeignTypes;
        }

        public static bool IsWindowsRuntimeType(this ITypeReference type)
        {
            var assemblyRef = type.GetAssemblyReference();
            return assemblyRef.IsWindowsRuntimeAssembly();
        }

        public static bool IsWindowsRuntimeMember(this ITypeMemberReference member)
        {
            var assemblyRef = member.GetAssemblyReference();
            return assemblyRef.IsWindowsRuntimeAssembly();
        }

        public static bool IsFunctionPointer(this ITypeReference type)
        {
            return type is IFunctionPointerTypeReference;
        }

        public static string GetTypeName(this ITypeReference type, bool includeNamespace = true)
        {
            var formatter = new TypeNameFormatter();
            var options = NameFormattingOptions.OmitTypeArguments | NameFormattingOptions.UseReflectionStyleForNestedTypeNames;
            if (!includeNamespace)
            {
                options |= NameFormattingOptions.OmitContainingNamespace;
            }

            string name = formatter.GetTypeName(type, options);
            return name;
        }

        public static string GetTypeName(this ITypeReference type, NameFormattingOptions options)
        {
            var formatter = new TypeNameFormatter();
            string name = formatter.GetTypeName(type, options);
            return name;
        }

        public static INamespaceDefinition GetNamespace(this ITypeDefinition type)
        {
            if (type is INamespaceTypeDefinition nsType)
            {
                return nsType.ContainingNamespace;
            }

            if (type is INestedTypeDefinition ntType)
            {
                return GetNamespace(ntType.ContainingTypeDefinition);
            }

            return null;
        }

        public static string GetNamespaceName(this ITypeReference type)
        {
            if (type is INamespaceTypeReference nsType)
            {
                return TypeHelper.GetNamespaceName(nsType.ContainingUnitNamespace, NameFormattingOptions.None);
            }

            if (type is INestedTypeReference ntType)
            {
                return GetNamespaceName(ntType.ContainingType);
            }

            return string.Empty;
        }

        public static string Name(this ITypeDefinition type)
        {
            if (type is INamespaceTypeDefinition nsType)
            {
                return nsType.Name.Value;
            }

            if (type is INestedTypeDefinition nType)
            {
                return nType.Name.Value;
            }

            throw new NotImplementedException(LocalizedStrings.CalledNameOnUnsupportedDefinition);
        }

        public static string FullName(this IReference reference)
        {
            Contract.Requires(reference != null);

            if (reference is ITypeReference type)
            {
                return TypeHelper.GetTypeName(type, NameFormattingOptions.TypeParameters);
            }

            if (reference is ITypeMemberReference member)
            {
                return MemberHelper.GetMemberSignature(member, NameFormattingOptions.TypeParameters | NameFormattingOptions.Signature);
            }

            if (reference is IUnitNamespaceReference ns)
            {
                return TypeHelper.GetNamespaceName(ns, NameFormattingOptions.None);
            }

            if (reference is INamedEntity named)
            {
                return named.Name.Value;
            }

            Contract.Assert(false, string.Format(CultureInfo.CurrentCulture, LocalizedStrings.FellThroughCasesIn, "TypeExtensions.FullName()", reference.GetType()));
            return LocalizedStrings.UnknownReferenceType;
        }

        public static string GetMethodSignature(this IMethodDefinition method)
        {
            return MemberHelper.GetMethodSignature(method, NameFormattingOptions.Signature);
        }

        public static string UniqueId(this IReference reference)
        {
            Contract.Requires(reference != null);

            if (reference is ITypeReference type)
            {
                return type.DocId();
            }

            if (reference is ITypeMemberReference member)
            {
                return member.DocId();
            }

            if (reference is IUnitNamespaceReference ns)
            {
                return ns.DocId();
            }

            if (reference is IAssemblyReference assembly)
            {
                return assembly.DocId();
            }

            // Include the hash code as well to make it unique so we can use this for a key
            return LocalizedStrings.UnknownReferenceType + reference.GetHashCode().ToString(CultureInfo.InvariantCulture);
        }

        public static IEnumerable<INamespaceDefinition> GetAllNamespaces(this IAssembly assembly)
        {
            yield return assembly.NamespaceRoot;

            foreach (var ns in assembly.NamespaceRoot.GetNamespaces(true))
            {
                yield return ns;
            }
        }

        public static IEnumerable<INamespaceDefinition> GetNamespaces(this INamespaceDefinition ns, bool recursive = true)
        {
            foreach (var nestedNs in ns.Members.OfType<INamespaceDefinition>())
            {
                yield return nestedNs;

                if (recursive)
                {
                    foreach (var nn in nestedNs.GetNamespaces(recursive))
                    {
                        yield return nn;
                    }
                }
            }
        }

        public static IEnumerable<INamespaceTypeDefinition> GetTypes(this INamespaceDefinition ns, bool includeForwards = false)
        {
            IEnumerable<INamespaceTypeDefinition> types;

            if (includeForwards)
            {
                types = ns.Members.Select<INamespaceMember, INamespaceTypeDefinition>(nsMember =>
                {
                    if (nsMember is INamespaceAliasForType nsAlias)
                    {
                        nsMember = nsAlias.AliasedType.ResolvedType as INamespaceTypeDefinition;
                    }

                    return nsMember as INamespaceTypeDefinition;
                }).Where(t => t != null);
            }
            else
            {
                types = ns.Members.OfType<INamespaceTypeDefinition>();
            }

            return types;
        }

        public static IEnumerable<ITypeDefinitionMember> GetAllMembers(this ITypeDefinition type)
        {
            foreach (var m in type.Members)
            {
                yield return m;
            }

            if (type.IsInterface)
            {
                foreach (var m in GetAllMembersFromInterfaces(type))
                {
                    yield return m;
                }
            }
            else
            {
                foreach (var m in GetAllMembersBaseType(type))
                {
                    yield return m;
                }
            }
        }

        public static IEnumerable<ITypeDefinitionMember> GetAllMembersBaseType(this ITypeDefinition type)
        {
            ITypeReference baseTypeRef = type.BaseClasses.FirstOrDefault();

            if (baseTypeRef == null || TypeHelper.TypesAreEquivalent(baseTypeRef, type.PlatformType.SystemObject))
            {
                yield break;
            }

            ITypeDefinition baseType = baseTypeRef.ResolvedType;

            foreach (var m in GetAllMembers(baseType))
            {
                yield return m;
            }
        }

        public static IEnumerable<ITypeDefinitionMember> GetAllMembersFromInterfaces(this ITypeDefinition type)
        {
            foreach (ITypeReference iface in type.Interfaces)
            {
                ITypeDefinition ifaceType = iface.ResolvedType;

                foreach (var m in ifaceType.Members)
                {
                    yield return m;
                }
            }
        }

        public static bool AreEquivalent(this ITypeReference type, string typeName)
        {
            return type.FullName() == typeName;
        }

        public static ITypeReference UnWrap(this ITypeReference reference)
        {
            if (reference is IPointerTypeReference pointer)
            {
                return pointer.TargetType.UnWrap();
            }

            if (reference is IArrayTypeReference array)
            {
                return array.ElementType.UnWrap();
            }

            if (reference is IModifiedTypeReference modified)
            {
                return modified.UnmodifiedType.UnWrap();
            }

            if (reference is ISpecializedNestedTypeReference specialized)
            {
                return specialized.UnspecializedVersion.UnWrap();
            }

            if (reference is IGenericTypeInstanceReference instantiation)
            {
                return instantiation.GenericType.UnWrap();
            }

            Contract.Assert(reference is INamedTypeReference
                || reference is INestedTypeReference
                || reference is INamespaceTypeReference
                || reference is IGenericTypeParameterReference
                || reference is IGenericMethodParameterReference
                || reference is IFunctionPointerTypeReference,
                string.Format(CultureInfo.CurrentCulture, LocalizedStrings.UnexpectedTypeReference, reference?.GetType()?.FullName ?? "null"));

            return reference;
        }

        public static T UnWrapMember<T>(this T member)
           where T : ITypeMemberReference
        {
            if (member is IGenericMethodInstanceReference genericMethod)
            {
                return (T)genericMethod.GenericMethod.UnWrapMember();
            }

            if (member is ISpecializedNestedTypeReference type)
            {
                return (T)type.UnspecializedVersion.UnWrapMember();
            }

            if (member is ISpecializedMethodReference method)
            {
                return (T)method.UnspecializedVersion.UnWrapMember();
            }

            if (member is ISpecializedFieldReference field)
            {
                return (T)field.UnspecializedVersion.UnWrapMember();
            }

            if (member is ISpecializedPropertyDefinition property)
            {
                return (T)property.UnspecializedVersion.UnWrapMember();
            }

            if (member is ISpecializedEventDefinition evnt)
            {
                return (T)evnt.UnspecializedVersion.UnWrapMember();
            }

            return member;
        }

        public static bool IsPropertyOrEventAccessor(this IMethodDefinition method)
        {
            return method.GetAccessorType() != AccessorType.None;
        }

        public static AccessorType GetAccessorType(this IMethodDefinition methodDefinition)
        {
            if (!methodDefinition.IsSpecialName)
            {
                return AccessorType.None;
            }

            foreach (var p in methodDefinition.ContainingTypeDefinition.Properties)
            {
                if (p.Getter != null && p.Getter.ResolvedMethod.InternedKey == methodDefinition.InternedKey)
                {
                    return AccessorType.PropertyGetter;
                }

                if (p.Setter != null && p.Setter.ResolvedMethod.InternedKey == methodDefinition.InternedKey)
                {
                    return AccessorType.PropertySetter;
                }
            }

            foreach (var e in methodDefinition.ContainingTypeDefinition.Events)
            {
                if (e.Adder != null && e.Adder.ResolvedMethod.InternedKey == methodDefinition.InternedKey)
                {
                    return AccessorType.EventAdder;
                }

                if (e.Remover != null && e.Remover.ResolvedMethod.InternedKey == methodDefinition.InternedKey)
                {
                    return AccessorType.EventRemover;
                }
            }

            return AccessorType.None;
        }

        public static string GetTargetFrameworkMoniker(this IAssembly assembly)
        {
            var tfmAttribute = (from a in assembly.AssemblyAttributes
                                where a.Type.FullName() == "System.Runtime.Versioning.TargetFrameworkAttribute"
                                select a).FirstOrDefault();

            if (tfmAttribute == null)
            {
                return string.Empty;
            }

            if (!(tfmAttribute.Arguments.FirstOrDefault() is IMetadataConstant nameArgument))
            {
                return string.Empty;
            }

            return !(nameArgument.Value is string name) ? string.Empty : name;
        }

        public static bool IsReferenceAssembly(this IAssembly assembly)
        {
            return assembly.AssemblyAttributes.Any(a => a.Type.FullName() == "System.Runtime.CompilerServices.ReferenceAssemblyAttribute");
        }

        private static readonly Dictionary<string, string> TokenNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "b77a5c561934e089", "ECMA" },
            { "b03f5f7f11d50a3a", "DEVDIV" },
            { "7cec85d7bea7798e", "SLPLAT" },
            { "31bf3856ad364e35", "SILVERLIGHT" },
            { "24eec0d8c86cda1e", "PHONE" },
        };

        public static string MapPublicKeyTokenToName(string publicKeyToken)
        {
            if (!TokenNames.TryGetValue(publicKeyToken, out var name))
            {
                name = publicKeyToken;
            }

            return name;
        }

        public static string GetPublicKeyTokenName(this IAssemblyReference assembly)
        {
            return MapPublicKeyTokenToName(assembly.GetPublicKeyToken());
        }

        public static string GetPublicKeyTokenName(this AssemblyIdentity identity)
        {
            return MapPublicKeyTokenToName(identity.GetPublicKeyToken());
        }

        public static string GetPublicKeyToken(this IAssemblyReference assembly)
        {
            return GetPublicKeyToken(assembly.AssemblyIdentity);
        }

        public static string GetPublicKeyToken(this AssemblyIdentity identity)
        {
            return identity.PublicKeyToken.Aggregate(string.Empty, (s, b) => s += b.ToString("x2", CultureInfo.InvariantCulture));
        }

        public static bool IsFrameworkAssembly(this AssemblyIdentity identity)
        {
            string token = identity.GetPublicKeyToken();

            string mapped = MapPublicKeyTokenToName(token);

            // If the token is the same then we don't a name for it and thus we don't know about it.
            return token != mapped;
        }

        public static bool IsFacade(this IAssembly assembly)
        {
            if (assembly.GetAllTypes().Any(t => t.Name.Value != "<Module>"))
            {
                return false;
            }

            return true;
        }

        public static IEnumerable<T> OrderByIdentity<T>(this IEnumerable<T> assemblies)
            where T : IAssemblyReference
        {
            return assemblies.OrderBy(a => a.Name.Value)
                             .ThenBy(a => a.GetPublicKeyToken())
                             .ThenBy(a => a.Version);
        }

        public static IEnumerable<AssemblyIdentity> OrderByIdentity(this IEnumerable<AssemblyIdentity> assemblies)
        {
            return assemblies.OrderBy(a => a.Name.Value)
                             .ThenBy(a => a.GetPublicKeyToken())
                             .ThenBy(a => a.Version);
        }

        public static bool IsEditorBrowseableStateNever(this ICustomAttribute attribute)
        {
            if (attribute.Type.FullName() != typeof(EditorBrowsableAttribute).FullName)
            {
                return false;
            }

            if (attribute.Arguments == null || attribute.Arguments.Count() != 1)
            {
                return false;
            }

            var singleArgument = attribute.Arguments.Single() as IMetadataConstant;

            if (singleArgument == null || !(singleArgument.Value is int))
            {
                return false;
            }

            if ((EditorBrowsableState)singleArgument.Value != EditorBrowsableState.Never)
            {
                return false;
            }

            return true;
        }
    }
}
