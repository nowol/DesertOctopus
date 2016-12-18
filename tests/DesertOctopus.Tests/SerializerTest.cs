using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DesertOctopus.Exceptions;
using DesertOctopus.Serialization;
using DesertOctopus.Tests.TestObjects;
using DesertOctopus.Utilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerializerTests.TestObjects;

namespace DesertOctopus.Tests
{
    [TestClass]
    public class SerializerTest : BaseDuplicationTest
    {
        public override T Duplicate<T>(T obj)
        {
            var bytes = Serializer.Serialize(obj);
            return Deserializer.Deserialize<T>(bytes);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(TypeNotFoundException))]
        public void DeserializeUnknownType()
        {
            ClassWithDynamicProperty instance = new ClassWithDynamicProperty { Value = 123 };
            var bytes = Serializer.Serialize(instance);
            bytes[25] = 54;

            Deserializer.Deserialize<ClassWithDynamicProperty>(bytes);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(TypeWasModifiedSinceSerializationException))]
        public void DeserializeTypeThatWasModified()
        {
            ClassWithDynamicProperty instance = new ClassWithDynamicProperty { Value = 123 };
            var bytes = Serializer.Serialize(instance);

            var needle = SerializedTypeResolver.GetShortNameFromType(typeof(ClassWithDynamicProperty));
            var index = System.Text.Encoding.UTF8.GetString(bytes).IndexOf(needle);

            // this is a hackish way to change the hashcode of a serialized object
            // if the way/order (currently TypeName + Hash) that an object is serialized changes the line below will need to be modified to target a byte of the hashcode
            bytes[index + needle.Length + 1] = (bytes[index + needle.Length + 1] == 255) ? SerializerObjectTracker.Value0 : (byte)(bytes[index + needle.Length] + 1); // change the hashcode to something invalid

            Deserializer.Deserialize<ClassWithDynamicProperty>(bytes);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void SerializeInParallel()
        {
            Serializer.ClearTypeSerializersCache(); // empty the serialization Type to TypeData dictionary to start from a fresh state.
            Deserializer.ClearTypeDeserializersCache(); // empty the serialization Type to TypeData dictionary to start from a fresh state.

            for (int i = 0; i < 100; i++)
            {
                Parallel.For(0,
                             1000,
                             k =>
                             {
                                 var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 1, 2, 3 }.Distinct() };

                                 var bytes = Serializer.Serialize(instance);
                                 var deserializedValue = Deserializer.Deserialize<ClassWithIEnumerable<int>>(bytes);

                                 Assert.AreEqual(instance.Items.Count(),
                                                 deserializedValue.Items.Count());
                                 CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                                             deserializedValue.Items.ToList());
                             });
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(NotSupportedException))]
        public void SerializeGroupByContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IEnumerable<IGrouping<int, ClassWithoutSerializableAttribute>>> { Value = list.GroupBy(x => x.PublicPropertyValue) };
            Serializer.Serialize(instance);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(NotSupportedException))]
        public void SerializeIQueryableDirectly()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            IQueryable<ClassWithoutSerializableAttribute> instance = list.AsQueryable();

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<IQueryable<ClassWithoutSerializableAttribute>>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.IsNotNull(deserializedValue);
            var deserializedArray = deserializedValue.ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, deserializedArray[0].PublicPropertyValue);
            Assert.IsNull(deserializedArray[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, deserializedArray[2].PublicPropertyValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void KrakenSerializerShouldWork()
        {
            var instance = new GenericBaseClass<IQueryable> { Value = null };

            var bytes = KrakenSerializer.Serialize(instance);
            var deserializedValue = KrakenSerializer.Deserialize<GenericBaseClass<IQueryable>>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.IsNull(deserializedValue.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void StringShouldNotBeSerializedTwiceInsideAList()
        {
            var str = RandomString(1000);
            var instance = new List<string> { str };
            var bytes = KrakenSerializer.Serialize(instance);
            var deserializedValue = KrakenSerializer.Deserialize<List<string>>(bytes);
            CollectionAssert.AreEqual(instance, deserializedValue);

            instance = new List<string> { str, str };
            var bytesTwice = KrakenSerializer.Serialize(instance);
            deserializedValue = KrakenSerializer.Deserialize<List<string>>(bytesTwice);
            CollectionAssert.AreEqual(instance, deserializedValue);

            Assert.IsTrue(bytes.Length + 500 > bytesTwice.Length);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void StringShouldNotBeSerializedTwice2Properties()
        {
            var str = RandomString(1000);
            var instance1 = new ClassWith2Property<string>(str, null);
            var instance2 = new ClassWith2Property<string>(str, str);
            var bytes = KrakenSerializer.Serialize(instance1);
            var deserializedValue = KrakenSerializer.Deserialize<ClassWith2Property<string>>(bytes);
            instance1.ShouldBeEquivalentTo(deserializedValue);
            
            var bytesTwice = KrakenSerializer.Serialize(instance2);
            deserializedValue = KrakenSerializer.Deserialize<ClassWith2Property<string>>(bytesTwice);
            instance2.ShouldBeEquivalentTo(deserializedValue);

            Assert.IsTrue(bytes.Length + 500 > bytesTwice.Length);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void StringShouldNotBeSerializedTwiceAsADictionaryValue()
        {
            var str = RandomString(1000);
            var instance1 = new Dictionary<int, string> { { 123, str } };
            var instance2 = new Dictionary<int, string> { { 123, str }, { 1235, str } };
            var bytes = KrakenSerializer.Serialize(instance1);
            var deserializedValue = KrakenSerializer.Deserialize<Dictionary<int, string>>(bytes);
            instance1.ShouldBeEquivalentTo(deserializedValue);
            
            var bytesTwice = KrakenSerializer.Serialize(instance2);
            deserializedValue = KrakenSerializer.Deserialize<Dictionary<int, string>>(bytesTwice);
            instance2.ShouldBeEquivalentTo(deserializedValue);

            Assert.IsTrue(bytes.Length + 500 > bytesTwice.Length);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void StringShouldNotBeSerializedTwiceAsType()
        {
            var instance1 = new ClassWithGenericInt { Value = 123 };
            var instance2 = new ClassWithGenericInt { Value = 321 };

            var instance = new List<object> { instance1 };
            var bytes = KrakenSerializer.Serialize(instance);
            var deserializedValue = KrakenSerializer.Deserialize<List<object>>(bytes);
            CollectionAssert.AreEqual(instance, deserializedValue);

            instance = new List<object> { instance1, instance2 };
            var bytesTwice = KrakenSerializer.Serialize(instance);
            deserializedValue = KrakenSerializer.Deserialize<List<object>>(bytesTwice);
            CollectionAssert.AreEqual(instance, deserializedValue);

            Assert.IsTrue(bytes.Length + 500 > bytesTwice.Length);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void SerializeClassWithAllPrimitiveTypes()
        {
            var instance = new ClassWithAllPrimitiveTypes();
            var bytes = KrakenSerializer.Serialize(instance);
            var deserializedValue = KrakenSerializer.Deserialize<ClassWithAllPrimitiveTypes>(bytes);
            instance.ShouldBeEquivalentTo(deserializedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateIQueryableInsideExpandoObject()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            dynamic eo = new ExpandoObject();
            eo.Property1 = 123;
            eo.Property2 = "abc";
            eo.Property3 = new ClassWithGenericInt(349);
            eo.Property4 = new object();
            eo.Property5 = null;
            eo.Property6 = list.AsQueryable();

            var instance = new List<ExpandoObject> { eo, null, eo };
            var duplicatedList = Duplicate(instance);

            Assert.AreEqual(3, duplicatedList.Count);
            dynamic duplicatedValue = duplicatedList[0];

            Assert.AreEqual(eo.Property1, duplicatedValue.Property1);
            Assert.AreEqual(eo.Property2, duplicatedValue.Property2);
            Assert.AreEqual(eo.Property3, duplicatedValue.Property3);
            Assert.IsTrue(duplicatedValue.Property4.GetType() == typeof(object));
            Assert.IsNull(duplicatedValue.Property5);
            Assert.IsTrue(ReferenceEquals(duplicatedList[0], duplicatedList[2]));

            var duplicatedValueProperty6 = duplicatedValue.Property6;
            Assert.IsTrue(duplicatedValueProperty6 is Array); // side effect of mixing ExpandoObject and IQueryable

            var duplicatedValueProperty6AsArray = ((ClassWithoutSerializableAttribute[])duplicatedValueProperty6);

            Assert.AreEqual(list[0].PublicPropertyValue, duplicatedValueProperty6AsArray[0].PublicPropertyValue);
            Assert.IsNull(duplicatedValueProperty6AsArray[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, duplicatedValueProperty6AsArray[2].PublicPropertyValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(InvalidSerializationVersionException))]
        public void UnexpectedVersionHeaderShouldThrowAnException()
        {
            ClassWithDynamicProperty instance = new ClassWithDynamicProperty { Value = 123 };
            var bytes = Serializer.Serialize(instance);

            // this is a hackish way to change the version of a serialized object
            bytes[0] = (byte) (bytes[0] + 1);

            Deserializer.Deserialize<ClassWithDynamicProperty>(bytes);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DictionaryIsDetectedAsNormalDictionary()
        {
            Assert.IsTrue(DictionaryHelper.IsObjectADictionaryWithDefaultComparerAndNoAdditionalProperties(new Dictionary<int, int>()));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void CustomDictionaryIsDetectedAsNormalDictionary()
        {
            Assert.IsTrue(DictionaryHelper.IsObjectADictionaryWithDefaultComparerAndNoAdditionalProperties(new CustomDictionary()));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DictionaryWithCustomComparerIsNotDetectedAsNormalDictionary()
        {
            Assert.IsFalse(DictionaryHelper.IsObjectADictionaryWithDefaultComparerAndNoAdditionalProperties(new Dictionary<StructForTesting, int>(new StructForTestingComparer())));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DictionaryAdditionalPropertiesAsNormalDictionary()
        {
            Assert.IsFalse(DictionaryHelper.IsObjectADictionaryWithDefaultComparerAndNoAdditionalProperties(new CustomDictionaryWithAdditionalPropertiesWithoutOverridingOnDeserializedCallback()));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DeserializerObjectTrackerShouldNotTrackNullObjects()
        {
            var tracker = new DeserializerObjectTracker();
            tracker.TrackObject(null);
            Assert.AreEqual(0, tracker.NumberOfTrackedObjects);
        }


        [TestMethod]
        [TestCategory("Unit")]
        public void DeserializerObjectTrackerShouldTrackNullObjects()
        {
            var tracker = new DeserializerObjectTracker();
            tracker.TrackObject(3);
            Assert.AreEqual(1, tracker.NumberOfTrackedObjects);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void OmittedRootTypeCanBeDeserialized()
        {
            var instance = new ClassWithAllPrimitiveTypes();

            var bytesWithType = KrakenSerializer.Serialize(instance);
            var bytesWithOmittedType = KrakenSerializer.Serialize(instance, new SerializationOptions() { OmitRootTypeName = true });

            Assert.IsTrue(bytesWithOmittedType.Length < bytesWithType.Length);

            var deserializedValue = KrakenSerializer.Deserialize<ClassWithAllPrimitiveTypes>(bytesWithType);
            var deserializedValueFromOmitted = KrakenSerializer.Deserialize<ClassWithAllPrimitiveTypes>(bytesWithOmittedType, new SerializationOptions() { OmitRootTypeName = true });

            instance.ShouldBeEquivalentTo(deserializedValue);
            deserializedValueFromOmitted.ShouldBeEquivalentTo(deserializedValue);
        }




        //[TestMethod]
        //[TestCategory("Unit")]
        //[ExpectedException(typeof(NotSupportedException))]
        //public void SerializingAStreamIsNotSupported()
        //{
        //    using (var sr = new StringReader("abc"))
        //    {
        //        var bytes = Serializer.Serialize(sr);
        //    }
        //}


        //[TestMethod]
        //[TestCategory("Unit")]
        //public void z_AdditionalTestsToImplements()
        //{
        //    // mix primitive and class in array/list
        //    Assert.Fail();
        //}
    }
}
