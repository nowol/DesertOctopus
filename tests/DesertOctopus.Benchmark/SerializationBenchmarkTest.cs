using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using DesertOctopus.Benchmark.Models;
using DesertOctopus.MammothCache;
using DesertOctopus.MammothCache.Common;
using DesertOctupos.MammothCache.Redis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DesertOctopus.Benchmark
{
    [TestClass]
    public class SerializationBenchmarkTest
    {
        [TestMethod]
        public void ProfileSerialization()
        {
            string json = @"{""CatalogId"":""Global"",""DefinitionName"":""MyDefinitionName"",""DisplayName"":{""Values"":{""en-US"":""Some red wine for testingt""}},""ListPrice"":666.6600,""Id"":""5465456"",""Created"":""\/Date(1355220564000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""Description"":{""Values"":{""en-US"":""“Sirloin chuck spare ribs alcatra venison beef ribs turkey fatback hamburger. Ball tip alcatra shoulder biltong flank. Tail leberkas bresaola kielbasa venison jerky prosciutto chicken meatball ham hock brisket chuck swine. Pig venison chicken tri-tip doner, prosciutto tenderloin jowl ribeye bresaola alcatra kielbasa picanha. Meatball ham hock rump ham jerky pastrami pork ribeye porchetta. Ribeye salami pig strip steak rump flank. Meatloaf turkey porchetta turducken beef shoulder biltong chuck ham hock strip steak pork belly tri-tip meatball. Prosciutto ground round jowl ham hock. Kielbasa bacon sausage tail meatball jerky doner strip steak shoulder alcatra. Corned beef flank meatball capicola, meatloaf andouille kevin pancetta alcatra. Tail boudin frankfurter leberkas. Jowl prosciutto fatback filet mignon pancetta.""}},""ParentCategories"":[{""Id"":""BrutRos"",""ChildCategories"":[],""IsSearchable"":true,""DisplayName"":{""Values"":{""en-US"":""Brut Ros""}},""DefinitionName"":""WineType"",""Created"":""\/Date(1432216913000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""RelatedCategories"":[],""RelatedProducts"":[],""CatalogId"":""Global"",""PrimaryParentCategoryId"":""Sparkling"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Category, DesertOctopus.Benchmark"",""TypeName"":""Category"",""PropertyBag"":{""ItemId"":16,""ItemType"":""Catty!"",""ParentItem_Id"":9},""SequenceNumber"":0,""HiddenInScope"":false,""Active"":true,""IncludeInSearch"":true,""Relationships"":[]},{""Id"":""Red"",""ChildCategories"":[],""IsSearchable"":true,""DisplayName"":{""Values"":{""en-US"":""Red""}},""DefinitionName"":""WineType"",""Created"":""\/Date(1432216913000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""RelatedCategories"":[],""RelatedProducts"":[],""CatalogId"":""Global"",""PrimaryParentCategoryId"":""PopCellar"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Category, DesertOctopus.Benchmark"",""TypeName"":""Category"",""PropertyBag"":{""Item_Id"":1,""ItemDiscriminator"":""CATEGORY"",""ParentItem_Id"":52138},""SequenceNumber"":1,""HiddenInScope"":false,""Active"":true,""IncludeInSearch"":true,""Relationships"":[]}],""PrimaryParentCategory"":{""Id"":""BrutRos"",""ChildCategories"":[],""IsSearchable"":true,""DisplayName"":{""Values"":{""en-US"":""Brut Ros""}},""DefinitionName"":""WineType"",""Created"":""\/Date(1432216913000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""RelatedCategories"":[],""RelatedProducts"":[],""CatalogId"":""Global"",""PrimaryParentCategoryId"":""Sparkling"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Category, DesertOctopus.Benchmark"",""TypeName"":""Category"",""PropertyBag"":{""Item_Id"":16,""ItemDiscriminator"":""CATEGORY"",""ParentItem_Id"":9},""SequenceNumber"":0,""HiddenInScope"":false,""Active"":true,""IncludeInSearch"":true,""Relationships"":[]},""PrimaryParentCategoryId"":""BrutRos"",""RelatedCategories"":[],""RelatedProducts"":[],""Variants"":[{""CatalogId"":""Global"",""DefinitionName"":""WineBottleVariant"",""ListPrice"":199.9800,""Id"":""34699Standard"",""Created"":""\/Date(1355220564000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""ProductId"":""34699"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Variant, DesertOctopus.Benchmark"",""TypeName"":""Variant"",""PropertyBag"":{""Item_Id"":27235,""ItemDiscriminator"":""VARIANT"",""ParentItemName"":""34699"",""ParentItem_Id"":4140,""IsOverridden"":false,""IncludeInSearch"":true,""Volume"":""Standard""},""Active"":true,""HiddenInScope"":false,""Prices"":[{""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.ProductPriceEntry, DesertOctopus.Benchmark"",""TypeName"":""ProductPriceEntry"",""PropertyBag"":{},""PriceListId"":""DEFAULT"",""Price"":199.9800,""SequenceNumber"":0,""IsInherited"":false}],""Sku"":""34699Standard""},{""CatalogId"":""Global"",""DefinitionName"":""WineBottleVariant"",""ListPrice"":266.6400,""Id"":""34699Magnum"",""Created"":""\/Date(1355220564000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""ProductId"":""34699"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Variant, DesertOctopus.Benchmark"",""TypeName"":""Variant"",""PropertyBag"":{""Item_Id"":27236,""ItemDiscriminator"":""VARIANT"",""ParentItemName"":""34699"",""ParentItem_Id"":4140,""IsOverridden"":false,""IncludeInSearch"":true,""Volume"":""Magnum""},""Active"":true,""HiddenInScope"":false,""Prices"":[{""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.ProductPriceEntry, DesertOctopus.Benchmark"",""TypeName"":""ProductPriceEntry"",""PropertyBag"":{},""PriceListId"":""DEFAULT"",""Price"":266.6400,""SequenceNumber"":0,""IsInherited"":false}],""Sku"":""34699Magnum""}],""Sku"":""4140"",""Active"":true,""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Product, DesertOctopus.Benchmark"",""TypeName"":""Product"",""PropertyBag"":{""Item_Id"":4140,""ItemDiscriminator"":""PRODUCT"",""MSRP"":18.00000,""DateReviewed"":""\/Date(809827200000)\/"",""Region"":""Sonoma"",""Score"":5,""URLString"":""LYETH Cabernet Blend Alexander Valley A Red Blend 1992 Cabernet Blend Red"",""Wine"":""A Red Blend Alexander Valley"",""WineID"":34699,""Winery"":""Lyeth"",""Year"":1992,""PublicationState"":""Published"",""Alcohol"":22.50000,""SommelierScore"":""Decanter Magazine 9.7 | Wine Advocate 9.3 | Wine Spectator Magazine 9.9"",""CustomerScore"":87.10000,""Pairing"":""Lasagna | MeatballsSpaghetti"",""Caracteristics"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""<p>\n\t&quot;Supple and p<strong>olished cedar, coffee, cherry and berry flav</strong>ors. This i<u>s elegant, finishing with firm tannins and good length. Drinkable now.&quot; -- test</u></p>\n""}},""Appellations"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Alexander Valley|Sonoma""}},""Body"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Elegant|Firm|Firm Tannins|Polished|Supple|Tannins""}},""Country"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""United States""}},""Designation"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Best Buy""}},""Flavors"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Berry|Cedar|Cherry|Coffee""}},""WineType"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Cabernet Blend""}}},""Prices"":[{""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.ProductPriceEntry, DesertOctopus.Benchmark"",""TypeName"":""ProductPriceEntry"",""PropertyBag"":{},""PriceListId"":""DEFAULT"",""Price"":66.6600,""SequenceNumber"":0,""IsInherited"":false,""PriceListType"":""Regular"",""PriceListCategory"":""Regular""}],""Relationships"":[],""SequenceNumber"":0,""HiddenInScope"":false,""IncludeInSearch"":true,""IsOverridden"":false,""TaxCategory"":""Taxable""}";
            var product = ServiceStack.Text.JsonSerializer.DeserializeFromString<Product>(json);
            var krakenBytes = DesertOctopus.KrakenSerializer.Serialize(product);
            krakenBytes = DesertOctopus.KrakenSerializer.Serialize(product);
            System.IO.File.WriteAllBytes(@"d:\z.bin", krakenBytes);

            DesertOctopus.KrakenSerializer.Deserialize<Product>(krakenBytes);
            DesertOctopus.ObjectCloner.Clone(product);

            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < 100000; i++)
            {
                //krakenBytes = DesertOctopus.KrakenSerializer.Serialize(product);
                DesertOctopus.KrakenSerializer.Deserialize<Product>(krakenBytes);
                //DesertOctopus.ObjectCloner.Clone(product);
            }

            sw.Stop();
            Assert.Fail(sw.Elapsed.ToString());
        }

        [TestMethod]
        public void PrintProductSerializationSizes()
        {
            var b = new ProductSerializationBenchMark();
            Console.WriteLine("JsonSerialization: {0}", b.JsonSerialization().Length);
            Console.WriteLine("OmniSerialization: {0}", b.OmniSerialization().Length);
            Console.WriteLine("KrakenSerialization: {0}", b.KrakenSerialization().Length);
            Console.WriteLine("BinaryFormatterSerialization: {0}", b.BinaryFormatterSerialization().Length);
        }

        [TestMethod]
        public void ProductSerializationBenchmark()
        {
            var summary = BenchmarkRunner.Run<ProductSerializationBenchMark>();
                
            var k = BenchmarkDotNet.Exporters.HtmlExporter.Default.ExportToFiles(summary);
            Console.WriteLine(k.First());

            foreach (var validationError in summary.ValidationErrors)
            {
                Console.WriteLine(validationError.Message);
            }

            PrintProductSerializationSizes();

            Assert.Fail(k.First());
        }

        [TestMethod]
        public void ProductCloningBenchmark()
        {
            var summary = BenchmarkRunner.Run<ProductCloningBenchMark>();

            var k = BenchmarkDotNet.Exporters.HtmlExporter.Default.ExportToFiles(summary);
            Console.WriteLine(k.First());

            foreach (var validationError in summary.ValidationErrors)
            {
                Console.WriteLine(validationError.Message);
            }

            Assert.Fail(k.First());
        }

        [TestMethod]
        public void SimpleDtoWithEveryPrimitivesSerializationBenchmark()
        {
            var summary = BenchmarkRunner.Run<SimpleDtoWithEveryPrimitivesSerializationBenchmark>();

            var k = BenchmarkDotNet.Exporters.HtmlExporter.Default.ExportToFiles(summary);
            Console.WriteLine(k.First());

            foreach (var validationError in summary.ValidationErrors)
            {
                Console.WriteLine(validationError.Message);
            }

            Assert.Fail(k.First());
        }

        [TestMethod]
        public void SimpleDtoWithEveryPrimitivesCloningBenchmark()
        {
            var summary = BenchmarkRunner.Run<SimpleDtoWithEveryPrimitivesCloningBenchmark>();

            var k = BenchmarkDotNet.Exporters.HtmlExporter.Default.ExportToFiles(summary);
            Console.WriteLine(k.First());

            foreach (var validationError in summary.ValidationErrors)
            {
                Console.WriteLine(validationError.Message);
            }

            Assert.Fail(k.First());
        }

        [TestMethod]
        public void MyTestMethod2()
        {
            string json = @"{""CatalogId"":""Global"",""DefinitionName"":""MyDefinitionName"",""DisplayName"":{""Values"":{""en-US"":""Some red wine for testingt""}},""ListPrice"":666.6600,""Id"":""5465456"",""Created"":""\/Date(1355220564000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""Description"":{""Values"":{""en-US"":""“Sirloin chuck spare ribs alcatra venison beef ribs turkey fatback hamburger. Ball tip alcatra shoulder biltong flank. Tail leberkas bresaola kielbasa venison jerky prosciutto chicken meatball ham hock brisket chuck swine. Pig venison chicken tri-tip doner, prosciutto tenderloin jowl ribeye bresaola alcatra kielbasa picanha. Meatball ham hock rump ham jerky pastrami pork ribeye porchetta. Ribeye salami pig strip steak rump flank. Meatloaf turkey porchetta turducken beef shoulder biltong chuck ham hock strip steak pork belly tri-tip meatball. Prosciutto ground round jowl ham hock. Kielbasa bacon sausage tail meatball jerky doner strip steak shoulder alcatra. Corned beef flank meatball capicola, meatloaf andouille kevin pancetta alcatra. Tail boudin frankfurter leberkas. Jowl prosciutto fatback filet mignon pancetta.""}},""ParentCategories"":[{""Id"":""BrutRos"",""ChildCategories"":[],""IsSearchable"":true,""DisplayName"":{""Values"":{""en-US"":""Brut Ros""}},""DefinitionName"":""WineType"",""Created"":""\/Date(1432216913000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""RelatedCategories"":[],""RelatedProducts"":[],""CatalogId"":""Global"",""PrimaryParentCategoryId"":""Sparkling"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Category, DesertOctopus.Benchmark"",""TypeName"":""Category"",""PropertyBag"":{""ItemId"":16,""ItemType"":""Catty!"",""ParentItem_Id"":9},""SequenceNumber"":0,""HiddenInScope"":false,""Active"":true,""IncludeInSearch"":true,""Relationships"":[]},{""Id"":""Red"",""ChildCategories"":[],""IsSearchable"":true,""DisplayName"":{""Values"":{""en-US"":""Red""}},""DefinitionName"":""WineType"",""Created"":""\/Date(1432216913000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""RelatedCategories"":[],""RelatedProducts"":[],""CatalogId"":""Global"",""PrimaryParentCategoryId"":""PopCellar"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Category, DesertOctopus.Benchmark"",""TypeName"":""Category"",""PropertyBag"":{""Item_Id"":1,""ItemDiscriminator"":""CATEGORY"",""ParentItem_Id"":52138},""SequenceNumber"":1,""HiddenInScope"":false,""Active"":true,""IncludeInSearch"":true,""Relationships"":[]}],""PrimaryParentCategory"":{""Id"":""BrutRos"",""ChildCategories"":[],""IsSearchable"":true,""DisplayName"":{""Values"":{""en-US"":""Brut Ros""}},""DefinitionName"":""WineType"",""Created"":""\/Date(1432216913000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""RelatedCategories"":[],""RelatedProducts"":[],""CatalogId"":""Global"",""PrimaryParentCategoryId"":""Sparkling"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Category, DesertOctopus.Benchmark"",""TypeName"":""Category"",""PropertyBag"":{""Item_Id"":16,""ItemDiscriminator"":""CATEGORY"",""ParentItem_Id"":9},""SequenceNumber"":0,""HiddenInScope"":false,""Active"":true,""IncludeInSearch"":true,""Relationships"":[]},""PrimaryParentCategoryId"":""BrutRos"",""RelatedCategories"":[],""RelatedProducts"":[],""Variants"":[{""CatalogId"":""Global"",""DefinitionName"":""WineBottleVariant"",""ListPrice"":199.9800,""Id"":""34699Standard"",""Created"":""\/Date(1355220564000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""ProductId"":""34699"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Variant, DesertOctopus.Benchmark"",""TypeName"":""Variant"",""PropertyBag"":{""Item_Id"":27235,""ItemDiscriminator"":""VARIANT"",""ParentItemName"":""34699"",""ParentItem_Id"":4140,""IsOverridden"":false,""IncludeInSearch"":true,""Volume"":""Standard""},""Active"":true,""HiddenInScope"":false,""Prices"":[{""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.ProductPriceEntry, DesertOctopus.Benchmark"",""TypeName"":""ProductPriceEntry"",""PropertyBag"":{},""PriceListId"":""DEFAULT"",""Price"":199.9800,""SequenceNumber"":0,""IsInherited"":false}],""Sku"":""34699Standard""},{""CatalogId"":""Global"",""DefinitionName"":""WineBottleVariant"",""ListPrice"":266.6400,""Id"":""34699Magnum"",""Created"":""\/Date(1355220564000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""ProductId"":""34699"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Variant, DesertOctopus.Benchmark"",""TypeName"":""Variant"",""PropertyBag"":{""Item_Id"":27236,""ItemDiscriminator"":""VARIANT"",""ParentItemName"":""34699"",""ParentItem_Id"":4140,""IsOverridden"":false,""IncludeInSearch"":true,""Volume"":""Magnum""},""Active"":true,""HiddenInScope"":false,""Prices"":[{""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.ProductPriceEntry, DesertOctopus.Benchmark"",""TypeName"":""ProductPriceEntry"",""PropertyBag"":{},""PriceListId"":""DEFAULT"",""Price"":266.6400,""SequenceNumber"":0,""IsInherited"":false}],""Sku"":""34699Magnum""}],""Sku"":""4140"",""Active"":true,""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Product, DesertOctopus.Benchmark"",""TypeName"":""Product"",""PropertyBag"":{""Item_Id"":4140,""ItemDiscriminator"":""PRODUCT"",""MSRP"":18.00000,""DateReviewed"":""\/Date(809827200000)\/"",""Region"":""Sonoma"",""Score"":5,""URLString"":""LYETH Cabernet Blend Alexander Valley A Red Blend 1992 Cabernet Blend Red"",""Wine"":""A Red Blend Alexander Valley"",""WineID"":34699,""Winery"":""Lyeth"",""Year"":1992,""PublicationState"":""Published"",""Alcohol"":22.50000,""SommelierScore"":""Decanter Magazine 9.7 | Wine Advocate 9.3 | Wine Spectator Magazine 9.9"",""CustomerScore"":87.10000,""Pairing"":""Lasagna | MeatballsSpaghetti"",""Caracteristics"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""<p>\n\t&quot;Supple and p<strong>olished cedar, coffee, cherry and berry flav</strong>ors. This i<u>s elegant, finishing with firm tannins and good length. Drinkable now.&quot; -- test</u></p>\n""}},""Appellations"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Alexander Valley|Sonoma""}},""Body"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Elegant|Firm|Firm Tannins|Polished|Supple|Tannins""}},""Country"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""United States""}},""Designation"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Best Buy""}},""Flavors"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Berry|Cedar|Cherry|Coffee""}},""WineType"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Cabernet Blend""}}},""Prices"":[{""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.ProductPriceEntry, DesertOctopus.Benchmark"",""TypeName"":""ProductPriceEntry"",""PropertyBag"":{},""PriceListId"":""DEFAULT"",""Price"":66.6600,""SequenceNumber"":0,""IsInherited"":false,""PriceListType"":""Regular"",""PriceListCategory"":""Regular""}],""Relationships"":[],""SequenceNumber"":0,""HiddenInScope"":false,""IncludeInSearch"":true,""IsOverridden"":false,""TaxCategory"":""Taxable""}";
            var product = ServiceStack.Text.JsonSerializer.DeserializeFromString<Product>(json);
            var krakenBytes = DesertOctopus.KrakenSerializer.Serialize(product);

        }

        [TestMethod]
        public void MammothCacheBenchmark()
        {
            var summary = BenchmarkRunner.Run<MammothCacheBenchmark>();

            var k = BenchmarkDotNet.Exporters.HtmlExporter.Default.ExportToFiles(summary);
            Console.WriteLine(k.First());

            foreach (var validationError in summary.ValidationErrors)
            {
                Console.WriteLine(validationError.Message);
            }

            Assert.Fail(k.First());
        }
    }

    public class MammothCacheBenchmark
    {
        private static readonly RedisConnection _connection;
        private static string _redisConnectionString = "172.16.100.100";
        private static readonly IRedisRetryPolicy _redisRetryPolicy;
        private static readonly IMammothCache _cache;
        private static readonly FirstLevelCacheConfig _config = new FirstLevelCacheConfig();
        private static readonly IFirstLevelCacheCloningProvider _noCloningProvider = new NoCloningProvider();
        private static readonly INonSerializableCache _nonSerializableCache = new NonSerializableCache();


        static MammothCacheBenchmark()
        {
            _config.AbsoluteExpiration = TimeSpan.FromSeconds(5);
            _config.MaximumMemorySize = 1000;
            _config.TimerInterval = 1;

            _redisRetryPolicy = new RedisRetryPolicy(50, 100, 150);
            _connection = new RedisConnection(_redisConnectionString, _redisRetryPolicy);

            //var firstLevelCache = new SquirrelCache(_config, _noCloningProvider);
            var firstLevelCache = new DummyCache();
            _cache = new MammothCache.MammothCache(firstLevelCache, _connection, _nonSerializableCache, new MammothCacheSerializationProvider());

            var p = _cache.GetOrAdd("key",
                                    () =>
                                    {
                                        var product = ServiceStack.Text.JsonSerializer.DeserializeFromString<Product>(ProductSerializationBenchMark.JsonProduct);
                                        return product;
                                    });

        }


        [Benchmark]
        public Product GetFromCache()
        {
            return _cache.GetOrAdd("key",
                                   () =>
                                   {
                                       var product = ServiceStack.Text.JsonSerializer.DeserializeFromString<Product>(ProductSerializationBenchMark.JsonProduct);
                                       return product;
                                   });
        }

        public class DummyCache : IFirstLevelCache
        {
            public ConditionalResult<T> Get<T>(string key) where T : class
            {
                return ConditionalResult.CreateFailure<T>();
            }

            public void Remove(string key) { }

            public void RemoveAll() { }

            public void Set(string key,
                            byte[] serializedValue,
                            TimeSpan? ttl = null) { }
        }
    }

    public class ProductCloningBenchMark
    {
        private Product product;

        public ProductCloningBenchMark()
        {
            product = ServiceStack.Text.JsonSerializer.DeserializeFromString<Product>(ProductSerializationBenchMark.JsonProduct);

            var clone = ObjectCloner.Clone(product);
        }

        [Benchmark]
        public Product Clone()
        {
            return ObjectCloner.Clone(product);
        }
    }

    public class SimpleDtoWithEveryPrimitivesCloningBenchmark
    {
        private ClassWithAllPrimitiveTypes obj;

        public SimpleDtoWithEveryPrimitivesCloningBenchmark()
        {
            obj = new ClassWithAllPrimitiveTypes();

            var clone = ObjectCloner.Clone(obj);
        }

        [Benchmark]
        public ClassWithAllPrimitiveTypes Clone()
        {
            return ObjectCloner.Clone(obj);
        }
    }

    public class SimpleDtoWithEveryPrimitivesSerializationBenchmark
    {
        private Orckestra.OmniSerializer.Serializer omni = new Orckestra.OmniSerializer.Serializer();
        private BinaryFormatter bf = new BinaryFormatter();
        private byte[] krakenBytes;
        private byte[] bfBytes;
        private byte[] omniBytes;
        private ClassWithAllPrimitiveTypes obj;
        private string Json = String.Empty;

        public SimpleDtoWithEveryPrimitivesSerializationBenchmark()
        {
            obj = new ClassWithAllPrimitiveTypes();
            Json = ServiceStack.Text.JsonSerializer.SerializeToString(obj);
            krakenBytes = DesertOctopus.KrakenSerializer.Serialize(obj);

            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                bfBytes = ms.ToArray();
            }

            using (var ms = new MemoryStream())
            {
                omni.SerializeObject(ms, obj);
                omniBytes = ms.ToArray();
            }

            DesertOctopus.KrakenSerializer.Deserialize<ClassWithAllPrimitiveTypes>(krakenBytes);

            ServiceStack.Text.JsonSerializer.DeserializeFromString<ClassWithAllPrimitiveTypes>(Json);
        }


        [Benchmark]
        public byte[] JsonSerialization()
        {
            using (var ms = new MemoryStream())
            {
                ServiceStack.Text.JsonSerializer.SerializeToStream(obj, ms);
                return ms.ToArray();
            }
        }

        [Benchmark]
        public ClassWithAllPrimitiveTypes JsonDeserialization()
        {
            return ServiceStack.Text.JsonSerializer.DeserializeFromString<ClassWithAllPrimitiveTypes>(Json);
        }

        [Benchmark]
        public byte[] OmniSerialization()
        {
            using (var ms = new MemoryStream())
            {
                omni.SerializeObject(ms, obj);
                return ms.ToArray();
            }
        }

        [Benchmark]
        public ClassWithAllPrimitiveTypes OmniDeserialization()
        {
            using (var ms = new MemoryStream(omniBytes))
            {
                return omni.Deserialize(ms) as ClassWithAllPrimitiveTypes;
            }
        }

        [Benchmark]
        public byte[] KrakenSerialization()
        {
            return DesertOctopus.KrakenSerializer.Serialize(obj);
        }

        [Benchmark]
        public ClassWithAllPrimitiveTypes KrakenDeserialization()
        {
            return DesertOctopus.KrakenSerializer.Deserialize<ClassWithAllPrimitiveTypes>(krakenBytes);
        }

        [Benchmark]
        public byte[] BinaryFormatterSerialization()
        {
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        [Benchmark]
        public ClassWithAllPrimitiveTypes BinaryFormatterDeserialization()
        {
            using (var ms = new MemoryStream(bfBytes))
            {
                return bf.Deserialize(ms) as ClassWithAllPrimitiveTypes;
            }
        }
    }

    public class ProductSerializationBenchMark
    {
        private Orckestra.OmniSerializer.Serializer omni = new Orckestra.OmniSerializer.Serializer();
        private BinaryFormatter bf = new BinaryFormatter();
        private byte[] krakenBytes;
        private byte[] bfBytes;
        private byte[] omniBytes;
        private Product product;
        public static string JsonProduct = @"{""CatalogId"":""Global"",""DefinitionName"":""MyDefinitionName"",""DisplayName"":{""Values"":{""en-US"":""Some red wine for testingt""}},""ListPrice"":666.6600,""Id"":""5465456"",""Created"":""\/Date(1355220564000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""Description"":{""Values"":{""en-US"":""“Sirloin chuck spare ribs alcatra venison beef ribs turkey fatback hamburger. Ball tip alcatra shoulder biltong flank. Tail leberkas bresaola kielbasa venison jerky prosciutto chicken meatball ham hock brisket chuck swine. Pig venison chicken tri-tip doner, prosciutto tenderloin jowl ribeye bresaola alcatra kielbasa picanha. Meatball ham hock rump ham jerky pastrami pork ribeye porchetta. Ribeye salami pig strip steak rump flank. Meatloaf turkey porchetta turducken beef shoulder biltong chuck ham hock strip steak pork belly tri-tip meatball. Prosciutto ground round jowl ham hock. Kielbasa bacon sausage tail meatball jerky doner strip steak shoulder alcatra. Corned beef flank meatball capicola, meatloaf andouille kevin pancetta alcatra. Tail boudin frankfurter leberkas. Jowl prosciutto fatback filet mignon pancetta.""}},""ParentCategories"":[{""Id"":""BrutRos"",""ChildCategories"":[],""IsSearchable"":true,""DisplayName"":{""Values"":{""en-US"":""Brut Ros""}},""DefinitionName"":""WineType"",""Created"":""\/Date(1432216913000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""RelatedCategories"":[],""RelatedProducts"":[],""CatalogId"":""Global"",""PrimaryParentCategoryId"":""Sparkling"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Category, DesertOctopus.Benchmark"",""TypeName"":""Category"",""PropertyBag"":{""ItemId"":16,""ItemType"":""Catty!"",""ParentItem_Id"":9},""SequenceNumber"":0,""HiddenInScope"":false,""Active"":true,""IncludeInSearch"":true,""Relationships"":[]},{""Id"":""Red"",""ChildCategories"":[],""IsSearchable"":true,""DisplayName"":{""Values"":{""en-US"":""Red""}},""DefinitionName"":""WineType"",""Created"":""\/Date(1432216913000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""RelatedCategories"":[],""RelatedProducts"":[],""CatalogId"":""Global"",""PrimaryParentCategoryId"":""PopCellar"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Category, DesertOctopus.Benchmark"",""TypeName"":""Category"",""PropertyBag"":{""Item_Id"":1,""ItemDiscriminator"":""CATEGORY"",""ParentItem_Id"":52138},""SequenceNumber"":1,""HiddenInScope"":false,""Active"":true,""IncludeInSearch"":true,""Relationships"":[]}],""PrimaryParentCategory"":{""Id"":""BrutRos"",""ChildCategories"":[],""IsSearchable"":true,""DisplayName"":{""Values"":{""en-US"":""Brut Ros""}},""DefinitionName"":""WineType"",""Created"":""\/Date(1432216913000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""RelatedCategories"":[],""RelatedProducts"":[],""CatalogId"":""Global"",""PrimaryParentCategoryId"":""Sparkling"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Category, DesertOctopus.Benchmark"",""TypeName"":""Category"",""PropertyBag"":{""Item_Id"":16,""ItemDiscriminator"":""CATEGORY"",""ParentItem_Id"":9},""SequenceNumber"":0,""HiddenInScope"":false,""Active"":true,""IncludeInSearch"":true,""Relationships"":[]},""PrimaryParentCategoryId"":""BrutRos"",""RelatedCategories"":[],""RelatedProducts"":[],""Variants"":[{""CatalogId"":""Global"",""DefinitionName"":""WineBottleVariant"",""ListPrice"":199.9800,""Id"":""34699Standard"",""Created"":""\/Date(1355220564000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""ProductId"":""34699"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Variant, DesertOctopus.Benchmark"",""TypeName"":""Variant"",""PropertyBag"":{""Item_Id"":27235,""ItemDiscriminator"":""VARIANT"",""ParentItemName"":""34699"",""ParentItem_Id"":4140,""IsOverridden"":false,""IncludeInSearch"":true,""Volume"":""Standard""},""Active"":true,""HiddenInScope"":false,""Prices"":[{""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.ProductPriceEntry, DesertOctopus.Benchmark"",""TypeName"":""ProductPriceEntry"",""PropertyBag"":{},""PriceListId"":""DEFAULT"",""Price"":199.9800,""SequenceNumber"":0,""IsInherited"":false}],""Sku"":""34699Standard""},{""CatalogId"":""Global"",""DefinitionName"":""WineBottleVariant"",""ListPrice"":266.6400,""Id"":""34699Magnum"",""Created"":""\/Date(1355220564000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""ProductId"":""34699"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Variant, DesertOctopus.Benchmark"",""TypeName"":""Variant"",""PropertyBag"":{""Item_Id"":27236,""ItemDiscriminator"":""VARIANT"",""ParentItemName"":""34699"",""ParentItem_Id"":4140,""IsOverridden"":false,""IncludeInSearch"":true,""Volume"":""Magnum""},""Active"":true,""HiddenInScope"":false,""Prices"":[{""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.ProductPriceEntry, DesertOctopus.Benchmark"",""TypeName"":""ProductPriceEntry"",""PropertyBag"":{},""PriceListId"":""DEFAULT"",""Price"":266.6400,""SequenceNumber"":0,""IsInherited"":false}],""Sku"":""34699Magnum""}],""Sku"":""4140"",""Active"":true,""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Product, DesertOctopus.Benchmark"",""TypeName"":""Product"",""PropertyBag"":{""Item_Id"":4140,""ItemDiscriminator"":""PRODUCT"",""MSRP"":18.00000,""DateReviewed"":""\/Date(809827200000)\/"",""Region"":""Sonoma"",""Score"":5,""URLString"":""LYETH Cabernet Blend Alexander Valley A Red Blend 1992 Cabernet Blend Red"",""Wine"":""A Red Blend Alexander Valley"",""WineID"":34699,""Winery"":""Lyeth"",""Year"":1992,""PublicationState"":""Published"",""Alcohol"":22.50000,""SommelierScore"":""Decanter Magazine 9.7 | Wine Advocate 9.3 | Wine Spectator Magazine 9.9"",""CustomerScore"":87.10000,""Pairing"":""Lasagna | MeatballsSpaghetti"",""Caracteristics"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""<p>\n\t&quot;Supple and p<strong>olished cedar, coffee, cherry and berry flav</strong>ors. This i<u>s elegant, finishing with firm tannins and good length. Drinkable now.&quot; -- test</u></p>\n""}},""Appellations"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Alexander Valley|Sonoma""}},""Body"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Elegant|Firm|Firm Tannins|Polished|Supple|Tannins""}},""Country"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""United States""}},""Designation"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Best Buy""}},""Flavors"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Berry|Cedar|Cherry|Coffee""}},""WineType"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Cabernet Blend""}}},""Prices"":[{""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.ProductPriceEntry, DesertOctopus.Benchmark"",""TypeName"":""ProductPriceEntry"",""PropertyBag"":{},""PriceListId"":""DEFAULT"",""Price"":66.6600,""SequenceNumber"":0,""IsInherited"":false,""PriceListType"":""Regular"",""PriceListCategory"":""Regular""}],""Relationships"":[],""SequenceNumber"":0,""HiddenInScope"":false,""IncludeInSearch"":true,""IsOverridden"":false,""TaxCategory"":""Taxable""}";

        public ProductSerializationBenchMark()
        {
            product = ServiceStack.Text.JsonSerializer.DeserializeFromString<Product>(JsonProduct);
            krakenBytes = DesertOctopus.KrakenSerializer.Serialize(product);

            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, product);
                bfBytes = ms.ToArray();
            }

            using (var ms = new MemoryStream())
            {
                omni.SerializeObject(ms, product);
                omniBytes = ms.ToArray();
            }

            DesertOctopus.KrakenSerializer.Deserialize<Product>(krakenBytes);

            ServiceStack.Text.JsonSerializer.DeserializeFromString<Product>(JsonProduct);
        }


        [Benchmark]
        public byte[] JsonSerialization()
        {
            using (var ms = new MemoryStream())
            {
                ServiceStack.Text.JsonSerializer.SerializeToStream(product, ms);
                return ms.ToArray();
            }
        }

        [Benchmark]
        public Product JsonDeserialization()
        {
            return ServiceStack.Text.JsonSerializer.DeserializeFromString<Product>(JsonProduct);
        }

        [Benchmark]
        public byte[] OmniSerialization()
        {
            using (var ms = new MemoryStream())
            {
                omni.SerializeObject(ms, product);
                return ms.ToArray();
            }
        }

        [Benchmark]
        public Product OmniDeserialization()
        {
            using (var ms = new MemoryStream(omniBytes))
            {
                return omni.Deserialize(ms) as Product;
            }
        }

        [Benchmark]
        public byte[] KrakenSerialization()
        {
            return DesertOctopus.KrakenSerializer.Serialize(product);
        }

        [Benchmark]
        public Product KrakenDeserialization()
        {
            return DesertOctopus.KrakenSerializer.Deserialize<Product>(krakenBytes);
        }

        [Benchmark]
        public byte[] BinaryFormatterSerialization()
        {
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, product);
                return ms.ToArray();
            }
        }

        [Benchmark]
        public Product BinaryFormatterDeserialization()
        {
            using (var ms = new MemoryStream(bfBytes))
            {
                return bf.Deserialize(ms) as Product;
            }
        }
    }
}
