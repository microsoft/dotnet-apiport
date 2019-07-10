// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Fx.Portability.Analyzer
{
    public class AlwaysTrueDependencyFilter : IDependencyFilter
    {
        public bool IsFrameworkAssembly(string name, PublicKeyToken publicKeyToken) => true;
    }
}
