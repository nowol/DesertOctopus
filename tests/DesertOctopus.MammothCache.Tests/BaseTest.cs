using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace DesertOctopus.MammothCache.Tests
{
    public abstract class BaseTest
    {
        protected void WaitFor(int seconds)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed.TotalSeconds <= seconds)
            {
                Thread.Sleep(50);
            }
        }

        protected void WaitFor(double seconds)
        {
            var sw = Stopwatch.StartNew();
            while (sw.Elapsed.TotalSeconds <= seconds)
            {
                Thread.Sleep(50);
            }
        }

        protected static string RandomKey()
        {
            return Guid.NewGuid().ToString();
        }

        protected string GetAppSetting(string key)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (!String.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return ConfigurationManager.AppSettings[key];
        }

        protected string RedisConnectionString { get { return GetAppSetting("RedisConnectionString"); } }

    }
}