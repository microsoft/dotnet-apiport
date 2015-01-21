// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Microsoft.Fx.Portability
{
    public interface ISearcher<T>
    {
        IEnumerable<T> Search(string query, int numberOfHits);
    }
}
