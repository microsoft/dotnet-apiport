// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Fx.Portability.MetadataReader.Tests.Tests
{
    namespace Microsoft.Fx.Portability.MetadataReader.Tests
    {
        internal class CallOtherClass_10_generic_params
        {
            private static void Main(string[] args)
            {
                var @class = new Class_10_generic_params<int, int, int, int, int, int, int, int, int, int>();
                var innerClass = new Class_10_generic_params<int, int, int, int, int, int, int, int, int, int>.InnerClass(@class, 1);
            }
        }

        public class Class_10_generic_params<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
        {
            public class InnerClass
            {
                public InnerClass(Class_10_generic_params<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> @class, T3 param)
                {
                }
            }
        }
    }
}
