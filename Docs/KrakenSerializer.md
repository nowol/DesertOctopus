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
----------------------------------------- |------------ |---------- |------------- |------- |------ |------ |------------------- |
                        JsonSerialization |  85.5520 us | 0.3439 us |         7121 | 126.70 |  0.11 |     - |          21 597.83 |
                      JsonDeserialization | 162.7548 us | 0.9821 us |              | 250.28 |  0.23 |     - |          36 718.12 |
                        OmniSerialization | 201.3288 us | 1.8155 us |        12253 | 310.75 |  0.47 |     - |          49 662.73 |
                      OmniDeserialization |  80.1745 us | 0.4964 us |              |  96.59 |  0.12 |     - |          13 860.13 |
                      KrakenSerialization | 267.5761 us | 1.6698 us |         5799 | 179.48 |  0.50 |     - |          28 888.18 |
                    KrakenDeserialization | 142.4381 us | 0.5822 us |              |  95.96 |  0.23 |     - |          14 007.31 |
   KrakenSerializationWithOmittedRootType | 265.5380 us | 1.2939 us |         5703 | 173.85 |  0.51 |     - |          28 758.10 |
 KrakenDeserializationWithOmittedRootType | 142.0321 us | 1.1465 us |              |  89.99 |  0.24 |     - |          13 071.26 |
             BinaryFormatterSerialization | 414.1764 us | 2.6414 us |        22223 | 485.62 |  0.94 |     - |          76 898.30 |
           BinaryFormatterDeserialization | 390.7855 us | 1.7794 us |              | 340.00 |  1.00 |     - |          50 014.02 |
               NetSerializerSerialization |  37.1479 us | 0.2528 us |         3993 |  37.06 |  0.06 |     - |           6 184.01 |
             NetSerializerDeserialization |  31.4520 us | 1.0027 us |              |  49.09 |  0.06 |     - |           7 083.51 |


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
                                   Method |         Median |      StdDev | Size (bytes) |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
----------------------------------------- |--------------- |------------ |------------- |------- |------ |------ |------------------- |
                        JsonSerialization | 14,148.1565 ns |  71.1537 ns |          591 | 411.25 |     - |     - |           3 783.38 |
                      JsonDeserialization | 15,161.8387 ns |  97.2291 ns |              | 136.93 |     - |     - |           1 139.98 |
                        OmniSerialization |  1,711.4228 ns |  12.3775 ns |          217 |  56.00 |     - |     - |             473.14 |
                      OmniDeserialization |  1,144.3908 ns |   8.5640 ns |              |  25.74 |     - |     - |             219.40 |
                      KrakenSerialization |  2,893.2125 ns |  16.2870 ns |          231 |  65.00 |     - |     - |             542.80 |
                    KrakenDeserialization |  1,719.2863 ns |  17.2078 ns |              |  47.93 |     - |     - |             400.59 |
   KrakenSerializationWithOmittedRootType |  2,203.3236 ns |  14.6613 ns |          120 |  58.49 |     - |     - |             494.51 |
 KrakenDeserializationWithOmittedRootType |  1,397.0115 ns |   6.7218 ns |              |  26.28 |     - |     - |             221.32 |
             BinaryFormatterSerialization | 43,721.3140 ns | 249.7550 ns |         1735 | 758.00 |     - |     - |           6 584.89 |
           BinaryFormatterDeserialization | 23,292.8529 ns |  98.8523 ns |              | 508.00 |     - |     - |           4 229.91 |
               NetSerializerSerialization |    832.6980 ns |   5.0810 ns |          105 |  24.53 |     - |     - |             209.02 |
             NetSerializerDeserialization |    798.8985 ns |   5.2423 ns |              |  13.16 |     - |     - |             114.42 |


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
                                   Method |         Median |      StdDev | Size (bytes) |  Gen 0 | Gen 1 |  Gen 2 | Bytes Allocated/Op |
