## Serialization / DesertOctopus.KrakenSerializer

The main use of this serializer is to serialize DTOs using binary serialization.  The serialization engine is implemented using expression trees which mean a small performance hit the first type you serialize a type for the creation of the expression tree.  Subsequent serialization for the same type can use the compiled expression tree without having to suffer the performance hit.

The main pros of DesertOctopus.KrakenSerializer are:

* Binary serialization for use in remote caching server scenarios
* Does not require objects to be decorated with the `Serializable` attribute
  * It is up to the user of the serializer to ensure that the objects can be safely serialized
* Serialize all fields (private, public, etc) of an object
* Thread safe
* Supports interface `ISerializable` to allow dictionaries to be correctly serialized
* Respect the `NotSerialized` attribute
* Supported types:
  * All primitive types
  * Multi dimensional arrays
  * Jagged arrays
  * Class / Struct
  * ExpandoObject
  * Basic support for `IEnumerable<>` and `IQueryable<>`
    * Don't go crazy and try to serialize a `GroupedEnumerable` or something similar
* No need to know the type of the object to deserialize. The object's type is embedded in the serialized payload.
* Automatic abort if an object's definition (e.g.: number of fields, name of a field, etc) was modified.
* Can handle circular references
  * There is one case that is not supported: a dictionary with a reference to itself
* Good unit tests
* You can say "Release the Kraken" when you serialize something

The mains cons are:

* It's not the fastest serializer around. Supporting the above features comes at the price of speed/cpu usage.
* Including the objects' types increase the size of the payload however special attention has been made to minimize the size of serialized object.

### Usage


```csharp

// serialize an object
var objectToSerialize = new MyDto();
var serializedBytes = KrakenSerializer.Serialize(objectToSerialize);

// deserialize the object
var deserializedObject = KrakenSerializer.Deserialize<MyDto>(serializedBytes);

```

### Benchmark


This benchmark serialize and deserialize a fairly large object containing array, lists and dictionaries.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=ProductSerializationBenchMark  Mode=Throughput  

```
                         Method |      Median |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |------------ |---------- |------- |------ |------ |------------------- |
              JsonSerialization |  91.1894 us | 1.5511 us | 132.41 |  0.12 |     - |          22 507.13 |
            JsonDeserialization | 171.4661 us | 1.1531 us | 296.53 |  0.24 |     - |          43 474.75 |
              OmniSerialization | 195.0922 us | 0.8069 us | 353.61 |  0.49 |     - |          54 677.04 |
            OmniDeserialization |  83.4611 us | 1.5536 us | 130.21 |  0.12 |     - |          18 384.34 |
            KrakenSerialization | 271.6643 us | 2.3709 us | 284.98 |  0.50 |     - |          44 878.25 |
          KrakenDeserialization | 151.5688 us | 0.9349 us | 167.77 |  0.24 |     - |          23 913.80 |
   BinaryFormatterSerialization | 436.2790 us | 1.8996 us | 648.00 |  1.00 |     - |         101 552.54 |
 BinaryFormatterDeserialization | 396.6432 us | 2.0647 us | 557.61 |  0.98 |     - |          79 863.58 |


This benchmark serialize and deserialize a normal sized object that contains all primitives types.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=SimpleDtoWithEveryPrimitivesSerializationBenchmark  Mode=Throughput  

```
                         Method |     Median |    StdDev |    Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |----------- |---------- |--------- |------ |------ |------------------- |
              JsonSerialization | 15.5787 us | 0.0559 us |   536.66 |     - |     - |           4 540.28 |
            JsonDeserialization | 14.8244 us | 0.0885 us |   234.24 |     - |     - |           1 879.97 |
              OmniSerialization |  1.6607 us | 0.0137 us |    73.63 |     - |     - |             579.52 |
            OmniDeserialization |  1.1015 us | 0.0068 us |    31.73 |     - |     - |             252.19 |
            KrakenSerialization |  2.8730 us | 0.0200 us |   137.22 |     - |     - |           1 082.71 |
          KrakenDeserialization |  1.7128 us | 0.0179 us |    68.82 |     - |     - |             543.89 |
   BinaryFormatterSerialization | 41.8114 us | 0.2076 us | 1,317.00 |     - |     - |          10 574.64 |
 BinaryFormatterDeserialization | 23.4455 us | 0.1193 us |   800.52 |     - |     - |           6 311.37 |


This benchmark serialize and deserialize an array of 100000 ints.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=IntArraySerializationBenchmark  Mode=Throughput  

