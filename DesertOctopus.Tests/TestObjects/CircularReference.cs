using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerializerTests.TestObjects
{
    [Serializable]
    public class CircularReference
    {
        public int Id { get; set; }
        public int[] Ids { get; set; }
        public CircularReference Parent { get; set; }
        public CircularReference Child { get; set; }
    }
}
