using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private static readonly ConcurrentDictionary<Type, bool> _canTypeBeSerialized = new ConcurrentDictionary<Type, bool>();

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

        /// <inheritdoc/>
        public bool CanSerialize(Type type)
        {
            return _canTypeBeSerialized.GetOrAdd(type, CanTypeBeSerialize);
        }

        private bool CanTypeBeSerialize(Type type)
        {
            if (type.GetCustomAttribute<NotSerializableAttribute>(true) != null)
            {
                return false;
            }

            if (type.Namespace != null
                && type.GetCustomAttribute<SerializableAttribute>(true) == null
                && (type.Namespace.StartsWith("System.") || type.Namespace.StartsWith("Microsoft.")))
            {
                return false;
            }

            return true;
        }
    }
}
