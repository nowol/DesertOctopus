using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DesertOctopus.MammothCache.Common;
using Polly;
using Polly.Retry;
using StackExchange.Redis;

namespace DesertOctopus.MammothCache.Redis
{
    /// <summary>
    /// Wrap StackExchange.Redis to add retry policy
    /// </summary>
    public sealed class RedisConnection : IRedisConnection, ISecondLevelCache, IDisposable
    {
        private readonly string _connectionString;
        private readonly IRedisRetryPolicy _redisRetryPolicy;
        private readonly ConnectionMultiplexer _multiplexer;
        private readonly RetryPolicy _retryPolicy;
        private readonly RetryPolicy _retryPolicyAsync;
        private readonly string _instanceId = Guid.NewGuid().ToString();
        private readonly string _setKeyChannel;
        private readonly string _removeAllItemsChannel = "RemoveAllItems";
        private bool _isDisposed = false;
        private ISubscriber _subscriber;
        private PolicyBuilder _baseRetryPolicy;

        /// <inheritdoc/>
        public event ItemEvictedFromCacheEventHandler OnItemRemovedFromCache;

        /// <inheritdoc/>
        public event RemoveAllItemsEventHandler OnRemoveAllItems;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisConnection"/> class.
        /// </summary>
        /// <param name="connectionString">Connection string to Redis</param>
        /// <param name="redisRetryPolicy">Retry policy</param>
        public RedisConnection(string connectionString, IRedisRetryPolicy redisRetryPolicy)
        {
            _setKeyChannel = "~SetKey~:" + _instanceId;

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
            GetRetryPolicy().Execute(() => _subscriber.Subscribe("~SetKey~:*", OnSetKeyFromRedis));
            GetRetryPolicy().Execute(() => _subscriber.Subscribe(_removeAllItemsChannel, OnRemoveAllItemsFromRedis));
        }

        private void OnRemoveAllItemsFromRedis(RedisChannel arg1,
                                               RedisValue arg2)
        {

            var eventCopy = OnRemoveAllItems;
            if (eventCopy != null)
            {
                eventCopy(this, new RemoveAllItemsEventArgs());
            }
        }

        private void OnSetKeyFromRedis(RedisChannel redisChannel,
                                       RedisValue redisValue)
        {
            var channel = (string)redisChannel;

            if (channel != _setKeyChannel)
            {
                SendOnItemRemoveFromCacheEvent(redisValue);
            }
        }

        private void OnKeyRemoveFromRedis(RedisChannel redisChannel,
                                          RedisValue redisValue)
        {
            SendOnItemRemoveFromCacheEvent(redisValue);
        }

