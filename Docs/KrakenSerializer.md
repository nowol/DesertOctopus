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
              JsonSerialization |  90.2987 us | 0.4427 us | 135.73 |  0.12 |     - |          23 613,53 |
            JsonDeserialization | 168.5623 us | 1.3368 us | 278.71 |  0.25 |     - |          41 899,97 |
              OmniSerialization | 195.1097 us | 2.6939 us | 324.10 |  0.49 |     - |          51 852,42 |
            OmniDeserialization |  83.7861 us | 0.6541 us | 133.51 |  0.13 |     - |          19 477,47 |
            KrakenSerialization | 247.1122 us | 3.2122 us | 288.02 |  0.51 |     - |          45 825,03 |
          KrakenDeserialization | 147.9556 us | 0.9077 us | 173.46 |  0.23 |     - |          25 289,09 |
   BinaryFormatterSerialization | 411.0509 us | 5.4249 us | 612.40 |  0.90 |     - |          97 723,19 |
 BinaryFormatterDeserialization | 393.1850 us | 2.7492 us | 510.00 |  1.00 |     - |          74 871,33 |


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
                         Method |     Median |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |----------- |---------- |------- |------ |------ |------------------- |
              JsonSerialization | 15.9892 us | 0.1978 us | 452.93 |     - |     - |           4 526,46 |
            JsonDeserialization | 15.0687 us | 0.1659 us | 236.05 |     - |     - |           2 207,83 |
              OmniSerialization |  1.7049 us | 0.0173 us |  56.86 |     - |     - |             524,77 |
            OmniDeserialization |  1.1446 us | 0.0101 us |  29.59 |     - |     - |             273,82 |
            KrakenSerialization |  2.9511 us | 0.0262 us | 130.20 |     - |     - |           1 192,00 |
          KrakenDeserialization |  1.5846 us | 0.0085 us |  68.37 |     - |     - |             630,97 |
   BinaryFormatterSerialization | 41.5274 us | 0.2770 us | 993.89 |     - |     - |           9 303,33 |
 BinaryFormatterDeserialization | 23.7067 us | 0.2305 us | 699.00 |     - |     - |           6 355,56 |


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
              JsonSerialization | 10,037.1608 us |  73.3983 us | 623.18 | 19.52 | 185.93 |       3 907 251,93 |
            JsonDeserialization | 13,446.8622 us | 151.1431 us | 542.00 | 17.00 | 118.00 |       3 389 583,06 |
              OmniSerialization |  2,059.4799 us |  12.4981 us |  12.30 |     - |  98.74 |         591 644,43 |
            OmniDeserialization |  1,529.8265 us |  10.1787 us |   0.12 |     - |  31.96 |         176 434,89 |
            KrakenSerialization |  2,464.2750 us |  23.0004 us |  11.20 |     - |  91.19 |         582 186,74 |
          KrakenDeserialization |  1,159.5630 us |  17.9704 us |   0.12 |     - |  32.86 |         181 547,03 |
   BinaryFormatterSerialization |    542.4038 us |  13.6916 us |  12.87 |  0.49 | 102.68 |         630 288,56 |
 BinaryFormatterDeserialization |    154.2937 us |   2.0364 us |   0.61 |  0.01 |  31.01 |         173 951,87 |


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
              JsonSerialization | 62,441.9794 us | 416.5320 us | 412.00 |  2.00 |  4.00 |       9 930 112,04 |
            JsonDeserialization | 21,363.7182 us | 124.9448 us | 168.45 |  0.48 | 36.34 |       5 131 889,90 |
              OmniSerialization |  5,532.4944 us |  29.0309 us |   2.65 |  0.06 | 23.96 |       1 381 533,03 |
            OmniDeserialization |  3,661.7286 us |  24.3443 us |   0.19 |  0.06 |  5.76 |         337 144,65 |
            KrakenSerialization |  5,497.9147 us |  29.0758 us |   2.44 |  0.06 | 23.72 |       1 186 952,03 |
          KrakenDeserialization |  2,973.7808 us |  21.4288 us |   0.09 |  0.03 |  8.87 |         374 055,57 |
   BinaryFormatterSerialization |  1,079.2461 us |  13.0991 us |   2.94 |  0.06 | 27.03 |       1 138 413,39 |
 BinaryFormatterDeserialization |    308.1252 us |   3.6488 us |   0.15 |  0.00 |  8.79 |         314 248,60 |


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
              JsonSerialization | 27.0072 ms | 0.2739 ms | 392.97 |  1.96 | 43.01 |       6 180 701,24 |
            JsonDeserialization | 33.8537 ms | 0.2587 ms | 362.00 |  2.00 | 42.00 |       6 842 876,67 |
              OmniSerialization |  5.7538 ms | 0.0309 ms | 184.38 |  7.85 | 34.70 |       2 887 337,51 |
            OmniDeserialization |  4.9651 ms | 0.1072 ms |   0.89 |  0.11 | 10.89 |         643 708,86 |
            KrakenSerialization | 10.8421 ms | 0.0589 ms | 176.23 |  2.23 | 45.61 |       3 848 477,57 |
          KrakenDeserialization |  5.8990 ms | 0.0375 ms | 170.91 |  0.25 | 20.79 |       2 196 535,43 |
   BinaryFormatterSerialization | 32.1001 ms | 0.1774 ms | 362.00 |  2.00 | 39.00 |       5 702 957,82 |
 BinaryFormatterDeserialization | 45.5208 ms | 0.2779 ms | 392.97 |  1.96 |  3.91 |       4 485 345,73 |


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
              JsonSerialization |  29.9433 ms | 0.2357 ms | 296.96 |   9.39 | 17.79 |      10 274 795,95 |
            JsonDeserialization |  37.9726 ms | 0.2070 ms | 297.86 |   9.17 | 16.90 |       8 213 452,34 |
              OmniSerialization |   7.5420 ms | 0.0623 ms |   3.70 |   0.22 | 15.34 |       1 736 961,27 |
            OmniDeserialization |   8.2819 ms | 0.0630 ms |   5.60 |   0.22 | 27.55 |       2 698 900,29 |
            KrakenSerialization |   6.9799 ms | 0.0613 ms | 105.74 |   4.46 |  8.70 |       3 266 157,89 |
          KrakenDeserialization |   7.3348 ms | 0.0524 ms | 100.39 |   4.79 | 20.58 |       4 279 165,37 |
   BinaryFormatterSerialization | 120.1326 ms | 0.6501 ms | 502.38 |   1.62 |  2.42 |      12 808 091,50 |
 BinaryFormatterDeserialization | 299.7036 ms | 1.7943 ms | 278.00 | 141.00 | 55.00 |      17 394 046,33 |


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
              JsonSerialization |  18.4759 ms | 0.0730 ms | 139.92 |   4.32 | 17.28 |       5 120 545,41 |
            JsonDeserialization |  57.6178 ms | 0.4283 ms | 125.19 |  34.73 | 21.00 |       9 769 204,38 |
              OmniSerialization |  12.3120 ms | 0.0838 ms |   3.35 |   0.24 | 13.76 |       2 027 155,60 |
            OmniDeserialization |  25.2917 ms | 0.3238 ms |  23.91 |  26.06 | 26.49 |       5 937 142,88 |
            KrakenSerialization |  41.1460 ms | 2.6979 ms |  55.10 |   9.48 | 35.68 |       7 982 212,67 |
          KrakenDeserialization |  63.1974 ms | 0.7119 ms | 130.00 |  67.00 | 23.00 |      11 989 790,19 |
   BinaryFormatterSerialization | 158.8621 ms | 0.9103 ms | 434.88 |   2.63 | 55.13 |      19 342 822,42 |
 BinaryFormatterDeserialization | 475.7842 ms | 4.3787 ms | 358.00 | 167.00 | 57.00 |      22 566 338,76 |


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
              JsonSerialization | 2.7077 us | 0.0203 us | 2,786.12 |     - |     - |           3 536,20 |
            JsonDeserialization | 3.6265 us | 0.0240 us | 1,671.62 |     - |     - |           2 130,82 |
              OmniSerialization | 3.0956 us | 0.0272 us | 2,103.00 |     - |     - |           2 543,10 |
            OmniDeserialization | 1.7812 us | 0.0226 us | 1,756.31 |     - |     - |           2 196,03 |
            KrakenSerialization | 4.9088 us | 0.0318 us | 1,737.76 |     - |     - |           2 057,83 |
          KrakenDeserialization | 1.8406 us | 0.0223 us | 1,418.52 |     - |     - |           1 727,72 |
   BinaryFormatterSerialization | 5.3068 us | 0.0339 us | 3,508.98 |     - |     - |           4 145,41 |
 BinaryFormatterDeserialization | 4.5381 us | 0.0450 us | 3,271.00 |     - |     - |           3 908,14 |

