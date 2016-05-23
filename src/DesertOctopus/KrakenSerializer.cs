using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.Serialization;

namespace DesertOctopus
{
    /// <summary>
    /// Binary serializer
    /// </summary>
    public static class KrakenSerializer
    {
        /// <summary>
        /// Serialize an object
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="obj">Object to serialize</param>
        /// <returns>Byte array containing the serialized object</returns>
        public static byte[] Serialize<T>(T obj)
            where T : class
        {
            return Serializer.Serialize(obj);
        }

        /// <summary>
        /// Deserialize a byte array to create an object
        /// </summary>
        /// <typeparam name="T">Any reference type</typeparam>
        /// <param name="bytes">Byte array that contains the object to be deserialized</param>
        /// <returns>The deserialized array</returns>
        public static T Deserialize<T>(byte[] bytes)
            where T : class
        {
            return Deserializer.Deserialize<T>(bytes);
        }
    }
}
