using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;
using StackExchange.Redis;

namespace DesertOctupos.MammothCache.Redis
{
    public interface IRedisConnection
    {
        byte[] Get(string key);
        Task<byte[]> GetAsync(string key);

        void Set(string key, byte[] serializedValue, TimeSpan? ttl = null);
        Task SetAsync(string key, byte[] serializedValue, TimeSpan? ttl = null);

        bool Remove(string key);
        Task<bool> RemoveAsync(string key);

        void RemoveAll();
        Task RemoveAllAsync();

        TimeSpan? GetTimeToLive(string key);
        Task<TimeSpan?> GetTimeToLiveAsync(string key);

        event ItemEvictedFromCacheEventHandler OnItemRemovedFromCache;

        KeyValuePair<string, string>[] GetConfig(string pattern);
        Task<KeyValuePair<string, string>[]> GetConfigAsync(string pattern);
    }
}