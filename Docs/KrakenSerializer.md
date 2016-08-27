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
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=ProductSerializationBenchMark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |      Median |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |------------ |---------- |------- |------ |------ |------------------- |
              JsonSerialization |  91.5241 us | 2.2568 us | 147.46 |  0.11 |     - |          25 034,44 |
            JsonDeserialization | 168.7297 us | 0.7089 us | 285.05 |  0.26 |     - |          41 824,30 |
              OmniSerialization | 190.7759 us | 1.3143 us | 346.58 |  0.54 |     - |          53 647,44 |
            OmniDeserialization |  83.4644 us | 1.5135 us | 141.52 |  0.11 |     - |          19 960,49 |
            KrakenSerialization | 268.6698 us | 2.7326 us | 293.97 |  0.50 |     - |          46 831,66 |
          KrakenDeserialization | 144.7275 us | 1.3585 us | 159.62 |  0.27 |     - |          22 874,09 |
   BinaryFormatterSerialization | 402.8711 us | 8.9116 us | 614.00 |  1.00 |     - |          96 269,31 |
 BinaryFormatterDeserialization | 387.3889 us | 8.1961 us | 590.62 |  0.94 |     - |          84 463,99 |


This benchmark serialize and deserialize a normal sized object that contains all primitives types.

```ini

Host Process Environment Information:
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=SimpleDtoWithEveryPrimitivesSerializationBenchmark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |     Median |    StdDev |    Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |----------- |---------- |--------- |------ |------ |------------------- |
              JsonSerialization | 15.7564 us | 0.1842 us |   443.72 |     - |     - |           4 238,74 |
            JsonDeserialization | 15.0644 us | 0.1271 us |   229.31 |     - |     - |           2 050,38 |
              OmniSerialization |  1.6818 us | 0.0295 us |    63.94 |     - |     - |             559,33 |
            OmniDeserialization |  1.1119 us | 0.0162 us |    27.56 |     - |     - |             243,48 |
            KrakenSerialization |  2.8614 us | 0.0503 us |   150.32 |     - |     - |           1 315,74 |
          KrakenDeserialization |  1.5255 us | 0.0333 us |    68.62 |     - |     - |             605,07 |
   BinaryFormatterSerialization | 40.9663 us | 0.5097 us | 1,137.00 |     - |     - |          10 148,73 |
 BinaryFormatterDeserialization | 23.7946 us | 0.1576 us |   676.46 |     - |     - |           5 927,49 |


This benchmark serialize and deserialize an array of 100000 ints.

```ini

Host Process Environment Information:
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=IntArraySerializationBenchmark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |         Median |      StdDev |  Gen 0 | Gen 1 |  Gen 2 | Bytes Allocated/Op |
------------------------------- |--------------- |------------ |------- |------ |------- |------------------- |
              JsonSerialization | 10,093.8785 us | 100.1530 us | 553.32 | 17.34 | 167.27 |       3 549 239,63 |
            JsonDeserialization | 13,395.6773 us | 120.9744 us | 637.00 | 20.00 | 139.00 |       4 075 327,62 |
              OmniSerialization |  1,998.8097 us |  15.8001 us |  11.33 |  0.13 |  89.70 |         550 494,47 |
            OmniDeserialization |  1,289.2596 us |   8.9914 us |   0.12 |     - |  28.72 |         162 550,33 |
            KrakenSerialization |  2,388.4248 us |  15.9721 us |  12.29 |     - |  98.93 |         647 246,64 |
          KrakenDeserialization |  1,154.4870 us |  23.4727 us |   0.12 |     - |  28.72 |         162 806,86 |
   BinaryFormatterSerialization |    530.2069 us |   7.6916 us |  11.48 |  0.38 |  90.95 |         572 456,04 |
 BinaryFormatterDeserialization |    157.3474 us |   5.0067 us |   0.61 |     - |  30.43 |         175 181,82 |


This benchmark serialize and deserialize an array of 100000 doubles.

