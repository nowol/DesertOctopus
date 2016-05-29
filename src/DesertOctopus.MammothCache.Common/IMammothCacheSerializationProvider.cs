using System;
using System.Linq;

namespace DesertOctopus.MammothCache.Common
{
    public interface IMammothCacheSerializationProvider
    {
        byte[] Serialize<T>(T value) where T : class;
        object Deserialize(byte[] bytes);
        T Deserialize<T>(byte[] bytes) where T : class;
    }
}