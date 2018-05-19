## Serialization / DesertOctopus.KrakenSerializer

The main use of this serializer is to serialize DTOs using binary serialization.  The serialization engine is implemented using expression trees which mean a small performance hit the first type you serialize a type for the creation of the expression tree.  Subsequent serialization for the same type can use the compiled expression tree without having to suffer the performance hit.

The main pros of DesertOctopus.KrakenSerializer are:

* Binary serialization for use in remote caching server scenarios
* Does not require objects to be decorated with the `Serializable` attribute
  * It is up to the user of the serializer to ensure that the objects can be safely serialized
* Serialize all fields (private, public, etc) of an object
* Thread safe
* Supports interface `ISerializable`
  * Your class needs to have the corresponding serialization constructor otherwise it will be serialized as a normal class
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

``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 8.1 (6.3.9600.0)
Intel Core i5-4690 CPU 3.50GHz (Haswell), 1 CPU, 4 logical and 4 physical cores
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0


```
|                                   Method |     Mean |      Error |     StdDev | Size (bytes) |    Gen 0 |   Gen 1 |   Gen 2 | Allocated |
|----------------------------------------- |---------:|-----------:|-----------:|------------- |---------:|--------:|--------:|----------:|
|                        JsonSerialization | 672.6 us | 18.7013 us | 19.2048 us |        37375 | 146.4844 |  0.9766 |       - | 454.55 KB |
|                      JsonDeserialization | 673.5 us |  5.5046 us |  5.1490 us |              |  36.1328 |  9.7656 |       - | 120.06 KB |
|                      KrakenSerialization | 314.8 us |  4.8563 us |  4.5426 us |        14719 |  30.2734 |       - |       - |  93.17 KB |
|                    KrakenDeserialization | 155.7 us |  1.0993 us |  1.0283 us |              |  21.7285 |       - |       - |  67.03 KB |
|   KrakenSerializationWithOmittedRootType | 310.7 us |  3.4887 us |  3.0926 us |        14603 |  29.7852 |       - |       - |     93 KB |
| KrakenDeserializationWithOmittedRootType | 155.8 us |  1.2038 us |  1.1260 us |              |  21.7285 |       - |       - |   66.9 KB |
|             BinaryFormatterSerialization | 996.6 us |  4.7385 us |  4.2006 us |        72220 | 164.0625 | 42.9688 | 41.0156 | 636.22 KB |
|           BinaryFormatterDeserialization | 855.7 us | 16.1918 us | 16.6278 us |              |  89.8438 | 30.2734 |       - |  390.5 KB |
|               NetSerializerSerialization | 143.5 us |  0.5811 us |  0.5436 us |        24599 |  29.7852 |       - |       - |  92.03 KB |
|             NetSerializerDeserialization | 143.2 us |  2.1457 us |  2.0071 us |              |  32.2266 |  0.7324 |       - |  99.58 KB |


This benchmark serialize and deserialize a normal sized object that contains all primitives types.

``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 8.1 (6.3.9600.0)
Intel Core i5-4690 CPU 3.50GHz (Haswell), 1 CPU, 4 logical and 4 physical cores
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0


```
|                                   Method |        Mean |      Error |     StdDev | Size (bytes) |  Gen 0 | Allocated |
|----------------------------------------- |------------:|-----------:|-----------:|------------- |-------:|----------:|
|                        JsonSerialization | 15,147.9 ns |  92.596 ns |  82.084 ns |          591 | 3.1128 |    9826 B |
|                      JsonDeserialization | 15,180.5 ns | 168.176 ns | 149.084 ns |              | 1.1597 |    3720 B |
|                      KrakenSerialization |  2,451.6 ns |  26.867 ns |  23.817 ns |          231 | 0.6714 |    2120 B |
|                    KrakenDeserialization |  1,434.2 ns |  10.059 ns |   9.409 ns |              | 0.4368 |    1376 B |
|   KrakenSerializationWithOmittedRootType |  1,758.4 ns |  19.239 ns |  17.996 ns |          120 | 0.5665 |    1784 B |
| KrakenDeserializationWithOmittedRootType |  1,130.4 ns |  10.443 ns |   9.768 ns |              | 0.2880 |     912 B |
|             BinaryFormatterSerialization | 39,372.8 ns | 345.972 ns | 323.623 ns |         1735 | 7.8125 |   24648 B |
|           BinaryFormatterDeserialization | 22,631.6 ns | 106.645 ns |  99.756 ns |              | 4.7913 |   15152 B |
|               NetSerializerSerialization |    770.1 ns |   4.145 ns |   3.878 ns |          105 | 0.1898 |     600 B |
|             NetSerializerDeserialization |    637.6 ns |   8.682 ns |   8.121 ns |              | 0.1240 |     392 B |


