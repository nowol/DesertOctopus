using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache
{
    public class MammothCacheSerializationProvider : IMammothCacheSerializationProvider
    {
        public byte[] Serialize<T>(T value) where T : class
        {
            return KrakenSerializer.Serialize(value);
        }

        public object Deserialize(byte[] bytes)
        {
            return KrakenSerializer.Deserialize(bytes);
        }

        public T Deserialize<T>(byte[] bytes) where T : class
        {
            return KrakenSerializer.Deserialize<T>(bytes);
        }
    }
}