        private void SendOnItemRemoveFromCacheEvent(RedisValue redisValue)
        {
            var eventCopy = OnItemRemovedFromCache;
            if (eventCopy != null
                && redisValue.HasValue)
            {
                eventCopy(this, new ItemEvictedEventArgs { Key = (string)redisValue });
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

        /// <summary>
        /// Dispose the <see cref="RedisConnection"/>
        /// </summary>
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

        /// <inheritdoc/>
        public byte[] Get(string key)
        {
            var redisValue = GetRetryPolicy().Execute<RedisValue>(() => GetDatabase().StringGet(key));
            if (redisValue.HasValue)
            {
                return redisValue;
            }

            return null;
        }

        /// <inheritdoc/>
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

            var redisValues = (RedisValue[])redisResult;

            if (redisValues.Length != keys.Count * 2)
            {
                throw new InvalidOperationException("Lua script did not return the expected number of records.");
            }

            var result = new Dictionary<CacheItemDefinition, byte[]>();
            int cpt = 0;
            foreach (var key in keys)
            {
                var cid = key.Clone();
                int ttl = (int)redisValues[cpt + 1];
                if (ttl <= 0)
                {
                    cid.TimeToLive = null;
                }
                else
                {
                    cid.TimeToLive = TimeSpan.FromSeconds(ttl);
                }

                result.Add(cid, redisValues[cpt]);

                cpt += 2;
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetAsync(string key)
        {

            var redisValue = await GetRetryPolicyAsync().ExecuteAsync<RedisValue>(() => GetDatabase().StringGetAsync(key)).ConfigureAwait(false);
            if (redisValue.HasValue)
            {
                return redisValue;
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<Dictionary<CacheItemDefinition, byte[]>> GetAsync(ICollection<CacheItemDefinition> keys)
        {
            var redisResult = await GetRetryPolicyAsync().ExecuteAsync<RedisResult>(() => GetDatabase().ScriptEvaluateAsync(LuaScripts.GetMultipleGetScript(), keys: keys.Select(x => (RedisKey)x.Key).ToArray()));
            return CreateCacheItemDefinitionArrayFromLuaScriptResult(keys, redisResult);
        }

        /// <inheritdoc/>
        public void Set(string key,
                        byte[] serializedValue,
                        TimeSpan? ttl = null)
        {
            GetRetryPolicy().Execute(() => GetDatabase().Publish(_setKeyChannel, key));
            GetRetryPolicy().Execute(() => GetDatabase().StringSet(key, serializedValue, expiry: ttl));
        }

        /// <inheritdoc/>
        public void Set(Dictionary<CacheItemDefinition, byte[]> objects)
        {
            RedisValue[] values;
            RedisKey[] keys;
            GetMultipleSetValues(objects, out keys, out values);

            foreach (var key in keys)
            {
                GetRetryPolicy().Execute(() => GetDatabase().Publish(_setKeyChannel, (string)key));
            }

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
                values[(cpt * 2) + 1] = kvp.Key.TimeToLive.HasValue
                                            ? kvp.Key.TimeToLive.Value.TotalSeconds
                                            : 0;
                cpt++;
            }
        }

        /// <inheritdoc/>
        public async Task SetAsync(string key,
                             byte[] serializedValue,
                             TimeSpan? ttl = null)
        {
            await GetRetryPolicyAsync().ExecuteAsync(() => GetDatabase().PublishAsync(_setKeyChannel, key)).ConfigureAwait(false);
            await GetRetryPolicyAsync().ExecuteAsync(() => GetDatabase().StringSetAsync(key, serializedValue, expiry: ttl)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task SetAsync(Dictionary<CacheItemDefinition, byte[]> objects)
        {
            RedisValue[] values;
            RedisKey[] keys;
            GetMultipleSetValues(objects, out keys, out values);

            foreach (var key in keys)
            {
                await GetRetryPolicyAsync().ExecuteAsync(() => GetDatabase().PublishAsync(_setKeyChannel, (string)key)).ConfigureAwait(false);
            }

            await GetRetryPolicyAsync().ExecuteAsync<RedisResult>(() => GetDatabase().ScriptEvaluateAsync(LuaScripts.GetMultipleSetScript(),
                                                                                                          keys: keys,
                                                                                                          values: values)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public bool Remove(string key)
        {
            return GetRetryPolicy().Execute<bool>(() => GetDatabase().KeyDelete(key));
        }

        /// <inheritdoc/>
        public Task<bool> RemoveAsync(string key)
        {
            return GetRetryPolicyAsync().ExecuteAsync<bool>(() => GetDatabase().KeyDeleteAsync(key));
        }

        /// <inheritdoc/>
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
            GetRetryPolicy().Execute(() => GetDatabase().Publish(_removeAllItemsChannel, String.Empty));
        }

        /// <inheritdoc/>
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
            await GetRetryPolicyAsync().ExecuteAsync(() => GetDatabase().PublishAsync(_removeAllItemsChannel, String.Empty)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public TimeToLiveResult GetTimeToLive(string key)
        {
            var redisResult = GetRetryPolicy().Execute<RedisResult>(() => GetDatabase().ScriptEvaluate(LuaScripts.GetTtlScript(), keys: new RedisKey[] { key }));
            return ConvertRedisResultTimeToLiveResult((int)redisResult);
        }

        /// <inheritdoc/>
        public async Task<TimeToLiveResult> GetTimeToLiveAsync(string key)
        {
            var redisResult = await GetRetryPolicyAsync().ExecuteAsync<RedisResult>(() => GetDatabase().ScriptEvaluateAsync(LuaScripts.GetTtlScript(), keys: new RedisKey[] { key })).ConfigureAwait(false);
            return ConvertRedisResultTimeToLiveResult((int)redisResult);
        }

        /// <inheritdoc/>
        public Dictionary<string, TimeToLiveResult> GetTimeToLives(string[] keys)
        {
            var redisResult = GetRetryPolicy().Execute<RedisResult>(() => GetDatabase().ScriptEvaluate(LuaScripts.GetTtlsScript(), keys: keys.Select(x => (RedisKey)x).ToArray()));

            if (redisResult == null)
            {
                throw new InvalidOperationException("Lua script did not return anything.");
            }

            return ConvertRedisResultTimeToLiveResults(keys, redisResult);
        }

        /// <inheritdoc/>
        public async Task<Dictionary<string, TimeToLiveResult>> GetTimeToLivesAsync(string[] keys)
        {
            var redisResult = await GetRetryPolicyAsync().ExecuteAsync<RedisResult>(() => GetDatabase().ScriptEvaluateAsync(LuaScripts.GetTtlsScript(), keys: keys.Select(x => (RedisKey)x).ToArray())).ConfigureAwait(false);

            if (redisResult == null)
            {
                throw new InvalidOperationException("Lua script did not return anything.");
            }

            return ConvertRedisResultTimeToLiveResults(keys, redisResult);
        }

        private static Dictionary<string, TimeToLiveResult> ConvertRedisResultTimeToLiveResults(string[] keys,
                                                                                                RedisResult redisResult)
        {
            var redisValues = (RedisValue[])redisResult;
            var result = new Dictionary<string, TimeToLiveResult>();
            int cpt = 0;
            foreach (var key in keys)
            {
                result.Add(key,
                           ConvertRedisResultTimeToLiveResult((int)redisValues[cpt]));
                cpt++;
            }

            return result;
        }

        private static TimeToLiveResult ConvertRedisResultTimeToLiveResult(int ttl)
        {
            if (ttl == -2 || ttl == 0)
            {
                return new TimeToLiveResult();
            }

            if (ttl == -1)
            {
                return new TimeToLiveResult() { KeyExists = true };
            }

            return new TimeToLiveResult() { KeyExists = true, TimeToLive = TimeSpan.FromSeconds(ttl) };
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

            if (result == null)
            {
                throw new InvalidOperationException("Requires exactly one master endpoint (found none)");
            }

            return result;
        }

        /// <inheritdoc/>
        public KeyValuePair<string, string>[] GetConfig(string pattern = null)
        {
            return GetRetryPolicy().Execute(() => GetServer(_multiplexer).ConfigGet(pattern));
        }

        /// <inheritdoc/>
        public Task<KeyValuePair<string, string>[]> GetConfigAsync(string pattern = null)
        {
            return GetRetryPolicyAsync().ExecuteAsync(() => GetServer(_multiplexer).ConfigGetAsync(pattern));
        }

        /// <inheritdoc/>
        public bool KeyExists(string key)
        {
            return GetRetryPolicy().Execute<bool>(() => GetDatabase().KeyExists(key));
        }

        /// <inheritdoc/>
        public Task<bool> KeyExistsAsync(string key)
        {
            return GetRetryPolicyAsync().ExecuteAsync(() => GetDatabase().KeyExistsAsync(key));
        }

        /// <inheritdoc/>
        public IDisposable AcquireLock(string key, TimeSpan lockExpiry, TimeSpan timeout)
        {
            return new RedisLock(this).AcquireLock(key, lockExpiry, timeout);
        }

        /// <inheritdoc/>
        public Task<IDisposable> AcquireLockAsync(string key, TimeSpan lockExpiry, TimeSpan timeout)
        {
            return new RedisLock(this).AcquireLockAsync(key, lockExpiry, timeout);
        }
    }
}