This benchmark serialize and deserialize an array of 100000 ints.

``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 8.1 (6.3.9600.0)
Intel Core i5-4690 CPU 3.50GHz (Haswell), 1 CPU, 4 logical and 4 physical cores
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0


```
|                                   Method |        Mean |     Error |    StdDev | Size (bytes) |     Gen 0 |    Gen 1 |    Gen 2 |  Allocated |
|----------------------------------------- |------------:|----------:|----------:|------------- |----------:|---------:|---------:|-----------:|
|                        JsonSerialization | 10,816.7 us | 34.447 us | 32.221 us |       600001 | 2625.0000 | 640.6250 | 562.5000 | 8890.69 KB |
|                      JsonDeserialization |  9,769.5 us | 68.712 us | 64.273 us |              | 1109.3750 | 312.5000 | 312.5000 | 3760.65 KB |
|                      KrakenSerialization |  2,134.7 us |  8.597 us |  7.621 us |       300033 |  363.2813 | 320.3125 | 320.3125 | 1323.63 KB |
|                    KrakenDeserialization |  1,061.7 us |  5.576 us |  5.216 us |              |  105.4688 | 105.4688 | 105.4688 |  392.13 KB |
|   KrakenSerializationWithOmittedRootType |  2,153.8 us | 14.298 us | 13.374 us |       300014 |  363.2813 | 320.3125 | 320.3125 | 1323.68 KB |
| KrakenDeserializationWithOmittedRootType |  1,061.1 us |  3.685 us |  3.447 us |              |  103.5156 | 103.5156 | 103.5156 |   392.4 KB |
|             BinaryFormatterSerialization |    617.7 us | 10.173 us |  9.516 us |       400028 |  255.8594 | 213.8672 | 212.8906 | 1427.62 KB |
|           BinaryFormatterDeserialization |    188.8 us |  3.227 us |  3.018 us |              |   73.7305 |  71.7773 |  71.5332 |  397.46 KB |
|               NetSerializerSerialization |  2,058.6 us | 17.642 us | 16.502 us |       300004 |  363.2813 | 324.2188 | 324.2188 | 1317.26 KB |
|             NetSerializerDeserialization |  1,192.7 us |  5.112 us |  4.781 us |              |  103.5156 | 103.5156 | 103.5156 |  391.35 KB |


This benchmark serialize and deserialize an array of 100000 doubles.

``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 8.1 (6.3.9600.0)
Intel Core i5-4690 CPU 3.50GHz (Haswell), 1 CPU, 4 logical and 4 physical cores
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0


```
|                                   Method |        Mean |      Error |     StdDev | Size (bytes) |     Gen 0 |    Gen 1 |    Gen 2 |   Allocated |
|----------------------------------------- |------------:|-----------:|-----------:|------------- |----------:|---------:|---------:|------------:|
|                        JsonSerialization | 57,815.4 us | 200.050 us | 177.340 us |      1200001 | 5875.0000 | 625.0000 | 500.0000 | 21681.33 KB |
|                      JsonDeserialization | 25,745.7 us |  96.432 us |  90.203 us |              | 2593.7500 | 281.2500 | 281.2500 |  9862.13 KB |
|                      KrakenSerialization |  6,315.2 us |  27.008 us |  25.263 us |       900034 |  343.7500 | 304.6875 | 304.6875 |   2931.7 KB |
|                    KrakenDeserialization |  3,456.8 us |  17.746 us |  16.599 us |              |   78.1250 |  78.1250 |  78.1250 |   782.28 KB |
|   KrakenSerializationWithOmittedRootType |  6,390.1 us |  55.197 us |  51.631 us |       900014 |  398.4375 | 367.1875 | 359.3750 |  2931.92 KB |
| KrakenDeserializationWithOmittedRootType |  3,441.7 us |  18.754 us |  17.542 us |              |   70.3125 |  70.3125 |  70.3125 |   782.52 KB |
|             BinaryFormatterSerialization |  1,201.8 us |  22.764 us |  23.377 us |       800028 |  289.0625 | 248.0469 | 246.0938 |  2849.01 KB |
|           BinaryFormatterDeserialization |    341.5 us |   6.565 us |   7.297 us |              |   85.4492 |  83.4961 |  83.4961 |    788.1 KB |
|               NetSerializerSerialization |  5,513.0 us |  32.687 us |  28.976 us |       900004 |  390.6250 | 351.5625 | 351.5625 |  2927.22 KB |
|             NetSerializerDeserialization |  3,170.1 us |  11.463 us |  10.722 us |              |   82.0313 |  82.0313 |  82.0313 |   781.86 KB |


