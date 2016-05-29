using System;
using System.Linq;
using System.Threading.Tasks;

namespace DesertOctopus.MammothCache.Common
{
    public delegate void ItemEvictedFromCacheEventHandler(string key);

    public interface ISecondLevelCache
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

    }
}