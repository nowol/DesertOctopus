using System;
using System.Linq;

namespace DesertOctopus.MammothCache.Redis
{
    internal static class LuaScripts
    {
        public static string GetMultipleGetScript()
        {
            return @"local result = {} 
                     for _, key in ipairs(KEYS) do
                         table.insert(result, redis.call('get', key))
                         table.insert(result, redis.call('ttl', key))
                     end
                    return result ";
        }

        public static string GetMultipleSetScript()
        {
            return @"local index = 1
                     for _, key in ipairs(KEYS) do
                         local bytes = ARGV[index]
                         local ttl = tonumber(ARGV[index + 1])
                         index = index + 2

                         if ttl <= 0 then
                            redis.call('set', key, bytes)
                         else
                            redis.call('setex', key, ttl, bytes)
                         end
                     end ";
        }

        public static string GetTtlScript()
        {
            return @"if table.getn(KEYS) == 0 then
                         return -2
                     end
                     return redis.call('ttl', KEYS[1]) ";
        }

        public static string GetTtlsScript()
        {
            return @"local result = {} 
                     for _, key in ipairs(KEYS) do
                         table.insert(result, redis.call('ttl', key))
                     end
                    return result ";
        }
    }
}
