// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Fx.Portability.MetadataReader.Tests
{
    class CallOtherClass
    {
        static void Main(string[] args)
        {
            OtherClass.GetValues<int>();
        }
    }

    public class OtherClass
    {
        internal static IEnumerable<Tuple<T, int>> GetValues<T>()
        {
            yield break;
        }
    }
}