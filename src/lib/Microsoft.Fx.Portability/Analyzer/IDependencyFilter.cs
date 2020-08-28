// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Fx.Portability.Analyzer
{
    public interface IDependencyFilter
    {
        /// <summary>
        /// Method used to identify if an assembly itself is a framework assembly.
        /// </summary>
        /// <param name="name">Name of the assembly.</param>
        /// <param name="publicKeyToken">Public key token.</param>
        /// <returns><c>true</c> if framwork assembly, otherwise <c>false</c>.</returns>
        bool IsFrameworkAssembly(string name, PublicKeyToken publicKeyToken);

        /// <summary>
        /// Method used to identify if a member is in a framework.
        /// </summary>
        /// <param name="name">Name of the assembly the member is contained in.</param>
        /// <param name="publicKeyToken">Public key token.</param>
        /// <returns><c>true</c> if framwork member, otherwise <c>false</c>.</returns>
        bool IsFrameworkMember(string name, PublicKeyToken publicKeyToken);
    }
}
