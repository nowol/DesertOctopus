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

Type=ProductSerializationBenchmark  Mode=Throughput  

```
                         Method |      Median |    StdDev | Size (bytes) |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |------------ |---------- |------------- |------- |------ |------ |------------------- |
              JsonSerialization |  87.9617 us | 0.7883 us |         7121 | 136.42 |  0.12 |     - |          23 179.01 |
            JsonDeserialization | 166.1926 us | 1.2054 us |              | 312.89 |  0.23 |     - |          45 677.99 |
              OmniSerialization | 195.9952 us | 1.5616 us |        12253 | 302.18 |  0.44 |     - |          46 764.60 |
            OmniDeserialization |  80.4705 us | 0.4885 us |              | 137.06 |  0.12 |     - |          19 347.95 |
            KrakenSerialization | 267.7394 us | 1.6879 us |         7005 | 276.11 |  0.51 |     - |          43 529.83 |
          KrakenDeserialization | 150.9507 us | 1.0393 us |              | 172.48 |  0.23 |     - |          24 580.16 |
   BinaryFormatterSerialization | 436.8176 us | 2.5251 us |        22223 | 614.00 |  1.00 |     - |          96 309.13 |
 BinaryFormatterDeserialization | 400.4278 us | 3.8875 us |              | 562.49 |  0.94 |     - |          80 538.71 |


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
                         Method |     Median |    StdDev | Size (bytes) |    Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |----------- |---------- |------------- |--------- |------ |------ |------------------- |
              JsonSerialization | 15.6250 us | 0.0807 us |          591 |   540.95 |     - |     - |           4 575.56 |
            JsonDeserialization | 15.0959 us | 0.0928 us |              |   256.63 |     - |     - |           2 058.23 |
              OmniSerialization |  1.6506 us | 0.0128 us |          217 |    68.82 |     - |     - |             541.99 |
            OmniDeserialization |  1.1200 us | 0.0089 us |              |    34.51 |     - |     - |             274.21 |
            KrakenSerialization |  2.8762 us | 0.0135 us |          303 |   150.13 |     - |     - |           1 184.13 |
          KrakenDeserialization |  1.6764 us | 0.0095 us |              |    64.43 |     - |     - |             509.19 |
   BinaryFormatterSerialization | 42.0151 us | 0.3363 us |         1735 | 1,257.00 |     - |     - |          10 094.60 |
 BinaryFormatterDeserialization | 23.4980 us | 0.1225 us |              |   784.18 |     - |     - |           6 182.56 |


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
                         Method |         Median |     StdDev | Size (bytes) |  Gen 0 | Gen 1 |  Gen 2 | Bytes Allocated/Op |
------------------------------- |--------------- |----------- |------------- |------- |------ |------- |------------------- |
              JsonSerialization | 10,146.8902 us | 62.6240 us |       600001 | 710.61 | 22.26 | 211.49 |       3 793 332.35 |
            JsonDeserialization | 13,211.6739 us | 86.7962 us |              | 640.00 | 20.00 | 140.00 |       3 393 241.20 |
              OmniSerialization |  1,982.8504 us | 16.8620 us |       300025 |  14.59 |     - | 116.30 |         591 970.21 |
            OmniDeserialization |  1,519.4201 us | 12.3794 us |              |   0.14 |     - |  35.99 |         169 090.99 |
            KrakenSerialization |  2,369.0066 us | 24.5998 us |       400035 |  12.75 |     - | 102.53 |         558 797.29 |
          KrakenDeserialization |  1,165.1577 us | 23.8287 us |              |   0.15 |     - |  36.44 |         171 745.04 |
   BinaryFormatterSerialization |    517.7519 us |  9.5671 us |       400028 |  13.75 |  0.55 | 109.95 |         574 332.06 |
 BinaryFormatterDeserialization |    148.8314 us |  1.9614 us |              |   0.63 |  0.01 |  32.19 |         153 324.80 |


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
                         Method |         Median |      StdDev | Size (bytes) |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |--------------- |------------ |------------- |------- |------ |------ |------------------- |
              JsonSerialization | 61,660.8134 us | 340.5214 us |      1200001 | 370.00 |  2.00 |  4.00 |       8 940 566.58 |
            JsonDeserialization | 22,246.3941 us | 127.1565 us |              | 177.35 |  0.48 | 38.26 |       5 400 911.59 |
              OmniSerialization |  5,499.8681 us |  37.6133 us |       900026 |   2.33 |  0.06 | 20.57 |       1 201 947.15 |
            OmniDeserialization |  4,000.9471 us |  22.3305 us |              |   0.16 |  0.05 |  4.93 |         291 808.18 |
            KrakenSerialization |  5,641.9193 us |  51.3201 us |       800036 |   2.63 |  0.06 | 25.35 |       1 289 858.31 |
          KrakenDeserialization |  2,988.4739 us |  20.0267 us |              |   0.09 |  0.03 |  7.84 |         332 026.49 |
   BinaryFormatterSerialization |  1,060.0437 us |  15.2611 us |       800028 |   3.10 |  0.04 | 28.12 |       1 190 753.46 |
 BinaryFormatterDeserialization |    302.2512 us |   9.7645 us |              |   0.15 |  0.00 |  8.93 |         324 262.95 |


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
                         Method |     Median |    StdDev | Size (bytes) |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |----------- |---------- |------------- |------- |------ |------ |------------------- |
              JsonSerialization | 25.6668 ms | 0.1987 ms |      1200001 | 363.66 |  2.87 | 40.19 |       5 615 092.47 |
            JsonDeserialization | 33.4461 ms | 0.1319 ms |              | 359.60 |  2.70 | 41.35 |       6 660 842.16 |
              OmniSerialization |  5.7152 ms | 0.0516 ms |       700027 | 187.51 |  8.01 | 37.83 |       2 870 151.17 |
            OmniDeserialization |  4.9905 ms | 0.0576 ms |              |   0.87 |  0.12 | 11.26 |         664 027.79 |
            KrakenSerialization | 10.7353 ms | 0.0895 ms |      1600037 | 167.86 |  3.45 | 43.99 |       3 613 011.37 |
          KrakenDeserialization |  8.7105 ms | 0.0432 ms |              |   1.74 |  0.25 |  9.92 |         723 836.35 |
   BinaryFormatterSerialization | 35.8666 ms | 0.2162 ms |      1200028 | 392.19 |  1.96 | 42.05 |       6 033 653.34 |
 BinaryFormatterDeserialization | 44.8678 ms | 0.4677 ms |              | 381.00 |  2.00 |  4.00 |       4 258 133.87 |


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
                         Method |      Median |    StdDev | Size (bytes) |  Gen 0 |  Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |------------ |---------- |------------- |------- |------- |------ |------------------- |
              JsonSerialization |  29.2939 ms | 0.2943 ms |      2400001 | 355.36 |  11.12 | 21.74 |      11 238 547.27 |
            JsonDeserialization |  37.7248 ms | 0.3913 ms |              | 335.47 |  10.22 | 19.52 |       8 439 245.74 |
              OmniSerialization |   7.4202 ms | 0.0788 ms |      1000201 |   3.29 |   0.35 | 14.12 |       1 499 551.94 |
            OmniDeserialization |   7.6163 ms | 0.1067 ms |              |   5.66 |   0.31 | 32.06 |       2 800 848.19 |
            KrakenSerialization |   6.1417 ms | 0.0734 ms |       800075 |   2.82 |   0.18 |  8.64 |       1 125 733.07 |
          KrakenDeserialization |   6.5634 ms | 0.0923 ms |              |   6.01 |   0.36 | 20.78 |       2 590 875.78 |
   BinaryFormatterSerialization | 118.5258 ms | 1.0860 ms |      1701335 | 511.00 |   3.00 |  4.00 |      11 954 489.61 |
 BinaryFormatterDeserialization | 285.1126 ms | 2.1718 ms |              | 262.58 | 137.04 | 52.71 |      15 171 283.96 |


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
                         Method |      Median |    StdDev | Size (bytes) |  Gen 0 |  Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |------------ |---------- |------------- |------- |------- |------ |------------------- |
              JsonSerialization |  18.5149 ms | 0.1878 ms |      1377781 | 163.15 |   5.77 | 20.77 |       5 572 615.27 |
            JsonDeserialization |  55.3362 ms | 0.7929 ms |              | 146.00 |  42.00 | 26.00 |      10 740 508.09 |
              OmniSerialization |  12.1742 ms | 0.1328 ms |       980839 |   3.51 |   0.25 | 14.54 |       2 082 143.31 |
            OmniDeserialization |  25.2233 ms | 0.4483 ms |              |  25.27 |  27.43 | 28.16 |       5 880 995.36 |
            KrakenSerialization |  40.8939 ms | 0.4345 ms |      1588966 |   5.06 |   1.01 | 22.25 |       6 408 449.15 |
          KrakenDeserialization |  61.4966 ms | 3.3098 ms |              | 162.39 |  46.96 | 24.46 |      11 758 652.70 |
   BinaryFormatterSerialization | 163.0233 ms | 1.1477 ms |      2390230 | 471.00 |   3.00 | 61.00 |      19 578 550.13 |
 BinaryFormatterDeserialization | 456.9167 ms | 6.5397 ms |              | 399.00 | 187.00 | 62.00 |      23 477 853.78 |


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
                         Method |    Median |    StdDev | Size (bytes) |    Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |---------- |---------- |------------- |--------- |------ |------ |------------------- |
              JsonSerialization | 2.6485 us | 0.0368 us |         1012 | 3,163.87 |     - |     - |           3 923.93 |
            JsonDeserialization | 3.5694 us | 0.0342 us |              | 1,710.50 |     - |     - |           2 130.86 |
              OmniSerialization | 2.9781 us | 0.0669 us |         1117 | 2,208.00 |     - |     - |           2 614.03 |
            OmniDeserialization | 1.7521 us | 0.0349 us |              | 1,900.80 |     - |     - |           2 325.21 |
            KrakenSerialization | 4.8922 us | 0.0548 us |         1128 | 1,685.39 |     - |     - |           1 948.80 |
          KrakenDeserialization | 1.7875 us | 0.0171 us |              | 1,556.50 |     - |     - |           1 842.30 |
   BinaryFormatterSerialization | 5.2190 us | 0.0747 us |         1281 | 3,618.76 |     - |     - |           4 185.60 |
 BinaryFormatterDeserialization | 4.4912 us | 0.0536 us |              | 3,590.94 |     - |     - |           4 185.48 |


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
                         Method |         Median |      StdDev | Size (bytes) |  Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |--------------- |------------ |------------- |------- |------ |------ |------------------- |
              JsonSerialization |    956.5846 ns |   8.6749 ns |           47 | 736.97 |     - |     - |           3 017.35 |
            JsonDeserialization | 12,638.9269 ns | 109.2316 ns |              | 108.00 |     - |     - |             447.05 |
              OmniSerialization |  1,405.8557 ns |   6.8714 ns |          225 | 160.20 |     - |     - |             584.70 |
            OmniDeserialization |    860.6120 ns |   4.2922 ns |              | 101.94 |     - |     - |             371.79 |
            KrakenSerialization |  1,908.0135 ns |  11.0203 ns |          225 | 151.31 |     - |     - |             555.98 |
          KrakenDeserialization |  1,115.2197 ns |   7.6045 ns |              | 136.46 |     - |     - |             497.83 |
   BinaryFormatterSerialization |  5,518.3988 ns |  38.9746 ns |          456 | 819.84 |     - |     - |           2 966.48 |
 BinaryFormatterDeserialization |  6,156.9420 ns |  74.1828 ns |              | 794.04 |     - |     - |           2 864.89 |


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
                         Method |        Median |     StdDev | Size (bytes) |    Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
------------------------------- |-------------- |----------- |------------- |--------- |------ |------ |------------------- |
              JsonSerialization | 2,827.6212 ns | 26.9933 ns |          124 | 1,170.59 |     - |     - |           2 738.44 |
            JsonDeserialization | 3,049.3568 ns | 20.2098 ns |              |   281.74 |     - |     - |             585.08 |
              OmniSerialization | 1,476.5596 ns | 10.8741 ns |          133 |   239.47 |     - |     - |             500.63 |
            OmniDeserialization |   995.1753 ns |  6.7681 ns |              |    83.72 |     - |     - |             175.48 |
            KrakenSerialization | 2,182.6399 ns |  9.2776 ns |          145 |   246.25 |     - |     - |             517.30 |
          KrakenDeserialization | 1,319.5514 ns |  4.8533 ns |              |   231.87 |     - |     - |             480.50 |
   BinaryFormatterSerialization | 8,957.5271 ns | 57.5852 ns |          393 | 1,556.00 |     - |     - |           3 258.91 |
 BinaryFormatterDeserialization | 9,793.0110 ns | 48.0129 ns |              | 1,735.00 |     - |     - |           3 605.62 |