----------------------------------------- |--------------- |------------ |------------- |------- |------ |------- |------------------- |
                        JsonSerialization | 14,472.5272 us | 277.0063 us |       600001 | 277.59 | 14.66 | 119.83 |       2 259 389.83 |
                      JsonDeserialization | 14,550.1307 us |  97.6562 us |              | 305.00 | 17.00 | 104.00 |       2 598 253.71 |
                        OmniSerialization |  1,991.4913 us |  12.3013 us |       300025 |  11.07 |  0.12 |  88.56 |         569 596.88 |
                      OmniDeserialization |  1,375.6502 us |   7.8931 us |              |   0.06 |     - |  28.40 |         163 922.03 |
                      KrakenSerialization |  2,097.6231 us |  16.5124 us |       300033 |   9.47 |     - |  74.92 |         481 740.53 |
                    KrakenDeserialization |  1,133.7900 us |   4.9804 us |              |   0.06 |     - |  28.64 |         165 609.94 |
   KrakenSerializationWithOmittedRootType |  2,095.2651 us |  13.5741 us |       300014 |  10.71 |     - |  84.70 |         544 747.49 |
 KrakenDeserializationWithOmittedRootType |  1,136.6319 us |   8.3575 us |              |   0.06 |     - |  28.96 |         167 874.59 |
             BinaryFormatterSerialization |    485.1699 us |   8.3861 us |       400028 |  13.13 |  0.60 | 102.64 |         679 860.33 |
           BinaryFormatterDeserialization |    141.6972 us |   2.0011 us |              |   0.52 |     - |  26.26 |         156 207.41 |
               NetSerializerSerialization |  1,973.5008 us |  25.3363 us |       300004 |  11.53 |     - |  92.22 |         578 232.35 |
             NetSerializerDeserialization |  1,405.1652 us |  10.9432 us |              |   0.05 |     - |  29.49 |         170 079.37 |


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
----------------------------------------- |--------------- |------------ |------------- |------- |------ |------ |------------------- |
                        JsonSerialization | 36,794.6817 us | 260.7060 us |      1200001 | 794.00 |  2.00 | 53.00 |       8 894 399.66 |
                      JsonDeserialization | 21,786.6759 us | 260.1258 us |              | 361.31 |  1.08 | 68.49 |       5 188 811.34 |
                        OmniSerialization |  5,860.1427 us |  47.6950 us |       900026 |   6.45 |  0.14 | 44.16 |       1 370 840.96 |
                      OmniDeserialization |  4,492.1605 us |  51.6831 us |              |   0.39 |  0.13 |  6.05 |         359 384.55 |
                      KrakenSerialization |  6,744.6978 us |  54.1861 us |       900034 |   5.77 |  0.27 | 46.69 |       1 258 746.77 |
                    KrakenDeserialization |  5,087.0565 us |  28.6655 us |              |   0.41 |  0.14 |  5.21 |         323 674.06 |
   KrakenSerializationWithOmittedRootType |  6,758.3081 us |  84.4159 us |       900014 |   6.04 |  0.27 | 48.89 |       1 323 962.21 |
 KrakenDeserializationWithOmittedRootType |  5,079.1917 us |  35.1247 us |              |   0.43 |  0.14 |  5.30 |         338 078.35 |
             BinaryFormatterSerialization |  1,017.8178 us |  19.7132 us |       800028 |   6.90 |  0.21 | 52.88 |       1 179 355.13 |
           BinaryFormatterDeserialization |    331.6928 us |   5.1507 us |              |   0.17 |  0.01 |  8.11 |         303 653.83 |
               NetSerializerSerialization |  5,777.7185 us |  81.3925 us |       900004 |   6.58 |  0.14 | 43.20 |       1 357 770.39 |
             NetSerializerDeserialization |  4,935.0644 us |  54.1042 us |              |   0.43 |  0.14 |  6.45 |         355 730.75 |


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
----------------------------------------- |----------- |---------- |------------- |------- |------ |------ |------------------- |
                        JsonSerialization | 33.5310 ms | 0.2922 ms |      1200001 | 285.61 | 21.09 | 43.94 |       5 437 727.36 |
                      JsonDeserialization | 30.5727 ms | 0.3779 ms |              | 270.56 | 20.08 | 79.35 |       6 374 031.45 |
                        OmniSerialization |  6.3054 ms | 0.0809 ms |       700027 | 147.62 |  5.45 | 41.86 |       2 744 397.18 |
                      OmniDeserialization |  5.5247 ms | 0.0806 ms |              |   0.85 |  0.12 |  9.68 |         664 817.37 |
                      KrakenSerialization |  6.0413 ms | 0.0684 ms |       700035 | 124.03 |  5.08 | 38.24 |       2 273 668.24 |
                    KrakenDeserialization |  8.1396 ms | 0.1502 ms |              |   1.62 |  0.23 | 10.67 |         762 661.85 |
   KrakenSerializationWithOmittedRootType |  6.0002 ms | 0.0607 ms |       700014 | 134.47 |  5.80 | 41.55 |       2 468 926.51 |
 KrakenDeserializationWithOmittedRootType |  8.8718 ms | 0.8486 ms |              |   1.59 |  0.23 |  9.99 |         713 254.46 |
             BinaryFormatterSerialization | 41.4437 ms | 0.2644 ms |      1200028 | 285.61 | 21.09 | 45.70 |       5 434 951.63 |
           BinaryFormatterDeserialization | 41.1944 ms | 0.1649 ms |              | 272.00 |  2.00 |  6.00 |       3 315 322.83 |
               NetSerializerSerialization |  6.2185 ms | 0.0639 ms |       700004 | 121.09 |  5.21 | 37.05 |       2 223 308.95 |
             NetSerializerDeserialization |  5.4584 ms | 0.0444 ms |              |   0.85 |  0.12 |  9.68 |         700 432.89 |


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
----------------------------------------- |------------ |---------- |------------- |------- |------ |------ |------------------- |
                        JsonSerialization |  40.1487 ms | 0.1985 ms |      2400001 | 214.07 |  1.48 | 19.28 |       8 415 791.79 |
                      JsonDeserialization |  36.7712 ms | 0.1996 ms |              | 229.19 |  1.45 |  1.93 |       6 724 487.57 |
                        OmniSerialization |   6.9789 ms | 0.0388 ms |      1000201 |   3.05 |  0.23 |  9.62 |       1 728 967.07 |
                      OmniDeserialization |   7.8173 ms | 0.0520 ms |              |   5.29 |  0.22 | 22.94 |       2 942 748.45 |
                      KrakenSerialization |   8.2772 ms | 0.1061 ms |      1000073 |   2.94 |  0.25 |  8.82 |       1 312 204.76 |
                    KrakenDeserialization |   7.6309 ms | 0.0724 ms |              |   5.28 |  0.24 | 13.67 |       2 569 136.48 |
   KrakenSerializationWithOmittedRootType |   8.3093 ms | 0.0683 ms |      1000014 |   3.12 |  0.24 |  9.59 |       1 420 868.59 |
 KrakenDeserializationWithOmittedRootType |   7.5256 ms | 0.0440 ms |              |   5.15 |  0.25 | 13.48 |       2 492 001.14 |
             BinaryFormatterSerialization | 122.4973 ms | 0.7996 ms |      1701335 | 286.00 |  3.00 |  4.00 |       8 301 769.07 |
           BinaryFormatterDeserialization | 199.2693 ms | 1.7816 ms |              | 164.00 | 90.00 | 56.00 |      10 090 948.43 |
               NetSerializerSerialization |   7.0192 ms | 0.0542 ms |      1000005 |   2.99 |  0.23 |  9.42 |       1 685 229.09 |
             NetSerializerDeserialization |   6.0050 ms | 0.0289 ms |              |   0.43 |  0.12 |  7.77 |       1 221 185.19 |


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
----------------------------------------- |------------ |---------- |------------- |------- |------- |------- |------------------- |
                        JsonSerialization |  22.1520 ms | 0.1647 ms |      1377781 |  76.92 |   5.53 |  14.58 |       3 780 202.86 |
                      JsonDeserialization |  51.3059 ms | 0.7008 ms |              |  59.00 |  42.00 |   7.00 |       6 559 513.13 |
                        OmniSerialization |  12.6944 ms | 0.1225 ms |       980839 |   1.05 |   1.05 |  10.78 |       1 786 613.07 |
                      OmniDeserialization |  23.0766 ms | 0.2704 ms |              |  10.60 |  24.81 |  24.81 |       4 035 479.50 |
                      KrakenSerialization |  46.4451 ms | 0.4051 ms |      1180708 |   1.94 |   1.94 |  21.31 |       4 882 355.99 |
                    KrakenDeserialization |  52.7013 ms | 0.4879 ms |              | 100.00 |  44.00 |   7.00 |       7 332 713.74 |
   KrakenSerializationWithOmittedRootType |  45.8321 ms | 0.3645 ms |      1180648 |   2.07 |   2.07 |  22.74 |       5 212 255.20 |
 KrakenDeserializationWithOmittedRootType |  52.1025 ms | 0.5893 ms |              |  79.28 |  43.06 |   6.85 |       7 175 500.57 |
             BinaryFormatterSerialization | 154.3698 ms | 0.9182 ms |      2390230 | 263.85 |  22.57 |  44.26 |      14 082 803.00 |
           BinaryFormatterDeserialization | 417.5772 ms | 2.1572 ms |              | 158.00 | 218.00 | 141.00 |      14 164 935.72 |
               NetSerializerSerialization |  13.8953 ms | 1.2317 ms |       980639 |   1.05 |   1.05 |  10.78 |       1 779 158.62 |
             NetSerializerDeserialization |  21.1504 ms | 0.2344 ms |              |  15.08 |  20.86 |   7.04 |       2 402 429.16 |


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
----------------------------------------- |---------- |---------- |------------- |--------- |------ |------ |------------------- |
                        JsonSerialization | 3.0127 us | 0.0308 us |         1012 | 2,903.00 |     - |     - |           3 442.68 |
                      JsonDeserialization | 3.6075 us | 0.0419 us |              | 1,677.00 |     - |     - |           1 983.27 |
                        OmniSerialization | 3.0887 us | 0.0372 us |         1117 | 2,189.40 |     - |     - |           2 478.88 |
                      OmniDeserialization | 1.6360 us | 0.0130 us |              | 1,681.61 |     - |     - |           1 971.90 |
                      KrakenSerialization | 4.4820 us | 0.0295 us |         1126 | 1,696.00 |     - |     - |           1 890.95 |
                    KrakenDeserialization | 1.8355 us | 0.0119 us |              | 1,535.84 |     - |     - |           1 746.77 |
   KrakenSerializationWithOmittedRootType | 3.7273 us | 0.0248 us |         1015 | 1,514.24 |     - |     - |           1 680.98 |
 KrakenDeserializationWithOmittedRootType | 1.5846 us | 0.0131 us |              | 1,498.75 |     - |     - |           1 726.14 |
             BinaryFormatterSerialization | 5.1771 us | 0.0344 us |         1281 | 3,237.00 |     - |     - |           3 594.07 |
           BinaryFormatterDeserialization | 4.2310 us | 0.0336 us |              | 2,854.94 |     - |     - |           3 223.71 |
               NetSerializerSerialization | 1.8834 us | 0.0133 us |         1005 | 1,241.02 |     - |     - |           1 369.72 |
             NetSerializerDeserialization | 1.2742 us | 0.0170 us |              | 1,598.50 |     - |     - |           1 877.06 |


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
----------------------------------------- |--------------- |------------ |------------- |------- |------ |------ |------------------- |
                        JsonSerialization |    928.6024 ns |   7.3254 ns |           47 | 568.00 |     - |     - |           2 676.10 |
                      JsonDeserialization | 16,322.3265 ns | 104.7594 ns |              |  58.00 |     - |     - |             258.78 |
                        OmniSerialization |  1,400.7716 ns |  28.8920 ns |          225 | 119.59 |     - |     - |             501.26 |
                      OmniDeserialization |    830.6650 ns |   7.7037 ns |              |  89.56 |     - |     - |             376.40 |
                      KrakenSerialization |  2,033.3678 ns |  36.9979 ns |          233 | 115.93 |     - |     - |             483.53 |
                    KrakenDeserialization |  1,236.2849 ns |   9.0834 ns |              |  95.22 |     - |     - |             398.62 |
   KrakenSerializationWithOmittedRootType |  1,160.9122 ns |  23.0223 ns |           52 |  67.50 |     - |     - |             282.81 |
 KrakenDeserializationWithOmittedRootType |    874.2491 ns |   7.9233 ns |              |  35.07 |     - |     - |             146.32 |
             BinaryFormatterSerialization |  5,336.1589 ns |  83.1719 ns |          456 | 489.92 |     - |     - |           2 030.71 |
           BinaryFormatterDeserialization |  6,028.1791 ns |  45.4195 ns |              | 443.27 |     - |     - |           1 838.08 |
               NetSerializerSerialization |    363.9515 ns |   2.7243 ns |           41 |  35.98 |     - |     - |             149.47 |
             NetSerializerDeserialization |    349.3767 ns |   3.3053 ns |              |   9.20 |     - |     - |              38.46 |


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
----------------------------------------- |---------- |---------- |------------- |--------- |------ |------ |------------------- |
                        JsonSerialization | 3.9949 us | 0.0585 us |          124 | 1,144.25 |     - |     - |           2 741.97 |
                      JsonDeserialization | 2.9968 us | 0.0233 us |              |   177.20 |     - |     - |             383.48 |
                        OmniSerialization | 1.9979 us | 0.0740 us |          132 |   170.25 |     - |     - |             366.26 |
                      OmniDeserialization | 1.4153 us | 0.0125 us |              |    57.86 |     - |     - |             127.29 |
                      KrakenSerialization | 2.8119 us | 0.0248 us |          143 |   189.50 |     - |     - |             410.32 |
                    KrakenDeserialization | 1.8851 us | 0.0374 us |              |   140.69 |     - |     - |             300.85 |
   KrakenSerializationWithOmittedRootType | 2.1045 us | 0.0198 us |           50 |   150.28 |     - |     - |             316.02 |
 KrakenDeserializationWithOmittedRootType | 1.5804 us | 0.0088 us |              |   101.86 |     - |     - |             216.89 |
             BinaryFormatterSerialization | 9.0709 us | 0.0666 us |          393 |   985.00 |     - |     - |           2 104.41 |
           BinaryFormatterDeserialization | 9.6764 us | 0.1232 us |              | 1,095.88 |     - |     - |           2 320.51 |
               NetSerializerSerialization | 1.0631 us | 0.0120 us |           38 |    79.72 |     - |     - |             170.87 |
             NetSerializerDeserialization | 1.0498 us | 0.0098 us |              |    28.51 |     - |     - |              60.38 |