This benchmark serialize and deserialize an array of 100000 decimals.

``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 8.1 (6.3.9600.0)
Intel Core i5-4690 CPU 3.50GHz (Haswell), 1 CPU, 4 logical and 4 physical cores
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0


```
|                                   Method |      Mean |     Error |    StdDev | Size (bytes) |     Gen 0 |    Gen 1 |    Gen 2 | Allocated |
|----------------------------------------- |----------:|----------:|----------:|------------- |----------:|---------:|---------:|----------:|
|                        JsonSerialization | 26.616 ms | 0.0935 ms | 0.0874 ms |      1200001 | 2812.5000 | 312.5000 | 250.0000 |  12.78 MB |
|                      JsonDeserialization | 18.051 ms | 0.0895 ms | 0.0793 ms |              | 1468.7500 | 437.5000 | 437.5000 |   8.58 MB |
|                      KrakenSerialization |  5.550 ms | 0.0499 ms | 0.0467 ms |       700035 | 1609.3750 | 359.3750 | 296.8750 |   6.48 MB |
|                    KrakenDeserialization |  6.457 ms | 0.0421 ms | 0.0394 ms |              |  117.1875 | 117.1875 | 117.1875 |   1.53 MB |
|   KrakenSerializationWithOmittedRootType |  5.602 ms | 0.0523 ms | 0.0489 ms |       700014 | 1578.1250 | 289.0625 | 265.6250 |   6.48 MB |
| KrakenDeserializationWithOmittedRootType |  6.444 ms | 0.0455 ms | 0.0426 ms |              |  132.8125 | 132.8125 | 132.8125 |   1.53 MB |
|             BinaryFormatterSerialization | 32.735 ms | 0.3129 ms | 0.2927 ms |      1200028 | 2812.5000 | 312.5000 | 250.0000 |  12.78 MB |
|           BinaryFormatterDeserialization | 44.297 ms | 0.3551 ms | 0.3322 ms |              | 2500.0000 |        - |        - |   9.16 MB |
|               NetSerializerSerialization |  5.725 ms | 0.0642 ms | 0.0600 ms |       700004 | 1609.3750 | 359.3750 | 296.8750 |   6.48 MB |
|             NetSerializerDeserialization |  4.250 ms | 0.0284 ms | 0.0252 ms |              |   93.7500 |  93.7500 |  93.7500 |   1.53 MB |


This benchmark serialize and deserialize an Dictionary of int,int with 100000 items.

``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 8.1 (6.3.9600.0)
Intel Core i5-4690 CPU 3.50GHz (Haswell), 1 CPU, 4 logical and 4 physical cores
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0


```
|                                   Method |       Mean |     Error |    StdDev | Size (bytes) |     Gen 0 |     Gen 1 |    Gen 2 | Allocated |
|----------------------------------------- |-----------:|----------:|----------:|------------- |----------:|----------:|---------:|----------:|
|                        JsonSerialization |  33.539 ms | 0.3623 ms | 0.3212 ms |      2400001 | 4875.0000 |  437.5000 | 312.5000 |  24.03 MB |
|                      JsonDeserialization |  27.163 ms | 0.2037 ms | 0.1806 ms |              | 1843.7500 |  375.0000 | 250.0000 |  10.34 MB |
|                      KrakenSerialization |   7.460 ms | 0.0704 ms | 0.0658 ms |      1000073 |  210.9375 |  171.8750 | 171.8750 |   2.96 MB |
|                    KrakenDeserialization |   7.624 ms | 0.0823 ms | 0.0730 ms |              |  226.5625 |  164.0625 | 148.4375 |   5.76 MB |
|   KrakenSerializationWithOmittedRootType |   7.425 ms | 0.0422 ms | 0.0353 ms |      1000014 |  195.3125 |  156.2500 | 156.2500 |   2.96 MB |
| KrakenDeserializationWithOmittedRootType |   7.805 ms | 0.1120 ms | 0.1047 ms |              |  234.3750 |  171.8750 | 156.2500 |   5.76 MB |
|             BinaryFormatterSerialization | 116.046 ms | 0.4676 ms | 0.4374 ms |      1701335 | 7000.0000 |  187.5000 | 125.0000 |  26.99 MB |
|           BinaryFormatterDeserialization | 278.718 ms | 2.3552 ms | 2.0878 ms |              | 6687.5000 | 2875.0000 | 937.5000 |  38.15 MB |
|               NetSerializerSerialization |   7.345 ms | 0.0538 ms | 0.0477 ms |      1000005 |  250.0000 |  210.9375 | 210.9375 |   3.72 MB |
|             NetSerializerDeserialization |   6.034 ms | 0.0173 ms | 0.0162 ms |              |  164.0625 |  164.0625 | 164.0625 |   2.84 MB |


