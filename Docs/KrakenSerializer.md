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

BenchmarkDotNet=v0.10.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
Host Runtime=Clr 4.0.30319.42000, Arch=32-bit 
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0
Job Runtime(s):
	Clr 4.0.30319.42000, Arch=32-bit RELEASE


```
                                   Method |        Mean |    StdDev |      Median | Size (bytes) |  Gen 0 | Bytes Allocated/Op |
----------------------------------------- |------------ |---------- |------------ |------------- |------- |------------------- |
                        JsonSerialization |  87.4849 us | 0.6315 us |  87.3484 us |         7121 | 139.18 |          76 021.30 |
                      JsonDeserialization | 166.4451 us | 1.2555 us | 166.2010 us |              | 296.57 |         139 213.16 |
                        OmniSerialization | 203.0830 us | 1.3062 us | 202.9670 us |        12253 | 374.27 |         190 980.45 |
                      OmniDeserialization |  81.3371 us | 0.3672 us |  81.3968 us |              | 127.98 |          58 649.10 |
                      KrakenSerialization | 268.6778 us | 1.4707 us | 268.3334 us |         5799 | 213.73 |         109 325.78 |
                    KrakenDeserialization | 143.0456 us | 1.1110 us | 143.3863 us |              | 146.19 |          67 826.39 |
   KrakenSerializationWithOmittedRootType | 268.3583 us | 1.1119 us | 268.4431 us |         5703 | 233.69 |         122 751.61 |
 KrakenDeserializationWithOmittedRootType | 153.3053 us | 1.4748 us | 152.9639 us |              | 126.70 |          58 383.80 |
             BinaryFormatterSerialization | 414.3429 us | 1.9417 us | 413.7634 us |        22223 | 555.33 |         280 225.61 |
           BinaryFormatterDeserialization | 398.1457 us | 3.8349 us | 398.5968 us |              | 529.00 |         246 284.89 |
               NetSerializerSerialization |  37.2763 us | 0.2054 us |  37.2911 us |         3993 |  51.44 |          27 410.85 |
             NetSerializerDeserialization |  31.0473 us | 0.2591 us |  30.9647 us |              |  56.18 |          25 913.81 |


This benchmark serialize and deserialize a normal sized object that contains all primitives types.

``` ini

BenchmarkDotNet=v0.10.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
Host Runtime=Clr 4.0.30319.42000, Arch=32-bit 
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0
Job Runtime(s):
	Clr 4.0.30319.42000, Arch=32-bit RELEASE


```
                                   Method |           Mean |      StdDev |         Median | Size (bytes) |  Gen 0 | Bytes Allocated/Op |
----------------------------------------- |--------------- |------------ |--------------- |------------- |------- |------------------- |
                        JsonSerialization | 14,224.1290 ns | 112.6104 ns | 14,238.6361 ns |          591 | 527.68 |          15 175.53 |
                      JsonDeserialization | 15,342.6841 ns |  97.4626 ns | 15,345.8011 ns |              | 183.50 |           4 766.13 |
                        OmniSerialization |  1,765.6634 ns |  17.2894 ns |  1,764.7096 ns |          217 |  72.76 |           1 923.23 |
                      OmniDeserialization |  1,144.2017 ns |   7.4595 ns |  1,143.8973 ns |              |  34.81 |             928.41 |
                      KrakenSerialization |  2,903.9580 ns |  18.6570 ns |  2,901.0649 ns |          231 |  89.06 |           2 326.28 |
                    KrakenDeserialization |  1,718.9431 ns |  11.6962 ns |  1,720.9399 ns |              |  72.38 |           1 890.57 |
   KrakenSerializationWithOmittedRootType |  2,240.3279 ns |  25.2850 ns |  2,229.4454 ns |          120 |  65.88 |           1 742.12 |
 KrakenDeserializationWithOmittedRootType |  1,399.7184 ns |   9.6175 ns |  1,401.4428 ns |              |  36.56 |             962.00 |
             BinaryFormatterSerialization | 44,581.7170 ns | 313.3155 ns | 44,585.5401 ns |         1735 | 958.00 |          26 028.09 |
           BinaryFormatterDeserialization | 23,830.0118 ns |  95.9942 ns | 23,845.0561 ns |              | 653.57 |          17 023.04 |
               NetSerializerSerialization |    868.0050 ns |   3.6654 ns |    868.3663 ns |          105 |  29.38 |             782.40 |
             NetSerializerDeserialization |    807.1599 ns |   3.3850 ns |    807.0821 ns |              |  17.94 |             487.09 |


This benchmark serialize and deserialize an array of 100000 ints.

``` ini

