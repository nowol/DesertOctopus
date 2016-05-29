using System;
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
            WaitFor(Convert.ToInt32(seconds));
        }

        protected static string RandomKey()
        {
            return Guid.NewGuid().ToString();
        }
    }
}