using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache
{
    /// <summary>
    /// Provide the serialization/deserialization of objects
    /// </summary>
    public class MammothCacheSerializationProvider : IMammothCacheSerializationProvider
    {
        /// <inheritdoc/>
        public byte[] Serialize<T>(T value)
            where T : class
        {
            return KrakenSerializer.Serialize(value);
        }

        /// <inheritdoc/>
        public object Deserialize(byte[] bytes)
        {
            return KrakenSerializer.Deserialize(bytes);
        }

        /// <inheritdoc/>
        public T Deserialize<T>(byte[] bytes)
            where T : class
        {
            return KrakenSerializer.Deserialize<T>(bytes);
        }
    }
}
