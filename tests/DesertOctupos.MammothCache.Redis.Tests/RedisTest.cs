using System;
using System.Threading.Tasks;
using DesertOctopus;
using DesertOctupos.MammothCache.Redis.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;

namespace DesertOctupos.MammothCache.Redis.Tests
{
    [TestClass]
    public class RedisTest
    {
        private CachingTestClass _testObject;
        private byte[] _serializedTestObject;
        private RedisConnection _connection;
        private string _redisConnectionString = "172.16.100.100";
        private IRedisRetryPolicy _redisRetryPolicy;

        [TestInitialize]
        public void Initialize()
        {
            _testObject = new CachingTestClass();
            _serializedTestObject = KrakenSerializer.Serialize(_testObject);

            _redisRetryPolicy = new RedisRetryPolicy(50, 100, 150);
            _connection = new RedisConnection(_redisConnectionString, _redisRetryPolicy);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _connection.RemoveAll();
            _connection.Dispose();
        }

        private static string RandomKey()
        {
            return Guid.NewGuid().ToString();
        }

        [TestMethod]
        public async Task GetAnObjectThatDoNotExistsAsync()
        {
            Assert.IsFalse((await _connection.GetAsync(RandomKey()).ConfigureAwait(false)).HasValue);
        }

        [TestMethod]
        public void GetAnObjectThatDoNotExistsSync()
        {
            Assert.IsFalse(_connection.Get(RandomKey()).HasValue);
        }

        [TestMethod]
        public async Task SetAnObjectWithTtlAsync()
        {
            var key = RandomKey();
            await _connection.SetAsync(key, _serializedTestObject, ttl: TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            var redisVal = await _connection.GetAsync(key).ConfigureAwait(false);
            Assert.IsTrue(redisVal.HasValue);
            var obj = KrakenSerializer.Deserialize<CachingTestClass>(redisVal);
            Assert.AreEqual(_testObject.Value, obj.Value);
            Assert.IsTrue((await _connection.GetTimeToLiveAsync(key).ConfigureAwait(false)) > TimeSpan.FromSeconds(25));
        }

        [TestMethod]
        public void SetAnObjectWithTtlSync()
        {
            var key = RandomKey();
            _connection.Set(key, _serializedTestObject, ttl: TimeSpan.FromSeconds(30));
            var redisVal = _connection.Get(key);
            Assert.IsTrue(redisVal.HasValue);
            var obj = KrakenSerializer.Deserialize<CachingTestClass>(redisVal);
            Assert.AreEqual(_testObject.Value, obj.Value);
            Assert.IsTrue(_connection.GetTimeToLive(key) > TimeSpan.FromSeconds(25));
        }

        [TestMethod]
        public async Task SetAnObjectWithoutTtlAsync()
        {
            var key = RandomKey();
            try
            {
                await _connection.SetAsync(key, _serializedTestObject).ConfigureAwait(false);
                var redisVal = await _connection.GetAsync(key).ConfigureAwait(false);
                Assert.IsTrue(redisVal.HasValue);
                var obj = KrakenSerializer.Deserialize<CachingTestClass>(redisVal);
                Assert.AreEqual(_testObject.Value, obj.Value);
                Assert.IsNull(await _connection.GetTimeToLiveAsync(key).ConfigureAwait(false));
            }
            finally
            {
                _connection.Remove(key);
            }
        }

        [TestMethod]
        public void SetAnObjectWithoutTtlSync()
        {
            var key = RandomKey();
            try
            {
                _connection.Set(key, _serializedTestObject);
                var redisVal = _connection.Get(key);
                Assert.IsTrue(redisVal.HasValue);
                var obj = KrakenSerializer.Deserialize<CachingTestClass>(redisVal);
                Assert.AreEqual(_testObject.Value, obj.Value);
                Assert.IsNull(_connection.GetTimeToLive(key));
            }
            finally
            {
                _connection.Remove(key);
            }
        }

        [TestMethod]
        public async Task RemoveAnObjectAsync()
        {
            var key = RandomKey();
            await _connection.SetAsync(key, _serializedTestObject, ttl: TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            var redisVal = await _connection.GetAsync(key).ConfigureAwait(false);
            Assert.IsTrue(redisVal.HasValue);
            await _connection.RemoveAsync(key).ConfigureAwait(false);
            redisVal = await _connection.GetAsync(key).ConfigureAwait(false);
            Assert.IsFalse(redisVal.HasValue);
        }

        [TestMethod]
        public void RemoveAnObjectSync()
        {
            var key = RandomKey();
            _connection.Set(key, _serializedTestObject, ttl: TimeSpan.FromSeconds(30));
            var redisVal = _connection.Get(key);
            Assert.IsTrue(redisVal.HasValue);
            _connection.Remove(key);
            redisVal = _connection.Get(key);
            Assert.IsFalse(redisVal.HasValue);
        }

    }
}
