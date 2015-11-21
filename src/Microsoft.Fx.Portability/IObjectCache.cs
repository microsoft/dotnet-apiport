// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability
{
    public interface IObjectCache : IDisposable
    {
        Task UpdateAsync();

        DateTimeOffset LastUpdated { get; }
    }

    public interface IObjectCache<TObject> : IObjectCache
    {
        TObject Value { get; }
    }
}
