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

BenchmarkDotNet=v0.9.7.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
HostCLR=MS.NET 4.0.30319.42000, Arch=32-bit RELEASE
JitModules=clrjit-v4.6.1055.0

Type=ProductSerializationBenchMark  Mode=Throughput  

```
                         Method |      Median |    StdDev |
------------------------------- |------------ |---------- |
              JsonSerialization |  83.3844 us | 1.6567 us |
            JsonDeserialization | 166.0238 us | 1.8122 us |
              OmniSerialization | 201.3290 us | 1.3124 us |
            OmniDeserialization |  85.1688 us | 2.3745 us |
            KrakenSerialization | 236.1410 us | 1.2424 us |
          KrakenDeserialization | 143.4187 us | 1.2910 us |
   BinaryFormatterSerialization | 420.0934 us | 3.5034 us |
 BinaryFormatterDeserialization | 390.0488 us | 5.1521 us |


This benchmark serialize and deserialize a normal sized object that contains all primitives types.

```ini

BenchmarkDotNet=v0.9.7.0
OS=Microsoft Windows NT 6.3.9600.0
Processor=Intel(R) Core(TM) i5-4690 CPU 3.50GHz, ProcessorCount=4
Frequency=14318180 ticks, Resolution=69.8413 ns, Timer=HPET
HostCLR=MS.NET 4.0.30319.42000, Arch=32-bit RELEASE
JitModules=clrjit-v4.6.1055.0

Type=SimpleDtoWithEveryPrimitivesSerializationBenchmark  Mode=Throughput  

```
                         Method |     Median |    StdDev |
------------------------------- |----------- |---------- |
              JsonSerialization | 14.2819 us | 0.1385 us |
            JsonDeserialization | 15.3280 us | 0.1109 us |
              OmniSerialization |  1.7782 us | 0.0183 us |
            OmniDeserialization |  1.1509 us | 0.0132 us |
            KrakenSerialization |  3.4494 us | 0.0808 us |
          KrakenDeserialization |  1.7009 us | 0.0232 us |
   BinaryFormatterSerialization | 44.0757 us | 0.6720 us |
 BinaryFormatterDeserialization | 23.4476 us | 0.1815 us |
