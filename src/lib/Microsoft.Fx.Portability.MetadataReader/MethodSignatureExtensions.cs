// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.Metadata;

namespace Microsoft.Fx.Portability
{
    internal static class MethodSignatureExtensions
    {
        /// <summary>
        /// Marks all types in a method signature as enclosed.  This does not change any of the inputs; instead it will create a new
        /// method signature from new parameters and return type
        /// </summary>
        /// <param name="methodSignature"></param>
        /// <returns>MethodSignature with types marked as enclosed</returns>
        public static MethodSignature<MemberMetadataInfo> MakeEnclosedType(this MethodSignature<MemberMetadataInfo> methodSignature)
        {
            var parameters = methodSignature.ParameterTypes
                .Select(p => new MemberMetadataInfo(p) { IsEnclosedType = true })
                .ToImmutableArray();
            var returnType = new MemberMetadataInfo(methodSignature.ReturnType) { IsEnclosedType = true };

            return new MethodSignature<MemberMetadataInfo>(
                methodSignature.Header,
                returnType,
                methodSignature.RequiredParameterCount,
                methodSignature.GenericParameterCount,
                parameters);
        }
    }
}
