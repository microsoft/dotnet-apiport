// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer;
using System;

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    /// <summary>
    /// This allows for method calls into the supplied <see cref="IAssemblyFile"/> to be recorded for tests.
    /// </summary>
    internal class AssemblyFileFrameworkFilter : DotNetFrameworkFilter
    {
        private readonly string _name;

        public AssemblyFileFrameworkFilter(IAssemblyFile file)
        {
            _name = file.Name;
        }

        public override bool IsFrameworkMember(string name, PublicKeyToken publicKeyToken)
        {
            return string.Equals(_name, name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
