using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability
{
    /// <summary>
    /// This class represents a cache when the value may be updated periodically.  When we are asked for the value of the 
    /// cache, we will return the value we currently have.  Periodically, we are going to check and see if the value 
    /// should be updated by using timestamps
    /// </summary>
    public abstract class UpdatingObjectCache<TObject> : IObjectCache<TObject>
    {
        private readonly TaskCompletionSource<bool> _loadedTask = new TaskCompletionSource<bool>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly CancellationToken _cancellationToken;
        private readonly TimeSpan _cachePollInterval;
        private readonly string _identifier;

        private TObject _cachedObject;
        private DateTimeOffset _timeStamp;

        bool disposed = false;

        public UpdatingObjectCache(CancellationToken cancellationToken, TimeSpan cachePollInterval, string identifier)
        {
            _cancellationToken = cancellationToken;
            _identifier = identifier;
            _cachePollInterval = cachePollInterval;
        }

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

        protected abstract Task<DateTimeOffset> GetTimeStampAsync(CancellationToken token);

        protected abstract Task<TObject> UpdateObjectAsync(CancellationToken token);

        protected virtual TObject GetDefaultObject()
        {
            return default(TObject);
        }

        public async Task UpdateAsync()
        {
            var lastModified = await GetTimeStampAsync(_cancellationToken);

            if (lastModified > _timeStamp)
            {
                Trace.TraceInformation("A newer version of '{0}' exists!", _identifier);

                try
                {
                    var updatedItem = await UpdateObjectAsync(_cancellationToken);

                    // update the value in the cache.
                    _lock.EnterWriteLock();

                    _cachedObject = updatedItem;
                    _timeStamp = lastModified;

                    Trace.TraceInformation("Object from '{0}' updated to modified date: {1}", _identifier, _timeStamp);
                }
                catch (Exception ex)
                {
                    // We catch all exceptions so that we don't crash when trying to update the catalog.
                    Trace.TraceError("An error occured while updating the cached value: {0}", ex);
                }
                finally
                {
                    _lock.ExitWriteLock();
                    _loadedTask.TrySetResult(true);
                }
            }
        }

        protected async void Start()
        {
            _cachedObject = GetDefaultObject();
            _timeStamp = DateTimeOffset.MinValue;

            while (!_cancellationToken.IsCancellationRequested)
            {
                await UpdateAsync();

                await Task.Delay(_cachePollInterval, _cancellationToken);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                _lock.Dispose();
            }

            disposed = true;
        }
    }
}
