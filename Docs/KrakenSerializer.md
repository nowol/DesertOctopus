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
              JsonSerialization |  88.0937 us | 0.7367 us | 125.68 |  0.12 |     - |          22 377,84 |
            JsonDeserialization | 169.0561 us | 1.3487 us | 297.53 |  0.23 |     - |          45 717,42 |
              OmniSerialization | 196.0990 us | 0.9261 us | 316.74 |  0.48 |     - |          51 866,34 |
            OmniDeserialization |  83.7059 us | 0.5846 us | 123.38 |  0.11 |     - |          18 410,63 |
            KrakenSerialization | 247.7671 us | 3.9636 us | 284.73 |  0.48 |     - |          46 336,11 |
          KrakenDeserialization | 146.8461 us | 1.7461 us | 170.78 |  0.24 |     - |          25 491,81 |
   BinaryFormatterSerialization | 413.2798 us | 2.1433 us | 633.27 |  0.98 |     - |         103 443,78 |
 BinaryFormatterDeserialization | 395.3170 us | 3.3793 us | 510.00 |  1.00 |     - |          76 607,62 |


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
              JsonSerialization | 16.0555 us | 0.1453 us |   439.44 |     - |     - |           4 106,19 |
            JsonDeserialization | 15.1646 us | 0.1188 us |   225.29 |     - |     - |           1 971,58 |
              OmniSerialization |  1.6887 us | 0.0149 us |    62.25 |     - |     - |             536,98 |
            OmniDeserialization |  1.1426 us | 0.0082 us |    35.62 |     - |     - |             307,81 |
            KrakenSerialization |  2.9472 us | 0.0154 us |   135.99 |     - |     - |           1 163,38 |
          KrakenDeserialization |  1.5995 us | 0.0185 us |    69.89 |     - |     - |             602,93 |
   BinaryFormatterSerialization | 41.4239 us | 0.3069 us | 1,077.00 |     - |     - |           9 421,77 |
 BinaryFormatterDeserialization | 23.7631 us | 0.1244 us |   738.00 |     - |     - |           6 270,64 |


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
              JsonSerialization | 10,019.8336 us |  85.7944 us | 591.63 | 18.53 | 177.05 |       3 793 104,23 |
            JsonDeserialization | 13,536.2045 us | 107.0501 us | 574.00 | 18.00 | 125.00 |       3 668 587,06 |
              OmniSerialization |  2,197.2542 us | 137.1488 us |  12.09 |     - |  98.09 |         600 584,34 |
            OmniDeserialization |  1,321.5391 us |   7.3180 us |   0.12 |     - |  32.99 |         185 781,61 |
            KrakenSerialization |  2,472.3169 us |  21.7308 us |  11.31 |     - |  91.82 |         601 252,73 |
          KrakenDeserialization |  1,159.2009 us |   7.6155 us |   0.12 |     - |  28.10 |         159 299,92 |
   BinaryFormatterSerialization |    527.2252 us |   5.5007 us |  12.75 |  0.51 | 101.56 |         639 683,54 |
 BinaryFormatterDeserialization |    152.4571 us |   1.9818 us |   0.62 |     - |  30.92 |         177 734,83 |


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
              JsonSerialization | 62,692.0810 us | 255.0361 us | 349.00 |  2.00 |  4.00 |       9 047 136,05 |
            JsonDeserialization | 21,486.9106 us | 120.5846 us | 153.75 |  0.49 | 33.16 |       5 024 791,63 |
              OmniSerialization |  5,523.0227 us |  31.7377 us |   2.57 |  0.06 | 21.66 |       1 399 773,51 |
            OmniDeserialization |  3,702.5557 us |  21.2859 us |   0.18 |  0.06 |  5.38 |         334 202,49 |
            KrakenSerialization |  5,506.6127 us |  28.2710 us |   2.19 |  0.05 | 21.57 |       1 173 804,77 |
          KrakenDeserialization |  2,989.5979 us |  13.8971 us |   0.09 |  0.03 |  6.96 |         318 187,33 |
   BinaryFormatterSerialization |  1,072.4717 us |  12.6809 us |   3.18 |  0.04 | 28.97 |       1 309 106,16 |
 BinaryFormatterDeserialization |    307.7846 us |   7.0684 us |   0.15 |  0.00 |  9.01 |         342 163,20 |


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
              JsonSerialization | 26.8368 ms | 0.4569 ms | 330.18 |  1.82 | 35.57 |       5 452 940,88 |
            JsonDeserialization | 34.1069 ms | 0.4848 ms | 337.60 |  1.87 | 39.17 |       6 689 071,84 |
              OmniSerialization |  5.6890 ms | 0.0767 ms | 177.54 |  7.39 | 37.63 |       2 911 048,25 |
            OmniDeserialization |  4.9256 ms | 0.1929 ms |   0.81 |  0.10 | 11.71 |         712 695,79 |
            KrakenSerialization | 10.6218 ms | 0.0753 ms | 170.84 |  3.39 | 44.52 |       3 938 148,65 |
          KrakenDeserialization |  5.8234 ms | 0.0258 ms | 166.85 |  0.24 | 20.30 |       2 247 682,17 |
   BinaryFormatterSerialization | 31.5472 ms | 0.1708 ms | 333.95 |  1.95 | 36.13 |       5 520 287,69 |
 BinaryFormatterDeserialization | 44.7145 ms | 0.3078 ms | 342.00 |  2.00 |  4.00 |       4 112 150,07 |


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
              JsonSerialization |  29.2729 ms | 0.1728 ms | 334.08 |   9.92 | 19.85 |      11 264 699,91 |
            JsonDeserialization |  37.8338 ms | 0.2939 ms | 368.51 |  11.10 | 20.81 |       9 892 899,63 |
              OmniSerialization |   7.3826 ms | 0.0397 ms |   3.83 |   0.24 | 15.45 |       1 727 065,44 |
            OmniDeserialization |   8.2167 ms | 0.0830 ms |   6.25 |   0.37 | 31.97 |       3 040 076,30 |
            KrakenSerialization |   6.8842 ms | 0.0672 ms | 109.82 |   4.64 |  9.03 |       3 314 131,82 |
          KrakenDeserialization |   7.2293 ms | 0.0867 ms | 115.29 |   5.62 | 24.02 |       4 804 793,36 |
   BinaryFormatterSerialization | 117.6890 ms | 0.4806 ms | 516.00 |   2.87 |  3.82 |      12 904 658,71 |
 BinaryFormatterDeserialization | 290.0702 ms | 6.4650 ms | 278.00 | 141.00 | 55.00 |      16 990 026,56 |


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
              JsonSerialization |  18.1417 ms | 0.2714 ms | 146.85 |   4.78 | 18.61 |       5 240 303,65 |
            JsonDeserialization |  54.5533 ms | 0.7443 ms | 134.73 |  37.27 | 22.93 |      10 266 882,13 |
              OmniSerialization |  12.1181 ms | 0.1464 ms |   3.21 |   0.23 | 12.50 |       1 897 598,28 |
            OmniDeserialization |  25.3894 ms | 0.3391 ms |  23.59 |  25.14 | 25.36 |       5 649 309,08 |
            KrakenSerialization |  40.7877 ms | 2.2060 ms |  63.32 |  10.87 | 34.49 |       8 984 727,60 |
          KrakenDeserialization |  61.8563 ms | 3.8770 ms | 129.00 |  66.00 | 22.00 |      11 710 240,09 |
   BinaryFormatterSerialization | 156.0886 ms | 1.9538 ms | 436.84 |   2.93 | 55.70 |      19 022 987,05 |
 BinaryFormatterDeserialization | 453.8013 ms | 8.1709 ms | 358.00 | 167.00 | 57.00 |      22 041 841,67 |


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
              JsonSerialization | 2.6116 us | 0.0231 us | 3,001.08 |     - |     - |           3 722,30 |
            JsonDeserialization | 3.5121 us | 0.0337 us | 1,622.26 |     - |     - |           2 021,37 |
              OmniSerialization | 2.9821 us | 0.0188 us | 1,927.75 |     - |     - |           2 278,23 |
            OmniDeserialization | 1.7067 us | 0.0173 us | 1,684.83 |     - |     - |           2 058,79 |
            KrakenSerialization | 4.7716 us | 0.0308 us | 1,817.69 |     - |     - |           2 101,50 |
          KrakenDeserialization | 1.7804 us | 0.0136 us | 1,659.50 |     - |     - |           1 974,83 |
   BinaryFormatterSerialization | 5.0991 us | 0.0393 us | 3,116.51 |     - |     - |           3 598,66 |
 BinaryFormatterDeserialization | 4.3829 us | 0.0469 us | 3,453.00 |     - |     - |           4 031,46 |

