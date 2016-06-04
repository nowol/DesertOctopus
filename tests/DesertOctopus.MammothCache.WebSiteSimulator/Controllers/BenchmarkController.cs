using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using DesertOctopus.MammothCache.Common;
using DesertOctupos.MammothCache.Redis;

namespace DesertOctopus.MammothCache.WebSiteSimulator.Controllers
{
    public class BenchmarkController : ApiController
    {
        private static readonly RedisConnection _connection;
        private static string _redisConnectionString = "172.16.100.100";
        private static readonly IRedisRetryPolicy _redisRetryPolicy;
        private static readonly IMammothCache _cache;
        private static readonly FirstLevelCacheConfig _config = new FirstLevelCacheConfig();
        private static readonly IFirstLevelCacheCloningProvider _noCloningProvider = new NoCloningProvider();


        static BenchmarkController()
        {
            _config.AbsoluteExpiration = TimeSpan.FromSeconds(5);
            _config.MaximumMemorySize = 1000;
            _config.TimerInterval = 1;

            _redisRetryPolicy = new RedisRetryPolicy(50, 100, 150);
            _connection = new RedisConnection(_redisConnectionString, _redisRetryPolicy);

            _cache = new MammothCache(new SquirrelCache(_config, _noCloningProvider), _connection, new MammothCacheSerializationProvider());
        }


        // GET: api/Benchmark
        public async Task<IEnumerable<string>> Get(int id)
        {
            await Task.Delay(1)
                      .ConfigureAwait(false);


            return new string[] { "value1", "value2" };
        }

        //// GET: api/Benchmark/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST: api/Benchmark
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT: api/Benchmark/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE: api/Benchmark/5
        //public void Delete(int id)
        //{
        //}
    }
}
