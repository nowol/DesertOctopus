using BenchmarkDotNet.Loggers;

namespace DesertOctopus.Benchmark
{
    internal class BenchmarkHelpers
    {
        public static ILogger GetLogger()
        {
            return new ConsoleLogger();
        }
    }
}
