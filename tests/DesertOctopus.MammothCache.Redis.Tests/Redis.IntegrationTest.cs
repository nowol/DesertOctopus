using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;
using DesertOctopus.MammothCache.Redis.Tests.Models;
using DesertOctopus.MammothCache.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DesertOctopus.MammothCache.Redis.Tests
{
    [TestClass]
    public class RedisIntegrationTest : BaseTest
    {
        private CachingTestClass _testObject;
        private byte[] _serializedTestObject;
        private RedisConnection _connection;
        private IRedisRetryPolicy _redisRetryPolicy;

        [TestInitialize]
        public void Initialize()
        {
            _testObject = new CachingTestClass();
            _serializedTestObject = KrakenSerializer.Serialize(_testObject);

            _redisRetryPolicy = new RedisRetryPolicy(50, 100, 150);
            _connection = new RedisConnection(RedisConnectionString, _redisRetryPolicy);
        }

        [TestCleanup]
        public void Cleanup()
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

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetAnObjectThatDoNotExistsAsync()
        {
            Assert.IsNull(await _connection.GetAsync(RandomKey()).ConfigureAwait(false));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void GetAnObjectThatDoNotExistsSync()
        {
            Assert.IsNull(_connection.Get(RandomKey()));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SetAnObjectWithTtlAsync()
        {
            var key = RandomKey();
            await _connection.SetAsync(key, _serializedTestObject, ttl: TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            var redisVal = await _connection.GetAsync(key).ConfigureAwait(false);
            Assert.IsNotNull(redisVal);
            var obj = KrakenSerializer.Deserialize<CachingTestClass>(redisVal);
            Assert.AreEqual(_testObject.Value, obj.Value);
            Assert.IsTrue((await _connection.GetTimeToLiveAsync(key).ConfigureAwait(false)).TimeToLive > TimeSpan.FromSeconds(25));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void SetAnObjectWithTtlSync()
        {
            var key = RandomKey();
            _connection.Set(key, _serializedTestObject, ttl: TimeSpan.FromSeconds(30));
            var redisVal = _connection.Get(key);
            Assert.IsNotNull(redisVal);
            var obj = KrakenSerializer.Deserialize<CachingTestClass>(redisVal);
            Assert.AreEqual(_testObject.Value, obj.Value);
            Assert.IsTrue(_connection.GetTimeToLive(key).TimeToLive > TimeSpan.FromSeconds(25));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task SetAnObjectWithoutTtlAsync()
        {
            var key = RandomKey();
            try
            {
                await _connection.SetAsync(key, _serializedTestObject).ConfigureAwait(false);
                var redisVal = await _connection.GetAsync(key).ConfigureAwait(false);
                Assert.IsNotNull(redisVal);
                var obj = KrakenSerializer.Deserialize<CachingTestClass>(redisVal);
                Assert.AreEqual(_testObject.Value, obj.Value);
                Assert.IsNull((await _connection.GetTimeToLiveAsync(key).ConfigureAwait(false)).TimeToLive);
            }
            finally
            {
                _connection.Remove(key);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void SetAnObjectWithoutTtlSync()
        {
            var key = RandomKey();
            try
            {
                _connection.Set(key, _serializedTestObject);
                var redisVal = _connection.Get(key);
                Assert.IsNotNull(redisVal);
                var obj = KrakenSerializer.Deserialize<CachingTestClass>(redisVal);
                Assert.AreEqual(_testObject.Value, obj.Value);
                Assert.IsNull(_connection.GetTimeToLive(key).TimeToLive);
            }
            finally
            {
                _connection.Remove(key);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task RemoveAnObjectAsync()
        {
            var key = RandomKey();
            await _connection.SetAsync(key, _serializedTestObject, ttl: TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            var redisVal = await _connection.GetAsync(key).ConfigureAwait(false);
            Assert.IsNotNull(redisVal);
            await _connection.RemoveAsync(key).ConfigureAwait(false);
            redisVal = await _connection.GetAsync(key).ConfigureAwait(false);
            Assert.IsNull(redisVal);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void RemoveAnObjectSync()
        {
            var key = RandomKey();
            _connection.Set(key, _serializedTestObject, ttl: TimeSpan.FromSeconds(30));
            var redisVal = _connection.Get(key);
            Assert.IsNotNull(redisVal);
            _connection.Remove(key);
            redisVal = _connection.Get(key);
            Assert.IsNull(redisVal);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task GetTheConfigAsync()
        {
            var config = await _connection.GetConfigAsync(null).ConfigureAwait(false);
            Assert.AreNotEqual(0, config.Length);

            config = await _connection.GetConfigAsync(pattern: config[0].Key).ConfigureAwait(false);
            Assert.AreEqual(1, config.Length);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void GetTheConfigSync()
        {
            var config = _connection.GetConfig(null);
            Assert.AreNotEqual(0, config.Length);

            config = _connection.GetConfig(pattern: config[0].Key);
            Assert.AreEqual(1, config.Length);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void DisposingTheConnectionTwiceShouldThrowAnException()
        {
            _connection.Dispose();
            _connection.Dispose();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task AcquiringAndReleasingALockShouldCreateTheKeyInRedisAsync()
        {
            var key = RandomKey();
            using (await _connection.AcquireLockAsync(key, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30)).ConfigureAwait(false))
            {
                Assert.IsTrue(await _connection.KeyExistsAsync("DistributedLock:" + key).ConfigureAwait(false));
            }
            Assert.IsFalse(_connection.KeyExists("DistributedLock:" + key));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void AcquiringAndReleasingALockShouldCreateTheKeyInRedisSync()
        {
            var key = RandomKey();
            using (_connection.AcquireLock(key, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30)))
            {
                Assert.IsTrue(_connection.KeyExists("DistributedLock:" + key));
            }
            Assert.IsFalse(_connection.KeyExists("DistributedLock:" + key));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task LockExpireAfterAGivenTimeAsync()
        {
            var key = RandomKey();
            using (await _connection.AcquireLockAsync(key, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(30)).ConfigureAwait(false))
            {
                Assert.IsTrue(_connection.KeyExists("DistributedLock:" + key));

                WaitFor(5);

                Assert.IsFalse(_connection.KeyExists("DistributedLock:" + key));
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void LockExpireAfterAGivenTimeSync()
        {
            var key = RandomKey();
            using (_connection.AcquireLock(key, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(30)))
            {
                Assert.IsTrue(_connection.KeyExists("DistributedLock:" + key));

                WaitFor(5);

                Assert.IsFalse(_connection.KeyExists("DistributedLock:" + key));
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [ExpectedException(typeof(UnableToAcquireLockException))]
        public async Task AcquiringALockASecondTimeWillThrowAnExceptionIfTheLockCannotBeAcquiredAsync()
        {
            var key = RandomKey();
            using (await _connection.AcquireLockAsync(key, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30)).ConfigureAwait(false))
            {
                var lock2 = await _connection.AcquireLockAsync(key, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        [ExpectedException(typeof(UnableToAcquireLockException))]
        public void AcquiringALockASecondTimeWillThrowAnExceptionIfTheLockCannotBeAcquiredSync()
        {
            var key = RandomKey();
            using (_connection.AcquireLock(key, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30)))
            {
                var lock2 = _connection.AcquireLock(key, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2));
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
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
                                 Assert.IsFalse(inLock);
                                 inLock = true;
                                 Thread.Sleep(1000);
                                 inLock = false;
                             }
                         });
            Assert.IsFalse(inLock);
            Assert.AreEqual(10, counter);
        }
    }
}
