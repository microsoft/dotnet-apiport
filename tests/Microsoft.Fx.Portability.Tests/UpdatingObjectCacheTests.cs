using Xunit;
using NSubstitute;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Fx.Portability.Tests
{
    public class UpdatingObjectCacheTests
    {
        [Fact]
        public async Task InitialUpdateOccurs()
        {
            var dt = new DateTimeOffset(1234, TimeSpan.FromHours(0));
            var methods = Substitute.For<IUpdateMethods<int>>();

            var initialValue = 0;
            var updatedValue = 1;

            var task = new TaskCompletionSource<bool>();

            methods.GetDefaultObject().Returns(a => initialValue);
            methods.GetTimeStampAsync(Arg.Any<CancellationToken>()).Returns(async a =>
            {
                // Don't update object until after we verify that the initial value was set
                await task.Task;

                return dt;
            });
            methods.UpdateObjectAsync(Arg.Any<CancellationToken>()).Returns(a => Task.FromResult(updatedValue));

            using (var cache = new UpdatingObjectCacheImpl<int>(methods, CancellationToken.None, TimeSpan.MaxValue, String.Empty))
            {
                Assert.Equal(initialValue, cache.Value);
                Assert.Equal(DateTimeOffset.MinValue, cache.LastUpdated);

                task.SetResult(true);

                await cache.WaitForInitialLoadAsync();

                Assert.Equal(updatedValue, cache.Value);
                Assert.Equal(dt, cache.LastUpdated);
            }
        }

        [Fact]
        public async Task SecondUpdateOccurs()
        {
            var dt1 = new DateTimeOffset(1234, TimeSpan.FromHours(0));
            var dt2 = dt1.AddDays(1);
            var methods = Substitute.For<IUpdateMethods<int>>();

            var initialValue = 0;
            var updatedValue1 = 1;
            var updatedValue2 = 1;

            var count = 0;

            var task = new TaskCompletionSource<bool>();
            var completed = new TaskCompletionSource<bool>();

            methods.GetDefaultObject().Returns(a => initialValue);
            methods.GetTimeStampAsync(Arg.Any<CancellationToken>()).Returns(async a =>
            {
                if (count == 0)
                {
                    return dt1;
                }
                else if (count == 1)
                {
                    await task.Task;

                    return dt2;
                }
                else
                {
                    completed.SetResult(true);

                    await Task.Delay(10000);

                    Assert.True(false, "Should only wait for 1 update");

                    return default(DateTimeOffset);
                }
            });

            methods.UpdateObjectAsync(Arg.Any<CancellationToken>()).Returns(async a =>
            {
                if (count == 0)
                {
                    count++;

                    return updatedValue1;
                }
                else if (count == 1)
                {
                    count++;

                    await task.Task;

                    return updatedValue2;
                }
                else
                {
                    Assert.True(false, "Should only wait for 1 update");

                    return default(int);
                }
            });

            using (var cache = new UpdatingObjectCacheImpl<int>(methods, CancellationToken.None, TimeSpan.FromMilliseconds(0), String.Empty))
            {
                await cache.WaitForInitialLoadAsync();

                Assert.Equal(updatedValue1, cache.Value);
                Assert.Equal(dt1, cache.LastUpdated);

                task.SetResult(true);

                await completed.Task;

                Assert.Equal(updatedValue2, cache.Value);
                Assert.Equal(dt2, cache.LastUpdated);
            }
        }

        public interface IUpdateMethods<T>
        {
            T GetDefaultObject();
            Task<DateTimeOffset> GetTimeStampAsync(CancellationToken token);
            Task<T> UpdateObjectAsync(CancellationToken token);
        }

        private class UpdatingObjectCacheImpl<T> : UpdatingObjectCache<T>
        {
            private readonly IUpdateMethods<T> _methods;

            public UpdatingObjectCacheImpl(IUpdateMethods<T> methods, CancellationToken cancellationToken, TimeSpan cachePollInterval, string identifier)
                : base(cancellationToken, cachePollInterval, identifier)
            {
                _methods = methods;

                Start();
            }

            protected override T GetDefaultObject()
            {
                return _methods.GetDefaultObject();
            }

            protected override Task<DateTimeOffset> GetTimeStampAsync(CancellationToken token)
            {
                return _methods.GetTimeStampAsync(token);
            }

            protected override Task<T> UpdateObjectAsync(CancellationToken token)
            {
                return _methods.UpdateObjectAsync(token);
            }
        }
    }
}
