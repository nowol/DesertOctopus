using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Benchmark.Models
{
    [Serializable]
    public sealed class BenchmarkObject2<TLocalizedType>
        where TLocalizedType : Dictionary<string, string>, new()
    {
        public BenchmarkObject2()
        {
            Children = new List<BenchmarkObject2<TLocalizedType>>();
            DisplayName = new TLocalizedType();
        }

        public string Id { get; set; }

        public ICollection<BenchmarkObject2<TLocalizedType>> Children { get; set; }
        public TLocalizedType DisplayName { get; set; }
        public string DefinitionName { get; set; }
        public TLocalizedType Description { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public string LastModifiedBy { get; set; }
        public bool Active { get; set; }
        public string CatalogId { get; set; }
        public string PrimaryId { get; set; }
        public object Ints { get; set; }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static BenchmarkObject2<TLocalizedType> GetRandomObject(int numberOfChildren)
        {
            var obj = new BenchmarkObject2<TLocalizedType>();


            obj.Id = RandomString(10);

            obj.Children = new List<BenchmarkObject2<TLocalizedType>>();
            for (int i = 0; i < numberOfChildren; i++)
            {
                obj.Children.Add(GetRandomObject(numberOfChildren - 1));
            }

            obj.DisplayName = new TLocalizedType { { "en-US", RandomString(20) }, { "en-CA", RandomString(20) }, { "fr-CA", RandomString(20) } };
            obj.DefinitionName = RandomString(256);
            obj.Description = new TLocalizedType { { "en-US", RandomString(20) }, { "en-CA", RandomString(50) }, { "fr-CA", RandomString(20) } };
            obj.Created = DateTime.UtcNow;
            obj.CreatedBy = RandomString(10);
            obj.LastModified = DateTime.UtcNow;
            obj.LastModifiedBy = RandomString(10);
            obj.Active = true;
            obj.CatalogId = RandomString(10);
            obj.PrimaryId = RandomString(10);
            obj.Ints = Enumerable.Range(0, 200).ToList();

            return obj;
        }
    }

    [Serializable]
    public class BenchmarkObject<TBagType, TLocalizedType>
        where TBagType : Dictionary<string, object>, new()
        where TLocalizedType : Dictionary<string, string>, new()
    {
        public Guid Id { get; set; }
        public string Text1 { get; set; }
        public string Text2 { get; set; }
        public string Text3 { get; set; }
        public string Description { get; set; }
        public decimal? Price1 { get; set; }
        public decimal? Price2 { get; set; }
        public decimal Price3 { get; set; }
        public decimal Price4 { get; set; }
        public DateTime Date1 { get; set; }
        public DateTime? Date2 { get; set; }
        public DateTime? Date3 { get; set; }
        public TBagType Bag { get; set; }
        public TLocalizedType Localized { get; set; }
        public ICollection<BenchmarkObject2<TLocalizedType>> ParentObjects { get; set; }
        public object ChildrenObjects { get; set; }


        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        protected static void InitializeObject(BenchmarkObject<TBagType, TLocalizedType> obj)
        {
            obj.Id = Guid.NewGuid();
            obj.Text1 = RandomString(20);
            obj.Text2 = null;
            obj.Text3 = RandomString(10);
            obj.Description = RandomString(256);
            obj.Price1 = Decimal.MaxValue;
            obj.Price2 = null;
            obj.Price3 = 654M;
            obj.Price4 = Decimal.MinValue;
            obj.Date1 = DateTime.UtcNow;
            obj.Date2 = null;
            obj.Date3 = DateTime.UtcNow;

            obj.Bag = new TBagType();
            obj.Bag.Add("abcdefg", 128323.23M);
            obj.Bag.Add("abcdefgh", new TLocalizedType { { "en-US", RandomString(15) }, { "en-CA", RandomString(15) }, { "fr-CA", RandomString(15) } });
            obj.Bag.Add("abcdefghi", "alksjdfklasjdf;lakjsd");
            obj.Bag.Add("abcdef", DateTime.UtcNow);
            obj.Bag.Add("abcde", 19278);
            obj.Bag.Add("abcd", 2984L);

            obj.Localized = new TLocalizedType { { "en-US", RandomString(15) }, { "en-CA", RandomString(15) }, { "fr-CA", RandomString(15) } };

            obj.ParentObjects = new List<BenchmarkObject2<TLocalizedType>>();
            for (int i = 0; i < 4; i++)
            {
                obj.ParentObjects.Add(BenchmarkObject2<TLocalizedType>.GetRandomObject(i));
            }

            var list = new List<BenchmarkObject2<TLocalizedType>>();
            for (int i = 0; i < 1; i++)
            {
                list.Add(BenchmarkObject2<TLocalizedType>.GetRandomObject(i));
            }
            obj.ChildrenObjects = list;
        }
    }

    [Serializable]
    public class BenchmarkObjectNormalDictionary : BenchmarkObject<Dictionary<string, object>, Dictionary<string, string>>
    {
        public static BenchmarkObjectNormalDictionary GetNewInitialized()
        {
            var obj = new BenchmarkObjectNormalDictionary();
            InitializeObject(obj);
            return obj;
        }
    }

    [Serializable]
    public class BenchmarkObjectNonISerializablerDictionary : BenchmarkObject<NonISerializableDictionary<string, object>, Dictionary<string, string>>
    {
        public static BenchmarkObjectNonISerializablerDictionary GetNewInitialized()
        {
            var obj = new BenchmarkObjectNonISerializablerDictionary();
            InitializeObject(obj);
            return obj;
        }
    }

    public class NonISerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public NonISerializableDictionary()
        {
        }
    }
}
