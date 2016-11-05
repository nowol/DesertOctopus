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
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=ProductSerializationBenchmark  Mode=Throughput  

```
                         Method |      Median |    StdDev | Size (bytes) |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |------------ |---------- |------------- |------- |------ |------ |------------------- |
              JsonSerialization |  86.7469 us | 0.5475 us |         7121 | 119.33 |  0.13 |     - |          19 922.71 |
            JsonDeserialization | 163.6618 us | 0.7335 us |              | 254.22 |  0.25 |     - |          36 491.80 |
              OmniSerialization | 200.1308 us | 1.5124 us |        12253 | 317.66 |  0.48 |     - |          49 650.90 |
            OmniDeserialization |  81.6274 us | 0.8973 us |              | 109.73 |  0.12 |     - |          15 387.05 |
            KrakenSerialization | 271.8820 us | 2.0880 us |         5798 | 193.70 |  0.51 |     - |          30 448.14 |
          KrakenDeserialization | 144.5933 us | 2.4019 us |              |  98.26 |  0.26 |     - |          14 029.64 |
   BinaryFormatterSerialization | 423.3251 us | 4.7081 us |        22223 | 437.46 |  0.94 |     - |          67 836.31 |
 BinaryFormatterDeserialization | 397.0724 us | 3.8015 us |              | 378.00 |  1.00 |     - |          54 264.58 |


This benchmark serialize and deserialize a normal sized object that contains all primitives types.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=SimpleDtoWithEveryPrimitivesSerializationBenchmark  Mode=Throughput  

```
                         Method |     Median |    StdDev | Size (bytes) |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |----------- |---------- |------------- |------- |------ |------ |------------------- |
              JsonSerialization | 13.9147 us | 0.1536 us |          591 | 436.33 |     - |     - |           3 698.73 |
            JsonDeserialization | 15.0574 us | 0.1093 us |              | 177.97 |     - |     - |           1 363.78 |
              OmniSerialization |  1.7858 us | 0.0151 us |          217 |  59.00 |     - |     - |             459.43 |
            OmniDeserialization |  1.1702 us | 0.0076 us |              |  30.94 |     - |     - |             242.75 |
            KrakenSerialization |  2.8423 us | 0.0278 us |          232 |  80.91 |     - |     - |             618.84 |
          KrakenDeserialization |  1.6764 us | 0.0178 us |              |  50.93 |     - |     - |             399.82 |
   BinaryFormatterSerialization | 44.1390 us | 0.4433 us |         1735 | 878.00 |     - |     - |           7 024.52 |
 BinaryFormatterDeserialization | 23.4729 us | 0.1614 us |              | 517.93 |     - |     - |           3 975.85 |


This benchmark serialize and deserialize an array of 100000 ints.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=IntArraySerializationBenchmark  Mode=Throughput  

```
                         Method |         Median |     StdDev | Size (bytes) |  Gen 0 | Gen 1 |  Gen 2 | Bytes Allocated/Op |
------------------------------- |--------------- |----------- |------------- |------- |------ |------- |------------------- |
              JsonSerialization | 14,353.2820 us | 96.3389 us |       600001 | 322.00 | 17.00 | 144.00 |       2 555 663.75 |
            JsonDeserialization | 14,913.6437 us | 88.7644 us |              | 291.95 | 16.27 | 121.57 |       2 427 673.10 |
              OmniSerialization |  2,001.7752 us | 19.1441 us |       300025 |  11.31 |     - |  92.87 |         567 238.95 |
            OmniDeserialization |  1,397.1675 us |  4.2567 us |              |   0.06 |     - |  25.85 |         145 775.94 |
            KrakenSerialization |  2,143.5352 us | 15.2044 us |       300032 |  12.45 |     - | 102.35 |         623 386.79 |
          KrakenDeserialization |  1,133.6462 us |  5.7867 us |              |   0.05 |     - |  31.05 |         174 555.76 |
   BinaryFormatterSerialization |    498.0558 us | 10.1222 us |       400028 |  11.73 |  0.65 |  95.20 |         600 518.43 |
 BinaryFormatterDeserialization |    144.1466 us |  6.0002 us |              |   0.62 |     - |  31.49 |         179 378.49 |


This benchmark serialize and deserialize an array of 100000 doubles.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=DoubleArraySerializationBenchmark  Mode=Throughput  

```
                         Method |         Median |      StdDev | Size (bytes) |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |--------------- |------------ |------------- |------- |------ |------ |------------------- |
              JsonSerialization | 37,190.9349 us | 516.8441 us |      1200001 | 602.00 |  2.00 | 40.00 |       7 811 604.92 |
            JsonDeserialization | 21,790.6121 us | 112.0737 us |              | 265.68 |  0.88 | 66.20 |       4 412 359.48 |
              OmniSerialization |  5,817.1821 us |  30.9035 us |       900026 |   5.24 |  0.22 | 53.69 |       1 348 559.54 |
            OmniDeserialization |  4,484.0109 us |  24.9719 us |              |   0.37 |  0.12 | 12.25 |         353 155.55 |
            KrakenSerialization |  7,423.0923 us |  66.8267 us |       900033 |   4.65 |  0.23 | 40.43 |       1 165 250.87 |
          KrakenDeserialization |  4,758.9962 us |  31.1871 us |              |   0.37 |  0.12 | 11.26 |         335 500.94 |
   BinaryFormatterSerialization |  1,042.4490 us |  13.0852 us |       800028 |   6.87 |  0.19 | 64.16 |       1 356 307.29 |
 BinaryFormatterDeserialization |    311.9798 us |   9.8842 us |              |   0.30 |  0.01 | 17.57 |         331 194.34 |