```
                         Method |         Median |      StdDev |  Gen 0 | Gen 1 |  Gen 2 | Bytes Allocated/Op |
------------------------------- |--------------- |------------ |------- |------ |------- |------------------- |
              JsonSerialization |  9,916.6900 us |  76.8526 us | 598.49 | 18.75 | 180.91 |       3 672 381.57 |
            JsonDeserialization | 13,217.9225 us | 107.6409 us | 675.00 | 20.00 | 145.00 |       4 095 724.28 |
              OmniSerialization |  2,006.6253 us |  17.4475 us |  12.44 |     - | 101.15 |         591 681.52 |
            OmniDeserialization |  1,514.4841 us |   9.5927 us |   0.13 |     - |  32.66 |         176 443.53 |
            KrakenSerialization |  2,356.2991 us |  19.3221 us |  13.10 |     - | 104.67 |         654 817.45 |
          KrakenDeserialization |  1,146.8531 us |   7.7147 us |   0.13 |     - |  30.01 |         162 817.11 |
   BinaryFormatterSerialization |    522.2847 us |  15.7257 us |  13.95 |  0.52 | 110.91 |         667 790.43 |
 BinaryFormatterDeserialization |    148.2286 us |   2.7969 us |   0.60 |  0.02 |  30.60 |         168 870.13 |


This benchmark serialize and deserialize an array of 100000 doubles.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=DoubleArraySerializationBenchmark  Mode=Throughput  

```
                         Method |         Median |      StdDev |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |--------------- |------------ |------- |------ |------ |------------------- |
              JsonSerialization | 60,804.2101 us | 416.6070 us | 412.00 |  2.00 |  4.00 |       9 120 398.53 |
            JsonDeserialization | 21,233.6126 us | 131.4088 us | 177.52 |  0.54 | 38.29 |       4 970 398.82 |
              OmniSerialization |  5,736.3490 us |  35.2949 us |   2.61 |  0.07 | 22.31 |       1 240 979.49 |
            OmniDeserialization |  3,638.7557 us |  37.9583 us |   0.19 |  0.06 |  6.88 |         358 727.75 |
            KrakenSerialization |  5,556.3926 us |  41.5983 us |   2.67 |  0.07 | 26.13 |       1 225 933.85 |
          KrakenDeserialization |  2,988.0385 us |  57.4621 us |   0.10 |  0.03 |  7.92 |         308 371.45 |
   BinaryFormatterSerialization |  1,095.2116 us |  27.6652 us |   3.85 |  0.03 | 34.69 |       1 349 587.00 |
 BinaryFormatterDeserialization |    308.2330 us |   6.1332 us |   0.18 |  0.00 | 10.70 |         356 473.30 |


This benchmark serialize and deserialize an array of 100000 decimals.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=DecimalArraySerializationBenchmark  Mode=Throughput  

```
                         Method |     Median |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |----------- |---------- |------- |------ |------ |------------------- |
              JsonSerialization | 25.6023 ms | 0.2533 ms | 386.06 |  2.76 | 42.28 |       5 818 892.77 |
            JsonDeserialization | 34.2993 ms | 0.1731 ms | 330.91 |  2.76 | 38.61 |       6 003 839.74 |
              OmniSerialization |  5.6500 ms | 0.0500 ms | 200.08 |  8.54 | 43.21 |       2 995 172.49 |
            OmniDeserialization |  4.9783 ms | 0.1040 ms |   0.91 |  0.13 | 11.91 |         679 137.25 |
            KrakenSerialization | 10.7961 ms | 0.0740 ms | 194.65 |  4.46 | 52.32 |       4 107 401.90 |
          KrakenDeserialization |  8.7222 ms | 0.0476 ms |   1.63 |  0.23 | 10.24 |         730 130.36 |
   BinaryFormatterSerialization | 35.8411 ms | 0.2737 ms | 401.00 |  2.00 | 43.00 |       6 033 551.80 |
 BinaryFormatterDeserialization | 44.9421 ms | 0.3513 ms | 301.49 |  1.58 |  3.17 |       3 295 672.99 |


This benchmark serialize and deserialize an Dictionary of int,int with 100000 items.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=DictionaryIntIntSerializationBenchmark  Mode=Throughput  

```
                         Method |      Median |    StdDev |  Gen 0 |  Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |------------ |---------- |------- |------- |------ |------------------- |
              JsonSerialization |  29.6042 ms | 0.2285 ms | 328.00 |  10.22 | 20.00 |      10 840 954.30 |
            JsonDeserialization |  37.1970 ms | 0.2684 ms | 321.35 |   9.89 | 18.79 |       8 463 578.31 |
              OmniSerialization |   7.5661 ms | 0.0587 ms |   3.55 |   0.37 | 15.69 |       1 720 430.67 |
            OmniDeserialization |   7.7148 ms | 0.0876 ms |   5.77 |   0.38 | 31.21 |       2 876 665.33 |
            KrakenSerialization |   6.2625 ms | 0.0618 ms |   3.40 |   0.35 |  8.92 |       1 243 427.50 |
          KrakenDeserialization |   6.7036 ms | 0.0485 ms |   5.64 |   0.37 | 18.63 |       2 493 512.12 |
   BinaryFormatterSerialization | 118.7579 ms | 0.9840 ms | 511.00 |   3.00 |  4.00 |      12 497 203.59 |
 BinaryFormatterDeserialization | 291.4521 ms | 2.0435 ms | 256.51 | 133.87 | 51.49 |      15 493 498.21 |


