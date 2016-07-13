using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using DesertOctopus.Benchmark.Models;
using DesertOctopus.MammothCache.Common;
using DesertOctopus.MammothCache.Redis;

namespace DesertOctopus.MammothCache.WebSiteSimulator.Controllers
{
    public class DummyCache : IFirstLevelCache
    {
        public ConditionalResult<T> Get<T>(string key) where T : class
        {
            return ConditionalResult.CreateFailure<T>();
        }

        public void Remove(string key)
        {
            
        }

        public void RemoveAll()
        {

        }

        public void Set(string key,
                        byte[] serializedValue)
        {

        }

        public void Set(string key,
                        byte[] serializedValue,
                        TimeSpan? ttl)
        {

        }
    }


    public class BenchmarkController : ApiController
    {
        private static readonly RedisConnection _connection;
        private static string _redisConnectionString = "172.16.100.100";
        private static readonly IRedisRetryPolicy _redisRetryPolicy;
        private static readonly IMammothCache _cache;
        private static readonly FirstLevelCacheConfig _config = new FirstLevelCacheConfig();
        private static readonly IFirstLevelCacheCloningProvider _noCloningProvider = new NoCloningProvider();
        private static readonly INonSerializableCache _nonSerializableCache = new NonSerializableCache();


        static BenchmarkController()
        {
            _config.AbsoluteExpiration = TimeSpan.FromSeconds(5);
            _config.MaximumMemorySize = 1000;
            _config.TimerInterval = 1;

            _redisRetryPolicy = new RedisRetryPolicy(50, 100, 150);
            _connection = new RedisConnection(_redisConnectionString, _redisRetryPolicy);

            //var firstLevelCache = new SquirrelCache(_config, _noCloningProvider);
            var firstLevelCache = new DummyCache();
            _cache = new MammothCache(firstLevelCache, _connection, _nonSerializableCache, new MammothCacheSerializationProvider());
        }


        // GET: api/Benchmark
        public async Task<IEnumerable<Product>> Get(int id)
        {
            var random = new Random();
            var r = await GetItems(id, random, true).ConfigureAwait(false);


            if (random.Next(20) > 10)
            {
                for (int i = 0; i < 5; i++)
                {
                    await GetItems(random.Next(500), random, true).ConfigureAwait(false);
                }
            }

            return r;
            //return new string[] { "value1", "value2" };
        }

