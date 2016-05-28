using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using StackExchange.Redis;

namespace DesertOctupos.MammothCache.Redis
{
    public sealed class RedisConnection : IRedisConnection, IDisposable
    {
        private readonly string _connectionString;
        private readonly IRedisRetryPolicy _redisRetryPolicy;
        private bool _isDisposed = false;
        private readonly ConnectionMultiplexer _multiplexer;
        private readonly RetryPolicy _retryPolicy;
        private readonly RetryPolicy _retryPolicyAsync;

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
            ConfigureIfMissing(options, "abortConnect", connectionString,  o => { o.AbortOnConnectFail = false; });
            ConfigureIfMissing(options, "allowAdmin", connectionString,  o => { o.AllowAdmin = true; });

            _multiplexer = ConnectionMultiplexer.Connect(options);
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

        public RedisValue Get(string key)
        {
            return GetRetryPolicy().Execute<RedisValue>(() => GetDatabase().StringGet(key));
        }

        public Task<RedisValue> GetAsync(string key)
        {
            return GetRetryPolicyAsync().ExecuteAsync<RedisValue>(() => GetDatabase().StringGetAsync(key));
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
    }
}