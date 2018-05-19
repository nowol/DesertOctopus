using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
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

        private string GetSetting(string settingsName, string defaultValue)
        {
            var envKey = Environment.GetEnvironmentVariables()
                                    .Keys.OfType<string>()
                                    .FirstOrDefault(x => String.Equals(x,
                                                                       settingsName,
                                                                       StringComparison.InvariantCultureIgnoreCase));
            if (envKey != null
                && Environment.GetEnvironmentVariables()[settingsName] != null)
            {
                return Environment.GetEnvironmentVariables()[settingsName].ToString();
            }

            return defaultValue;
        }

        protected string RedisConnectionString => GetSetting("RedisConnectionString", TestConfiguration.Instance.AppSettings.RedisConnectionString);

        protected string RedisMaxMemory => GetSetting("RedisMaxMemory", TestConfiguration.Instance.AppSettings.RedisMaxMemory.ToString());
    }
}