This benchmark serialize and deserialize an Dictionary of string,int with 100000 items.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=DictionaryStringIntSerializationBenchmark  Mode=Throughput  

```
                         Method |      Median |    StdDev |  Gen 0 |  Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |------------ |---------- |------- |------- |------ |------------------- |
              JsonSerialization |  18.6523 ms | 0.1263 ms | 150.19 |   5.41 | 19.17 |       5 265 064.72 |
            JsonDeserialization |  58.7662 ms | 0.8433 ms | 132.96 |  38.26 | 23.91 |      10 007 074.52 |
              OmniSerialization |  11.8213 ms | 0.0804 ms |   3.14 |   0.22 | 13.19 |       1 929 001.24 |
            OmniDeserialization |  26.3920 ms | 0.6235 ms |  24.00 |  27.53 | 28.00 |       5 881 995.85 |
            KrakenSerialization |  41.3858 ms | 0.2628 ms |   4.84 |   0.97 | 22.24 |       6 581 238.26 |
          KrakenDeserialization |  62.1249 ms | 2.0377 ms | 152.17 |  46.75 | 23.83 |      11 270 597.96 |
   BinaryFormatterSerialization | 166.2305 ms | 1.2011 ms | 446.00 |   3.00 | 59.00 |      18 984 053.23 |
 BinaryFormatterDeserialization | 462.8769 ms | 5.3528 ms | 432.51 | 203.15 | 66.47 |      26 001 435.40 |


This benchmark serialize and deserialize a string of 1000 characters.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=StringSerializationBenchmark  Mode=Throughput  

```
                         Method |    Median |    StdDev |    Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |---------- |---------- |--------- |------ |------ |------------------- |
              JsonSerialization | 2.6950 us | 0.1092 us | 3,025.56 |     - |     - |           3 669.14 |
            JsonDeserialization | 3.5846 us | 0.0164 us | 1,526.00 |     - |     - |           1 859.75 |
              OmniSerialization | 2.9819 us | 0.0301 us | 2,400.65 |     - |     - |           2 777.99 |
            OmniDeserialization | 1.7158 us | 0.0125 us | 1,681.36 |     - |     - |           2 012.27 |
            KrakenSerialization | 4.9201 us | 0.0341 us | 1,826.25 |     - |     - |           2 064.14 |
          KrakenDeserialization | 1.8035 us | 0.0106 us | 1,528.00 |     - |     - |           1 768.63 |
   BinaryFormatterSerialization | 5.1514 us | 0.0206 us | 3,469.69 |     - |     - |           3 923.99 |
 BinaryFormatterDeserialization | 4.4291 us | 0.0380 us | 3,377.93 |     - |     - |           3 850.52 |


This benchmark serialize and deserialize a large struct.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=LargeStructSerializationBenchmark  Mode=Throughput  

```
                         Method |         Median |     StdDev |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |--------------- |----------- |------- |------ |------ |------------------- |
              JsonSerialization |    996.8579 ns | 30.8450 ns | 670.58 |     - |     - |           2 851.36 |
            JsonDeserialization | 12,302.2083 ns | 66.6966 ns |  88.00 |     - |     - |             380.24 |
              OmniSerialization |  1,422.1572 ns | 23.9954 ns | 154.27 |     - |     - |             584.69 |
            OmniDeserialization |    869.3663 ns |  5.7891 ns |  99.12 |     - |     - |             375.36 |
            KrakenSerialization |  1,881.0574 ns |  9.3692 ns | 152.04 |     - |     - |             580.14 |
          KrakenDeserialization |  1,098.1586 ns | 14.7686 ns | 143.48 |     - |     - |             543.70 |
   BinaryFormatterSerialization |  5,457.9319 ns | 24.1619 ns | 705.95 |     - |     - |           2 653.33 |
 BinaryFormatterDeserialization |  6,127.6498 ns | 26.6277 ns | 778.91 |     - |     - |           2 917.79 |


This benchmark serialize and deserialize a small class used by the Wire project.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=WireSmallObjectSerializationBenchmark  Mode=Throughput  

```
                         Method |    Median |    StdDev |    Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |---------- |---------- |--------- |------ |------ |------------------- |
              JsonSerialization | 2.7594 us | 0.0631 us | 1,276.00 |     - |     - |           3 174.55 |
            JsonDeserialization | 3.0154 us | 0.0189 us |   281.90 |     - |     - |             624.86 |
              OmniSerialization | 1.4559 us | 0.0057 us |   210.87 |     - |     - |             471.06 |
            OmniDeserialization | 1.0015 us | 0.0131 us |    78.37 |     - |     - |             175.47 |
            KrakenSerialization | 2.1763 us | 0.0128 us |   246.25 |     - |     - |             552.57 |
          KrakenDeserialization | 1.3511 us | 0.0109 us |   229.09 |     - |     - |             506.96 |
   BinaryFormatterSerialization | 8.9124 us | 0.0745 us | 1,400.00 |     - |     - |           3 133.09 |
 BinaryFormatterDeserialization | 9.7675 us | 0.0646 us | 1,542.81 |     - |     - |           3 425.38 |

