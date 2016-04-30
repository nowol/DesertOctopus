using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Serialization.Helpers
{
    internal static class StreamMIH
    {
        public static MethodInfo WriteByte()
        {
            return typeof(System.IO.Stream).GetMethod("WriteByte");
        }

        public static MethodInfo Write()
        {
            return typeof(System.IO.Stream).GetMethod("Write");
        }

        public static MethodInfo ReadByte()
        {
            return typeof(System.IO.Stream).GetMethod("ReadByte");
        }

        public static MethodInfo Read()
        {
            return typeof(System.IO.Stream).GetMethod("Read");
        }
    }
}
