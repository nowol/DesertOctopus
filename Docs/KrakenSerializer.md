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
              JsonSerialization |  92.3901 us | 1.0645 us | 138.29 |  0.11 |     - |          23 482,84 |
            JsonDeserialization | 171.2535 us | 1.5296 us | 287.82 |  0.24 |     - |          42 211,92 |
              OmniSerialization | 194.7613 us | 2.1685 us | 353.61 |  0.49 |     - |          54 669,68 |
            OmniDeserialization |  85.4060 us | 0.8384 us | 115.81 |  0.11 |     - |          16 357,71 |
            KrakenSerialization | 280.1507 us | 5.9773 us | 273.06 |  0.44 |     - |          43 475,05 |
          KrakenDeserialization | 149.3576 us | 0.8888 us | 157.73 |  0.25 |     - |          22 587,16 |
   BinaryFormatterSerialization | 412.6623 us | 4.2626 us | 595.09 |  0.92 |     - |          93 261,83 |
 BinaryFormatterDeserialization | 402.2942 us | 4.1327 us | 570.00 |  1.00 |     - |          81 632,03 |


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
              JsonSerialization | 15.7001 us | 0.1908 us |   449.00 |     - |     - |           4 195,39 |
            JsonDeserialization | 15.5450 us | 0.3955 us |   225.29 |     - |     - |           1 971,71 |
              OmniSerialization |  1.6828 us | 0.0258 us |    61.93 |     - |     - |             529,88 |
            OmniDeserialization |  1.1270 us | 0.0063 us |    29.84 |     - |     - |             257,78 |
            KrakenSerialization |  2.9837 us | 0.0176 us |   133.88 |     - |     - |           1 146,87 |
          KrakenDeserialization |  1.5788 us | 0.0079 us |    65.76 |     - |     - |             567,25 |
   BinaryFormatterSerialization | 42.0547 us | 0.2880 us | 1,077.00 |     - |     - |           9 406,18 |
 BinaryFormatterDeserialization | 23.5949 us | 0.1695 us |   816.05 |     - |     - |           6 989,98 |


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
              JsonSerialization | 10,062.7003 us |  75.4722 us | 599.05 | 18.77 | 179.87 |       3 757 624,74 |
            JsonDeserialization | 13,545.4672 us | 125.2624 us | 605.00 | 19.00 | 134.00 |       3 787 181,86 |
              OmniSerialization |  2,051.4281 us |  14.0778 us |  11.33 |     - |  91.11 |         545 520,32 |
            OmniDeserialization |  1,286.7822 us |   8.7434 us |   0.12 |     - |  30.34 |         167 724,16 |
            KrakenSerialization |  2,406.2356 us |  20.0584 us |  12.56 |     - | 101.76 |         647 314,02 |
          KrakenDeserialization |  1,156.9582 us |   6.4692 us |   0.11 |     - |  30.54 |         168 530,22 |
   BinaryFormatterSerialization |    565.8579 us |  10.8964 us |  10.92 |  0.37 |  86.97 |         533 253,31 |
 BinaryFormatterDeserialization |    157.8869 us |   2.0437 us |   0.56 |  0.01 |  29.03 |         161 825,27 |


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
              JsonSerialization | 61,620.7420 us | 495.2885 us | 370.00 |  2.00 |  4.00 |       9 355 798,42 |
            JsonDeserialization | 21,423.9563 us | 242.5837 us | 162.91 |  0.49 | 34.89 |       5 197 253,91 |
              OmniSerialization |  5,541.5917 us |  44.3481 us |   2.47 |  0.06 | 21.58 |       1 278 230,19 |
            OmniDeserialization |  3,612.9428 us |  12.3898 us |   0.17 |  0.06 |  5.95 |         356 932,47 |
            KrakenSerialization |  5,632.8923 us |  48.9776 us |   2.29 |  0.06 | 21.81 |       1 199 325,95 |
          KrakenDeserialization |  2,985.0023 us |  15.2048 us |   0.09 |  0.03 |  7.25 |         321 785,59 |
   BinaryFormatterSerialization |  1,150.7409 us |  38.3019 us |   3.06 |  0.05 | 27.62 |       1 237 138,74 |
 BinaryFormatterDeserialization |    328.3601 us |   8.9957 us |   0.14 |  0.00 |  8.52 |         327 658,35 |


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
              JsonSerialization | 25.8181 ms | 0.2144 ms | 354.04 |  1.96 | 39.12 |       5 453 444,09 |
            JsonDeserialization | 34.9996 ms | 0.2309 ms | 311.91 |  1.63 | 35.93 |       5 760 689,65 |
              OmniSerialization |  5.8993 ms | 0.0510 ms | 183.95 |  8.23 | 36.97 |       2 821 609,57 |
            OmniDeserialization |  5.0420 ms | 0.2370 ms |   0.73 |  0.09 | 11.96 |         700 866,05 |
            KrakenSerialization | 11.0004 ms | 0.0916 ms | 186.68 |  2.23 | 47.35 |       3 982 173,50 |
          KrakenDeserialization |  5.8327 ms | 0.0352 ms | 179.16 |  0.21 | 21.86 |       2 247 849,54 |
   BinaryFormatterSerialization | 32.2341 ms | 0.1930 ms | 393.16 |  1.96 | 42.05 |       6 044 606,13 |
 BinaryFormatterDeserialization | 45.5365 ms | 0.2180 ms | 382.00 |  2.00 |  4.00 |       4 269 051,30 |


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
              JsonSerialization |  29.9215 ms | 0.3339 ms | 337.52 |  10.61 | 20.72 |      10 232 998,42 |
            JsonDeserialization |  38.0714 ms | 0.1618 ms | 402.42 |  12.12 | 23.27 |       9 684 370,91 |
              OmniSerialization |   7.4942 ms | 0.0480 ms |   3.74 |   0.40 | 16.45 |       1 633 209,09 |
            OmniDeserialization |   8.3158 ms | 0.0700 ms |   5.32 |   0.35 | 28.80 |       2 433 116,06 |
            KrakenSerialization |   6.2357 ms | 0.0463 ms |   3.66 |   0.39 |  9.42 |       1 206 757,85 |
          KrakenDeserialization |   6.6377 ms | 0.0687 ms |   6.02 |   0.38 | 22.31 |       2 463 924,08 |
   BinaryFormatterSerialization | 115.8087 ms | 1.9773 ms | 553.47 |   2.94 |  3.92 |      12 382 687,67 |
 BinaryFormatterDeserialization | 299.6978 ms | 3.7935 ms | 334.00 | 175.00 | 67.00 |      18 524 039,21 |


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
              JsonSerialization |  18.9857 ms | 0.2088 ms | 142.57 |   4.92 | 18.19 |       4 999 516,21 |
            JsonDeserialization |  57.7024 ms | 0.9147 ms | 143.73 |  40.09 | 24.44 |      10 751 960,67 |
              OmniSerialization |  12.0912 ms | 0.0734 ms |   3.48 |   0.24 | 14.51 |       2 140 574,26 |
            OmniDeserialization |  25.4500 ms | 0.3227 ms |  23.63 |  28.66 | 28.66 |       5 964 024,51 |
            KrakenSerialization |  43.4736 ms | 0.4618 ms |  13.15 |   0.91 | 31.75 |       7 037 256,95 |
          KrakenDeserialization |  60.3652 ms | 2.3365 ms | 155.05 |  47.49 | 22.35 |      11 476 756,57 |
   BinaryFormatterSerialization | 157.9394 ms | 1.0781 ms | 446.00 |   3.00 | 58.00 |      18 996 853,91 |
 BinaryFormatterDeserialization | 470.5906 ms | 2.5739 ms | 410.67 | 192.62 | 64.53 |      24 716 000,36 |


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
              JsonSerialization | 2.6432 us | 0.0323 us | 2,914.26 |     - |     - |           3 614,80 |
            JsonDeserialization | 3.5689 us | 0.0359 us | 1,632.58 |     - |     - |           2 034,02 |
              OmniSerialization | 2.9306 us | 0.0239 us | 2,362.25 |     - |     - |           2 795,32 |
            OmniDeserialization | 1.6920 us | 0.0249 us | 1,644.00 |     - |     - |           2 012,27 |
            KrakenSerialization | 4.9245 us | 0.0409 us | 1,704.00 |     - |     - |           1 970,32 |
          KrakenDeserialization | 1.8833 us | 0.0292 us | 1,528.00 |     - |     - |           1 805,60 |
   BinaryFormatterSerialization | 5.1171 us | 0.0347 us | 3,571.33 |     - |     - |           4 130,49 |
 BinaryFormatterDeserialization | 4.3813 us | 0.0358 us | 3,271.00 |     - |     - |           3 813,71 |


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
                         Method |         Median |      StdDev |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |--------------- |------------ |------- |------ |------ |------------------- |
              JsonSerialization |    940.7919 ns |  26.6331 ns | 641.43 |     - |     - |           2 954,64 |
            JsonDeserialization | 12,634.9902 ns | 225.7332 ns |  98.00 |     - |     - |             457,35 |
              OmniSerialization |  1,401.8544 ns |  25.3255 ns | 139.30 |     - |     - |             571,98 |
            OmniDeserialization |    854.1212 ns |  12.9632 ns |  94.28 |     - |     - |             386,71 |
            KrakenSerialization |  1,906.0590 ns |  24.2976 ns | 155.73 |     - |     - |             631,65 |
          KrakenDeserialization |  1,088.1010 ns |  12.1435 ns | 118.53 |     - |     - |             483,55 |
   BinaryFormatterSerialization |  5,305.5527 ns |  32.2754 ns | 651.65 |     - |     - |           2 653,32 |
 BinaryFormatterDeserialization |  5,999.7123 ns |  70.8346 ns | 690.13 |     - |     - |           2 801,16 |


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
                         Method |        Median |      StdDev |    Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |-------------- |------------ |--------- |------ |------ |------------------- |
              JsonSerialization | 2,765.9856 ns |  38.6250 ns | 1,322.35 |     - |     - |           3 015,85 |
            JsonDeserialization | 2,947.5594 ns |  32.5742 ns |   320.08 |     - |     - |             650,37 |
              OmniSerialization | 1,457.1122 ns |  17.2350 ns |   227.36 |     - |     - |             465,49 |
            OmniDeserialization |   991.6720 ns |   9.8140 ns |    91.50 |     - |     - |             187,63 |
            KrakenSerialization | 2,209.6397 ns |  17.9015 ns |   291.13 |     - |     - |             597,72 |
          KrakenDeserialization | 1,308.3838 ns |   8.7498 ns |   232.53 |     - |     - |             473,35 |
   BinaryFormatterSerialization | 8,617.9754 ns | 127.0153 ns | 1,478.00 |     - |     - |           3 031,52 |
 BinaryFormatterDeserialization | 9,691.1755 ns |  81.9457 ns | 1,735.00 |     - |     - |           3 530,46 |