```ini

Host Process Environment Information:
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=DoubleArraySerializationBenchmark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |         Median |      StdDev |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |--------------- |------------ |------- |------ |------ |------------------- |
              JsonSerialization | 61,491.5618 us | 568.1738 us | 349.00 |  2.00 |  4.00 |       8 444 496,98 |
            JsonDeserialization | 22,472.5292 us | 117.4217 us | 166.66 |  0.45 | 35.95 |       5 074 746,17 |
              OmniSerialization |  5,495.7797 us |  37.9725 us |   2.82 |  0.06 | 24.13 |       1 440 874,37 |
            OmniDeserialization |  3,592.2611 us |  22.6007 us |   0.18 |  0.06 |  6.35 |         373 998,28 |
            KrakenSerialization |  5,583.1252 us |  45.3663 us |   2.33 |  0.06 | 22.84 |       1 161 689,56 |
          KrakenDeserialization |  2,954.6653 us |  40.0399 us |   0.09 |  0.03 |  8.26 |         349 046,59 |
   BinaryFormatterSerialization |  1,073.8749 us |  28.7323 us |   3.46 |  0.03 | 31.39 |       1 322 134,27 |
 BinaryFormatterDeserialization |    301.5320 us |   3.2388 us |   0.15 |  0.00 |  9.08 |         331 633,71 |


This benchmark serialize and deserialize an array of 100000 decimals.

```ini

Host Process Environment Information:
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=DecimalArraySerializationBenchmark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |     Median |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |----------- |---------- |------- |------ |------ |------------------- |
              JsonSerialization | 25.7091 ms | 0.2752 ms | 338.13 |  1.87 | 36.43 |       5 453 219,52 |
            JsonDeserialization | 34.0140 ms | 0.1818 ms | 342.00 |  2.00 | 40.00 |       6 621 481,72 |
              OmniSerialization |  5.8099 ms | 0.0433 ms | 197.15 |  8.31 | 37.68 |       3 156 590,43 |
            OmniDeserialization |  4.9694 ms | 0.1960 ms |   0.64 |  0.08 | 12.41 |         744 812,10 |
            KrakenSerialization | 10.7877 ms | 0.0823 ms | 187.76 |  2.60 | 49.25 |       4 201 255,62 |
          KrakenDeserialization |  5.8067 ms | 0.0533 ms | 172.84 |  0.24 | 21.04 |       2 272 734,72 |
   BinaryFormatterSerialization | 32.0357 ms | 0.2408 ms | 373.22 |  1.95 | 40.06 |       6 012 657,06 |
 BinaryFormatterDeserialization | 45.1696 ms | 0.3212 ms | 338.13 |  1.87 |  3.74 |       3 963 062,68 |


This benchmark serialize and deserialize an Dictionary of int,int with 100000 items.

```ini

Host Process Environment Information:
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=DictionaryIntIntSerializationBenchmark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |      Median |    StdDev |  Gen 0 |  Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |------------ |---------- |------- |------- |------ |------------------- |
              JsonSerialization |  29.7494 ms | 0.1981 ms | 312.94 |   9.89 | 19.28 |      10 354 581,26 |
            JsonDeserialization |  38.3480 ms | 0.2454 ms | 321.35 |   9.89 | 18.79 |       8 463 765,26 |
              OmniSerialization |   7.2919 ms | 0.0947 ms |   3.60 |   0.36 | 16.19 |       1 768 376,66 |
            OmniDeserialization |   8.1919 ms | 0.0877 ms |   5.99 |   0.36 | 33.21 |       3 050 767,54 |
            KrakenSerialization |   6.2248 ms | 0.0458 ms |   3.06 |   0.18 |  9.30 |       1 265 329,77 |
          KrakenDeserialization |   6.6788 ms | 0.0758 ms |   5.77 |   0.38 | 18.68 |       2 550 317,14 |
   BinaryFormatterSerialization | 119.3068 ms | 0.5201 ms | 511.00 |   3.00 |  4.00 |      12 497 405,23 |
 BinaryFormatterDeserialization | 297.3665 ms | 1.4974 ms | 289.00 | 151.00 | 58.00 |      17 464 035,86 |


This benchmark serialize and deserialize an Dictionary of string,int with 100000 items.

```ini

