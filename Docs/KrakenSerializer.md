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
              JsonSerialization |  87.2389 us | 0.9968 us |         7121 | 116.49 |  0.11 |     - |          20 323.66 |
            JsonDeserialization | 163.5082 us | 1.0747 us |              | 228.72 |  0.25 |     - |          34 358.12 |
              OmniSerialization | 201.6952 us | 2.1492 us |        12253 | 361.45 |  0.43 |     - |          58 959.37 |
            OmniDeserialization |  82.3295 us | 0.5610 us |              |  99.70 |  0.12 |     - |          14 624.63 |
            KrakenSerialization | 268.6803 us | 1.4302 us |         5798 | 181.24 |  0.48 |     - |          29 797.56 |
          KrakenDeserialization | 147.5321 us | 3.2727 us |              | 105.40 |  0.23 |     - |          15 689.24 |
   BinaryFormatterSerialization | 427.5882 us | 3.6484 us |        22223 | 470.61 |  0.96 |     - |          76 259.21 |
 BinaryFormatterDeserialization | 398.9481 us | 3.9106 us |              | 340.00 |  1.00 |     - |          51 145.13 |
     NetSerializerSerialization |  38.0259 us | 0.4254 us |         3993 |  35.86 |  0.06 |     - |           6 117.26 |
   NetSerializerDeserialization |  31.2940 us | 0.2446 us |              |  49.02 |  0.06 |     - |           7 233.80 |


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
------------------------------- |--------------- |------------ |------------- |------- |------ |------ |------------------- |
              JsonSerialization | 14,338.8996 ns |  78.3844 ns |          591 | 410.11 |     - |     - |           3 618.92 |
            JsonDeserialization | 15,422.6309 ns | 135.6558 ns |              | 156.25 |     - |     - |           1 247.97 |
              OmniSerialization |  1,787.4015 ns |  25.2008 ns |          217 |  52.66 |     - |     - |             426.77 |
            OmniDeserialization |  1,164.1773 ns |  14.5117 ns |              |  25.97 |     - |     - |             212.28 |
            KrakenSerialization |  2,867.9292 ns |  12.1134 ns |          230 |  70.31 |     - |     - |             560.01 |
          KrakenDeserialization |  1,696.7114 ns |  30.2790 ns |              |  45.34 |     - |     - |             370.63 |
   BinaryFormatterSerialization | 44,676.9510 ns | 416.1398 ns |         1735 | 798.00 |     - |     - |           6 648.20 |
 BinaryFormatterDeserialization | 23,795.5799 ns | 161.8168 ns |              | 508.93 |     - |     - |           4 066.80 |
     NetSerializerSerialization |    847.7114 ns |   9.1629 ns |          105 |  25.83 |     - |     - |             211.03 |
   NetSerializerDeserialization |    795.3223 ns |   9.9544 ns |              |  15.11 |     - |     - |             126.00 |


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
------------------------------- |--------------- |------------ |------------- |------- |------ |------- |------------------- |
              JsonSerialization | 14,503.7637 us | 188.2410 us |       600001 | 335.51 | 17.81 | 135.89 |       2 662 851.03 |
            JsonDeserialization | 15,308.1585 us | 211.9007 us |              | 322.00 | 18.00 | 115.00 |       2 677 973.02 |
              OmniSerialization |  2,032.0770 us |  10.8459 us |       300025 |  12.07 |     - |  95.81 |         599 408.97 |
            OmniDeserialization |  1,409.1245 us |  13.0602 us |              |   0.06 |     - |  27.56 |         155 236.73 |
            KrakenSerialization |  2,172.5562 us |  22.9244 us |       300032 |  10.38 |     - |  83.14 |         521 618.06 |
          KrakenDeserialization |  1,152.0949 us |   6.6751 us |              |   0.06 |     - |  30.02 |         168 901.50 |
   BinaryFormatterSerialization |    505.4889 us |  12.0370 us |       400028 |  12.89 |  0.33 | 100.18 |         640 785.19 |
 BinaryFormatterDeserialization |    147.4558 us |   1.9720 us |              |   0.60 |     - |  29.93 |         173 502.96 |
     NetSerializerSerialization |  2,019.4370 us |  19.1988 us |       300004 |  10.32 |     - |  82.53 |         506 071.04 |
   NetSerializerDeserialization |  1,467.3771 us |   7.5058 us |              |   0.06 |     - |  29.41 |         165 223.31 |


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
              JsonSerialization | 37,292.1502 us | 196.4337 us |      1200001 | 666.00 |  2.00 | 44.00 |       8 097 422.00 |
            JsonDeserialization | 22,164.5772 us | 307.1569 us |              | 279.64 |  0.97 | 66.74 |       4 357 548.12 |
              OmniSerialization |  5,716.6576 us |  46.7239 us |       900026 |   4.80 |  0.11 | 34.52 |       1 180 181.84 |
            OmniDeserialization |  4,503.3007 us |  35.4464 us |              |   0.38 |  0.13 |  9.49 |         322 363.32 |
            KrakenSerialization |  7,442.5066 us |  45.8264 us |       900033 |   4.67 |  0.23 | 30.58 |       1 098 681.26 |
          KrakenDeserialization |  4,766.6269 us |  19.3960 us |              |   0.37 |  0.12 |  9.91 |         349 148.06 |
   BinaryFormatterSerialization |  1,051.7631 us |   8.5268 us |       800028 |   5.49 |  0.42 | 48.38 |       1 061 808.61 |
 BinaryFormatterDeserialization |    327.5341 us |   2.8196 us |              |   0.35 |  0.01 | 18.90 |         363 353.44 |
     NetSerializerSerialization |  5,796.3105 us |  37.0747 us |       900004 |   5.34 |  0.12 | 36.88 |       1 239 493.13 |
   NetSerializerDeserialization |  4,892.6342 us |  48.1650 us |              |   0.37 |  0.12 | 10.40 |         349 011.25 |


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
              JsonSerialization | 33.4165 ms | 0.2172 ms |      1200001 | 291.92 | 21.62 | 44.15 |       5 313 660.90 |
            JsonDeserialization | 30.9425 ms | 0.1802 ms |              | 295.99 | 22.99 | 87.17 |       6 686 319.61 |
              OmniSerialization |  6.4724 ms | 0.1974 ms |       700027 | 109.07 |  4.09 | 27.75 |       1 945 927.75 |
            OmniDeserialization |  5.6849 ms | 0.0850 ms |              |   0.78 |  0.13 | 15.66 |         677 496.29 |
            KrakenSerialization |  6.1848 ms | 0.0492 ms |       700034 | 135.42 |  6.15 | 42.37 |       2 381 646.53 |
          KrakenDeserialization |  7.9756 ms | 0.0519 ms |              |   1.46 |  0.24 |  9.95 |         689 533.59 |
   BinaryFormatterSerialization | 41.9568 ms | 0.2358 ms |      1200028 | 272.08 | 20.22 | 41.36 |       4 954 729.70 |
 BinaryFormatterDeserialization | 42.0067 ms | 0.2954 ms |              | 271.00 |  2.00 |  5.00 |       3 149 796.49 |
     NetSerializerSerialization |  6.3035 ms | 0.0665 ms |       700004 | 115.63 |  4.33 | 30.79 |       2 062 248.18 |
   NetSerializerDeserialization |  5.4801 ms | 0.0374 ms |              |   0.76 |  0.13 | 15.82 |         662 388.86 |


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
              JsonSerialization |  40.8870 ms | 0.4697 ms |      2400001 | 239.96 |  0.91 | 20.87 |       9 382 033.68 |
            JsonDeserialization |  37.7003 ms | 0.2645 ms |              | 248.39 |  0.95 | 11.35 |       7 230 457.16 |
              OmniSerialization |   7.2095 ms | 0.0300 ms |      1000201 |   2.76 |  0.23 |  8.50 |       1 523 407.11 |
            OmniDeserialization |   8.0363 ms | 0.1185 ms |              |   5.51 |  0.24 | 24.34 |       3 043 108.47 |
            KrakenSerialization |   7.9999 ms | 0.0828 ms |      1000072 |   3.12 |  0.24 |  5.16 |       1 412 274.52 |
          KrakenDeserialization |   7.5941 ms | 0.0543 ms |              |   5.26 |  0.25 | 20.18 |       2 541 567.46 |
   BinaryFormatterSerialization | 124.0316 ms | 0.9694 ms |      1701335 | 280.62 |  1.96 |  3.91 |       8 065 840.00 |
 BinaryFormatterDeserialization | 205.9421 ms | 1.3275 ms |              | 172.00 | 96.00 | 58.00 |      10 584 203.73 |
     NetSerializerSerialization |   7.0996 ms | 0.0976 ms |      1000005 |   3.12 |  0.24 |  9.83 |       1 752 386.78 |
   NetSerializerDeserialization |   5.8946 ms | 0.0348 ms |              |   0.42 |  0.12 |  9.88 |       1 320 935.73 |


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
              JsonSerialization |  22.1963 ms | 0.1159 ms |      1377781 |  71.90 |   4.93 |   9.63 |       3 743 027.61 |
            JsonDeserialization |  52.5525 ms | 0.5522 ms |              |  57.00 |  38.00 |   6.00 |       6 521 447.77 |
              OmniSerialization |  12.6774 ms | 0.0884 ms |       980839 |   0.90 |   0.68 |   9.91 |       1 764 627.79 |
            OmniDeserialization |  24.2626 ms | 0.2994 ms |              |   9.66 |  22.30 |  26.67 |       3 887 843.59 |
            KrakenSerialization |  48.1085 ms | 0.4129 ms |      1180707 |   1.89 |   1.42 |  46.31 |       5 024 568.89 |
          KrakenDeserialization |  53.7790 ms | 0.6857 ms |              |  71.98 |  38.33 |   5.61 |       6 843 495.83 |
   BinaryFormatterSerialization | 156.7179 ms | 1.0622 ms |      2390230 | 245.46 |  21.50 |  40.31 |      14 026 473.23 |
 BinaryFormatterDeserialization | 420.3118 ms | 1.5002 ms |              | 156.77 | 213.21 | 138.85 |      14 756 390.40 |
     NetSerializerSerialization |  12.7373 ms | 0.2306 ms |       980639 |   0.90 |   0.68 |  10.81 |       1 920 401.03 |
   NetSerializerDeserialization |  21.4216 ms | 0.1756 ms |              |  13.76 |  18.92 |   9.83 |       2 350 879.21 |


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
              JsonSerialization | 3.1037 us | 0.1314 us |         1012 | 2,750.00 |     - |     - |           3 333.98 |
            JsonDeserialization | 3.5701 us | 0.0408 us |              | 1,650.94 |     - |     - |           1 995.65 |
              OmniSerialization | 2.9631 us | 0.0574 us |         1117 | 2,367.77 |     - |     - |           2 739.75 |
            OmniDeserialization | 1.6280 us | 0.0155 us |              | 1,625.74 |     - |     - |           1 948.57 |
            KrakenSerialization | 4.4296 us | 0.0501 us |         1125 | 1,525.21 |     - |     - |           1 740.27 |
          KrakenDeserialization | 1.7562 us | 0.0229 us |              | 1,481.65 |     - |     - |           1 729.58 |
   BinaryFormatterSerialization | 5.0606 us | 0.0368 us |         1281 | 3,427.94 |     - |     - |           3 889.97 |
 BinaryFormatterDeserialization | 4.1594 us | 0.0487 us |              | 2,771.00 |     - |     - |           3 198.63 |
     NetSerializerSerialization | 1.8725 us | 0.0295 us |         1005 | 1,250.10 |     - |     - |           1 410.14 |
   NetSerializerDeserialization | 1.2742 us | 0.0165 us |              | 1,433.44 |     - |     - |           1 720.64 |


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
              JsonSerialization |    922.3178 ns |  15.8764 ns |           47 | 636.46 |     - |     - |           2 998.20 |
            JsonDeserialization | 16,659.6707 ns | 292.3789 ns |              |  55.00 |     - |     - |             245.87 |
              OmniSerialization |  1,424.5516 ns |  14.7690 ns |          225 | 122.31 |     - |     - |             512.65 |
            OmniDeserialization |    861.3631 ns |  11.4685 ns |              |  82.38 |     - |     - |             346.28 |
            KrakenSerialization |  1,973.9505 ns |  12.8665 ns |          232 | 108.69 |     - |     - |             458.09 |
          KrakenDeserialization |  1,219.4933 ns |   9.1498 ns |              |  91.55 |     - |     - |             385.27 |
   BinaryFormatterSerialization |  5,377.0463 ns |  34.1667 ns |          456 | 516.51 |     - |     - |           2 140.67 |
 BinaryFormatterDeserialization |  6,041.7075 ns |  72.5204 ns |              | 430.31 |     - |     - |           1 785.61 |
     NetSerializerSerialization |    364.4196 ns |   3.5747 ns |           41 |  38.44 |     - |     - |             159.65 |
   NetSerializerDeserialization |    351.7982 ns |   1.5887 ns |              |   9.20 |     - |     - |              38.45 |


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
              JsonSerialization | 4.2874 us | 0.0217 us |          124 | 1,182.41 |     - |     - |           2 833.81 |
            JsonDeserialization | 3.0365 us | 0.0175 us |              |   171.54 |     - |     - |             371.04 |
              OmniSerialization | 2.0629 us | 0.0198 us |          133 |   194.52 |     - |     - |             420.16 |
            OmniDeserialization | 1.4930 us | 0.0072 us |              |    63.12 |     - |     - |             138.85 |
            KrakenSerialization | 2.9002 us | 0.0273 us |          142 |   182.43 |     - |     - |             393.30 |
          KrakenDeserialization | 1.9027 us | 0.0134 us |              |   159.65 |     - |     - |             339.70 |
   BinaryFormatterSerialization | 9.2686 us | 0.0917 us |          393 | 1,043.62 |     - |     - |           2 229.61 |
 BinaryFormatterDeserialization | 9.8775 us | 0.1127 us |              | 1,003.00 |     - |     - |           2 125.43 |
     NetSerializerSerialization | 1.1899 us | 0.0116 us |           39 |    79.72 |     - |     - |             170.87 |
   NetSerializerDeserialization | 1.1563 us | 0.0099 us |              |    29.75 |     - |     - |              63.00 |

