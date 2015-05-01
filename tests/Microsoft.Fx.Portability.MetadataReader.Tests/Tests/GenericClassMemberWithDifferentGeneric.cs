// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Fx.Portability.MetadataReader.Tests.Tests
{
    internal class CallOtherClass
    {
        private static void Main(string[] args)
        {
            GenericClass<int>.MemberWithDifferentGeneric("hello");
        }
    }

    public class GenericClass<TResult>
    {
        internal static TResult MemberWithDifferentGeneric<TAntecedentResult>(TAntecedentResult result)
        {
            return default(TResult);
        }
    }
}