BenchmarkDotNet=v0.10.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
Host Runtime=Clr 4.0.30319.42000, Arch=32-bit 
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0
Job Runtime(s):
	Clr 4.0.30319.42000, Arch=32-bit RELEASE


```
                                   Method |           Mean |      StdDev |         Median | Size (bytes) |  Gen 0 | Bytes Allocated/Op |
----------------------------------------- |--------------- |------------ |--------------- |------------- |------- |------------------- |
                        JsonSerialization | 14,150.9716 us | 210.5868 us | 14,148.9337 us |       600001 | 450.00 |      10 551 951.90 |
                      JsonDeserialization | 15,051.9468 us |  90.6118 us | 15,058.4353 us |              | 459.00 |      11 298 666.75 |
                        OmniSerialization |  2,050.6169 us |  15.0750 us |  2,046.0224 us |       300025 |  16.21 |       2 260 809.36 |
                      OmniDeserialization |  1,388.5158 us |  16.7653 us |  1,386.1549 us |              |      - |         639 410.74 |
                      KrakenSerialization |  2,168.7722 us |  23.2069 us |  2,166.4740 us |       300033 |  14.00 |       2 108 756.63 |
                    KrakenDeserialization |  1,157.8406 us |  11.4568 us |  1,154.9572 us |              |      - |         613 488.13 |
   KrakenSerializationWithOmittedRootType |  2,156.7874 us |   8.0011 us |  2,156.4601 us |       300014 |  16.25 |       2 385 643.89 |
 KrakenDeserializationWithOmittedRootType |  1,159.7133 us |   7.8155 us |  1,159.1926 us |              |      - |         694 224.13 |
             BinaryFormatterSerialization |    524.7461 us |   5.4615 us |    524.0833 us |       400028 |  15.31 |       2 271 273.32 |
           BinaryFormatterDeserialization |    150.2854 us |   1.4224 us |    150.2009 us |              |   0.71 |         623 848.76 |
               NetSerializerSerialization |  2,034.5062 us |  11.8592 us |  2,033.1746 us |       300004 |  14.38 |       2 058 310.97 |
             NetSerializerDeserialization |  1,395.9767 us |  11.5597 us |  1,396.9176 us |              |      - |         656 293.87 |


This benchmark serialize and deserialize an array of 100000 doubles.

``` ini

BenchmarkDotNet=v0.10.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
Host Runtime=Clr 4.0.30319.42000, Arch=32-bit 
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0
Job Runtime(s):
	Clr 4.0.30319.42000, Arch=32-bit RELEASE


```
                                   Method |           Mean |      StdDev |         Median | Size (bytes) |    Gen 0 | Bytes Allocated/Op |
----------------------------------------- |--------------- |------------ |--------------- |------------- |--------- |------------------- |
                        JsonSerialization | 37,198.4722 us | 206.9992 us | 37,117.4671 us |      1200001 | 1,408.00 |      28 674 190.75 |
                      JsonDeserialization | 22,013.5759 us | 137.4376 us | 21,986.0674 us |              |   704.00 |      18 398 035.02 |
                        OmniSerialization |  5,701.4977 us |  43.4871 us |  5,703.7201 us |       900026 |    14.00 |       5 999 822.24 |
                      OmniDeserialization |  4,483.4893 us |  25.9775 us |  4,491.4018 us |              |        - |       1 267 271.80 |
                      KrakenSerialization |  6,797.1197 us |  54.6481 us |  6,799.4293 us |       900034 |    11.20 |       4 750 963.37 |
                    KrakenDeserialization |  5,123.2368 us |  21.1907 us |  5,121.9981 us |              |        - |       1 321 272.79 |
   KrakenSerializationWithOmittedRootType |  6,840.4158 us |  79.4492 us |  6,836.0145 us |       900014 |    14.00 |       6 181 608.52 |
 KrakenDeserializationWithOmittedRootType |  5,103.0578 us |  26.6138 us |  5,091.9778 us |              |        - |       1 358 146.24 |
             BinaryFormatterSerialization |  1,051.5380 us |  10.8932 us |  1,050.3522 us |       800028 |    14.58 |       4 693 092.07 |
           BinaryFormatterDeserialization |    341.8705 us |   2.1529 us |    341.5268 us |              |     0.44 |       1 612 730.29 |
               NetSerializerSerialization |  5,781.3153 us |  34.0148 us |  5,773.4719 us |       900004 |    11.43 |       4 748 756.72 |
             NetSerializerDeserialization |  4,881.2043 us |  22.3153 us |  4,877.0114 us |              |        - |       1 373 951.80 |


This benchmark serialize and deserialize an array of 100000 decimals.

``` ini

