// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.Analyzer;
using Microsoft.Fx.Portability.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ApiPort
{
    internal class EmptyDependendencyFinder : IDependencyFinder
    {
        public IDependencyInfo FindDependencies(IEnumerable<FileInfo> inputAssemblyPaths, IProgressReporter progressReport)
        {
            return new EmptyDependencyInfo();
        }

        private class EmptyDependencyInfo : IDependencyInfo
        {
            public IEnumerable<string> AssembliesWithErrors
            {
                get { return Enumerable.Empty<string>(); }
            }

            public IDictionary<MemberInfo, ICollection<AssemblyInfo>> Dependencies
            {
                get
                {
                    return new Dictionary<MemberInfo, ICollection<AssemblyInfo>>();
                }
            }

            public IDictionary<string, ICollection<string>> UnresolvedAssemblies
            {
                get
                {
                    return new Dictionary<string, ICollection<string>>();
                }
            }

            public IEnumerable<AssemblyInfo> UserAssemblies
            {
                get
                {
                    return Enumerable.Empty<AssemblyInfo>();
                }
            }
        }
    }
}
