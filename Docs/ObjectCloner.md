## Object Cloning

The main use of this object cloner is to clone DTOs.  The cloning engine is implemented using expression trees which mean a small performance hit the first type you serialize a type for the creation of the expression tree.  Subsequent serialization for the same type can use the compiled expression tree without having to suffer the performance hit.

The main pros are:

* Fairly fast
* Does not require objects to be decorated with any attribute
  * It is up to the user of the object cloner to ensure that the objects can be safely cloned
* Clone all fields (private, public, etc) of an object
* Thread safe
* Supports interface `ISerializable`
  * Your class needs to have the corresponding serialization constructor otherwise it will be cloned as a normal class
* Respect the `NotSerialized` attribute
* Supported types:
  * All primitive types
  * Multi dimensional arrays
  * Jagged arrays
  * Class / Struct
  * ExpandoObject
  * Basic support for `IEnumerable<>` and `IQueryable<>`
      * Don't go crazy and try to serialize a `GroupedEnumerable` or something similar
* Can handle circular references
  * There is one case that is not supported: a dictionary with a reference to itself
* Good unit tests

The mains cons are:

* Does not have a cool name.

### Usage

Cloning an object is fairly straightforward:

```csharp

var objectToClone = new MyDto();
var clonedObject = ObjectCloner.Clone(objectToClone);

```

### Benchmark


This benchmark clones a fairly large object containing array, lists and dictionaries.  Dictionaries are serialized using the ISerializable interface.

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
        Method |          Mean |     StdDev |        Median |  Gen 0 | Bytes Allocated/Op |
-------------- |-------------- |----------- |-------------- |------- |------------------- |
 DesertOctopus |   326.2451 us |  1.5497 us |   326.2468 us | 139.02 |         239 883.71 |
  GeorgeCloney | 1,920.6397 us | 22.8559 us | 1,917.4020 us | 361.00 |       1 242 881.87 |
        NClone |   235.1646 us |  1.8069 us |   235.5212 us | 102.88 |         191 471.96 |
    DeepCloner |    76.8716 us |  0.4209 us |    76.8490 us |  50.94 |          93 623.84 |


This benchmark clones a fairly large object containing array, lists and dictionaries.  Dictionaries are serialized as a normal class.

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
        Method |        Mean |    StdDev |      Median | Gen 0 | Bytes Allocated/Op |
-------------- |------------ |---------- |------------ |------ |------------------- |
 DesertOctopus | 327.9923 us | 2.3280 us | 327.1245 us |   NaN |            +Infini |
  GeorgeCloney |          NA |        NA |          NA |   NaN |            +Infini |
        NClone | 238.2210 us | 2.0623 us | 238.3448 us |  0.00 |         173 415.23 |
    DeepCloner |  76.9366 us | 0.5420 us |  76.8971 us |  0.00 |          97 522.28 |

Benchmarks with issues:
  BenchmarkObjectNonISerializablerDictionaryCloningBenchMark.GeorgeCloney: DefaultJob


This benchmark clone a normal sized object that contains all primitives types (30 properties).

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
        Method |           Mean |      StdDev |         Median |  Gen 0 | Bytes Allocated/Op |
-------------- |--------------- |------------ |--------------- |------- |------------------- |
 DesertOctopus |    368.5086 ns |   5.8461 ns |    368.8217 ns |  13.13 |             790.39 |
  GeorgeCloney | 75,324.8672 ns | 706.3032 ns | 75,179.5282 ns | 836.00 |          52 300.53 |
        NClone |  7,982.6989 ns |  95.0663 ns |  7,946.9845 ns |  87.32 |           5 355.37 |
    DeepCloner |    336.3211 ns |   4.0476 ns |    336.1630 ns |  11.28 |             679.56 |

