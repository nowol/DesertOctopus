using System;
using System.Collections.Generic;
using System.Linq;

namespace DesertOctopus.Benchmark.Models
{
    public interface ILocalizable
    {
        int Count { get; }
        IEnumerable<string> Cultures { get; }
    }
}