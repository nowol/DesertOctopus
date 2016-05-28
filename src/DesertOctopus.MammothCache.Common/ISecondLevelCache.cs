using System;
using System.Linq;
using System.Threading.Tasks;

namespace DesertOctopus.MammothCache.Common
{
    public interface ISecondLevelCache
    {
        T Get<T>(string key) where T : class;
        Task<T> GetAsync<T>(string key) where T : class;

        void Set(string key, byte[] serializedValue, TimeSpan? ttl = null);
        Task SetAsync(string key, byte[] serializedValue, TimeSpan? ttl = null);

        void Remove(string key);
        Task RemoveAsync(string key);

        void RemoveAll();
        Task RemoveAllAsync();
    }
}