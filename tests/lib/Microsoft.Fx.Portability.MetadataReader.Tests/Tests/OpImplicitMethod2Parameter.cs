// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    internal class OpImplicit_Method_2Parameter
    {
        private static void Main(string[] args)
        {
            var method = new OpImplicit_Method_2Parameter<int>();

            method.op_Implicit(1, 2);
        }
    }

    internal struct OpImplicit_Method_2Parameter<T>
    {
        public int op_Implicit(T arg1, T arg2)
        {
            return 0;
        }
    }
}