        private static async Task<IEnumerable<Product>> GetItems(int id, Random random, bool addDelay)
        {
            var r = await _cache.GetOrAddAsync<IEnumerable<Product>>("Item:" + id,
                                                                    () =>
                                                                          {
                                                                              //if (addDelay)
                                                                              //{
                                                                              //    await Task.Delay(TimeSpan.FromMilliseconds(200)).ConfigureAwait(false);
                                                                              //}

                                                                              //List<string> s = new List<string>();

                                                                              //for (int i = 0, max = random.Next(15); i < max; i++)
                                                                              //{
                                                                              //    s.Add("value" + i);
                                                                              //}

                                                                              string json = @"{""CatalogId"":""Global"",""DefinitionName"":""MyDefinitionName"",""DisplayName"":{""Values"":{""en-US"":""Some red wine for testingt""}},""ListPrice"":666.6600,""Id"":""5465456"",""Created"":""\/Date(1355220564000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""Description"":{""Values"":{""en-US"":""“Sirloin chuck spare ribs alcatra venison beef ribs turkey fatback hamburger. Ball tip alcatra shoulder biltong flank. Tail leberkas bresaola kielbasa venison jerky prosciutto chicken meatball ham hock brisket chuck swine. Pig venison chicken tri-tip doner, prosciutto tenderloin jowl ribeye bresaola alcatra kielbasa picanha. Meatball ham hock rump ham jerky pastrami pork ribeye porchetta. Ribeye salami pig strip steak rump flank. Meatloaf turkey porchetta turducken beef shoulder biltong chuck ham hock strip steak pork belly tri-tip meatball. Prosciutto ground round jowl ham hock. Kielbasa bacon sausage tail meatball jerky doner strip steak shoulder alcatra. Corned beef flank meatball capicola, meatloaf andouille kevin pancetta alcatra. Tail boudin frankfurter leberkas. Jowl prosciutto fatback filet mignon pancetta.""}},""ParentCategories"":[{""Id"":""BrutRos"",""ChildCategories"":[],""IsSearchable"":true,""DisplayName"":{""Values"":{""en-US"":""Brut Ros""}},""DefinitionName"":""WineType"",""Created"":""\/Date(1432216913000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""RelatedCategories"":[],""RelatedProducts"":[],""CatalogId"":""Global"",""PrimaryParentCategoryId"":""Sparkling"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Category, DesertOctopus.Benchmark"",""TypeName"":""Category"",""PropertyBag"":{""ItemId"":16,""ItemType"":""Catty!"",""ParentItem_Id"":9},""SequenceNumber"":0,""HiddenInScope"":false,""Active"":true,""IncludeInSearch"":true,""Relationships"":[]},{""Id"":""Red"",""ChildCategories"":[],""IsSearchable"":true,""DisplayName"":{""Values"":{""en-US"":""Red""}},""DefinitionName"":""WineType"",""Created"":""\/Date(1432216913000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""RelatedCategories"":[],""RelatedProducts"":[],""CatalogId"":""Global"",""PrimaryParentCategoryId"":""PopCellar"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Category, DesertOctopus.Benchmark"",""TypeName"":""Category"",""PropertyBag"":{""Item_Id"":1,""ItemDiscriminator"":""CATEGORY"",""ParentItem_Id"":52138},""SequenceNumber"":1,""HiddenInScope"":false,""Active"":true,""IncludeInSearch"":true,""Relationships"":[]}],""PrimaryParentCategory"":{""Id"":""BrutRos"",""ChildCategories"":[],""IsSearchable"":true,""DisplayName"":{""Values"":{""en-US"":""Brut Ros""}},""DefinitionName"":""WineType"",""Created"":""\/Date(1432216913000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""RelatedCategories"":[],""RelatedProducts"":[],""CatalogId"":""Global"",""PrimaryParentCategoryId"":""Sparkling"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Category, DesertOctopus.Benchmark"",""TypeName"":""Category"",""PropertyBag"":{""Item_Id"":16,""ItemDiscriminator"":""CATEGORY"",""ParentItem_Id"":9},""SequenceNumber"":0,""HiddenInScope"":false,""Active"":true,""IncludeInSearch"":true,""Relationships"":[]},""PrimaryParentCategoryId"":""BrutRos"",""RelatedCategories"":[],""RelatedProducts"":[],""Variants"":[{""CatalogId"":""Global"",""DefinitionName"":""WineBottleVariant"",""ListPrice"":199.9800,""Id"":""34699Standard"",""Created"":""\/Date(1355220564000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""ProductId"":""34699"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Variant, DesertOctopus.Benchmark"",""TypeName"":""Variant"",""PropertyBag"":{""Item_Id"":27235,""ItemDiscriminator"":""VARIANT"",""ParentItemName"":""34699"",""ParentItem_Id"":4140,""IsOverridden"":false,""IncludeInSearch"":true,""Volume"":""Standard""},""Active"":true,""HiddenInScope"":false,""Prices"":[{""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.ProductPriceEntry, DesertOctopus.Benchmark"",""TypeName"":""ProductPriceEntry"",""PropertyBag"":{},""PriceListId"":""DEFAULT"",""Price"":199.9800,""SequenceNumber"":0,""IsInherited"":false}],""Sku"":""34699Standard""},{""CatalogId"":""Global"",""DefinitionName"":""WineBottleVariant"",""ListPrice"":266.6400,""Id"":""34699Magnum"",""Created"":""\/Date(1355220564000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""ProductId"":""34699"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Variant, DesertOctopus.Benchmark"",""TypeName"":""Variant"",""PropertyBag"":{""Item_Id"":27236,""ItemDiscriminator"":""VARIANT"",""ParentItemName"":""34699"",""ParentItem_Id"":4140,""IsOverridden"":false,""IncludeInSearch"":true,""Volume"":""Magnum""},""Active"":true,""HiddenInScope"":false,""Prices"":[{""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.ProductPriceEntry, DesertOctopus.Benchmark"",""TypeName"":""ProductPriceEntry"",""PropertyBag"":{},""PriceListId"":""DEFAULT"",""Price"":266.6400,""SequenceNumber"":0,""IsInherited"":false}],""Sku"":""34699Magnum""}],""Sku"":""4140"",""Active"":true,""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Product, DesertOctopus.Benchmark"",""TypeName"":""Product"",""PropertyBag"":{""Item_Id"":4140,""ItemDiscriminator"":""PRODUCT"",""MSRP"":18.00000,""DateReviewed"":""\/Date(809827200000)\/"",""Region"":""Sonoma"",""Score"":5,""URLString"":""LYETH Cabernet Blend Alexander Valley A Red Blend 1992 Cabernet Blend Red"",""Wine"":""A Red Blend Alexander Valley"",""WineID"":34699,""Winery"":""Lyeth"",""Year"":1992,""PublicationState"":""Published"",""Alcohol"":22.50000,""SommelierScore"":""Decanter Magazine 9.7 | Wine Advocate 9.3 | Wine Spectator Magazine 9.9"",""CustomerScore"":87.10000,""Pairing"":""Lasagna | MeatballsSpaghetti"",""Caracteristics"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""<p>\n\t&quot;Supple and p<strong>olished cedar, coffee, cherry and berry flav</strong>ors. This i<u>s elegant, finishing with firm tannins and good length. Drinkable now.&quot; -- test</u></p>\n""}},""Appellations"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Alexander Valley|Sonoma""}},""Body"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Elegant|Firm|Firm Tannins|Polished|Supple|Tannins""}},""Country"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""United States""}},""Designation"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Best Buy""}},""Flavors"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Berry|Cedar|Cherry|Coffee""}},""WineType"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Cabernet Blend""}}},""Prices"":[{""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.ProductPriceEntry, DesertOctopus.Benchmark"",""TypeName"":""ProductPriceEntry"",""PropertyBag"":{},""PriceListId"":""DEFAULT"",""Price"":66.6600,""SequenceNumber"":0,""IsInherited"":false,""PriceListType"":""Regular"",""PriceListCategory"":""Regular""}],""Relationships"":[],""SequenceNumber"":0,""HiddenInScope"":false,""IncludeInSearch"":true,""IsOverridden"":false,""TaxCategory"":""Taxable""}";
                                                                              var product = ServiceStack.Text.JsonSerializer.DeserializeFromString<Product>(json);



                                                                              return Task.FromResult(new [] { product } as IEnumerable<Product>);
                                                                          },
                                                                    ttl: TimeSpan.FromMinutes(20))
                                .ConfigureAwait(false);
            return r;
        }

        //// GET: api/Benchmark/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST: api/Benchmark
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT: api/Benchmark/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE: api/Benchmark/5
        //public void Delete(int id)
        //{
        //}
    }
}
