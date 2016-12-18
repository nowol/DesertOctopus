using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using DesertOctopus.Benchmark;
using DesertOctopus.Benchmark.Models;
using DesertOctopus.MammothCache.Common;
using DesertOctopus.MammothCache.Redis;

namespace DesertOctopus.MammothCache.WebSiteSimulator.Controllers
{
    public class DummyCache : IFirstLevelCache
    {
        public ConditionalResult<T> Get<T>(string key) where T : class
        {
            return ConditionalResult.CreateFailure<T>();
        }

        public void Remove(string key)
        {
            // dummy cache
        }

        public void RemoveAll()
        {
            // dummy cache
        }

        public void Set(string key,
                        byte[] serializedValue)
        {
            // dummy cache
        }

        public void Set(string key,
                        byte[] serializedValue,
                        TimeSpan? ttl)
        {
            // dummy cache
        }
    }


    public class BenchmarkController : ApiController
    {
        private static string _redisConnectionString = "172.16.100.100";
        private static readonly IMammothCache _cache;
        private static readonly FirstLevelCacheConfig _config = new FirstLevelCacheConfig();
        private static readonly INonSerializableCache _nonSerializableCache = new NonSerializableCache();


        static BenchmarkController()
        {
            _config.AbsoluteExpiration = TimeSpan.FromSeconds(5);
            _config.MaximumMemorySize = 1000;
            _config.TimerInterval = TimeSpan.FromSeconds(1);

            IRedisRetryPolicy redisRetryPolicy = new RedisRetryPolicy(50, 100, 150);
            var connection = new RedisConnection(_redisConnectionString, redisRetryPolicy);

            var firstLevelCache = new DummyCache();
            _cache = new MammothCache(firstLevelCache, connection, _nonSerializableCache, new MammothCacheSerializationProvider());
        }
    }
}
