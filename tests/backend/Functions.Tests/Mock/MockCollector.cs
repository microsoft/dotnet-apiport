// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs;
using System.Collections.Generic;

namespace Functions.Tests.Mock
{
    internal class MockCollector<T> : ICollector<T>
    {
        public readonly List<T> Items = new List<T>();

        public void Add(T item)
        {
            Items.Add(item);
        }
    }
}
