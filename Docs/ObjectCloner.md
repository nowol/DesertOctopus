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


This benchmark clones a fairly large object containing array, lists and dictionaries.

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
-------------------------- |------------ |---------- |------------ |------------- |------- |------------------- |
 DesertOctopusObjectCloner |  93.6214 us | 1.0082 us |  93.5876 us |              |  50.88 |          43 218.29 |
              GeorgeCloney | 876.5749 us | 6.1545 us | 877.0224 us |              | 543.00 |         486 399.68 |
                    NClone |  76.9679 us | 0.4795 us |  77.0902 us |              |  64.88 |          60 841.30 |
                DeepCloner |  29.9827 us | 0.3109 us |  29.8911 us |              |  25.81 |          23 009.48 |


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
                    Method |           Mean |      StdDev |         Median | Size (bytes) |  Gen 0 | Bytes Allocated/Op |
-------------------------- |--------------- |------------ |--------------- |------------- |------- |------------------- |
 DesertOctopusObjectCloner |    359.6018 ns |   3.2497 ns |    359.2888 ns |              |  14.81 |             768.32 |
              GeorgeCloney | 74,694.3078 ns | 458.5144 ns | 74,599.2204 ns |              | 776.00 |          42 084.49 |
                    NClone |  8,011.2636 ns |  66.9791 ns |  8,012.1323 ns |              |  95.89 |           5 100.12 |
                DeepCloner |    327.3792 ns |   2.6361 ns |    327.1587 ns |              |  13.02 |             679.56 |