BenchmarkDotNet=v0.10.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
Host Runtime=Clr 4.0.30319.42000, Arch=32-bit 
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0
Job Runtime(s):
	Clr 4.0.30319.42000, Arch=32-bit RELEASE


```
                                   Method |       Mean |    StdDev |     Median | Size (bytes) |  Gen 0 | Bytes Allocated/Op |
----------------------------------------- |----------- |---------- |----------- |------------- |------- |------------------- |
                        JsonSerialization | 33.7821 ms | 0.1381 ms | 33.7482 ms |      1200001 | 616.00 |      16 779 584.48 |
                      JsonDeserialization | 30.1621 ms | 0.1156 ms | 30.1174 ms |              | 672.00 |      22 760 084.52 |
                        OmniSerialization |  6.3607 ms | 0.0475 ms |  6.3610 ms |       700027 | 343.00 |       9 840 542.63 |
                      OmniDeserialization |  5.5413 ms | 0.0251 ms |  5.5465 ms |              |      - |       2 715 166.95 |
                      KrakenSerialization |  5.9703 ms | 0.0629 ms |  5.9489 ms |       700035 | 379.08 |      10 705 533.23 |
                    KrakenDeserialization |  8.0967 ms | 0.0316 ms |  8.0919 ms |              |      - |       2 901 448.29 |
   KrakenSerializationWithOmittedRootType |  5.9163 ms | 0.0410 ms |  5.9136 ms |       700014 | 326.90 |       9 279 162.21 |
 KrakenDeserializationWithOmittedRootType |  8.0719 ms | 0.0431 ms |  8.0675 ms |              |      - |       2 400 925.51 |
             BinaryFormatterSerialization | 42.3149 ms | 0.2828 ms | 42.2588 ms |      1200028 | 690.00 |      18 785 371.73 |
           BinaryFormatterDeserialization | 41.5476 ms | 0.2214 ms | 41.5106 ms |              | 672.00 |      12 186 426.15 |
               NetSerializerSerialization |  6.2180 ms | 0.0303 ms |  6.2108 ms |       700004 | 320.13 |       9 181 601.76 |
             NetSerializerDeserialization |  5.5748 ms | 0.0545 ms |  5.5629 ms |              |      - |       2 639 064.40 |


This benchmark serialize and deserialize an Dictionary of int,int with 100000 items.

``` ini

BenchmarkDotNet=v0.10.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
Host Runtime=Clr 4.0.30319.42000, Arch=32-bit 
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0
Job Runtime(s):
	Clr 4.0.30319.42000, Arch=32-bit RELEASE


```
                                   Method |        Mean |    StdDev |      Median | Size (bytes) |    Gen 0 | Bytes Allocated/Op |
----------------------------------------- |------------ |---------- |------------ |------------- |--------- |------------------- |
                        JsonSerialization |  40.4908 ms | 0.2088 ms |  40.4966 ms |      2400001 | 1,058.20 |      34 024 950.82 |
                      JsonDeserialization |  37.2785 ms | 0.1410 ms |  37.3054 ms |              |   934.27 |      23 026 329.80 |
                        OmniSerialization |   7.0115 ms | 0.0661 ms |   6.9980 ms |      1000201 |     9.97 |       6 138 725.27 |
                      OmniDeserialization |   7.8531 ms | 0.0790 ms |   7.8483 ms |              |    20.37 |      10 764 509.38 |
                      KrakenSerialization |   8.3124 ms | 0.0392 ms |   8.3038 ms |      1000073 |    10.68 |       5 238 009.55 |
                    KrakenDeserialization |   7.5491 ms | 0.0322 ms |   7.5537 ms |              |    21.82 |      10 189 777.45 |
   KrakenSerializationWithOmittedRootType |   8.3078 ms | 0.0653 ms |   8.2928 ms |      1000014 |     9.53 |       4 681 587.84 |
 KrakenDeserializationWithOmittedRootType |   7.5543 ms | 0.0830 ms |   7.5558 ms |              |    19.50 |       9 108 968.45 |
             BinaryFormatterSerialization | 122.8747 ms | 0.6551 ms | 122.8190 ms |      1701335 | 1,472.00 |      34 580 284.50 |
           BinaryFormatterDeserialization | 203.1670 ms | 0.6691 ms | 202.8649 ms |              |   686.40 |      35 271 975.22 |
               NetSerializerSerialization |   7.1108 ms | 0.0419 ms |   7.1081 ms |      1000005 |    10.40 |       6 369 044.47 |
             NetSerializerDeserialization |   5.9858 ms | 0.0276 ms |   5.9835 ms |              |        - |       5 104 473.51 |


This benchmark serialize and deserialize an Dictionary of string,int with 100000 items.

``` ini

