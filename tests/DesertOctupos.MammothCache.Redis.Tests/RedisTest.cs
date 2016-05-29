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
            Assert.IsNull(await _connection.GetAsync(RandomKey()).ConfigureAwait(false));
        }

        [TestMethod]
        public void GetAnObjectThatDoNotExistsSync()
        {
            Assert.IsNull(_connection.Get(RandomKey()));
        }

        [TestMethod]
        public async Task SetAnObjectWithTtlAsync()
        {
            var key = RandomKey();
            await _connection.SetAsync(key, _serializedTestObject, ttl: TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            var redisVal = await _connection.GetAsync(key).ConfigureAwait(false);
            Assert.IsNotNull(redisVal);
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
            Assert.IsNotNull(redisVal);
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
                Assert.IsNotNull(redisVal);
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
                Assert.IsNotNull(redisVal);
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
            Assert.IsNotNull(redisVal);
            await _connection.RemoveAsync(key).ConfigureAwait(false);
            redisVal = await _connection.GetAsync(key).ConfigureAwait(false);
            Assert.IsNull(redisVal);
        }

        [TestMethod]
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
        public async Task GetTheConfigAsync()
        {
            var config = await _connection.GetConfigAsync().ConfigureAwait(false);
            Assert.AreNotEqual(0, config.Length);

            config = await _connection.GetConfigAsync(pattern: config[0].Key).ConfigureAwait(false);
            Assert.AreEqual(1, config.Length);
        }

        [TestMethod]
        public void GetTheConfigSync()
        {
            var config = _connection.GetConfig();
            Assert.AreNotEqual(0, config.Length);

            config = _connection.GetConfig(pattern: config[0].Key);
            Assert.AreEqual(1, config.Length);
        }

    }
}
