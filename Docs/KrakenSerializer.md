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
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=ProductSerializationBenchMark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |      Median |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |------------ |---------- |------- |------ |------ |------------------- |
              JsonSerialization |  86.7885 us | 1.1767 us | 124.69 |  0.13 |     - |          20 432,67 |
            JsonDeserialization | 164.0155 us | 2.8931 us | 225.57 |  0.26 |     - |          31 959,42 |
              OmniSerialization | 201.7485 us | 1.3753 us | 358.83 |  0.49 |     - |          55 450,03 |
            OmniDeserialization |  81.8164 us | 0.9403 us | 126.05 |  0.12 |     - |          17 096,97 |
            KrakenSerialization | 232.0733 us | 1.1898 us | 192.07 |  0.51 |     - |          30 160,13 |
          KrakenDeserialization | 143.6285 us | 2.1854 us | 119.62 |  0.27 |     - |          16 314,53 |
   BinaryFormatterSerialization | 411.3850 us | 2.6584 us | 518.00 |  1.00 |     - |          78 579,88 |
 BinaryFormatterDeserialization | 380.4440 us | 2.0524 us | 407.97 |  0.50 |     - |          55 488,60 |


This benchmark serialize and deserialize a normal sized object that contains all primitives types.

```ini

Host Process Environment Information:
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=SimpleDtoWithEveryPrimitivesSerializationBenchmark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |     Median |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |----------- |---------- |------- |------ |------ |------------------- |
              JsonSerialization | 14.0388 us | 0.1553 us | 376.25 |     - |     - |           3 775,89 |
            JsonDeserialization | 15.2069 us | 0.1315 us | 143.50 |     - |     - |           1 311,72 |
              OmniSerialization |  1.7563 us | 0.0203 us |  49.75 |     - |     - |             454,80 |
            OmniDeserialization |  1.2270 us | 0.0459 us |  26.90 |     - |     - |             250,73 |
            KrakenSerialization |  3.3218 us | 0.1398 us |  92.98 |     - |     - |             864,74 |
          KrakenDeserialization |  1.7189 us | 0.0135 us |  44.99 |     - |     - |             416,36 |
   BinaryFormatterSerialization | 43.9358 us | 0.3190 us | 647.87 |     - |     - |           6 167,84 |
 BinaryFormatterDeserialization | 23.3881 us | 0.1256 us | 447.00 |     - |     - |           4 174,56 |


This benchmark serialize and deserialize an array of 100000 ints.

```ini

Host Process Environment Information:
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=IntArraySerializationBenchmark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |         Median |      StdDev |  Gen 0 | Gen 1 |  Gen 2 | Bytes Allocated/Op |
------------------------------- |--------------- |------------ |------- |------ |------- |------------------- |
              JsonSerialization | 14,387.4911 us | 181.6013 us | 332.23 | 17.59 | 148.53 |       2 758 447,55 |
            JsonDeserialization | 14,854.6212 us | 155.6219 us | 288.00 | 16.00 | 119.00 |       2 507 971,90 |
              OmniSerialization |  2,054.0175 us |  24.6995 us |  11.26 |  0.11 |  92.38 |         591 140,04 |
            OmniDeserialization |  1,228.8840 us |  13.4983 us |   0.06 |     - |  27.75 |         163 995,79 |
            KrakenSerialization |  2,498.5011 us |  19.6503 us |  11.42 |     - |  94.07 |         642 181,30 |
          KrakenDeserialization |  1,486.9334 us |  10.2637 us |   0.06 |     - |  31.38 |         184 841,15 |
   BinaryFormatterSerialization |    505.8570 us |  12.5987 us |  11.78 |  0.47 |  93.94 |         622 184,06 |
 BinaryFormatterDeserialization |    145.6355 us |   2.4004 us |   0.55 |     - |  28.10 |         168 418,10 |


This benchmark serialize and deserialize an array of 100000 doubles.

```ini

Host Process Environment Information:
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=DoubleArraySerializationBenchmark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |         Median |      StdDev |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |--------------- |------------ |------- |------ |------ |------------------- |
              JsonSerialization | 37,369.3881 us | 296.2636 us | 665.00 |  3.00 | 45.00 |       8 085 908,31 |
            JsonDeserialization | 21,407.3122 us | 168.7659 us | 322.15 |  0.53 | 80.14 |       5 003 126,41 |
              OmniSerialization |  5,835.3802 us |  61.5085 us |   5.29 |  0.13 | 50.34 |       1 295 409,40 |
            OmniDeserialization |  4,487.6235 us |  36.5223 us |   0.41 |  0.14 | 12.97 |         342 878,04 |
            KrakenSerialization |  6,103.5996 us |  45.5542 us |   5.41 |  0.14 | 59.73 |       1 281 269,81 |
          KrakenDeserialization |  3,641.3109 us |  30.7268 us |   0.41 |  0.14 | 12.58 |         351 020,50 |
   BinaryFormatterSerialization |  1,026.6087 us |  15.7329 us |   6.90 |  0.03 | 62.99 |       1 244 321,95 |
 BinaryFormatterDeserialization |    299.4381 us |   4.0749 us |   0.32 |  0.01 | 19.45 |         331 150,63 |


This benchmark serialize and deserialize an array of 100000 decimals.

```ini

