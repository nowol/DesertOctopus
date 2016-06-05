# DesertOctopus

DesertOctopus is a .Net utility library that currently does the following:

* Serialization (KrakenSerializer)
* Object Cloning
* Distributed caching (MammothCache)


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

* Including the objects' types increase the size of the payload
* It's not the fastest serializer around.  I should point out that no development has been made to improve the performance so this point could change in the future.


### Benchmark

This benchmark serialize and deserialize a fairly large object containing array, lists and dictionaries.  A big performance hit is the handling of `ISerializable`

```ini

BenchmarkDotNet=v0.9.6.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU @ 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
HostCLR=MS.NET 4.0.30319.42000, Arch=64-bit RELEASE [RyuJIT]
JitModules=clrjit-v4.6.1055.0

Type=ProductSerializationBenchMark  Mode=Throughput  

```
                         Method |      Median |    StdDev |
------------------------------- |------------ |---------- |
              JsonSerialization |  91.9330 us | 4.9447 us |
            JsonDeserialization | 176.9681 us | 5.0118 us |
              OmniSerialization | 195.9051 us | 2.0565 us |
            OmniDeserialization |  83.3034 us | 0.5823 us |
            KrakenSerialization | 377.8699 us | 6.8978 us |
          KrakenDeserialization | 199.7201 us | 3.3580 us |
   BinaryFormatterSerialization | 417.4507 us | 6.7281 us |
 BinaryFormatterDeserialization | 399.5219 us | 3.7520 us |


This benchmark serialize and deserialize a normal sized object that contains all primitives types.

```ini

BenchmarkDotNet=v0.9.6.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU @ 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
HostCLR=MS.NET 4.0.30319.42000, Arch=32-bit RELEASE
JitModules=clrjit-v4.6.1055.0

Type=SimpleDtoWithEveryPrimitivesBenchmark  Mode=Throughput  

```
                         Method |     Median |    StdDev |
------------------------------- |----------- |---------- |
              JsonSerialization | 14.1693 us | 0.1343 us |
            JsonDeserialization | 15.4511 us | 0.2316 us |
              OmniSerialization |  1.7784 us | 0.0244 us |
            OmniDeserialization |  1.1477 us | 0.0077 us |
            KrakenSerialization |  6.0231 us | 0.0789 us |
          KrakenDeserialization |  3.6023 us | 0.0296 us |
   BinaryFormatterSerialization | 43.3992 us | 0.2911 us |
 BinaryFormatterDeserialization | 23.6350 us | 0.1203 us |



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

### Benchmark

This benchmark clones a fairly large object containing array, lists and dictionaries.

```ini

BenchmarkDotNet=v0.9.6.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU @ 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
HostCLR=MS.NET 4.0.30319.42000, Arch=32-bit RELEASE
JitModules=clrjit-v4.6.1055.0

Type=CloningBenchMark  Mode=Throughput  

```
 Method |     Median |    StdDev |
------- |----------- |---------- |
  Clone | 63.6519 us | 0.4349 us |


This benchmark clone a normal sized object that contains all primitives types.

```ini

BenchmarkDotNet=v0.9.6.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU @ 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
HostCLR=MS.NET 4.0.30319.42000, Arch=32-bit RELEASE
JitModules=clrjit-v4.6.1055.0

Type=SimpleDtoWithEveryPrimitivesCloningBenchmark  Mode=Throughput  

```
 Method |      Median |    StdDev |
------- |------------ |---------- |
  Clone | 338.9273 ns | 4.0972 ns |


## Distributed Caching / MammothCache

MammothCache is a distributed cache that uses Redis as its data store.  It consists of 2 levels of cache:

* First level: in memory store
* Second level: remote data store

Its features are as follow:

* Distributed cache (yes, really!)
  * The default implementation that uses Redis but it is possible to use another data store if required.
  * Objects can have an optional time to live
* L1 cache in memory to speed up access for frequently used items
  * If an object is removed from the remote data store (L2 cache) it will be removed from the L1 cache
  * The L1 cache can be configured to use a small amount of memory