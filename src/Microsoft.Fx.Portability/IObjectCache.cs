// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability
{
    public interface IObjectCache<TObject> : IDisposable
    {
        TObject Value { get; }
        DateTimeOffset LastUpdated { get; }
        Task UpdateAsync();
    }
}
