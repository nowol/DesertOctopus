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

        public event ItemEvictedFromCacheEventHandler OnItemRemovedFromCache;

        public RedisConnection(string connectionString, IRedisRetryPolicy redisRetryPolicy)
        {
            _connectionString = connectionString;
            _redisRetryPolicy = redisRetryPolicy;
            _retryPolicy = Policy.Handle<TimeoutException>()
                                 .Or<TimeoutException>()
                                 .Or<SocketException>()
                                 .Or<IOException>() // for async
                                 .WaitAndRetry(_redisRetryPolicy.SleepDurations);
            _retryPolicyAsync = Policy.Handle<TimeoutException>()
                                      .Or<TimeoutException>()
                                      .Or<SocketException>()
                                      .Or<IOException>() // for async
                                      .WaitAndRetryAsync(_redisRetryPolicy.SleepDurations);

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

        private RetryPolicy GetRetryPolicy()
        {
            GuardDisposed();
            return _retryPolicy;
        }

        private RetryPolicy GetRetryPolicyAsync()
        {
            GuardDisposed();
            return _retryPolicyAsync;
        }

        private IDatabase GetDatabase()
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

        public async Task<byte[]> GetAsync(string key)
        {

            var redisValue = await GetRetryPolicyAsync().ExecuteAsync<RedisValue>(() => GetDatabase().StringGetAsync(key)).ConfigureAwait(false);
            if (redisValue.HasValue)
            {
                return redisValue;
            }
            return null;
        }

        public void Set(string key,
                        byte[] serializedValue,
                        TimeSpan? ttl = null)
        {
            GetRetryPolicy().Execute(() => GetDatabase().StringSet(key, serializedValue, expiry: ttl));
        }

        public Task SetAsync(string key,
                             byte[] serializedValue,
                             TimeSpan? ttl = null)
        {
            return GetRetryPolicyAsync().ExecuteAsync(() => GetDatabase().StringSetAsync(key, serializedValue, expiry: ttl));
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
    }
}