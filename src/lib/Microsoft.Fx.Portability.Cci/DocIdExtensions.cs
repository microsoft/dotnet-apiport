// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Cci.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;

namespace Microsoft.Cci.Extensions
{
    public static class DocIdExtensions
    {
        public static string DocId(this ICustomAttribute attribute)
        {
            return attribute.Type.DocId();
        }

        public static string DocId(this ITypeReference type)
        {
            type = type.UnWrap();
            return TypeHelper.GetTypeName(type, NameFormattingOptions.DocumentationId);
        }

        public static string DocId(this ITypeMemberReference member)
        {
            // Do we need to unwrap members?
            // member = member.UnWrapMember();
            return MemberHelper.GetMemberSignature(member, NameFormattingOptions.DocumentationId);
        }

        public static string DocId(this INamespaceDefinition ns)
        {
            return DocId((IUnitNamespaceReference)ns);
        }

        public static string DocId(this IUnitNamespaceReference ns)
        {
            return "N:" + TypeHelper.GetNamespaceName(ns, NameFormattingOptions.None);
        }

        public static string DocId(this IAssemblyReference assembly)
        {
            return DocId(assembly.AssemblyIdentity);
        }

        public static string DocId(this AssemblyIdentity assembly)
        {
            return string.Format(CultureInfo.InvariantCulture, "A:{0}", assembly.Name.Value);
        }

        public static string DocId(this IPlatformInvokeInformation platformInvoke)
        {
            // return string.Format("I:{0}.{1}", platformInvoke.ImportModule.Name.Value, platformInvoke.ImportName.Value);

            // For now so we can use this to match up with the modern sdk names only include the pinvoke name in the identifier.
            return string.Format(CultureInfo.InvariantCulture, "{0}", platformInvoke.ImportName.Value);
        }

        public static string RefDocId(this IReference reference)
        {
            Contract.Requires(reference != null);

            if (reference is ITypeReference type)
                return type.DocId();

            if (reference is ITypeMemberReference member)
                return member.DocId();

            if (reference is IUnitNamespaceReference ns)
                return ns.DocId();

            if (reference is IAssemblyReference assembly)
                return assembly.DocId();

            Contract.Assert(false, string.Format(CultureInfo.CurrentUICulture, LocalizedStrings.FellThroughCasesIn, "DocIdExtensions.RefDocId()", reference.GetType()));
            return LocalizedStrings.UnknownReferenceType;
        }

        public static HashSet<string> ReadDocIds(string docIdsFile)
        {
            HashSet<string> ids = new HashSet<string>();

            if (!File.Exists(docIdsFile))
                return ids;

            foreach (string id in File.ReadAllLines(docIdsFile))
            {
                if (string.IsNullOrWhiteSpace(id) || id.StartsWith("#", StringComparison.Ordinal) || id.StartsWith("//", StringComparison.Ordinal))
                    continue;

                ids.Add(id.Trim());
            }

            return ids;
        }
    }
}