This benchmark serialize and deserialize an Dictionary of string,int with 100000 items.

``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 8.1 (6.3.9600.0)
Intel Core i5-4690 CPU 3.50GHz (Haswell), 1 CPU, 4 logical and 4 physical cores
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0


```
|                                   Method |      Mean |     Error |    StdDev | Size (bytes) |     Gen 0 |     Gen 1 |     Gen 2 | Allocated |
|----------------------------------------- |----------:|----------:|----------:|------------- |----------:|----------:|----------:|----------:|
|                        JsonSerialization |  22.03 ms | 0.1132 ms | 0.1059 ms |      1377781 | 2250.0000 |  250.0000 |  187.5000 |  11.42 MB |
|                      JsonDeserialization |  45.00 ms | 0.5256 ms | 0.4659 ms |              | 1250.0000 |  562.5000 |  250.0000 |  14.16 MB |
|                      KrakenSerialization |  42.46 ms | 0.3940 ms | 0.3685 ms |      1180708 |  437.5000 |  375.0000 |  375.0000 |  13.19 MB |
|                    KrakenDeserialization |  63.46 ms | 0.5791 ms | 0.5417 ms |              | 3250.0000 | 1250.0000 |  500.0000 |  24.68 MB |
|   KrakenSerializationWithOmittedRootType |  42.26 ms | 0.4422 ms | 0.4136 ms |      1180648 |  562.5000 |  500.0000 |  500.0000 |  13.19 MB |
| KrakenDeserializationWithOmittedRootType |  63.65 ms | 0.8864 ms | 0.7857 ms |              | 3375.0000 | 1125.0000 |  500.0000 |  24.66 MB |
|             BinaryFormatterSerialization | 151.19 ms | 0.5978 ms | 0.5592 ms |      2390230 | 6937.5000 |  687.5000 |  500.0000 |  43.44 MB |
|           BinaryFormatterDeserialization | 456.80 ms | 7.8719 ms | 7.3634 ms |              | 8375.0000 | 3250.0000 | 1000.0000 |  52.31 MB |
|               NetSerializerSerialization |  11.92 ms | 0.0705 ms | 0.0625 ms |       980639 |  265.6250 |  234.3750 |  234.3750 |   4.46 MB |
|             NetSerializerDeserialization |  22.55 ms | 0.2004 ms | 0.1776 ms |              |  812.5000 |  468.7500 |  218.7500 |   8.24 MB |


This benchmark serialize and deserialize a string of 1000 characters.

``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 8.1 (6.3.9600.0)
Intel Core i5-4690 CPU 3.50GHz (Haswell), 1 CPU, 4 logical and 4 physical cores
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0


```
|                                   Method |     Mean |     Error |    StdDev | Size (bytes) |  Gen 0 | Allocated |
|----------------------------------------- |---------:|----------:|----------:|------------- |-------:|----------:|
|                        JsonSerialization | 2.762 us | 0.0302 us | 0.0282 us |         1012 | 2.3994 |   7.38 KB |
|                      JsonDeserialization | 3.177 us | 0.0183 us | 0.0162 us |              | 0.6638 |   2.05 KB |
|                      KrakenSerialization | 5.087 us | 0.0322 us | 0.0301 us |         1126 | 1.4343 |   4.43 KB |
|                    KrakenDeserialization | 1.869 us | 0.0148 us | 0.0138 us |              | 1.3180 |   4.06 KB |
|   KrakenSerializationWithOmittedRootType | 4.314 us | 0.0196 us | 0.0184 us |         1015 | 1.2970 |   3.99 KB |
| KrakenDeserializationWithOmittedRootType | 1.647 us | 0.0168 us | 0.0149 us |              | 1.1711 |    3.6 KB |
|             BinaryFormatterSerialization | 5.241 us | 0.0478 us | 0.0424 us |         1281 | 3.0136 |   9.27 KB |
|           BinaryFormatterDeserialization | 4.277 us | 0.0822 us | 0.0914 us |              | 2.8076 |   8.63 KB |
|               NetSerializerSerialization | 1.846 us | 0.0361 us | 0.0443 us |         1005 | 0.9518 |   2.93 KB |
|             NetSerializerDeserialization | 1.328 us | 0.0169 us | 0.0158 us |              | 1.3218 |   4.06 KB |