Host Process Environment Information:
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=DictionaryStringIntSerializationBenchmark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |      Median |    StdDev |  Gen 0 |  Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |------------ |---------- |------- |------- |------ |------------------- |
              JsonSerialization |  18.6242 ms | 0.1343 ms | 141.01 |   4.89 | 18.01 |       4 952 318,47 |
            JsonDeserialization |  57.5843 ms | 0.5994 ms | 133.00 |  37.00 | 23.00 |       9 952 307,00 |
              OmniSerialization |  12.1345 ms | 0.0695 ms |   3.38 |   0.25 | 14.42 |       2 023 872,50 |
            OmniDeserialization |  25.7527 ms | 0.2824 ms |  19.18 |  21.90 | 22.31 |       4 680 893,59 |
            KrakenSerialization |  44.8449 ms | 1.1016 ms |  13.66 |   1.01 | 37.43 |       7 093 408,64 |
          KrakenDeserialization |  61.1208 ms | 2.4941 ms | 124.14 |  36.14 | 18.86 |       9 207 548,93 |
   BinaryFormatterSerialization | 156.6380 ms | 0.5911 ms | 421.00 |   3.00 | 55.00 |      17 957 564,00 |
 BinaryFormatterDeserialization | 466.7526 ms | 1.9811 ms | 421.83 | 198.00 | 66.00 |      25 378 781,30 |


This benchmark serialize and deserialize a string of 1000 characters.

```ini

Host Process Environment Information:
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=StringSerializationBenchmark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |    Median |    StdDev |    Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |---------- |---------- |--------- |------ |------ |------------------- |
              JsonSerialization | 2.6946 us | 0.0212 us | 2,545.29 |     - |     - |           3 307,80 |
            JsonDeserialization | 3.6565 us | 0.0513 us | 1,453.71 |     - |     - |           1 897,67 |
              OmniSerialization | 3.0528 us | 0.0646 us | 2,085.00 |     - |     - |           2 586,23 |
            OmniDeserialization | 1.7449 us | 0.0208 us | 1,534.40 |     - |     - |           1 967,55 |
            KrakenSerialization | 5.2095 us | 0.1997 us | 1,717.23 |     - |     - |           2 079,78 |
          KrakenDeserialization | 1.9659 us | 0.0302 us | 1,517.36 |     - |     - |           1 877,96 |
   BinaryFormatterSerialization | 5.2902 us | 0.0697 us | 3,424.47 |     - |     - |           4 149,76 |
 BinaryFormatterDeserialization | 4.5026 us | 0.0453 us | 3,194.93 |     - |     - |           3 902,41 |


This benchmark serialize and deserialize a large struct.

```ini

Host Process Environment Information:
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=LargeStructSerializationBenchmark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |         Median |      StdDev |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |--------------- |------------ |------- |------ |------ |------------------- |
              JsonSerialization |    954.0803 ns |  12.4941 ns | 588.10 |     - |     - |           2 826,96 |
            JsonDeserialization | 12,593.4303 ns | 105.7674 ns |  98.00 |     - |     - |             477,24 |
              OmniSerialization |  1,428.0672 ns |  10.8814 ns | 130.66 |     - |     - |             559,82 |
            OmniDeserialization |    876.6764 ns |  10.8524 ns |  87.69 |     - |     - |             375,36 |
            KrakenSerialization |  2,127.9747 ns | 173.4100 ns | 147.70 |     - |     - |             625,05 |
          KrakenDeserialization |  1,112.3619 ns |  11.7483 ns | 117.31 |     - |     - |             499,32 |
   BinaryFormatterSerialization |  5,436.2578 ns |  29.3032 ns | 709.42 |     - |     - |           3 013,59 |
 BinaryFormatterDeserialization |  6,162.6872 ns |  70.4752 ns | 742.57 |     - |     - |           3 143,97 |


This benchmark serialize and deserialize a small class used by the Wire project.

```ini

Host Process Environment Information:
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=64-bit ? [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=WireSmallObjectSerializationBenchmark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |    Median |    StdDev |    Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |---------- |---------- |--------- |------ |------ |------------------- |
              JsonSerialization | 2.8194 us | 0.0321 us | 1,212.15 |     - |     - |           3 015,83 |
            JsonDeserialization | 2.8328 us | 0.0299 us |   249.82 |     - |     - |             554,32 |
              OmniSerialization | 1.4860 us | 0.0072 us |   203.76 |     - |     - |             455,25 |
            OmniDeserialization | 1.0187 us | 0.0193 us |    80.91 |     - |     - |             181,10 |
            KrakenSerialization | 2.2390 us | 0.0173 us |   224.04 |     - |     - |             502,12 |
          KrakenDeserialization | 1.3411 us | 0.0097 us |   195.00 |     - |     - |             433,17 |
   BinaryFormatterSerialization | 8.7499 us | 0.0801 us | 1,400.00 |     - |     - |           3 133,11 |
 BinaryFormatterDeserialization | 9.8067 us | 0.0597 us | 1,705.70 |     - |     - |           3 785,77 |

