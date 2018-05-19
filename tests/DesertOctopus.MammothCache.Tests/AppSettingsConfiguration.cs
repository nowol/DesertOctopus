using System;
using System.Linq;
using Newtonsoft.Json;

namespace DesertOctopus.MammothCache.Tests
{
    internal class AppSettingsConfiguration
    {
        public string RedisConnectionString { get; set; }

        public int RedisMaxMemory { get; set; }
    }

    internal class TestConfiguration
    {
        public AppSettingsConfiguration AppSettings { get; set; }

        private static readonly Lazy<TestConfiguration> LazyInstance = new Lazy<TestConfiguration>(LoadConfiguration);

        public static TestConfiguration Instance => LazyInstance.Value;

        private static TestConfiguration LoadConfiguration()
        {
            var currentPathDll = new Uri(typeof(TestConfiguration).Assembly.CodeBase).LocalPath;

            var settingsContent = System.IO.File.ReadAllText(System.IO.Path.Combine(System.IO.Directory.GetParent(currentPathDll).FullName, "appsettings.json"));
            return new TestConfiguration
                   {
                       AppSettings = JsonConvert.DeserializeObject<AppSettingsConfiguration>(settingsContent)
                   };
        }
    }
}