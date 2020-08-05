// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Reflection;

namespace Microsoft.Fx.Portability.Analyzer
{
    public static class DependencyFilterExtensions
    {
        public static bool IsFrameworkAssembly(this IDependencyFilter filter, AssemblyName assembly)
        {
            var publicKey = assembly.GetPublicKeyToken();

            return filter.IsFrameworkAssembly(assembly.Name, publicKey is null ? PublicKeyToken.Empty : new PublicKeyToken(publicKey.ToImmutableArray()));
        }
    }
}
