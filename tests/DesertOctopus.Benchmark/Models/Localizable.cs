using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace DesertOctopus.Benchmark.Models
{
    [Serializable]
    public static class Localizable
    {
    }

    [Serializable]
    public class Localizable<T> : ILocalizable
    {
        private IDictionary<string, T> _values;

        public Localizable()
        {
            Values = new Dictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);
        }
        
        [IgnoreDataMember]
        public IEnumerable<string> Cultures
        {
            get { return Values.Keys; }
        }

        public IDictionary<string, T> Values
        {
            get { return _values; }
            set
            {
                if (value != null)
                {
                    _values = new Dictionary<string, T>(value,
                                                        StringComparer.InvariantCultureIgnoreCase);
                }
                else
                {
                    _values = new Dictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);
                }
            }
        }

        public int Count
        {
            get { return Values.Count; }
        }
    }
}