This benchmark serialize and deserialize an array of 100000 decimals.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=DecimalArraySerializationBenchmark  Mode=Throughput  

```
                         Method |     Median |    StdDev | Size (bytes) |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |----------- |---------- |------------- |------- |------ |------ |------------------- |
              JsonSerialization | 33.6071 ms | 0.2418 ms |      1200001 | 253.00 | 19.00 | 39.00 |       4 832 796.15 |
            JsonDeserialization | 33.8660 ms | 3.7011 ms |              | 265.31 | 20.67 | 78.39 |       6 270 514.29 |
              OmniSerialization |  6.2517 ms | 0.1378 ms |       700027 | 129.70 |  2.73 | 33.16 |       2 379 295.20 |
            OmniDeserialization |  5.5152 ms | 0.0583 ms |              |   0.58 |  0.12 | 21.43 |         734 132.47 |
            KrakenSerialization |  6.1346 ms | 0.1133 ms |       700034 | 143.15 |  4.54 | 44.46 |       2 596 013.36 |
          KrakenDeserialization |  8.0676 ms | 0.0515 ms |              |   1.24 |  0.25 | 13.88 |         695 056.38 |
   BinaryFormatterSerialization | 41.7935 ms | 0.2710 ms |      1200028 | 270.16 | 20.15 | 42.13 |       5 144 256.49 |
 BinaryFormatterDeserialization | 41.6031 ms | 0.4887 ms |              | 294.68 |  1.87 |  3.74 |       3 553 831.06 |


This benchmark serialize and deserialize an Dictionary of int,int with 100000 items.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=DictionaryIntIntSerializationBenchmark  Mode=Throughput  

```
                         Method |      Median |    StdDev | Size (bytes) |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |------------ |---------- |------------- |------- |------ |------ |------------------- |
              JsonSerialization |  39.9382 ms | 0.2303 ms |      2400001 | 218.93 |  1.01 | 19.21 |       8 368 169.90 |
            JsonDeserialization |  38.4063 ms | 0.9503 ms |              | 218.93 |  9.61 | 18.71 |       6 477 711.25 |
              OmniSerialization |   7.2025 ms | 0.0584 ms |      1000201 |   2.94 |  0.25 |  8.95 |       1 585 103.27 |
            OmniDeserialization |   7.3492 ms | 0.1223 ms |              |   5.29 |  0.23 | 28.66 |       2 851 715.16 |
            KrakenSerialization |   8.0501 ms | 0.0955 ms |      1000072 |   3.16 |  0.23 |  7.11 |       1 420 474.32 |
          KrakenDeserialization |   7.6134 ms | 0.0765 ms |              |   5.26 |  0.25 | 22.44 |       2 479 309.64 |
   BinaryFormatterSerialization | 124.1615 ms | 0.6837 ms |      1701335 | 303.00 |  2.00 |  3.00 |       8 461 458.36 |
 BinaryFormatterDeserialization | 206.0618 ms | 1.1911 ms |              | 179.02 | 96.85 | 59.67 |      10 609 635.20 |


This benchmark serialize and deserialize an Dictionary of string,int with 100000 items.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=DictionaryStringIntSerializationBenchmark  Mode=Throughput  

