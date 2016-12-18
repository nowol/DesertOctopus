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
                                   Method |          Mean |    StdDev |        Median | Size (bytes) |  Gen 0 | Bytes Allocated/Op |
----------------------------------------- |-------------- |---------- |-------------- |------------- |------- |------------------- |
                        JsonSerialization |   760.1757 us | 3.5616 us |   759.5675 us |        37375 | 602.93 |         580 121.34 |
                      JsonDeserialization |   792.3838 us | 7.7759 us |   792.6065 us |              | 588.93 |       1 472 458.71 |
                        OmniSerialization |   517.2926 us | 2.9151 us |   517.5234 us |        46034 | 778.62 |         749 931.57 |
                      OmniDeserialization |   239.4034 us | 1.0790 us |   239.4963 us |              | 257.13 |         231 844.18 |
                      KrakenSerialization |   346.1289 us | 1.8473 us |   346.2651 us |        14719 | 156.33 |         166 037.28 |
                    KrakenDeserialization |   174.3601 us | 1.4889 us |   174.0837 us |              | 111.87 |         104 090.56 |
   KrakenSerializationWithOmittedRootType |   344.2359 us | 1.3462 us |   344.2318 us |        14603 | 128.33 |         136 329.78 |
 KrakenDeserializationWithOmittedRootType |   168.8510 us | 1.0705 us |   168.7558 us |              |  85.75 |          79 373.33 |
             BinaryFormatterSerialization | 1,023.5922 us | 6.8080 us | 1,024.0853 us |        72220 | 543.00 |       1 063 136.27 |
           BinaryFormatterDeserialization |   894.5377 us | 5.9825 us |   893.7035 us |              | 464.00 |         519 942.58 |
               NetSerializerSerialization |   162.0279 us | 1.6074 us |   161.6630 us |        24599 | 192.15 |         205 545.23 |
             NetSerializerDeserialization |   142.1073 us | 0.9268 us |   142.1894 us |              | 153.42 |         140 184.07 |


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
                        JsonSerialization | 14,263.0348 ns |  91.7261 ns | 14,239.6151 ns |          591 | 479.73 |          14 779.07 |
                      JsonDeserialization | 15,344.8374 ns | 152.8088 ns | 15,289.3156 ns |              | 171.27 |           4 767.00 |
                        OmniSerialization |  1,769.5525 ns |  14.9268 ns |  1,772.7259 ns |          217 |  63.06 |           1 785.85 |
                      OmniDeserialization |  1,242.9994 ns |  13.7206 ns |  1,244.9006 ns |              |  33.75 |             964.08 |
                      KrakenSerialization |  2,899.3990 ns |  17.2698 ns |  2,905.7836 ns |          231 |  77.58 |           2 171.19 |
                    KrakenDeserialization |  1,905.2842 ns |  19.5156 ns |  1,908.7072 ns |              |  55.88 |           1 564.61 |
   KrakenSerializationWithOmittedRootType |  2,255.3861 ns |  17.1105 ns |  2,251.7457 ns |          120 |  64.17 |           1 817.91 |
 KrakenDeserializationWithOmittedRootType |  1,416.5637 ns |   6.9916 ns |  1,418.1436 ns |              |  31.38 |             885.01 |
             BinaryFormatterSerialization | 44,556.7607 ns | 202.3351 ns | 44,582.7307 ns |         1735 | 958.00 |          27 887.24 |
           BinaryFormatterDeserialization | 23,782.5613 ns | 154.0516 ns | 23,785.7541 ns |              | 593.13 |          16 550.39 |
               NetSerializerSerialization |    857.3839 ns |  11.4285 ns |    855.6650 ns |          105 |  27.42 |             782.43 |
             NetSerializerDeserialization |    809.6690 ns |   5.3618 ns |    809.1832 ns |              |  17.44 |             507.40 |


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
                        JsonSerialization | 14,546.0727 us | 259.8550 us | 14,482.0873 us |       600001 | 468.00 |      10 974 672.05 |
                      JsonDeserialization | 15,007.4157 us |  95.6095 us | 14,982.2827 us |              | 391.00 |       9 617 601.18 |
                        OmniSerialization |  2,086.4726 us |  26.6066 us |  2,083.1347 us |       300025 |  15.63 |       2 201 727.15 |
                      OmniDeserialization |  1,404.0705 us |  12.0425 us |  1,407.1756 us |              |      - |         735 829.71 |
                      KrakenSerialization |  2,184.7826 us |  24.7004 us |  2,181.4887 us |       300033 |  15.67 |       2 357 406.66 |
                    KrakenDeserialization |  1,155.3645 us |  10.8886 us |  1,157.3100 us |              |      - |         640 162.88 |
   KrakenSerializationWithOmittedRootType |  2,177.4166 us |  18.4657 us |  2,183.2019 us |       300014 |  16.21 |       2 259 459.39 |
 KrakenDeserializationWithOmittedRootType |  1,152.9265 us |   7.2283 us |  1,150.6353 us |              |      - |         640 798.27 |
             BinaryFormatterSerialization |    528.5742 us |  14.0054 us |    525.4307 us |       400028 |  17.91 |       2 643 084.92 |
           BinaryFormatterDeserialization |    150.1267 us |   2.6823 us |    150.2069 us |              |   0.86 |         755 422.18 |
               NetSerializerSerialization |  2,044.2846 us |  16.4941 us |  2,045.5274 us |       300004 |  20.34 |       2 789 402.67 |
             NetSerializerDeserialization |  1,429.2286 us |   8.9143 us |  1,429.5651 us |              |      - |         639 131.33 |


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
                        JsonSerialization | 37,121.3130 us | 187.3717 us | 37,109.9071 us |      1200001 | 1,472.00 |      32 280 499.37 |
                      JsonDeserialization | 21,756.3557 us | 162.3044 us | 21,793.9952 us |              |   654.64 |      18 395 445.63 |
                        OmniSerialization |  5,743.7307 us |  37.6558 us |  5,742.8789 us |       900026 |    11.61 |       5 359 496.65 |
                      OmniDeserialization |  4,481.6099 us |  20.7327 us |  4,472.7562 us |              |        - |       1 212 985.73 |
                      KrakenSerialization |  6,812.4164 us |  32.4021 us |  6,806.4794 us |       900034 |     9.53 |       4 546 200.59 |
                    KrakenDeserialization |  5,134.5538 us |  33.1266 us |  5,133.4289 us |              |        - |       1 513 945.15 |
   KrakenSerializationWithOmittedRootType |  6,798.8638 us |  51.7854 us |  6,785.6040 us |       900014 |     9.53 |       4 546 602.10 |
 KrakenDeserializationWithOmittedRootType |  5,085.4846 us |  26.9452 us |  5,090.1564 us |              |        - |       1 414 393.58 |
             BinaryFormatterSerialization |  1,044.3870 us |   8.3794 us |  1,043.8519 us |       800028 |    13.99 |       4 815 291.87 |
           BinaryFormatterDeserialization |    342.5965 us |   4.1685 us |    343.9567 us |              |     0.33 |       1 323 888.01 |
               NetSerializerSerialization |  5,856.7406 us |  37.2992 us |  5,860.3994 us |       900004 |    10.18 |       4 548 306.74 |
             NetSerializerDeserialization |  4,915.9526 us |  43.1068 us |  4,915.7363 us |              |        - |       1 479 886.18 |


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
                        JsonSerialization | 33.7082 ms | 0.1722 ms | 33.6564 ms |      1200001 | 616.00 |      16 778 907.85 |
                      JsonDeserialization | 30.2868 ms | 0.1468 ms | 30.2750 ms |              | 728.00 |      24 670 893.93 |
                        OmniSerialization |  6.3449 ms | 0.0596 ms |  6.3648 ms |       700027 | 346.27 |       9 936 850.77 |
                      OmniDeserialization |  5.5823 ms | 0.0315 ms |  5.5812 ms |              |      - |       2 427 491.40 |
                      KrakenSerialization |  6.0184 ms | 0.0454 ms |  5.9986 ms |       700035 | 324.25 |       9 137 445.86 |
                    KrakenDeserialization |  8.1006 ms | 0.0395 ms |  8.0911 ms |              |      - |       2 824 677.49 |
   KrakenSerializationWithOmittedRootType |  5.9633 ms | 0.0368 ms |  5.9695 ms |       700014 | 337.75 |       9 539 516.56 |
 KrakenDeserializationWithOmittedRootType |  8.0860 ms | 0.0289 ms |  8.0865 ms |              |      - |       2 565 276.26 |
             BinaryFormatterSerialization | 41.9004 ms | 0.1341 ms | 41.8815 ms |      1200028 | 690.00 |      18 784 572.29 |
           BinaryFormatterDeserialization | 41.7689 ms | 0.1472 ms | 41.7291 ms |              | 720.00 |      13 057 838.91 |
               NetSerializerSerialization |  6.2429 ms | 0.0562 ms |  6.2500 ms |       700004 | 294.00 |       8 430 571.96 |
             NetSerializerDeserialization |  5.5779 ms | 0.0270 ms |  5.5833 ms |              |      - |       2 639 174.64 |


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
                        JsonSerialization |  40.6379 ms | 0.2305 ms |  40.6717 ms |      2400001 | 1,186.27 |      35 431 405.73 |
                      JsonDeserialization |  37.0234 ms | 0.1130 ms |  36.9894 ms |              | 1,143.33 |      26 167 353.10 |
                        OmniSerialization |   7.0234 ms | 0.0329 ms |   7.0179 ms |      1000201 |    12.00 |       6 856 397.56 |
                      OmniDeserialization |   7.8652 ms | 0.0613 ms |   7.8501 ms |              |    24.73 |      12 138 800.60 |
                      KrakenSerialization |   8.3381 ms | 0.0607 ms |   8.3602 ms |      1000073 |    11.85 |       5 400 832.49 |
                    KrakenDeserialization |   7.5636 ms | 0.0531 ms |   7.5574 ms |              |    25.31 |      10 971 984.74 |
   KrakenSerializationWithOmittedRootType |   8.3510 ms | 0.0386 ms |   8.3563 ms |      1000014 |    12.13 |       5 517 943.41 |
 KrakenDeserializationWithOmittedRootType |   7.5239 ms | 0.0428 ms |   7.5258 ms |              |    22.87 |       9 917 950.65 |
             BinaryFormatterSerialization | 123.6566 ms | 0.3428 ms | 123.6286 ms |      1701335 | 1,433.60 |      31 271 924.05 |
           BinaryFormatterDeserialization | 205.9206 ms | 1.7760 ms | 205.6679 ms |              |   792.00 |      37 791 802.21 |
               NetSerializerSerialization |   7.0719 ms | 0.0759 ms |   7.0916 ms |      1000005 |    10.73 |       6 109 032.58 |
             NetSerializerDeserialization |   5.9731 ms | 0.0277 ms |   5.9746 ms |              |        - |       4 707 172.23 |


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
                        JsonSerialization |  22.3986 ms | 0.2122 ms |  22.3878 ms |      1377781 |   353.60 |      14 725 388.47 |
                      JsonDeserialization |  56.8216 ms | 0.8788 ms |  56.4667 ms |              |   285.07 |      22 781 320.80 |
                        OmniSerialization |  12.6819 ms | 0.0979 ms |  12.6694 ms |       980839 |        - |       5 721 737.07 |
                      OmniDeserialization |  23.7828 ms | 0.3208 ms |  23.6672 ms |              |    45.07 |      16 066 880.80 |
                      KrakenSerialization |  47.9139 ms | 0.6468 ms |  47.6692 ms |      1180708 |    24.27 |      21 403 794.23 |
                    KrakenDeserialization |  53.8311 ms | 0.8487 ms |  54.0011 ms |              |   365.86 |      24 519 918.82 |
   KrakenSerializationWithOmittedRootType |  47.4442 ms | 0.2061 ms |  47.4948 ms |      1180648 |    20.80 |      16 820 679.27 |
 KrakenDeserializationWithOmittedRootType |  54.9771 ms | 1.1076 ms |  54.8182 ms |              |   312.87 |      22 911 893.82 |
             BinaryFormatterSerialization | 159.6224 ms | 1.1388 ms | 159.2813 ms |      2390230 | 1,183.93 |      52 084 355.98 |
           BinaryFormatterDeserialization | 428.0154 ms | 0.8690 ms | 428.0230 ms |              |   637.87 |      48 580 982.88 |
               NetSerializerSerialization |  12.7689 ms | 0.0796 ms |  12.7743 ms |       980639 |        - |       8 062 933.15 |
             NetSerializerDeserialization |  21.6624 ms | 0.3084 ms |  21.5864 ms |              |    57.20 |       7 884 882.58 |


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
                        JsonSerialization | 2.7619 us | 0.0177 us | 2.7638 us |         1012 | 3,282.53 |          12 778.37 |
                      JsonDeserialization | 3.5764 us | 0.0271 us | 3.5812 us |              | 1,929.00 |           7 491.98 |
                        OmniSerialization | 3.0569 us | 0.0423 us | 3.0649 us |         1117 | 2,638.53 |           9 809.00 |
                      OmniDeserialization | 1.6452 us | 0.0148 us | 1.6410 us |              | 2,086.00 |           8 032.39 |
                      KrakenSerialization | 4.4994 us | 0.0576 us | 4.4922 us |         1126 | 1,979.60 |           7 246.74 |
                    KrakenDeserialization | 1.8329 us | 0.0191 us | 1.8306 us |              | 1,818.13 |           6 791.85 |
   KrakenSerializationWithOmittedRootType | 3.7405 us | 0.0380 us | 3.7418 us |         1015 | 1,721.07 |           6 273.86 |
 KrakenDeserializationWithOmittedRootType | 1.5690 us | 0.0153 us | 1.5684 us |              | 1,554.93 |           5 883.04 |
             BinaryFormatterSerialization | 5.1749 us | 0.0484 us | 5.1722 us |         1281 | 3,626.00 |          13 224.32 |
           BinaryFormatterDeserialization | 4.2551 us | 0.0463 us | 4.2368 us |              | 3,355.00 |          12 443.85 |
               NetSerializerSerialization | 1.8975 us | 0.0139 us | 1.8969 us |         1005 | 1,409.33 |           5 108.37 |
             NetSerializerDeserialization | 1.2798 us | 0.0130 us | 1.2733 us |              | 2,653.54 |          10 234.92 |


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
                        JsonSerialization |    943.5005 ns |  17.1157 ns |    937.7636 ns |           47 | 634.20 |          10 026.91 |
                      JsonDeserialization | 16,065.3962 ns | 188.1142 ns | 16,027.4202 ns |              |  73.00 |           1 077.51 |
                        OmniSerialization |  1,400.8205 ns |  14.1429 ns |  1,402.6228 ns |          225 | 166.37 |           2 338.60 |
                      OmniDeserialization |    848.0193 ns |   7.9000 ns |    848.1407 ns |              | 106.98 |           1 508.54 |
                      KrakenSerialization |  2,042.0678 ns |  19.7326 ns |  2,041.1815 ns |          233 | 125.53 |           1 755.13 |
                    KrakenDeserialization |  1,270.0546 ns |   9.1809 ns |  1,271.5086 ns |              | 116.75 |           1 639.56 |
   KrakenSerializationWithOmittedRootType |  1,175.3180 ns |  15.3674 ns |  1,173.0915 ns |           52 |  84.50 |           1 186.80 |
 KrakenDeserializationWithOmittedRootType |    891.5114 ns |   5.7075 ns |    890.8942 ns |              |  37.39 |             523.07 |
             BinaryFormatterSerialization |  5,333.0477 ns |  34.1883 ns |  5,327.6433 ns |          456 | 514.73 |           7 159.99 |
           BinaryFormatterDeserialization |  6,055.2517 ns | 141.4980 ns |  6,000.5330 ns |              | 596.00 |           8 279.26 |
               NetSerializerSerialization |    363.4106 ns |   2.1925 ns |    363.5344 ns |           41 |  51.17 |             720.34 |
             NetSerializerDeserialization |    352.0487 ns |   2.5669 ns |    351.3378 ns |              |  10.09 |             141.40 |


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
                        JsonSerialization | 4.0041 us | 0.0354 us | 4.0112 us |          124 | 1,526.00 |          12 004.40 |
                      JsonDeserialization | 2.9349 us | 0.0156 us | 2.9365 us |              |   227.25 |           1 604.35 |
                        OmniSerialization | 1.9711 us | 0.0237 us | 1.9598 us |          133 |   220.97 |           1 557.05 |
                      OmniDeserialization | 1.4093 us | 0.0075 us | 1.4098 us |              |    86.69 |             625.96 |
                      KrakenSerialization | 2.8285 us | 0.0289 us | 2.8168 us |          143 |   213.73 |           1 516.12 |
                    KrakenDeserialization | 1.8689 us | 0.0157 us | 1.8700 us |              |   186.43 |           1 307.95 |
   KrakenSerializationWithOmittedRootType | 2.1471 us | 0.0124 us | 2.1460 us |           50 |   165.20 |           1 183.31 |
 KrakenDeserializationWithOmittedRootType | 1.6065 us | 0.0112 us | 1.6060 us |              |   101.27 |             707.57 |
             BinaryFormatterSerialization | 9.2511 us | 0.0942 us | 9.2331 us |          393 | 1,113.47 |           7 807.67 |
           BinaryFormatterDeserialization | 9.9508 us | 0.1838 us | 9.8910 us |              | 1,427.00 |           9 918.58 |
               NetSerializerSerialization | 1.1165 us | 0.0103 us | 1.1170 us |           39 |   126.70 |             890.28 |
             NetSerializerDeserialization | 1.0795 us | 0.0075 us | 1.0775 us |              |    32.63 |             247.85 |

