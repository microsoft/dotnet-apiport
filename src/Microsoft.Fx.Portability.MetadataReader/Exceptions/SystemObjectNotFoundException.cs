// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Analyzer.Resources;
using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Fx.Portability.Analyzer.Exceptions
{
    /// <summary>
    /// Exception thrown when assembly containing <see cref="System.Object"/>
    /// cannot be found.
    /// </summary>
    public class SystemObjectNotFoundException : PortabilityAnalyzerException
    {
        public SystemObjectNotFoundException(IEnumerable<AssemblyReferenceInformation> assemblies)
            : base(string.Format(CultureInfo.CurrentCulture, LocalizedStrings.MissingSystemObjectAssembly,
                string.Join(", ", assemblies)))
        {
            AssembliesReferenced = assemblies;
        }

        public IEnumerable<AssemblyReferenceInformation> AssembliesReferenced { get; }
    }
}