```
                         Method |      Median |    StdDev | Size (bytes) |  Gen 0 |  Gen 1 |  Gen 2 | Bytes Allocated/Op |
------------------------------- |------------ |---------- |------------- |------- |------- |------- |------------------- |
              JsonSerialization |  22.9258 ms | 0.1316 ms |      1377781 |  81.78 |   5.99 |  20.27 |       4 152 603.53 |
            JsonDeserialization |  52.5432 ms | 0.6090 ms |              |  55.73 |  41.07 |   5.87 |       6 502 160.33 |
              OmniSerialization |  12.7189 ms | 0.0766 ms |       980839 |   0.68 |   0.90 |  10.15 |       1 796 603.13 |
            OmniDeserialization |  25.9613 ms | 0.3072 ms |              |   9.21 |  24.19 |  19.81 |       3 987 640.66 |
            KrakenSerialization |  48.2894 ms | 0.2428 ms |      1180707 |   1.52 |   2.02 |  38.44 |       4 963 680.10 |
          KrakenDeserialization |  53.2499 ms | 0.7226 ms |              |  71.50 |  41.25 |   5.50 |       6 834 907.54 |
   BinaryFormatterSerialization | 156.7387 ms | 1.6969 ms |      2390230 | 241.00 |  27.00 |  57.00 |      13 746 202.05 |
 BinaryFormatterDeserialization | 420.1801 ms | 1.7497 ms |              | 158.78 | 218.09 | 141.57 |      14 655 739.00 |


This benchmark serialize and deserialize a string of 1000 characters.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=StringSerializationBenchmark  Mode=Throughput  

```
                         Method |    Median |    StdDev | Size (bytes) |    Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |---------- |---------- |------------- |--------- |------ |------ |------------------- |
              JsonSerialization | 3.0916 us | 0.0184 us |         1012 | 3,056.00 |     - |     - |           3 623.81 |
            JsonDeserialization | 3.6454 us | 0.0179 us |              | 1,687.62 |     - |     - |           1 995.62 |
              OmniSerialization | 3.0953 us | 0.0314 us |         1117 | 2,483.04 |     - |     - |           2 810.38 |
            OmniDeserialization | 1.6522 us | 0.0211 us |              | 1,574.28 |     - |     - |           1 846.02 |
            KrakenSerialization | 4.5075 us | 0.0297 us |         1125 | 1,641.32 |     - |     - |           1 831.82 |
          KrakenDeserialization | 1.8153 us | 0.0139 us |              | 1,452.75 |     - |     - |           1 658.98 |
   BinaryFormatterSerialization | 5.1888 us | 0.0461 us |         1281 | 2,946.87 |     - |     - |           3 272.09 |
 BinaryFormatterDeserialization | 4.2906 us | 0.0661 us |              | 2,817.96 |     - |     - |           3 181.75 |


This benchmark serialize and deserialize a large struct.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=LargeStructSerializationBenchmark  Mode=Throughput  

```
                         Method |         Median |      StdDev | Size (bytes) |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |--------------- |------------ |------------- |------- |------ |------ |------------------- |
              JsonSerialization |    985.2296 ns |  10.5076 ns |           47 | 628.59 |     - |     - |           2 783.64 |
            JsonDeserialization | 16,677.0916 ns | 303.8054 ns |              |  67.00 |     - |     - |             279.35 |
              OmniSerialization |  1,424.9432 ns |   7.9541 ns |          225 | 130.11 |     - |     - |             512.64 |
            OmniDeserialization |    870.3426 ns |   7.5952 ns |              |  89.78 |     - |     - |             354.67 |
            KrakenSerialization |  1,959.0558 ns |   9.2451 ns |          232 | 114.49 |     - |     - |             453.69 |
          KrakenDeserialization |  1,204.1739 ns |   5.7617 ns |              | 107.56 |     - |     - |             425.32 |
   BinaryFormatterSerialization |  5,382.3961 ns |  41.4604 ns |          456 | 506.11 |     - |     - |           1 972.04 |
 BinaryFormatterDeserialization |  6,016.8891 ns |  83.2728 ns |              | 472.83 |     - |     - |           1 843.75 |


This benchmark serialize and deserialize a small class used by the Wire project.

```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=WireSmallObjectSerializationBenchmark  Mode=Throughput  

```
                         Method |    Median |    StdDev | Size (bytes) |    Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |---------- |---------- |------------- |--------- |------ |------ |------------------- |
              JsonSerialization | 4.2112 us | 0.0584 us |          124 | 1,156.13 |     - |     - |           2 770.80 |
            JsonDeserialization | 3.0664 us | 0.0409 us |              |   169.50 |     - |     - |             366.78 |
              OmniSerialization | 2.0651 us | 0.0157 us |          133 |   177.99 |     - |     - |             384.70 |
            OmniDeserialization | 1.5125 us | 0.0215 us |              |    69.70 |     - |     - |             159.41 |
            KrakenSerialization | 2.8999 us | 0.0147 us |          142 |   195.26 |     - |     - |             420.55 |
          KrakenDeserialization | 1.9292 us | 0.0071 us |              |   146.00 |     - |     - |             311.16 |
   BinaryFormatterSerialization | 9.2864 us | 0.0820 us |          393 | 1,037.00 |     - |     - |           2 215.71 |
 BinaryFormatterDeserialization | 9.9307 us | 0.0455 us |              |   950.00 |     - |     - |           2 013.54 |

