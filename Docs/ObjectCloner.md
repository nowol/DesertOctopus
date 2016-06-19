## Object Cloning

The main use of this object cloner is to clone DTOs.  The cloning engine is implemented using expression trees which mean a small performance hit the first type you serialize a type for the creation of the expression tree.  Subsequent serialization for the same type can use the compiled expression tree without having to suffer the performance hit.

The main pros are:

* Fairly fast
* Does not require objects to be decorated with any attribute
  * It is up to the user of the object cloner to ensure that the objects can be safely cloned
* Clone all fields (private, public, etc) of an object
* Thread safe
* Supports interface `ISerializable` to allow dictionaries to be correctly cloned
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

```ini

BenchmarkDotNet=v0.9.7.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
HostCLR=MS.NET 4.0.30319.42000, Arch=32-bit RELEASE
JitModules=clrjit-v4.6.1055.0

Type=ProductCloningBenchMark  Mode=Throughput  

```
 Method |     Median |    StdDev |
------- |----------- |---------- |
  Clone | 91.6395 us | 0.5154 us |


This benchmark clone a normal sized object that contains all primitives types (30 properties).

```ini

BenchmarkDotNet=v0.9.6.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU @ 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
HostCLR=MS.NET 4.0.30319.42000, Arch=64-bit RELEASE [RyuJIT]
JitModules=clrjit-v4.6.1055.0

Type=SimpleDtoWithEveryPrimitivesCloningBenchmark  Mode=Throughput  

```
 Method |      Median |    StdDev |
------- |------------ |---------- |
  Clone | 316.8997 ns | 4.2676 ns |