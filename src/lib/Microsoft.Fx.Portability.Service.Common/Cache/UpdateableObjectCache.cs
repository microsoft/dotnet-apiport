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
    /// cache, we will return the value we currently have.  When an update occurs, the timestamp is checked to ensure we
    /// only attempt to update a cache that has an available update.
    /// </summary>
    public abstract class UpdateableObjectCache<TObject> : IObjectCache<TObject>
    {
        private readonly TaskCompletionSource<bool> _loadedTask = new TaskCompletionSource<bool>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private TObject _cachedObject;
        private DateTimeOffset _timeStamp;

        private bool _disposed = false;

        public TObject Value
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _cachedObject;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Create an object cache that can be updated
        /// </summary>
        /// <param name="identifier">An optional name to identify this cache</param>
        public UpdateableObjectCache(string identifier = null)
        {
            Identifier = identifier ?? this.GetType().ToString();
        }

        public DateTimeOffset LastUpdated
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _timeStamp;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public Task WaitForInitialLoadAsync()
        {
            return _loadedTask.Task;
        }

        public string Identifier { get; }

        /// <summary>
        /// Gets the DateTimeOffset of the latest modified item in the remote collection.
        /// </summary>
        protected abstract Task<DateTimeOffset> GetTimeStampAsync(CancellationToken token);

        /// <summary>
        /// From the tracked collection, fetches the latest TObject.
        /// </summary>
        protected abstract Task<TObject> UpdateObjectAsync(CancellationToken token);

        protected virtual TObject GetDefaultObject()
        {
            return default(TObject);
        }

        public async Task<CacheUpdateStatus> UpdateAsync(CancellationToken token)
        {
            var lastModified = await GetTimeStampAsync(token);

            if (lastModified <= _timeStamp && lastModified > DateTimeOffset.MinValue)
            {
                return CacheUpdateStatus.AlreadyUpToDate;
            }

            Trace.TraceInformation($"A newer version of '{Identifier}' exists!");

            try
            {
                var updatedItem = await UpdateObjectAsync(token);

                UpdateValues(updatedItem, lastModified);

                return CacheUpdateStatus.Success;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());

                return CacheUpdateStatus.Failure;
            }
            finally
            {
                _loadedTask.TrySetResult(true);
            }
        }

        protected void Initialize()
        {
            UpdateValues(GetDefaultObject(), DateTimeOffset.MinValue);
        }

        public virtual void Start(CancellationToken token = default(CancellationToken))
        {
            Initialize();

            Task.Run(() => UpdateAsync(token));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _lock.Dispose();
            }

            _disposed = true;
        }

        private void UpdateValues(TObject value, DateTimeOffset time)
        {
            // Wait to enter the try/catch until after the update is attempted. Otherwise, the lock attempts to
            // exit a write lock that was never entered if updating failed.
            try
            {
                // Update the value in the cache.
                _lock.EnterWriteLock();

                // Dispose current object if possible 
                (_cachedObject as IDisposable)?.Dispose();

                _cachedObject = value;
                _timeStamp = time;

                Trace.TraceInformation($"Object from '{Identifier}' updated to modified date: {_timeStamp}");
            }
            catch (Exception ex)
            {
                // We catch all exceptions so that we don't crash when trying to update the catalog.
                Trace.TraceError($"An error occured while updating the cache '{Identifier}': {ex}");
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
