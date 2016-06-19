
## Distributed Caching / MammothCache

MammothCache is a distributed cache that uses Redis as its data store.  It consists of 2 levels of cache:

* First level: in memory store
* Second level: remote data store

Its features are as follow:

* async/await support
* Distributed cache (yes, really!)
  * The default implementation that uses Redis but it is possible to use another data store if required.
  * Objects can have an optional time to live
* L1 cache in memory to speed up access for frequently used items
  * If an object is removed from the remote data store (L2 cache) it will be removed from the L1 cache
  * The L1 cache can be configured to use a small amount of memory

### Usage

#### Initialize the cache

To initialize a `MammothCache` you have to pass 4 variables to its constructor:

* First level cache
  * Local memory cache that provide faster access to object recently cached
  * The current implementation, `SquirrelCache`, can take a cloning provider that controls if objects retrieved from the cache are cloned or not. You do not have to clone your objects if they can safely be shared between threads, otherwise you should use a cloning provider that always clone the objects.
* Second level cache
  * The remote cache server that provide long term storage for cached objects
  * The current implementation uses Redis as the distributed cache and it uses (Polly)[https://github.com/App-vNext/Polly/] to handle its retry policy
* Non serializable cache
  * Local memory cache for object that cannot be serialized
  * While some object cannot be serialize it can be useful to cache them to provide fast access
* Serialization provider
  * Class that serialize and deserialize objects

The `MammothCache` is made to be shared between multiple thread. Typical usage would be to configure it as a singleton in your choosen IoC container.

```csharp
// This configuration object controls the lifetime of objects stored in the first level cache
var config = new FirstLevelCacheConfig();
// Objects will stay at most 20 seconds in the cache.  You will want to increase this value for your use case.
config.AbsoluteExpiration = TimeSpan.FromSeconds(20);
// The maximum memory allowed for the cache (in bytes)
config.MaximumMemorySize = 1000;
// Timer that cleanups the cache and remove object and the memory usage is greater than the maximum allowed
config.TimerInterval = 60;
// This cloning provider always cloned objects from the first level cache
// Other providers available are NoCloningProvider and NamespacesBasedCloningProvider
var cloningProvider = new AlwaysCloningProvider()
var firstLevelCache = new SquirrelCache(config, cloningProvider);
// Create a retry policy that will retry 3  times and it will wait 50ms, then 100ms and finally 150ms for each respective retries
var redisRetryPolicy = new RedisRetryPolicy(50, 100, 150);
// Create an instance of the Redis second level cache
var secondLevelCache = new RedisConnection("<connection string>", redisRetryPolicy);

var mammothCacheSerializationProvider = new MammothCacheSerializationProvider();
var cache = new MammothCache(firstLevelCache, secondLevelCache, nonSerializableCache, mammothCacheSerializationProvider);

```

#### Retrieve object from the cache

```csharp
var retrievedDto = cache.Get<MyDto>("Key");
```

#### Retrieve multiple objects from the cache

If you need to access multiple objects of the same type from the cache you can use the `Get` overloads that accepts a list of keys.  This overload returns a typed `Dictionary` with the objects that were retrieved from the cache.  If an object cannot be found in the cache it will not be present in the dictionary.

```csharp
var keys = new List<CacheItemDefinition>();
keys.Add(new CacheItemDefinition { Key = "Key1" });
keys.Add(new CacheItemDefinition { Key = "Key2" });

Dictionary<CacheItemDefinition, MyDto> values = _cache.Get<MyDto>(keys);

```

#### Store object in the cache

```csharp
var dto = new MyDto();
cache.Set("Key", dto, ttl: TimeSpan.FromSeconds(30));
```

#### Store multiple objects in the cache

Similar to the multiple `Get` overload, the multiple `Set` overload is used to store multiple items in the cache at the same time.

```csharp
var dto1 = new MyDto();
var dto2 = new MyDto();

var dtos = new Dictionary<CacheItemDefinition, MyDto>();
dtos.Add(new CacheItemDefinition { Key = "Key1", TimeToLive = TimeSpan.FromSeconds(30) }, dto1);
dtos.Add(new CacheItemDefinition { Key = "Key2" }, dto2);

cache.Set(dtos);
```

#### Remove object from the cache

```csharp
cache.Remove("Key");
```

#### Remove every objects from the cache

```csharp
cache.RemoveAll();
```

#### Get or add object from the cache

Sometimes we want to get an object from the cache and if it does not exists fetch it from somewhere (e.g.: a database) and store it in the cache.

```csharp

var retrievedDto = cache.GetOrAdd<MyDto>("Key",
                                         () =>
                                         {
                                              return GetDtoFromDatabase();
                                         },
                                         ttl: TimeSpan.FromSeconds(30));

```


#### Distributed locks

If retrieving your object from your datasource takes a long time or uses too much resources you can create a distributed lock to ensure that only one thread touch your datasource.

```csharp
var retrievedDto = cache.GetOrAdd<MyDto>("Key",
                                         () =>
                                         {
                                              using (var lock = cache.AcquireLock("Key", TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30))
                                              {
                                                  var dto = cache.Get("Key");
                                                  if (dto != null)
                                                  {
                                                      return dto;
                                                  }
                                                  return GetDtoFromDatabase();
                                              }
                                         },
                                         ttl: TimeSpan.FromSeconds(30));
```


### Cloning providers

Cloning providers controls if objects retrieved from the first level cache are cloned when calling `Get`.  Out of the box `MammothCache` provides 3 cloning providers:

* NoCloningProvider
  * You can use the `NoCloningProvider` if your objects can be shared between multiple threads
  * Perfect for immutable objects
* AlwaysCloningProvider
  * Objects retrieved from the cache are always cloned
  * Perfect if you do not control the `setter` of your objects
  * Cloning objects can be expensive depending on the size of your objects so make sure to benchmark your use cases
* NamespacesBasedCloningProvider
  * Decides which objects need to be cloned based on their namespace
  * Perfect if you have some objects that can have to be cloned and other that do not require cloning
