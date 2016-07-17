using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var propKey = TestContext.Properties.Keys.OfType<string>()
                                     .FirstOrDefault(x => String.Equals(x,
                                                                        key,
                                                                        StringComparison.InvariantCultureIgnoreCase));
            if (propKey != null)
            {
                return TestContext.Properties[propKey] as string;
            }

            var envKey = Environment.GetEnvironmentVariables()
                                    .Keys.OfType<string>()
                                    .FirstOrDefault(x => String.Equals(x,
                                                                       key,
                                                                       StringComparison.InvariantCultureIgnoreCase));
            if (envKey != null
                && Environment.GetEnvironmentVariables()[key] != null)
            {
                return Environment.GetEnvironmentVariables()[key].ToString();
            }

            return ConfigurationManager.AppSettings[key];
        }

        protected string RedisConnectionString { get { return GetAppSetting("RedisConnectionString"); } }


        private TestContext _testContextInstance;
        public TestContext TestContext
        {
            get
            {
                return _testContextInstance;
            }
            set
            {
                _testContextInstance = value;
            }
        }
    }
}