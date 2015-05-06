// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    internal class OpImplicit_Method
    {
        private static void Main(string[] args)
        {
            var method = new OpImplicit_Method<int>();

            method.op_Implicit(1);
        }
    }

    internal struct OpImplicit_Method<T>
    {
        public int op_Implicit(T other)
        {
            return 0;
        }
    }
}