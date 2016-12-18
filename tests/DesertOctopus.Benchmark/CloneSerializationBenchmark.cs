using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using DesertOctopus.Benchmark.Models;
using DesertOctopus.MammothCache;
using DesertOctopus.MammothCache.Common;
using DesertOctopus.MammothCache.Redis;
using DesertOctopus.Serialization;
using DesertOctopus.Utilities;
using Force.DeepCloner;
using GeorgeCloney;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClone;
using NetSerializer;
using Serializer = NetSerializer.Serializer;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable UnusedVariable

namespace DesertOctopus.Benchmark
{
    [TestClass]
    public class CloneSerializationBenchmark
    {
        public CloneSerializationBenchmark()
        {
//#if DEBUG
//            throw new Exception("Use release mode");
//#endif
        }



        [TestMethod]
        [TestCategory("Benchmark")]
        public void ProfileCloning()
        {
            string json = @"{""CatalogId"":""Global"",""DefinitionName"":""MyDefinitionName"",""DisplayName"":{""Values"":{""en-US"":""Some red wine for testingt""}},""ListPrice"":666.6600,""Id"":""5465456"",""Created"":""\/Date(1355220564000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""Description"":{""Values"":{""en-US"":""“Sirloin chuck spare ribs alcatra venison beef ribs turkey fatback hamburger. Ball tip alcatra shoulder biltong flank. Tail leberkas bresaola kielbasa venison jerky prosciutto chicken meatball ham hock brisket chuck swine. Pig venison chicken tri-tip doner, prosciutto tenderloin jowl ribeye bresaola alcatra kielbasa picanha. Meatball ham hock rump ham jerky pastrami pork ribeye porchetta. Ribeye salami pig strip steak rump flank. Meatloaf turkey porchetta turducken beef shoulder biltong chuck ham hock strip steak pork belly tri-tip meatball. Prosciutto ground round jowl ham hock. Kielbasa bacon sausage tail meatball jerky doner strip steak shoulder alcatra. Corned beef flank meatball capicola, meatloaf andouille kevin pancetta alcatra. Tail boudin frankfurter leberkas. Jowl prosciutto fatback filet mignon pancetta.""}},""ParentCategories"":[{""Id"":""BrutRos"",""ChildCategories"":[],""IsSearchable"":true,""DisplayName"":{""Values"":{""en-US"":""Brut Ros""}},""DefinitionName"":""WineType"",""Created"":""\/Date(1432216913000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""RelatedCategories"":[],""CatalogId"":""Global"",""PrimaryParentCategoryId"":""Sparkling"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Category, DesertOctopus.Benchmark"",""TypeName"":""Category"",""PropertyBag"":{""ItemId"":16,""ItemType"":""Catty!"",""ParentItem_Id"":9},""SequenceNumber"":0,""HiddenInScope"":false,""Active"":true,""IncludeInSearch"":true,""Relationships"":[]},{""Id"":""Red"",""ChildCategories"":[],""IsSearchable"":true,""DisplayName"":{""Values"":{""en-US"":""Red""}},""DefinitionName"":""WineType"",""Created"":""\/Date(1432216913000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""RelatedCategories"":[],""RelatedProducts"":[],""CatalogId"":""Global"",""PrimaryParentCategoryId"":""PopCellar"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Category, DesertOctopus.Benchmark"",""TypeName"":""Category"",""PropertyBag"":{""Item_Id"":1,""ItemDiscriminator"":""CATEGORY"",""ParentItem_Id"":52138},""SequenceNumber"":1,""HiddenInScope"":false,""Active"":true,""IncludeInSearch"":true,""Relationships"":[]}],""PrimaryParentCategory"":{""Id"":""BrutRos"",""ChildCategories"":[],""IsSearchable"":true,""DisplayName"":{""Values"":{""en-US"":""Brut Ros""}},""DefinitionName"":""WineType"",""Created"":""\/Date(1432216913000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""RelatedCategories"":[],""RelatedProducts"":[],""CatalogId"":""Global"",""PrimaryParentCategoryId"":""Sparkling"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Category, DesertOctopus.Benchmark"",""TypeName"":""Category"",""PropertyBag"":{""Item_Id"":16,""ItemDiscriminator"":""CATEGORY"",""ParentItem_Id"":9},""SequenceNumber"":0,""HiddenInScope"":false,""Active"":true,""IncludeInSearch"":true,""Relationships"":[]},""PrimaryParentCategoryId"":""BrutRos"",""RelatedCategories"":[],""RelatedProducts"":[],""Variants"":[{""CatalogId"":""Global"",""DefinitionName"":""WineBottleVariant"",""ListPrice"":199.9800,""Id"":""34699Standard"",""Created"":""\/Date(1355220564000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""ProductId"":""34699"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Variant, DesertOctopus.Benchmark"",""TypeName"":""Variant"",""PropertyBag"":{""Item_Id"":27235,""ItemDiscriminator"":""VARIANT"",""ParentItemName"":""34699"",""ParentItem_Id"":4140,""IsOverridden"":false,""IncludeInSearch"":true,""Volume"":""Standard""},""Active"":true,""HiddenInScope"":false,""Prices"":[{""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.ProductPriceEntry, DesertOctopus.Benchmark"",""TypeName"":""ProductPriceEntry"",""PropertyBag"":{},""PriceListId"":""DEFAULT"",""Price"":199.9800,""SequenceNumber"":0,""IsInherited"":false}],""Sku"":""34699Standard""},{""CatalogId"":""Global"",""DefinitionName"":""WineBottleVariant"",""ListPrice"":266.6400,""Id"":""34699Magnum"",""Created"":""\/Date(1355220564000)\/"",""LastModified"":""\/Date(1432231952000)\/"",""LastModifiedBy"":""Import Process"",""ProductId"":""34699"",""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Variant, DesertOctopus.Benchmark"",""TypeName"":""Variant"",""PropertyBag"":{""Item_Id"":27236,""ItemDiscriminator"":""VARIANT"",""ParentItemName"":""34699"",""ParentItem_Id"":4140,""IsOverridden"":false,""IncludeInSearch"":true,""Volume"":""Magnum""},""Active"":true,""HiddenInScope"":false,""Prices"":[{""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.ProductPriceEntry, DesertOctopus.Benchmark"",""TypeName"":""ProductPriceEntry"",""PropertyBag"":{},""PriceListId"":""DEFAULT"",""Price"":266.6400,""SequenceNumber"":0,""IsInherited"":false}],""Sku"":""34699Magnum""}],""Sku"":""4140"",""Active"":true,""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.Product, DesertOctopus.Benchmark"",""TypeName"":""Product"",""PropertyBag"":{""Item_Id"":4140,""ItemDiscriminator"":""PRODUCT"",""MSRP"":18.00000,""DateReviewed"":""\/Date(809827200000)\/"",""Region"":""Sonoma"",""Score"":5,""URLString"":""LYETH Cabernet Blend Alexander Valley A Red Blend 1992 Cabernet Blend Red"",""Wine"":""A Red Blend Alexander Valley"",""WineID"":34699,""Winery"":""Lyeth"",""Year"":1992,""PublicationState"":""Published"",""Alcohol"":22.50000,""SommelierScore"":""Decanter Magazine 9.7 | Wine Advocate 9.3 | Wine Spectator Magazine 9.9"",""CustomerScore"":87.10000,""Pairing"":""Lasagna | MeatballsSpaghetti"",""Caracteristics"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""<p>\n\t&quot;Supple and p<strong>olished cedar, coffee, cherry and berry flav</strong>ors. This i<u>s elegant, finishing with firm tannins and good length. Drinkable now.&quot; -- test</u></p>\n""}},""Appellations"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Alexander Valley|Sonoma""}},""Body"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Elegant|Firm|Firm Tannins|Polished|Supple|Tannins""}},""Country"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""United States""}},""Designation"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Best Buy""}},""Flavors"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Berry|Cedar|Cherry|Coffee""}},""WineType"":{""__type"":""DesertOctopus.Benchmark.Models.Localizable`1[[System.String, mscorlib]], DesertOctopus.Benchmark"",""Values"":{""en-US"":""Cabernet Blend""}}},""Prices"":[{""FullTypeName"":""DesertOctopus.Benchmark.Models.Products.ProductPriceEntry, DesertOctopus.Benchmark"",""TypeName"":""ProductPriceEntry"",""PropertyBag"":{},""PriceListId"":""DEFAULT"",""Price"":66.6600,""SequenceNumber"":0,""IsInherited"":false,""PriceListType"":""Regular"",""PriceListCategory"":""Regular""}],""Relationships"":[],""SequenceNumber"":0,""HiddenInScope"":false,""IncludeInSearch"":true,""IsOverridden"":false,""TaxCategory"":""Taxable""}";
            var root = ServiceStack.Text.JsonSerializer.DeserializeFromString<Product>(json);
            DesertOctopus.ObjectCloner.Clone(root);

            Stopwatch sw = Stopwatch.StartNew();

            for (int i = 0; i < 100000; i++)
            {
                DesertOctopus.ObjectCloner.Clone(root);
            }

            sw.Stop();
            Assert.Fail(sw.Elapsed.ToString());
        }

        [TestMethod]
        [TestCategory("Benchmark")]
        public void BenchmarkRootObjectCloningBenchmark()
        {
            var summary = BenchmarkRunner.Run<RootObjectCloningBenchMark>();

            var k = BenchmarkDotNet.Exporters.HtmlExporter.Default.ExportToFiles(summary);
            Console.WriteLine(k.First());

            foreach (var validationError in summary.ValidationErrors)
            {
                Console.WriteLine(validationError.Message);
            }

            Assert.Fail(k.First());
        }

        [TestMethod]
        [TestCategory("Benchmark")]
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
        [TestCategory("Benchmark")]
        public void CloneBenchmarks()
        {
#if DEBUG
            throw new Exception("Use release mode");
#endif

            var benchmarks = new Dictionary<string, Type>();
            benchmarks.Add("This benchmark clones a fairly large object containing array, lists and dictionaries.  ObjectCloner is slow here because it uses the serialization constructor to clone the dictionaries.", typeof(RootObjectCloningBenchMark));
            benchmarks.Add("This benchmark clone a normal sized object that contains all primitives types (30 properties).", typeof(SimpleDtoWithEveryPrimitivesCloningBenchmark));


            var sb = new StringBuilder();

            foreach (var kvp in benchmarks)
            {
                using (var ms = new MemoryStream())
                {
                    using (var sw = new StreamWriter(ms))
                    {
                        sb.AppendLine();
                        sb.AppendLine(kvp.Key);
                        sb.AppendLine();

                        var logger = new StreamLogger(sw);
                        var summary = BenchmarkRunner.Run(kvp.Value);

                        if (summary.ValidationErrors.Length > 0)
                        {
                            foreach (var validationError in summary.ValidationErrors)
                            {
                                Console.WriteLine(validationError.Message);
                            }
                            Assert.Fail(kvp.Key);
                        }

                        BenchmarkDotNet.Exporters.MarkdownExporter.GitHub.ExportToLog(summary, logger);
                        sw.Flush();


                        sb.AppendLine(System.Text.Encoding.UTF8.GetString(ms.ToArray()));

                    }
                }
            }

            // ### Benchmark

            var file = @"..\..\..\..\Docs\ObjectCloner.md";
            var fileContent = System.IO.File.ReadAllText(file);
            var marker = "### Benchmark";
            var pos = fileContent.IndexOf(marker, StringComparison.InvariantCultureIgnoreCase);
            if (pos == -1)
            {
                Assert.Fail("Could not find position of " + marker);
            }

            fileContent = fileContent.Substring(0, pos + marker.Length);
            fileContent += "\r\n\r\n" + sb.ToString();

            System.IO.File.WriteAllText(file, fileContent, Encoding.UTF8);
        }
    }



    [Config(typeof(CloneBenchmarkConfiguration))]
    public class CloneBenchmarkBase
    {
        protected class CloneBenchmarkConfiguration : ManualConfig
        {
            public CloneBenchmarkConfiguration()
            {
                Add(new MemoryDiagnoser());
                Add(new InliningDiagnoser());
            }
        }
    }

    public class CloneBenchmarkBase<T> : CloneBenchmarkBase
        where T : class
    {
        private readonly object _objectToTest;


        public CloneBenchmarkBase(object objectToTest)
        {
            _objectToTest = objectToTest;
        }

        [Benchmark]
        public object DesertOctopusObjectCloner()
        {
            return ObjectCloner.Clone(_objectToTest);
        }

        [Benchmark]
        public object GeorgeCloney()
        {
            return CloneExtension.DeepClone(_objectToTest);
        }

        [Benchmark]
        public object NClone()
        {
            return Clone.ObjectGraph(_objectToTest);
        }

        [Benchmark]
        public object DeepCloner()
        {
            return DeepClonerExtensions.DeepClone(_objectToTest);
        }
    }

    public class RootObjectCloningBenchMark : CloneBenchmarkBase<Product>
    {
        public RootObjectCloningBenchMark()
            : base(ServiceStack.Text.JsonSerializer.DeserializeFromString<Product>(ProductSerializationBenchmark.JsonProduct))
        {
        }
    }

    public class SimpleDtoWithEveryPrimitivesCloningBenchmark : CloneBenchmarkBase<ClassWithAllPrimitiveTypes>
    {
        public SimpleDtoWithEveryPrimitivesCloningBenchmark()
            : base(new ClassWithAllPrimitiveTypes())
        {
        }
    }
}
