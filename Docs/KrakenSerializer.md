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
                         Method |      Median |    StdDev |
------------------------------- |------------ |---------- |
              JsonSerialization |  88.6700 us | 0.8293 us |
            JsonDeserialization | 170.2807 us | 2.4112 us |
              OmniSerialization | 189.9103 us | 1.8282 us |
            OmniDeserialization |  83.1365 us | 0.9025 us |
            KrakenSerialization | 237.4399 us | 2.1391 us |
          KrakenDeserialization | 138.3925 us | 1.6673 us |
   BinaryFormatterSerialization | 402.5713 us | 2.6870 us |
 BinaryFormatterDeserialization | 387.0419 us | 3.6949 us |


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
              JsonSerialization | 15.5479 us | 0.0937 us |   452.93 |     - |     - |           4 526,10 |
            JsonDeserialization | 14.9042 us | 0.1866 us |   236.05 |     - |     - |           2 207,70 |
              OmniSerialization |  1.7040 us | 0.0122 us |    57.61 |     - |     - |             531,44 |
            OmniDeserialization |  1.1246 us | 0.0081 us |    29.59 |     - |     - |             273,82 |
            KrakenSerialization |  2.8756 us | 0.0295 us |   135.99 |     - |     - |           1 238,14 |
          KrakenDeserialization |  1.5519 us | 0.0133 us |    65.78 |     - |     - |             603,52 |
   BinaryFormatterSerialization | 41.0101 us | 0.4093 us | 1,017.00 |     - |     - |           9 519,36 |
 BinaryFormatterDeserialization | 23.4704 us | 0.3266 us |   667.93 |     - |     - |           6 073,19 |


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
              JsonSerialization | 14,053.5865 us | 141.0809 us | 736.00 |  4.00 | 144.00 |       5 378 245,27 |
            JsonDeserialization | 18,149.1075 us | 154.2761 us | 736.00 |  3.00 | 139.00 |       4 675 040,35 |
              OmniSerialization |  3,157.4893 us |  42.5340 us |   9.44 |  1.99 |  76.02 |         615 464,45 |
            OmniDeserialization |  2,338.1632 us |  93.4456 us |   0.27 |  0.13 |  24.59 |         178 476,87 |
            KrakenSerialization |  2,470.0386 us |  39.4308 us |  11.47 |  0.11 |  77.16 |         584 150,15 |
          KrakenDeserialization |  1,324.0410 us |   7.1417 us |   0.12 |  0.06 |  31.60 |         186 382,51 |
   BinaryFormatterSerialization |    537.8107 us |  10.4295 us |  14.94 |  0.15 |  98.41 |         694 775,42 |
 BinaryFormatterDeserialization |    154.4521 us |   2.1852 us |   0.63 |  0.01 |  28.36 |         173 973,33 |


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
              JsonSerialization | 63,102.4858 us | 876.2506 us | 370.00 |  2.00 |  4.00 |       8 745 208,74 |
            JsonDeserialization | 21,806.7454 us | 153.0074 us | 170.38 |  0.51 | 36.75 |       5 081 079,14 |
              OmniSerialization |  5,743.8343 us |  79.7935 us |   2.43 |  0.06 | 20.60 |       1 228 679,58 |
            OmniDeserialization |  3,915.5329 us |  45.8703 us |   0.19 |  0.06 |  6.38 |         364 667,07 |
            KrakenSerialization |  5,523.3790 us |  64.5997 us |   2.48 |  0.07 | 24.24 |       1 174 222,22 |
          KrakenDeserialization |  3,190.8341 us |  34.7238 us |   0.21 |  0.07 |  5.83 |         342 307,55 |
   BinaryFormatterSerialization |  1,060.3438 us |  13.3393 us |   3.17 |  0.05 | 28.82 |       1 190 667,43 |
 BinaryFormatterDeserialization |    303.9988 us |   3.9599 us |   0.15 |  0.00 |  9.05 |         313 206,02 |


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
              JsonSerialization |  30.0451 ms | 0.2822 ms | 328.97 |  10.34 | 20.17 |      10 619 497,56 |
            JsonDeserialization |  38.9162 ms | 0.3967 ms | 330.67 |   9.61 | 18.71 |       8 490 183,06 |
              OmniSerialization |   7.5919 ms | 0.0941 ms |   3.97 |   0.38 | 16.03 |       1 677 001,83 |
            OmniDeserialization |   7.7030 ms | 0.0784 ms |   6.39 |   0.38 | 33.09 |       2 972 142,56 |
            KrakenSerialization |   6.9969 ms | 0.0535 ms | 122.28 |   5.28 | 10.08 |       3 526 410,26 |
          KrakenDeserialization |   7.3306 ms | 0.0956 ms | 107.38 |   5.52 | 21.87 |       4 287 087,08 |
   BinaryFormatterSerialization | 119.4860 ms | 0.8852 ms | 449.02 |   2.93 |  3.91 |      10 769 106,39 |
 BinaryFormatterDeserialization | 292.8191 ms | 2.9905 ms | 262.00 | 135.00 | 52.00 |      15 340 173,16 |


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
              JsonSerialization |  18.3806 ms | 0.1672 ms | 132.00 |   4.80 | 16.56 |       4 854 621,68 |
            JsonDeserialization |  55.8819 ms | 1.1299 ms | 139.40 |  39.32 | 23.23 |      10 841 007,11 |
              OmniSerialization |  12.5285 ms | 0.1959 ms |   3.20 |   0.34 | 11.79 |       1 840 550,13 |
            OmniDeserialization |  25.3816 ms | 0.4350 ms |  23.49 |  27.47 | 27.47 |       6 029 341,32 |
            KrakenSerialization |  37.1892 ms | 0.5032 ms | 115.16 |  18.06 | 38.84 |       9 295 262,86 |
          KrakenDeserialization |  60.8138 ms | 2.2220 ms | 159.92 |  65.42 | 22.62 |      11 961 579,50 |
   BinaryFormatterSerialization | 156.1871 ms | 1.6842 ms | 393.87 |   4.67 | 50.40 |      17 618 396,18 |
 BinaryFormatterDeserialization | 464.7215 ms | 4.1593 ms | 359.00 | 168.00 | 56.00 |      22 602 531,95 |