BenchmarkDotNet=v0.10.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
Host Runtime=Clr 4.0.30319.42000, Arch=32-bit 
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0
Job Runtime(s):
	Clr 4.0.30319.42000, Arch=32-bit RELEASE


```
                                   Method |        Mean |    StdDev |      Median | Size (bytes) |    Gen 0 | Bytes Allocated/Op |
----------------------------------------- |------------ |---------- |------------ |------------- |--------- |------------------- |
                        JsonSerialization |  22.1459 ms | 0.1207 ms |  22.1459 ms |      1377781 |   364.93 |      14 111 006.87 |
                      JsonDeserialization |  56.0165 ms | 0.4082 ms |  55.9777 ms |              |   308.00 |      22 821 845.84 |
                        OmniSerialization |  12.6593 ms | 0.0715 ms |  12.6559 ms |       980839 |        - |       8 080 867.29 |
                      OmniDeserialization |  22.7164 ms | 0.1539 ms |  22.7759 ms |              |    41.07 |      13 593 291.37 |
                      KrakenSerialization |  47.0298 ms | 0.3205 ms |  46.9645 ms |      1180708 |    25.20 |      20 640 051.02 |
                    KrakenDeserialization |  53.2849 ms | 0.2083 ms |  53.3156 ms |              |   373.33 |      23 930 019.90 |
   KrakenSerializationWithOmittedRootType |  46.7669 ms | 0.1661 ms |  46.7996 ms |      1180648 |    26.13 |      19 879 494.18 |
 KrakenDeserializationWithOmittedRootType |  53.9315 ms | 1.3865 ms |  53.5673 ms |              |   329.47 |      22 916 619.47 |
             BinaryFormatterSerialization | 154.9906 ms | 0.4462 ms | 155.0041 ms |      2390230 | 1,246.00 |      50 821 555.38 |
           BinaryFormatterDeserialization | 417.2442 ms | 3.8565 ms | 417.5326 ms |              |   716.80 |      50 692 815.17 |
               NetSerializerSerialization |  12.8494 ms | 0.0885 ms |  12.8527 ms |       980639 |        - |       6 655 230.11 |
             NetSerializerDeserialization |  21.4151 ms | 0.3057 ms |  21.4681 ms |              |    72.80 |       9 318 442.58 |


This benchmark serialize and deserialize a string of 1000 characters.

``` ini

BenchmarkDotNet=v0.10.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
Host Runtime=Clr 4.0.30319.42000, Arch=32-bit 
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0
Job Runtime(s):
	Clr 4.0.30319.42000, Arch=32-bit RELEASE


```
                                   Method |      Mean |    StdDev |    Median | Size (bytes) |    Gen 0 | Bytes Allocated/Op |
----------------------------------------- |---------- |---------- |---------- |------------- |--------- |------------------- |
                        JsonSerialization | 2.7773 us | 0.0371 us | 2.7737 us |         1012 | 3,282.53 |          12 778.43 |
                      JsonDeserialization | 3.5768 us | 0.0310 us | 3.5813 us |              | 1,800.40 |           6 992.76 |
                        OmniSerialization | 3.0109 us | 0.0394 us | 2.9945 us |         1117 | 2,709.00 |          10 071.98 |
                      OmniDeserialization | 1.6380 us | 0.0104 us | 1.6374 us |              | 1,918.93 |           7 390.00 |
                      KrakenSerialization | 4.5116 us | 0.0287 us | 4.5104 us |         1126 | 2,121.00 |           7 764.51 |
                    KrakenDeserialization | 1.8409 us | 0.0157 us | 1.8401 us |              | 1,672.53 |           6 248.72 |
   KrakenSerializationWithOmittedRootType | 3.8310 us | 0.0536 us | 3.8306 us |         1015 | 1,921.00 |           7 002.20 |
 KrakenDeserializationWithOmittedRootType | 1.5702 us | 0.0113 us | 1.5674 us |              | 1,554.93 |           5 883.16 |
             BinaryFormatterSerialization | 5.1388 us | 0.0204 us | 5.1424 us |         1281 | 3,723.00 |          13 578.54 |
           BinaryFormatterDeserialization | 4.2674 us | 0.0508 us | 4.2731 us |              | 3,501.00 |          12 985.02 |
               NetSerializerSerialization | 1.8849 us | 0.0176 us | 1.8772 us |         1005 | 1,409.33 |           5 108.37 |
             NetSerializerDeserialization | 1.2784 us | 0.0092 us | 1.2794 us |              | 1,903.07 |           7 340.93 |


This benchmark serialize and deserialize a large struct.

``` ini

