using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;
using Polly;
using Polly.Retry;
using StackExchange.Redis;

namespace DesertOctupos.MammothCache.Redis
{
    public sealed class RedisConnection : IRedisConnection, ISecondLevelCache, IDisposable
    {
        private readonly string _connectionString;
        private readonly IRedisRetryPolicy _redisRetryPolicy;
        private bool _isDisposed = false;
        private readonly ConnectionMultiplexer _multiplexer;
        private readonly RetryPolicy _retryPolicy;
        private readonly RetryPolicy _retryPolicyAsync;
        private ISubscriber _subscriber;
        private PolicyBuilder _baseRetryPolicy;

        public event ItemEvictedFromCacheEventHandler OnItemRemovedFromCache;

        public RedisConnection(string connectionString, IRedisRetryPolicy redisRetryPolicy)
        {
            _connectionString = connectionString;
            _redisRetryPolicy = redisRetryPolicy;
            _baseRetryPolicy = Policy.Handle<TimeoutException>()
                                     .Or<TimeoutException>()
                                     .Or<SocketException>()
                                     .Or<IOException>();  // for async
            _retryPolicy = _baseRetryPolicy.WaitAndRetry(_redisRetryPolicy.SleepDurations);
            _retryPolicyAsync = _baseRetryPolicy.WaitAndRetryAsync(_redisRetryPolicy.SleepDurations);

            var options = ConfigurationOptions.Parse(connectionString);
            ConfigureIfMissing(options, "abortConnect", connectionString, o => { o.AbortOnConnectFail = false; });
            ConfigureIfMissing(options, "allowAdmin", connectionString, o => { o.AllowAdmin = true; });

            _multiplexer = ConnectionMultiplexer.Connect(options);
            _multiplexer.PreserveAsyncOrder = false;

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _subscriber = _multiplexer.GetSubscriber();
            GetRetryPolicy().Execute(() => _subscriber.Subscribe("__key*__:*del", OnKeyRemoveFromRedis));
            GetRetryPolicy().Execute(() => _subscriber.Subscribe("__key*__:*expired", OnKeyRemoveFromRedis));
            GetRetryPolicy().Execute(() => _subscriber.Subscribe("__key*__:*evicted", OnKeyRemoveFromRedis));
        }

        private void OnKeyRemoveFromRedis(RedisChannel redisChannel,
                                          RedisValue redisValue)
        {
            var eventCopy = OnItemRemovedFromCache;
            if (eventCopy != null
                && redisValue.HasValue)
            {
                eventCopy(redisValue);
            }
        }

        private void ConfigureIfMissing(ConfigurationOptions options,
                                        string configOption,
                                        string connectionString,
                                        Action<ConfigurationOptions> action)
        {
            if (connectionString.IndexOf(configOption, StringComparison.InvariantCultureIgnoreCase) == -1)
            {
                action(options);
            }
        }

        public void Dispose()
        {
            GuardDisposed();
            GetRetryPolicy().Execute(() => _subscriber.UnsubscribeAll());
            _multiplexer.Dispose();
            _isDisposed = true;
        }

