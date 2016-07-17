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
            if (TestContext.Properties.Contains(key))
            {
                return TestContext.Properties[key] as string;
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