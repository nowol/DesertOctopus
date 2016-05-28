using System;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace DesertOctupos.MammothCache.Redis
{
    public interface IRedisConnection
    {
        RedisValue Get(string key);
        Task<RedisValue> GetAsync(string key);

        void Set(string key, byte[] serializedValue, TimeSpan? ttl = null);
        Task SetAsync(string key, byte[] serializedValue, TimeSpan? ttl = null);

        bool Remove(string key);
        Task<bool> RemoveAsync(string key);

        void RemoveAll();
        Task RemoveAllAsync();

        TimeSpan? GetTimeToLive(string key);
        Task<TimeSpan?> GetTimeToLiveAsync(string key);
    }
}