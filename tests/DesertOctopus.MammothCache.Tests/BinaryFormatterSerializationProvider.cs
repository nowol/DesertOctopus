using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;

namespace DesertOctopus.MammothCache.Tests
{
    public class BinaryFormatterSerializationProvider : IMammothCacheSerializationProvider
    {
        public byte[] Serialize<T>(T value) where T : class
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, value);
                return ms.ToArray();
            }
        }

        public object Deserialize(byte[] bytes)
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                return bf.Deserialize(ms);
            }
        }

        public T Deserialize<T>(byte[] bytes) where T : class
        {
            return Deserialize(bytes) as T;
        }

        public bool CanSerialize(Type type)
        {
            return type.GetCustomAttribute(typeof(SerializableAttribute)) != null;
        }
    }
}
