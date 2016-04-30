using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesertOctopus.Serialization;

namespace DesertOctopus
{
    public class KrakenSerializer
    {
        public static byte[] Serialize<T>(T obj)
            where T : class
        {
            return Serializer.Serialize(obj);
        }
        
        public static T Deserialize<T>(byte[] bytes)
            where T : class
        {
            return Deserializer.Deserialize<T>(bytes);

        }
    }
}