BenchmarkDotNet=v0.10.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
Host Runtime=Clr 4.0.30319.42000, Arch=32-bit 
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0
Job Runtime(s):
	Clr 4.0.30319.42000, Arch=32-bit RELEASE


```
                                   Method |           Mean |      StdDev |         Median | Size (bytes) |  Gen 0 | Bytes Allocated/Op |
----------------------------------------- |--------------- |------------ |--------------- |------------- |------- |------------------- |
                        JsonSerialization |    924.1343 ns |   9.0909 ns |    923.2239 ns |           47 | 709.13 |          10 466.04 |
                      JsonDeserialization | 16,404.3028 ns | 169.3162 ns | 16,385.8231 ns |              |  67.00 |             925.46 |
                        OmniSerialization |  1,386.8986 ns |  11.1236 ns |  1,386.5156 ns |          225 | 146.38 |           1 921.00 |
                      OmniDeserialization |    846.5479 ns |   4.3599 ns |    847.2176 ns |              |  97.63 |           1 285.07 |
                      KrakenSerialization |  2,301.4561 ns |  14.9835 ns |  2,298.5192 ns |          233 | 146.25 |           1 907.71 |
                    KrakenDeserialization |  1,257.6248 ns |   7.9770 ns |  1,258.1848 ns |              | 116.75 |           1 530.25 |
   KrakenSerializationWithOmittedRootType |  1,167.5650 ns |  10.6829 ns |  1,171.3001 ns |           52 |  88.19 |           1 155.85 |
 KrakenDeserializationWithOmittedRootType |    892.6731 ns |   5.9035 ns |    892.6184 ns |              |  48.25 |             629.78 |
             BinaryFormatterSerialization |  5,301.2868 ns |  27.0752 ns |  5,308.9893 ns |          456 | 551.50 |           7 160.09 |
           BinaryFormatterDeserialization |  5,966.7781 ns |  24.7727 ns |  5,958.2905 ns |              | 551.00 |           7 138.66 |
               NetSerializerSerialization |    361.0000 ns |   2.3316 ns |    360.8033 ns |           41 |  57.29 |             752.50 |
             NetSerializerDeserialization |    381.2215 ns |   2.5798 ns |    381.4734 ns |              |   9.97 |             135.51 |


This benchmark serialize and deserialize a small class used by the Wire project.

``` ini

BenchmarkDotNet=v0.10.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 Hz, Resolution=69.8413 ns, Timer=HPET
Host Runtime=Clr 4.0.30319.42000, Arch=32-bit 
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1055.0
Job Runtime(s):
	Clr 4.0.30319.42000, Arch=32-bit RELEASE


```
                                   Method |      Mean |    StdDev |    Median | Size (bytes) |    Gen 0 | Bytes Allocated/Op |
----------------------------------------- |---------- |---------- |---------- |------------- |--------- |------------------- |
                        JsonSerialization | 4.0033 us | 0.0309 us | 3.9993 us |          124 | 1,572.00 |          11 542.81 |
                      JsonDeserialization | 3.0396 us | 0.0197 us | 3.0369 us |              |   236.75 |           1 559.78 |
                        OmniSerialization | 1.9210 us | 0.0125 us | 1.9234 us |          131 |   246.25 |           1 628.93 |
                      OmniDeserialization | 1.4230 us | 0.0079 us | 1.4251 us |              |    80.50 |             542.53 |
                      KrakenSerialization | 2.7739 us | 0.0164 us | 2.7714 us |          142 |   249.50 |           1 654.53 |
                    KrakenDeserialization | 1.8705 us | 0.0138 us | 1.8627 us |              |   191.75 |           1 255.64 |
   KrakenSerializationWithOmittedRootType | 2.1409 us | 0.0141 us | 2.1453 us |           49 |   184.75 |           1 234.74 |
 KrakenDeserializationWithOmittedRootType | 1.6122 us | 0.0099 us | 1.6128 us |              |   108.50 |             707.57 |
             BinaryFormatterSerialization | 9.2489 us | 0.0455 us | 9.2443 us |          393 | 1,245.00 |           8 147.47 |
           BinaryFormatterDeserialization | 9.9048 us | 0.0872 us | 9.8804 us |              | 1,268.00 |           8 228.91 |
               NetSerializerSerialization | 1.0893 us | 0.0064 us | 1.0902 us |           37 |    97.50 |             639.91 |
             NetSerializerDeserialization | 1.0866 us | 0.0075 us | 1.0861 us |              |    35.38 |             250.61 |

