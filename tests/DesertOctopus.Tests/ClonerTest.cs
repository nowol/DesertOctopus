using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using DesertOctopus.Cloning;
using SerializerTests.TestObjects;
using Xunit;
using Xunit.Abstractions;

namespace DesertOctopus.Tests
{
    public class ClonerTest : BaseDuplicationTest
    {
        public ClonerTest(ITestOutputHelper output)
            : base(output)
        {
        }

        public override T Duplicate<T>(T obj)
        {
            return ObjectCloner.Clone(obj);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CloneInParallel()
        {
            ObjectCloner.ClearTypeCache(); // empty the Type cache to start from a fresh state.

            for (int i = 0; i < 100; i++)
            {
                Parallel.For(0,
                             1000,
                             k =>
                             {
                                 var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 1, 2, 3 }.Distinct() };

                                 var clonedValue = Duplicate(instance);

                                 Assert.Equal(instance.Items.Count(),
                                                 clonedValue.Items.Count());
                                 Assert.Equal(instance.Items.ToList(),
                                              clonedValue.Items.ToList());
                             });
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void CloneIQueryableDirectly()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            IQueryable<ClassWithoutSerializableAttribute> instance = list.AsQueryable();

            var clonedValue = Duplicate(instance);

            Assert.NotNull(clonedValue);
            Assert.NotNull(clonedValue);
            var deserializedArray = clonedValue.ToArray();

            Assert.Equal(list[0].PublicPropertyValue, deserializedArray[0].PublicPropertyValue);
            Assert.Null(deserializedArray[1]);
            Assert.Equal(list[2].PublicPropertyValue, deserializedArray[2].PublicPropertyValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateIQueryableInsideExpandoObject()
        {
            var list = new List<ClassWithoutSerializableAttribute>
                       {
                           new ClassWithoutSerializableAttribute
                           {
                               PublicPropertyValue = 123
                           },
                           null,
                           new ClassWithoutSerializableAttribute
                           {
                               PublicPropertyValue = 456
                           }
                       };
            dynamic eo = new ExpandoObject();
            eo.Property1 = 123;
            eo.Property2 = "abc";
            eo.Property3 = new ClassWithGenericInt(349);
            eo.Property4 = new object();
            eo.Property5 = null;
            eo.Property6 = list.AsQueryable();

            var instance = new List<ExpandoObject>
                           {
                               eo,
                               null,
                               eo
                           };
            var duplicatedList = Duplicate(instance);

            Assert.Equal(3,
                            duplicatedList.Count);
            dynamic duplicatedValue = duplicatedList[0];

            Assert.Equal(eo.Property1,
                            duplicatedValue.Property1);
            Assert.Equal(eo.Property2,
                            duplicatedValue.Property2);
            Assert.Equal(eo.Property3,
                            duplicatedValue.Property3);
            Assert.True(duplicatedValue.Property4.GetType() == typeof(object));
            Assert.Null(duplicatedValue.Property5);
            Assert.True(ReferenceEquals(duplicatedList[0],
                                          duplicatedList[2]));

            var duplicatedValueProperty6 = duplicatedValue.Property6;
            Assert.True(duplicatedValueProperty6 is IQueryable);
            Assert.True(((object)duplicatedValueProperty6).GetType()
                                                          .IsGenericType);
            Assert.True(IQueryableCloner.IsGenericIQueryableType(((object) duplicatedValueProperty6).GetType()));

            var duplicatedValueProperty6AsArray = System.Linq.Enumerable.ToArray((IQueryable<ClassWithoutSerializableAttribute>) duplicatedValueProperty6);

            Assert.Equal(list[0].PublicPropertyValue,
                            duplicatedValueProperty6AsArray[0].PublicPropertyValue);
            Assert.Null(duplicatedValueProperty6AsArray[1]);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void QueryableClonerConvertToNonGenericQueryableShouldReturnNull()
        {
            Assert.Null(IQueryableCloner.ConvertToNonGenericQueryable(null));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void QueryableClonerConvertToNonGenericQueryableShouldNotModifyAClass()
        {
            var instance = new ClassWithGenericInt() { Value = 44 };
            Assert.True(ReferenceEquals(instance, IQueryableCloner.ConvertToNonGenericQueryable(instance)));
        }

        //[Fact]
        //public void z_AdditionalTestsToImplements()
        //{

        //    Assert.Fail();
        //}
    }
}