Host Process Environment Information:
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=DecimalArraySerializationBenchmark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |     Median |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |----------- |---------- |------- |------ |------ |------------------- |
              JsonSerialization | 33.5245 ms | 0.1801 ms | 277.37 | 20.58 | 42.05 |       5 403 893,55 |
            JsonDeserialization | 30.8496 ms | 0.1840 ms | 241.63 | 19.10 | 56.35 |       5 852 915,48 |
              OmniSerialization |  6.2729 ms | 0.0938 ms | 129.56 |  4.84 | 27.85 |       2 471 740,81 |
            OmniDeserialization |  5.4471 ms | 0.0697 ms |   0.66 |  0.11 | 11.64 |         687 140,30 |
            KrakenSerialization | 10.8245 ms | 0.0978 ms | 119.00 |  0.23 | 38.99 |       3 340 826,87 |
          KrakenDeserialization |  6.6678 ms | 0.0686 ms | 118.70 |  0.50 | 17.10 |       1 787 433,27 |
   BinaryFormatterSerialization | 41.3460 ms | 0.1706 ms | 254.14 | 18.89 | 39.49 |       4 950 531,25 |
 BinaryFormatterDeserialization | 41.3649 ms | 0.2242 ms | 256.00 |  2.00 |  4.00 |       3 187 791,76 |


This benchmark serialize and deserialize an Dictionary of int,int with 100000 items.

```ini

Host Process Environment Information:
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=DictionaryIntIntSerializationBenchmark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |      Median |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |------------ |---------- |------- |------ |------ |------------------- |
              JsonSerialization |  40.6955 ms | 0.2379 ms | 197.93 |  0.97 | 33.31 |       8 104 491,31 |
            JsonDeserialization |  38.0549 ms | 0.2185 ms | 227.93 |  9.91 | 19.82 |       7 195 943,12 |
              OmniSerialization |   7.1917 ms | 0.0562 ms |   3.16 |  0.21 | 16.74 |       1 853 178,62 |
            OmniDeserialization |   7.4347 ms | 0.0489 ms |   5.15 |  0.24 | 29.79 |       2 865 867,20 |
            KrakenSerialization |   6.8963 ms | 0.1337 ms |  52.65 |  0.94 |  8.42 |       2 184 443,15 |
          KrakenDeserialization |   7.9230 ms | 0.2220 ms |  54.17 |  6.67 | 18.72 |       3 634 671,33 |
   BinaryFormatterSerialization | 123.3149 ms | 0.7623 ms | 273.00 |  2.00 |  3.00 |       8 161 652,14 |
 BinaryFormatterDeserialization | 199.9160 ms | 2.1919 ms | 162.14 | 86.93 | 53.72 |      10 257 199,33 |


This benchmark serialize and deserialize an Dictionary of string,int with 100000 items.

```ini

Host Process Environment Information:
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=DictionaryStringIntSerializationBenchmark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |      Median |    StdDev |  Gen 0 |  Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |------------ |---------- |------- |------- |------ |------------------- |
              JsonSerialization |  26.7234 ms | 4.6367 ms |  80.15 |   5.63 | 19.36 |       4 157 062,09 |
            JsonDeserialization |  52.7845 ms | 0.4732 ms |  53.28 |  40.20 |  5.61 |       6 381 962,72 |
              OmniSerialization |  12.8622 ms | 0.0886 ms |   0.94 |   0.70 |  9.87 |       1 748 201,61 |
            OmniDeserialization |  26.0075 ms | 0.2130 ms |  10.09 |  23.30 | 14.89 |       4 052 309,87 |
            KrakenSerialization |  43.0580 ms | 0.4964 ms |  36.35 |   1.77 | 20.39 |       6 026 097,64 |
          KrakenDeserialization |  52.8656 ms | 1.1373 ms |  78.00 |  40.00 |  5.00 |       7 803 266,00 |
   BinaryFormatterSerialization | 159.1567 ms | 1.1000 ms | 246.53 |  23.89 | 74.53 |      14 197 212,49 |
 BinaryFormatterDeserialization | 424.8695 ms | 1.6089 ms | 117.53 | 173.91 | 99.38 |      12 185 321,82 |


This benchmark serialize and deserialize a string of 1000 characters.

```ini

Host Process Environment Information:
BenchmarkDotNet=v0.9.8.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
CLR=MS.NET 4.0.30319.42000, Arch=32-bit ?
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0

Type=StringSerializationBenchmark  Mode=Throughput  GarbageCollection=Concurrent Workstation  

```
                         Method |    Median |    StdDev |    Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |---------- |---------- |--------- |------ |------ |------------------- |
              JsonSerialization | 3.0581 us | 0.0251 us | 2,750.00 |     - |     - |           3 488,96 |
            JsonDeserialization | 3.6133 us | 0.0198 us | 1,457.43 |     - |     - |           1 843,98 |
              OmniSerialization | 3.0483 us | 0.0162 us | 2,087.45 |     - |     - |           2 524,58 |
            OmniDeserialization | 1.6869 us | 0.0192 us | 1,632.09 |     - |     - |           2 045,79 |
            KrakenSerialization | 4.2266 us | 0.0719 us | 1,428.15 |     - |     - |           1 717,23 |
          KrakenDeserialization | 1.7949 us | 0.0127 us | 1,419.50 |     - |     - |           1 729,37 |
   BinaryFormatterSerialization | 5.2830 us | 0.0890 us | 2,783.53 |     - |     - |           3 320,72 |
 BinaryFormatterDeserialization | 4.1850 us | 0.0367 us | 2,893.54 |     - |     - |           3 486,09 |

