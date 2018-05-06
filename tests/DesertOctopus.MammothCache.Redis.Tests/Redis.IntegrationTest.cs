using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;
using DesertOctopus.MammothCache.Redis.Tests.Models;
using DesertOctopus.MammothCache.Tests;
using Xunit;

namespace DesertOctopus.MammothCache.Redis.Tests
{
    public class RedisIntegrationTest : BaseTest, IDisposable
    {
        private CachingTestClass _testObject;
        private byte[] _serializedTestObject;
        private RedisConnection _connection;
        private IRedisRetryPolicy _redisRetryPolicy;

        public RedisIntegrationTest()
        {
            _testObject = new CachingTestClass();
            _serializedTestObject = KrakenSerializer.Serialize(_testObject);

            _redisRetryPolicy = new RedisRetryPolicy(50, 100, 150);
            _connection = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
        }

        public void Dispose()
        {
            try
            {
                _connection.RemoveAll();
                _connection.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetAnObjectThatDoNotExistsAsync()
        {
            Assert.Null(await _connection.GetAsync(RandomKey()).ConfigureAwait(false));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetAnObjectThatDoNotExistsSync()
        {
            Assert.Null(_connection.Get(RandomKey()));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task SetAnObjectWithTtlAsync()
        {
            var key = RandomKey();
            await _connection.SetAsync(key, _serializedTestObject, ttl: TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            var redisVal = await _connection.GetAsync(key).ConfigureAwait(false);
            Assert.NotNull(redisVal);
            var obj = KrakenSerializer.Deserialize<CachingTestClass>(redisVal);
            Assert.Equal(_testObject.Value, obj.Value);
            Assert.True((await _connection.GetTimeToLiveAsync(key).ConfigureAwait(false)).TimeToLive > TimeSpan.FromSeconds(25));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void SetAnObjectWithTtlSync()
        {
            var key = RandomKey();
            _connection.Set(key, _serializedTestObject, ttl: TimeSpan.FromSeconds(30));
            var redisVal = _connection.Get(key);
            Assert.NotNull(redisVal);
            var obj = KrakenSerializer.Deserialize<CachingTestClass>(redisVal);
            Assert.Equal(_testObject.Value, obj.Value);
            Assert.True(_connection.GetTimeToLive(key).TimeToLive > TimeSpan.FromSeconds(25));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task SetAnObjectWithoutTtlAsync()
        {
            var key = RandomKey();
            try
            {
                await _connection.SetAsync(key, _serializedTestObject).ConfigureAwait(false);
                var redisVal = await _connection.GetAsync(key).ConfigureAwait(false);
                Assert.NotNull(redisVal);
                var obj = KrakenSerializer.Deserialize<CachingTestClass>(redisVal);
                Assert.Equal(_testObject.Value, obj.Value);
                Assert.Null((await _connection.GetTimeToLiveAsync(key).ConfigureAwait(false)).TimeToLive);
            }
            finally
            {
                _connection.Remove(key);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void SetAnObjectWithoutTtlSync()
        {
            var key = RandomKey();
            try
            {
                _connection.Set(key, _serializedTestObject);
                var redisVal = _connection.Get(key);
                Assert.NotNull(redisVal);
                var obj = KrakenSerializer.Deserialize<CachingTestClass>(redisVal);
                Assert.Equal(_testObject.Value, obj.Value);
                Assert.Null(_connection.GetTimeToLive(key).TimeToLive);
            }
            finally
            {
                _connection.Remove(key);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task RemoveAnObjectAsync()
        {
            var key = RandomKey();
            await _connection.SetAsync(key, _serializedTestObject, ttl: TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            var redisVal = await _connection.GetAsync(key).ConfigureAwait(false);
            Assert.NotNull(redisVal);
            await _connection.RemoveAsync(key).ConfigureAwait(false);
            redisVal = await _connection.GetAsync(key).ConfigureAwait(false);
            Assert.Null(redisVal);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void RemoveAnObjectSync()
        {
            var key = RandomKey();
            _connection.Set(key, _serializedTestObject, ttl: TimeSpan.FromSeconds(30));
            var redisVal = _connection.Get(key);
            Assert.NotNull(redisVal);
            _connection.Remove(key);
            redisVal = _connection.Get(key);
            Assert.Null(redisVal);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task GetTheConfigAsync()
        {
            var config = await _connection.GetConfigAsync(null).ConfigureAwait(false);
            Assert.NotEmpty(config);

            config = await _connection.GetConfigAsync(pattern: config[0].Key).ConfigureAwait(false);
            Assert.Single(config);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void GetTheConfigSync()
        {
            var config = _connection.GetConfig(null);
            Assert.NotEmpty(config);

            config = _connection.GetConfig(pattern: config[0].Key);
            Assert.Single(config);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void DisposingTheConnectionTwiceShouldThrowAnException()
        {
            _connection.Dispose();
            Assert.Throws<ObjectDisposedException>(() => _connection.Dispose());
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task AcquiringAndReleasingALockShouldCreateTheKeyInRedisAsync()
        {
            var key = RandomKey();
            using (await _connection.AcquireLockAsync(key, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30)).ConfigureAwait(false))
            {
                Assert.True(await _connection.KeyExistsAsync("DistributedLock:" + key).ConfigureAwait(false));
            }
            Assert.False(_connection.KeyExists("DistributedLock:" + key));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void AcquiringAndReleasingALockShouldCreateTheKeyInRedisSync()
        {
            var key = RandomKey();
            using (_connection.AcquireLock(key, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30)))
            {
                Assert.True(_connection.KeyExists("DistributedLock:" + key));
            }
            Assert.False(_connection.KeyExists("DistributedLock:" + key));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task LockExpireAfterAGivenTimeAsync()
        {
            var key = RandomKey();
            using (await _connection.AcquireLockAsync(key, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(30)).ConfigureAwait(false))
            {
                Assert.True(_connection.KeyExists("DistributedLock:" + key));

                WaitFor(5);

                Assert.False(_connection.KeyExists("DistributedLock:" + key));
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void LockExpireAfterAGivenTimeSync()
        {
            var key = RandomKey();
            using (_connection.AcquireLock(key, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(30)))
            {
                Assert.True(_connection.KeyExists("DistributedLock:" + key));

                WaitFor(5);

                Assert.False(_connection.KeyExists("DistributedLock:" + key));
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task AcquiringALockASecondTimeWillThrowAnExceptionIfTheLockCannotBeAcquiredAsync()
        {
            var key = RandomKey();
            using (await _connection.AcquireLockAsync(key, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30)).ConfigureAwait(false))
            {
                await Assert.ThrowsAsync<UnableToAcquireLockException>(() => _connection.AcquireLockAsync(key, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2))).ConfigureAwait(false);
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void AcquiringALockASecondTimeWillThrowAnExceptionIfTheLockCannotBeAcquiredSync()
        {
            var key = RandomKey();
            using (_connection.AcquireLock(key, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30)))
            {
                Assert.Throws<UnableToAcquireLockException>(() => _connection.AcquireLock(key, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2)));
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public void AcquireLockInDifferentThreadsAsync()
        {
            var key = RandomKey();
            int counter = 0;
            bool inLock = false;
            Parallel.For(0,
                         10,
                         i =>
                         {
                             using (_connection.AcquireLock(key, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30)))
                             {
                                 Interlocked.Increment(ref counter);
                                 Assert.False(inLock);
                                 inLock = true;
                                 Thread.Sleep(1000);
                                 inLock = false;
                             }
                         });
            Assert.False(inLock);
            Assert.Equal(10, counter);
        }
    }
}