This benchmark serialize and deserialize a large struct.

``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 8.1 (6.3.9600.0)
Intel Core i5-4690 CPU 3.50GHz (Haswell), 1 CPU, 4 logical and 4 physical cores
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0


```
|                                   Method |        Mean |      Error |     StdDev | Size (bytes) |  Gen 0 | Allocated |
|----------------------------------------- |------------:|-----------:|-----------:|------------- |-------:|----------:|
|                        JsonSerialization |    985.8 ns |  10.473 ns |   9.796 ns |           47 | 1.8673 |    5880 B |
|                      JsonDeserialization | 50,096.5 ns | 505.951 ns | 473.267 ns |              | 0.3052 |    1112 B |
|                      KrakenSerialization |  2,056.0 ns |  11.411 ns |  10.116 ns |          233 | 0.4845 |    1536 B |
|                    KrakenDeserialization |  1,207.7 ns |   6.076 ns |   5.387 ns |              | 0.4005 |    1264 B |
|   KrakenSerializationWithOmittedRootType |  1,150.9 ns |   6.535 ns |   5.793 ns |           52 | 0.3376 |    1064 B |
| KrakenDeserializationWithOmittedRootType |    855.5 ns |   4.080 ns |   3.817 ns |              | 0.1879 |     592 B |
|             BinaryFormatterSerialization |  5,317.2 ns |  46.628 ns |  41.334 ns |          456 | 2.1820 |    6864 B |
|           BinaryFormatterDeserialization |  6,153.7 ns |  41.805 ns |  39.104 ns |              | 2.2507 |    7104 B |
|               NetSerializerSerialization |    333.0 ns |   1.280 ns |   1.069 ns |           41 | 0.1445 |     456 B |
|             NetSerializerDeserialization |    299.1 ns |   3.302 ns |   3.088 ns |              | 0.0405 |     128 B |


This benchmark serialize and deserialize a small class used by the Wire project.

``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 8.1 (6.3.9600.0)
Intel Core i5-4690 CPU 3.50GHz (Haswell), 1 CPU, 4 logical and 4 physical cores
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
  [Host]     : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0
  DefaultJob : .NET Framework 4.7 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2563.0


```
|                                   Method |       Mean |      Error |     StdDev | Size (bytes) |  Gen 0 | Allocated |
|----------------------------------------- |-----------:|-----------:|-----------:|------------- |-------:|----------:|
|                        JsonSerialization | 2,797.8 ns |  24.063 ns |  20.094 ns |          124 | 2.0370 |    6426 B |
|                      JsonDeserialization | 2,282.5 ns |  26.583 ns |  24.866 ns |              | 0.3204 |    1016 B |
|                      KrakenSerialization | 2,306.4 ns |  16.392 ns |  15.333 ns |          143 | 0.4387 |    1392 B |
|                    KrakenDeserialization | 1,375.9 ns |   6.833 ns |   5.706 ns |              | 0.3757 |    1184 B |
|   KrakenSerializationWithOmittedRootType | 1,612.6 ns |   9.370 ns |   8.764 ns |           50 | 0.3605 |    1136 B |
| KrakenDeserializationWithOmittedRootType | 1,130.8 ns |  10.235 ns |   9.073 ns |              | 0.2556 |     808 B |
|             BinaryFormatterSerialization | 8,616.4 ns |  46.589 ns |  41.300 ns |          393 | 2.4567 |    7736 B |
|           BinaryFormatterDeserialization | 9,820.7 ns | 111.603 ns | 104.394 ns |              | 2.7161 |    8568 B |
|               NetSerializerSerialization |   639.5 ns |   3.292 ns |   2.918 ns |           39 | 0.1545 |     488 B |
|             NetSerializerDeserialization |   606.4 ns |   6.021 ns |   5.633 ns |              | 0.0677 |     216 B |

