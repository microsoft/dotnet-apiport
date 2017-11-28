// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability
{
    public class AssemblyFileComparer : IComparer<IAssemblyFile>
    {
        public static AssemblyFileComparer Instance { get; } = new AssemblyFileComparer();

        private AssemblyFileComparer()
        {
        }

        public int Compare(IAssemblyFile x, IAssemblyFile y)
        {
            if (x == null)
            {
                return y == null ? 0 : -1;
            }

            return string.Compare(x.Name, y?.Name, StringComparison.Ordinal);
        }
    }
}
