// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    internal class CallOtherClass_OpExplicit
    {
        private static void Main(string[] args)
        {
            Class1_OpExplicit<int> list = (Class1_OpExplicit<int>)new Class2_OpExplicit<int>();
        }
    }

    public class Class1_OpExplicit<T> { }

    internal struct Class2_OpExplicit<T>
    {
        public static explicit operator Class1_OpExplicit<T>(Class2_OpExplicit<T> other)
        {
            return new Class1_OpExplicit<T>();
        }
    }
}