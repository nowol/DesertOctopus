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
* A design decision has been made to only serialize class.  You should use another serializer if you need to serialize primitive/value types or wrap your value type in a class.

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
              JsonSerialization |  89.3981 us | 0.6611 us | 123.25 |  0.12 |     - |          20 951,54 |
            JsonDeserialization | 167.2077 us | 1.9596 us | 294.21 |  0.25 |     - |          43 232,20 |
              OmniSerialization | 190.7253 us | 2.7334 us | 373.35 |  0.24 |     - |          58 023,73 |
            OmniDeserialization |  84.1456 us | 1.4532 us | 123.36 |  0.12 |     - |          17 588,03 |
            KrakenSerialization | 243.3185 us | 2.8817 us | 297.97 |  0.50 |     - |          46 345,63 |
          KrakenDeserialization | 142.9836 us | 1.3913 us | 182.79 |  0.26 |     - |          26 075,27 |
   BinaryFormatterSerialization | 411.8838 us | 8.4578 us | 649.00 |  1.00 |     - |         101 161,91 |
 BinaryFormatterDeserialization | 384.1768 us | 2.5481 us | 587.93 |  0.98 |     - |          84 071,98 |


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
              JsonSerialization | 15.5069 us | 0.0926 us |   433.86 |     - |     - |           4 238,79 |
            JsonDeserialization | 14.6484 us | 0.2614 us |   193.51 |     - |     - |           1 770,51 |
              OmniSerialization |  1.7146 us | 0.0206 us |    60.25 |     - |     - |             543,25 |
            OmniDeserialization |  1.1227 us | 0.0141 us |    32.91 |     - |     - |             297,44 |
            KrakenSerialization |  2.8897 us | 0.0165 us |   137.95 |     - |     - |           1 230,98 |
          KrakenDeserialization |  1.6331 us | 0.0253 us |    67.64 |     - |     - |             610,14 |
   BinaryFormatterSerialization | 41.5045 us | 0.3234 us | 1,030.17 |     - |     - |           9 421,77 |
 BinaryFormatterDeserialization | 23.7353 us | 0.1882 us |   699.00 |     - |     - |           6 211,09 |


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
              JsonSerialization | 13,918.9631 us | 169.6306 us | 742.15 |  1.83 | 144.76 |       5 791 535,09 |
            JsonDeserialization | 17,944.3896 us | 169.5754 us | 662.00 |  3.00 | 125.00 |       4 497 848,86 |
              OmniSerialization |  3,133.6778 us |  13.0959 us |   9.71 |  0.25 |  70.70 |         656 478,66 |
            OmniDeserialization |  2,245.8171 us |  43.4993 us |   0.25 |  0.12 |  21.77 |         169 371,80 |
            KrakenSerialization |  2,471.3236 us |  17.7188 us |  11.45 |  0.13 |  77.25 |         623 993,94 |
          KrakenDeserialization |  1,218.7227 us |   7.3965 us |   0.12 |  0.06 |  26.30 |         166 846,81 |
   BinaryFormatterSerialization |    525.9088 us |   7.8323 us |  12.40 |  0.19 |  82.36 |         619 895,14 |
 BinaryFormatterDeserialization |    150.7751 us |   2.1502 us |   0.55 |  0.01 |  24.55 |         161 691,99 |


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
              JsonSerialization | 62,244.3285 us | 380.6121 us | 412.00 |  2.00 |  4.00 |       9 714 517,17 |
            JsonDeserialization | 22,327.5584 us | 147.4108 us | 172.19 |  0.49 | 37.14 |       5 132 350,74 |
              OmniSerialization |  5,497.0063 us |  48.2634 us |   2.63 |  0.06 | 22.91 |       1 333 915,17 |
            OmniDeserialization |  4,035.3816 us |  33.3874 us |   0.19 |  0.06 |  5.89 |         337 141,82 |
            KrakenSerialization |  5,542.4402 us | 102.5323 us |   2.68 |  0.07 | 25.85 |       1 280 667,50 |
          KrakenDeserialization |  3,235.1326 us |  45.4099 us |   0.20 |  0.07 |  6.03 |         353 046,84 |
   BinaryFormatterSerialization |  1,061.1412 us |  17.1481 us |   3.04 |  0.03 | 27.70 |       1 138 366,88 |
 BinaryFormatterDeserialization |    302.8572 us |   3.8349 us |   0.15 |  0.00 |  9.15 |         320 510,70 |


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
              JsonSerialization | 27.4200 ms | 0.2824 ms | 373.60 |  1.96 | 40.10 |       5 749 060,99 |
            JsonDeserialization | 34.3730 ms | 0.3438 ms | 373.60 |  1.96 | 43.03 |       6 899 350,73 |
              OmniSerialization |  5.7768 ms | 0.0467 ms | 194.46 |  8.36 | 39.12 |       2 977 283,54 |
            OmniDeserialization |  4.9785 ms | 0.0845 ms |   0.99 |  0.12 | 12.50 |         700 969,35 |
            KrakenSerialization | 10.7834 ms | 0.0743 ms | 190.17 |  3.04 | 49.95 |       4 072 580,58 |
          KrakenDeserialization |  6.1768 ms | 0.0371 ms | 196.65 |  0.24 | 23.98 |       2 467 990,50 |
   BinaryFormatterSerialization | 32.0796 ms | 0.3367 ms | 382.00 |  2.00 | 41.00 |       5 877 397,08 |
 BinaryFormatterDeserialization | 45.7325 ms | 0.6659 ms | 403.85 |  1.91 |  3.83 |       4 500 078,88 |


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
              JsonSerialization |  29.8051 ms | 0.3534 ms | 300.61 |   8.97 | 17.93 |      10 381 338,43 |
            JsonDeserialization |  38.4057 ms | 0.3939 ms | 342.13 |  10.38 | 19.82 |       9 414 004,65 |
              OmniSerialization |   7.5552 ms | 0.0615 ms |   3.51 |   0.23 | 13.69 |       1 552 405,31 |
            OmniDeserialization |   8.2013 ms | 0.0703 ms |   6.02 |   0.25 | 29.34 |       2 857 062,59 |
            KrakenSerialization |   6.9564 ms | 0.0730 ms | 100.25 |   4.23 |  8.24 |       3 097 498,00 |
          KrakenDeserialization |   7.4698 ms | 0.0447 ms | 112.84 |   5.26 | 24.15 |       4 804 845,19 |
   BinaryFormatterSerialization | 119.6410 ms | 0.6596 ms | 483.45 |   1.79 |  2.68 |      12 355 691,49 |
 BinaryFormatterDeserialization | 293.9149 ms | 1.6072 ms | 261.00 | 135.00 | 52.00 |      16 436 123,57 |


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
              JsonSerialization |  18.5081 ms | 0.1024 ms | 120.41 |   3.93 | 15.31 |       4 414 288,93 |
            JsonDeserialization |  56.7711 ms | 0.6508 ms | 121.23 |  33.41 | 21.00 |       9 454 629,95 |
              OmniSerialization |  12.2460 ms | 0.0820 ms |   3.41 |   0.21 | 14.34 |       2 210 255,12 |
            OmniDeserialization |  25.2952 ms | 0.6064 ms |  24.77 |  27.57 | 27.78 |       6 228 613,59 |
            KrakenSerialization |  38.0625 ms | 0.4450 ms |  56.31 |   9.69 | 36.92 |       8 158 516,75 |
          KrakenDeserialization |  63.8471 ms | 2.3487 ms | 127.79 |  67.02 | 21.45 |      11 916 924,68 |
   BinaryFormatterSerialization | 157.8951 ms | 1.0261 ms | 422.00 |   3.00 | 54.00 |      18 840 072,67 |
 BinaryFormatterDeserialization | 466.9921 ms | 6.2495 ms | 365.22 | 170.74 | 57.52 |      23 004 729,91 |


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
              JsonSerialization | 2.6216 us | 0.0275 us | 2,635.26 |     - |     - |           3 424,54 |
            JsonDeserialization | 3.5067 us | 0.0306 us | 1,441.00 |     - |     - |           1 881,89 |
              OmniSerialization | 2.9927 us | 0.0640 us | 2,054.09 |     - |     - |           2 543,10 |
            OmniDeserialization | 1.7698 us | 0.0339 us | 1,525.67 |     - |     - |           1 953,50 |
            KrakenSerialization | 4.8746 us | 0.0454 us | 1,735.07 |     - |     - |           2 101,51 |
          KrakenDeserialization | 1.8286 us | 0.0242 us | 1,500.54 |     - |     - |           1 870,91 |
   BinaryFormatterSerialization | 5.2552 us | 0.0684 us | 3,472.93 |     - |     - |           4 200,71 |
 BinaryFormatterDeserialization | 4.5291 us | 0.0415 us | 3,499.12 |     - |     - |           4 278,91 |

