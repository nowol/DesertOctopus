using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class DeserializerMIH
    {
        public static MethodInfo ConvertObjectToIQueryable()
        {
            return typeof(Deserializer).GetMethod("ConvertObjectToIQueryable", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public static MethodInfo GetTypeDeserializer()
        {
            return typeof(Deserializer).GetMethod("GetTypeDeserializer", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public static MethodInfo GetTrackedObject()
        {
            return typeof(Deserializer).GetMethod("GetTrackedObject", BindingFlags.NonPublic | BindingFlags.Static, null, new []{ typeof(List<object>), typeof(int) }, new ParameterModifier[0]);
        }
    }
}