        private void GuardDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException("This RedisConnection is disposed");
            }
        }

        internal PolicyBuilder GetBaseRetryPolicyBuilder()
        {
            GuardDisposed();
            return _baseRetryPolicy;
        }

        internal RetryPolicy GetRetryPolicy()
        {
            GuardDisposed();
            return _retryPolicy;
        }

        internal RetryPolicy GetRetryPolicyAsync()
        {
            GuardDisposed();
            return _retryPolicyAsync;
        }

        internal IDatabase GetDatabase()
        {
            return _multiplexer.GetDatabase();
        }

        public byte[] Get(string key)
        {
            var redisValue = GetRetryPolicy().Execute<RedisValue>(() => GetDatabase().StringGet(key));
            if (redisValue.HasValue)
            {
                return redisValue;
            }
            return null;
        }

        public Dictionary<CacheItemDefinition, byte[]> Get(ICollection<CacheItemDefinition> keys)
        {
            var redisResult = GetRetryPolicy().Execute<RedisResult>(() => GetDatabase().ScriptEvaluate(LuaScripts.GetMultipleGetScript(), keys: keys.Select(x => (RedisKey)x.Key).ToArray()));
            return CreateCacheItemDefinitionArrayFromLuaScriptResult(keys, redisResult);
        }

        private static Dictionary<CacheItemDefinition, byte[]> CreateCacheItemDefinitionArrayFromLuaScriptResult(ICollection<CacheItemDefinition> keys,
                                                                                                                 RedisResult redisResult)
        {
            if (redisResult == null)
            {
                throw new InvalidOperationException("Lua script did not return anything.");
            }

            var redisValues = (RedisValue[]) redisResult;

            if (redisValues.Length != keys.Count * 2)
            {
                throw new InvalidOperationException("Lua script did not return the expected number of records.");
            }
            
            var result = new Dictionary<CacheItemDefinition, byte[]>();
            int cpt = 0;
            foreach (var key in keys)
            {
                var cid = key.Clone();
                int ttl = (int) redisValues[cpt + 1];
                if (ttl <= 0)
                {
                    cid.TimeToLive = null;
                }
                else
                {
                    cid.TimeToLive = TimeSpan.FromSeconds(ttl);
                }
                result.Add(cid,
                           redisValues[cpt]);

                cpt += 2;
            }

            return result;
        }

        public async Task<byte[]> GetAsync(string key)
        {

            var redisValue = await GetRetryPolicyAsync().ExecuteAsync<RedisValue>(() => GetDatabase().StringGetAsync(key)).ConfigureAwait(false);
            if (redisValue.HasValue)
            {
                return redisValue;
            }
            return null;
        }

        public async Task<Dictionary<CacheItemDefinition, byte[]>> GetAsync(ICollection<CacheItemDefinition> keys)
        {
            var redisResult = await GetRetryPolicyAsync().ExecuteAsync<RedisResult>(() => GetDatabase().ScriptEvaluateAsync(LuaScripts.GetMultipleGetScript(), keys: keys.Select(x => (RedisKey)x.Key).ToArray()));
            return CreateCacheItemDefinitionArrayFromLuaScriptResult(keys, redisResult);
        }

        public void Set(string key,
                        byte[] serializedValue,
                        TimeSpan? ttl = null)
        {
            GetRetryPolicy().Execute(() => GetDatabase().StringSet(key, serializedValue, expiry: ttl));
        }

        public void Set(Dictionary<CacheItemDefinition, byte[]> objects)
        {
            RedisValue[] values;
            RedisKey[] keys;
            GetMultipleSetValues(objects, out keys, out values);

            GetRetryPolicy().Execute<RedisResult>(() => GetDatabase().ScriptEvaluate(LuaScripts.GetMultipleSetScript(),
                                                                                     keys: keys,
                                                                                     values: values));
        }

        private static void GetMultipleSetValues(Dictionary<CacheItemDefinition, byte[]> objects,
                                                 out RedisKey[] keys,
                                                 out RedisValue[] values)
        {
            keys = new RedisKey[objects.Count];
            values = new RedisValue[objects.Count * 2];
            int cpt = 0;
            foreach (var kvp in objects)
            {
                keys[cpt] = kvp.Key.Key;
                values[cpt * 2] = kvp.Value;
                values[cpt * 2 + 1] = kvp.Key.TimeToLive.HasValue
                                          ? kvp.Key.TimeToLive.Value.TotalSeconds
                                          : 0;
                cpt++;
            }
        }

        public Task SetAsync(string key,
                             byte[] serializedValue,
                             TimeSpan? ttl = null)
        {
            return GetRetryPolicyAsync().ExecuteAsync(() => GetDatabase().StringSetAsync(key, serializedValue, expiry: ttl));
        }

        public Task SetAsync(Dictionary<CacheItemDefinition, byte[]> objects)
        {
            RedisValue[] values;
            RedisKey[] keys;
            GetMultipleSetValues(objects, out keys, out values);

            return GetRetryPolicyAsync().ExecuteAsync<RedisResult>(() => GetDatabase().ScriptEvaluateAsync(LuaScripts.GetMultipleSetScript(),
                                                                                                           keys: keys,
                                                                                                           values: values));
        }

        public bool Remove(string key)
        {
            return GetRetryPolicy().Execute<bool>(() => GetDatabase().KeyDelete(key));
        }

        public Task<bool> RemoveAsync(string key)
        {
            return GetRetryPolicyAsync().ExecuteAsync<bool>(() => GetDatabase().KeyDeleteAsync(key));
        }

        public void RemoveAll()
        {
            GetRetryPolicy()
                .Execute(() =>
                         {
                             var endpoints = _multiplexer.GetEndPoints(true);
                             foreach (var endpoint in endpoints)
                             {
                                 var server = _multiplexer.GetServer(endpoint);
                                 server.FlushAllDatabases();
                             }
                         });
        }

        public async Task RemoveAllAsync()
        {
            await GetRetryPolicyAsync()
                .ExecuteAsync(async () =>
                                    {
                                        var endpoints = _multiplexer.GetEndPoints(true);
                                        foreach (var endpoint in endpoints)
                                        {
                                            var server = _multiplexer.GetServer(endpoint);
                                            await server.FlushAllDatabasesAsync()
                                                        .ConfigureAwait(false);
                                        }
                                    })
                .ConfigureAwait(false);
        }

        public TimeSpan? GetTimeToLive(string key)
        {
            return GetRetryPolicy().Execute<TimeSpan?>(() => GetDatabase().KeyTimeToLive(key));
        }

        public Task<TimeSpan?> GetTimeToLiveAsync(string key)
        {
            return GetRetryPolicyAsync().ExecuteAsync<TimeSpan?>(() => GetDatabase().KeyTimeToLiveAsync(key));
        }

        private IServer GetServer(ConnectionMultiplexer muxer)
        {
            EndPoint[] endpoints = _multiplexer.GetEndPoints();
            IServer result = null;
            foreach (var endpoint in endpoints)
            {
                var server = muxer.GetServer(endpoint);
                if (server.IsSlave
                    || !server.IsConnected)
                {
                    continue;
                }

                if (result != null)
                {
                    throw new InvalidOperationException("Requires exactly one master endpoint (found " + server.EndPoint + " and " + result.EndPoint + ")");
                }

                result = server;
            }
            if (result == null) throw new InvalidOperationException("Requires exactly one master endpoint (found none)");
            return result;
        }

        public KeyValuePair<string, string>[] GetConfig(string pattern = null)
        {
            return GetRetryPolicy().Execute(() => GetServer(_multiplexer).ConfigGet(pattern));
        }

        public Task<KeyValuePair<string, string>[]> GetConfigAsync(string pattern = null)
        {
            return GetRetryPolicyAsync().ExecuteAsync(() => GetServer(_multiplexer).ConfigGetAsync(pattern));
        }

        public bool KeyExists(string key)
        {
            return GetRetryPolicy().Execute<bool>(() => GetDatabase().KeyExists(key));
        }

        public IDisposable AcquireLock(string key, TimeSpan lockExpiry, TimeSpan timeout)
        {
            return new RedisLock(this).AcquireLock(key, lockExpiry, timeout);
        }

        public Task<IDisposable> AcquireLockAsync(string key, TimeSpan lockExpiry, TimeSpan timeout)
        {
            return new RedisLock(this).AcquireLockAsync(key, lockExpiry, timeout);
        }
    }
}