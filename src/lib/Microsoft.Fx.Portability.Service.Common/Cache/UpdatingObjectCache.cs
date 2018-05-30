// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Cache
{
    /// <summary>
    /// This class represents a cache when the value may be updated periodically.  When we are asked for the value of the 
    /// cache, we will return the value we currently have.  Periodically, we are going to check and see if the value 
    /// should be updated by using timestamps
    /// </summary>
    public abstract class UpdatingObjectCache<TObject> : UpdateableObjectCache<TObject>
    {
        private readonly CancellationToken _cancellationToken;
        private readonly TimeSpan _cachePollInterval;

        public UpdatingObjectCache(TimeSpan cachePollInterval, string identifier, CancellationToken cancellationToken)
            : base(identifier)
        {
            _cancellationToken = cancellationToken;
            _cachePollInterval = cachePollInterval;
        }

        public override async void Start(CancellationToken token = default(CancellationToken))
        {
            Initialize();

            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateAsync(_cancellationToken);
                }
                catch (Exception)
                {
                    Trace.TraceInformation("Update from '{0}' updated to modified date: {1}", Identifier, LastUpdated);
                }

                await Task.Delay(_cachePollInterval, _cancellationToken);
            }
        }
    }
}
