using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using DesertOctopus.Cloning;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerializerTests.TestObjects;

namespace DesertOctopus.Tests
{
    [TestClass]
    public class ClonerTest : BaseDuplicationTest
    {
        public override T Duplicate<T>(T obj)
        {
            return ObjectCloner.Clone(obj);
        }

        [TestMethod]
        [TestCategory("Unit")]
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

                                 Assert.AreEqual(instance.Items.Count(),
                                                 clonedValue.Items.Count());
                                 CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                                             clonedValue.Items.ToList());
                             });
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CloneIQueryableDirectly()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            IQueryable<ClassWithoutSerializableAttribute> instance = list.AsQueryable();

            var clonedValue = Duplicate(instance);

            Assert.IsNotNull(clonedValue);
            Assert.IsNotNull(clonedValue);
            var deserializedArray = clonedValue.ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, deserializedArray[0].PublicPropertyValue);
            Assert.IsNull(deserializedArray[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, deserializedArray[2].PublicPropertyValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
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

            Assert.AreEqual(3,
                            duplicatedList.Count);
            dynamic duplicatedValue = duplicatedList[0];

            Assert.AreEqual(eo.Property1,
                            duplicatedValue.Property1);
            Assert.AreEqual(eo.Property2,
                            duplicatedValue.Property2);
            Assert.AreEqual(eo.Property3,
                            duplicatedValue.Property3);
            Assert.IsTrue(duplicatedValue.Property4.GetType() == typeof(object));
            Assert.IsNull(duplicatedValue.Property5);
            Assert.IsTrue(ReferenceEquals(duplicatedList[0],
                                          duplicatedList[2]));

            var duplicatedValueProperty6 = duplicatedValue.Property6;
            Assert.IsTrue(duplicatedValueProperty6 is IQueryable);
            Assert.IsTrue(((object) duplicatedValueProperty6).GetType()
                                                             .IsGenericType);
            Assert.IsTrue(IQueryableCloner.IsGenericIQueryableType(((object) duplicatedValueProperty6).GetType()));

            var duplicatedValueProperty6AsArray = System.Linq.Enumerable.ToArray((IQueryable<ClassWithoutSerializableAttribute>) duplicatedValueProperty6);

            Assert.AreEqual(list[0].PublicPropertyValue,
                            duplicatedValueProperty6AsArray[0].PublicPropertyValue);
            Assert.IsNull(duplicatedValueProperty6AsArray[1]);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void QueryableClonerConvertToNonGenericQueryableShouldReturnNull()
        {
            Assert.IsNull(IQueryableCloner.ConvertToNonGenericQueryable(null));
        }

        //[TestMethod]
        //public void z_AdditionalTestsToImplements()
        //{

        //    Assert.Fail();
        //}
    }
}
