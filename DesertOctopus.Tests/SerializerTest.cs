using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using DesertOctopus;
using DesertOctopus.Serialization;
using DesertOctopus.Serialization.Exceptions;
using DesertOctopus.Serialization.Helpers;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerializerTests.TestObjects;

namespace SerializerTests
{
    [TestClass]
    public class SerializerTest
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void TestPrimitives()
        {
            PrimitiveTestSuite<bool>(true, false);
            PrimitiveTestSuite<bool?>(true, false, null);
            PrimitiveTestSuite<byte>(byte.MaxValue, Byte.MinValue, 3, 0);
            PrimitiveTestSuite<byte?>(byte.MaxValue, Byte.MinValue, 3, 0, null);
            PrimitiveTestSuite<sbyte>(sbyte.MaxValue, sbyte.MinValue, -127, -64, 0, 64, 127);
            PrimitiveTestSuite<sbyte?>(sbyte.MaxValue, sbyte.MinValue, -127, -64, 0, 64, 127, null);
            PrimitiveTestSuite<Int16>(short.MaxValue, short.MinValue, -32767, 32767, 0);
            PrimitiveTestSuite<Int16?>(short.MaxValue, short.MinValue, -32767, 32767, 0, null);
            PrimitiveTestSuite<UInt16>(ushort.MaxValue, ushort.MinValue, 65000, 0);
            PrimitiveTestSuite<UInt16?>(ushort.MaxValue, ushort.MinValue, 65000, 0, null);
            PrimitiveTestSuite<Int32>(int.MaxValue, int.MinValue, -32767, 32767, 0);
            PrimitiveTestSuite<Int32?>(int.MaxValue, int.MinValue, -32767, 32767, 0, null);
            PrimitiveTestSuite<UInt32>(uint.MaxValue, uint.MinValue, 65000, 0);
            PrimitiveTestSuite<UInt32?>(uint.MaxValue, uint.MinValue, 65000, 0, null);
            PrimitiveTestSuite<Int64>(long.MaxValue, long.MinValue, -32767, 32767, 0);
            PrimitiveTestSuite<Int64?>(long.MaxValue, long.MinValue, -32767, 32767, 0, null);
            PrimitiveTestSuite<UInt64>(ulong.MaxValue, ulong.MinValue, 65000, 0);
            PrimitiveTestSuite<UInt64?>(ulong.MaxValue, ulong.MinValue, 65000, 0, null);


            PrimitiveTestSuite<char>(char.MaxValue, char.MinValue, 'a', 'z');
            PrimitiveTestSuite<char?>(char.MaxValue, char.MinValue, 'a', 'z', null);

            PrimitiveTestSuite<double>(double.MaxValue, double.MinValue, 1.1D, -1.1D, 0);
            PrimitiveTestSuite<double?>(double.MaxValue, double.MinValue, 1.1D, -1.1D, 0, null);
            PrimitiveTestSuite<Decimal>(Decimal.MaxValue, Decimal.MinValue, 1.1M, -1.1M, 0);
            PrimitiveTestSuite<Decimal?>(Decimal.MaxValue, Decimal.MinValue, 1.1M, -1.1M, 0, null);
            PrimitiveTestSuite<Single>(Single.MaxValue, Single.MinValue, 1.1f, -1.1f, 0);
            PrimitiveTestSuite<Single?>(Single.MaxValue, Single.MinValue, 1.1f, -1.1f, 0, null);
            PrimitiveTestSuite<DateTime>(DateTime.MaxValue, DateTime.MinValue, DateTime.Now, DateTime.UtcNow);
            PrimitiveTestSuite<DateTime?>(DateTime.MaxValue, DateTime.MinValue, DateTime.Now, DateTime.UtcNow, null);
            PrimitiveTestSuite<TimeSpan>(TimeSpan.MaxValue, TimeSpan.MinValue, TimeSpan.FromSeconds(30));
            PrimitiveTestSuite<TimeSpan?>(TimeSpan.MaxValue, TimeSpan.MinValue, TimeSpan.FromSeconds(30), null);
            PrimitiveTestSuite<BigInteger>(BigInteger.MinusOne, BigInteger.One, BigInteger.Zero, 98, -1928);
            PrimitiveTestSuite<BigInteger?>(BigInteger.MinusOne, BigInteger.One, BigInteger.Zero, 98, -1928, null);

        }

        [TestMethod]
        public void SerializeTuple()
        {
            PrimitiveTestSuite<Tuple<int, string>>(new Tuple<int, string>(1, "a"), new Tuple<int, string>(2, "b"));
            PrimitiveTestSuite<Tuple<string, int>>(new Tuple<string, int>("a", 1), new Tuple<string, int>("b", 2));
            PrimitiveTestSuite<Tuple<int, string, bool>>(new Tuple<int, string, bool>(1, "a", true), new Tuple<int, string, bool>(2, "b", false));
        }

        [TestMethod]
        public void SerializeUtcDateTime()
        {
            var instance = new Wrapper<DateTime> { Value = DateTime.UtcNow };
            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<Wrapper<DateTime>>(bytes);
            Assert.IsNotNull(deserializedValue.Value);
            Assert.AreEqual(instance.Value, deserializedValue.Value);
            Assert.AreEqual(instance.Value.Kind, deserializedValue.Value.Kind);
        }

        [TestMethod]
        public void SerializeNullPrimitiveArray()
        {
            int[] nullArray = null;
            var bytes = Serializer.Serialize(nullArray);
            var deserializedValue = Deserializer.Deserialize<int[]>(bytes);
            Assert.IsNull(deserializedValue);
        }

        [TestMethod]
        public void SerializeEmptyObjectArray()
        {
            Object[] emptyArray = new Object[0];
            var bytes = Serializer.Serialize(emptyArray);
            var deserializedValue = Deserializer.Deserialize<Object[]>(bytes);
            Assert.IsNotNull(deserializedValue);
            Assert.AreEqual(0, deserializedValue.Length);
        }

        [TestMethod]
        public void SerializeNullObjectArray()
        {
            Object[] nullArray = null;
            var bytes = Serializer.Serialize(nullArray);
            var deserializedValue = Deserializer.Deserialize<Object[]>(bytes);
            Assert.IsNull(deserializedValue);
        }

        [TestMethod]
        public void SerializeEmptyPrimitiveArray()
        {
            int[] emptyArray = new int[0];
            var bytes = Serializer.Serialize(emptyArray);
            var deserializedValue = Deserializer.Deserialize<int[]>(bytes);
            Assert.IsNotNull(deserializedValue);
            Assert.AreEqual(0, deserializedValue.Length);
        }

        [TestMethod]
        public void SerializeObjectArrayWithNullValues()
        {
            var array = new ClassWithoutSerializableAttribute[] { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var bytes = Serializer.Serialize(array);
            var deserializedValue = Deserializer.Deserialize<ClassWithoutSerializableAttribute[]>(bytes);
            Assert.AreEqual(array.Length, deserializedValue.Length);
            Assert.AreEqual(array[0].PublicPropertyValue, deserializedValue[0].PublicPropertyValue);
            Assert.IsNull(deserializedValue[1]);
            Assert.AreEqual(array[2].PublicPropertyValue, deserializedValue[2].PublicPropertyValue);
        }

        [TestMethod]
        public void SerializeNullPrimitiveList()
        {
            List<int> nullList = null;
            var bytes = Serializer.Serialize(nullList);
            var deserializedValue = Deserializer.Deserialize<List<int>>(bytes);
            Assert.IsNull(deserializedValue);
        }

        [TestMethod]
        public void SerializeEmptyObjectList()
        {
            List<Object> emptyList = new List<Object>();
            var bytes = Serializer.Serialize(emptyList);
            var deserializedValue = Deserializer.Deserialize<List<object>>(bytes);
            Assert.IsNotNull(deserializedValue);
            Assert.AreEqual(0, deserializedValue.Count);
        }

        [TestMethod]
        public void SerializeNullObjectList()
        {
            List<object> nullList = null;
            var bytes = Serializer.Serialize(nullList);
            var deserializedValue = Deserializer.Deserialize<List<object>>(bytes);
            Assert.IsNull(deserializedValue);
        }

        [TestMethod]
        public void SerializeEmptyPrimitiveList()
        {
            List<int> emptyList = new List<int>();
            var bytes = Serializer.Serialize(emptyList);
            var deserializedValue = Deserializer.Deserialize<List<int>>(bytes);
            Assert.IsNotNull(deserializedValue);
            Assert.AreEqual(0, deserializedValue.Count);
        }

        [TestMethod]
        public void SerializeObjectListWithNullValues()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var bytes = Serializer.Serialize(list);
            var deserializedValue = Deserializer.Deserialize<List<ClassWithoutSerializableAttribute>>(bytes);
            Assert.AreEqual(list.Count, deserializedValue.Count);
            Assert.AreEqual(list[0].PublicPropertyValue, deserializedValue[0].PublicPropertyValue);
            Assert.IsNull(deserializedValue[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, deserializedValue[2].PublicPropertyValue);
        }

        public void PrimitiveTestSuite<T>(params T[] values)
        {
            foreach (var value in values)
            {
                SerializeWrappedValue(value);
                SerializeArrayOfOne(value);
                SerializeListOfOne(value);
            }
            SerializeArray<T>(values);
            SerializeList<T>(values);
        }

        private static void SerializeWrappedValue<T>(T value)
        {
            var wrappedObject = new Wrapper<T>
            {
                Value = value
            };

            Type targetType = typeof(T);
            var bytes = Serializer.Serialize(wrappedObject);
            Wrapper<T> deserializedValue = Deserializer.Deserialize<Wrapper<T>>(bytes);

            Assert.AreEqual(wrappedObject.Value,
                            deserializedValue.Value,
                            string.Format("Type {0} does not have the same value after being deserialized.",
                                            targetType));

        }

        private static void SerializeArray<T>(T[] value)
        {
            var bytes = Serializer.Serialize(value);

            T[] deserializedValue = Deserializer.Deserialize<T[]>(bytes);

            CollectionAssert.AreEquivalent(value,
                                           deserializedValue);
        }

        private static void SerializeArrayOfOne<T>(T value)
        {
            var array = new T[1];
            array[0] = value;

            var bytes = Serializer.Serialize(array);

            T[] deserializedValue = Deserializer.Deserialize<T[]>(bytes);

            CollectionAssert.AreEquivalent(array,
                                            deserializedValue, "Type: " + typeof(T));
        }

        private static void SerializeListOfOne<T>(T value)
        {
            var list = new List<T>
                        {
                            value
                        };

            var bytes = Serializer.Serialize(list);
            List<T> deserializedValue = Deserializer.Deserialize<List<T>>(bytes);

            CollectionAssert.AreEquivalent(list,
                                            deserializedValue);
        }

        private static void SerializeList<T>(T[] value)
        {
            var list = value.ToList();

            var bytes = Serializer.Serialize(list);
            List<T> deserializedValue = Deserializer.Deserialize<List<T>>(bytes);

            CollectionAssert.AreEquivalent(list,
                                            deserializedValue);
        }

        [TestMethod]
        public void SerializeStruct()
        {
            var instance = new Wrapper<StructForTesting> { Value = new StructForTesting { Value = 1 } };
            var bytes = Serializer.Serialize(instance);
            Wrapper<StructForTesting> deserializedValue = Deserializer.Deserialize<Wrapper<StructForTesting>>(bytes);
            Assert.AreEqual(instance.Value, deserializedValue.Value);
        }

        [TestMethod]
        public void SerializeShortString()
        {
            var bytes = Serializer.Serialize("abc");
            //Assert.AreEqual(5, bytes.Length);
            var deserializedValue = Deserializer.Deserialize<string>(bytes);
            Assert.AreEqual("abc", deserializedValue);
        }

        [TestMethod]
        public void SerializeMediumString()
        {
            var str = RandomString(3000);
            var bytes = Serializer.Serialize(str);
            var deserializedValue = Deserializer.Deserialize<string>(bytes);
            Assert.AreEqual(str, deserializedValue);
        }

        [TestMethod]
        public void SerializeLongString()
        {
            var str = RandomString(100000);
            var bytes = Serializer.Serialize(str);
            var deserializedValue = Deserializer.Deserialize<string>(bytes);
            Assert.AreEqual(str, deserializedValue);
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        
        [TestMethod]
        public void SerializeTestClass()
        {
            ClassWithDifferentAccessModifiers classInstance = new ClassWithDifferentAccessModifiers
            {
                PublicFieldValue = 1,
                InternalFieldValue = 3,
                PublicPropertyValue = 4,
                InternalPropertyValue = 6
            };
            classInstance.SetPrivateFieldValue(2);
            classInstance.SetPrivatePropertyValue(5);

            var bytes = Serializer.Serialize(classInstance);
            var deserializedValue = Deserializer.Deserialize<ClassWithDifferentAccessModifiers>(bytes);

            Assert.AreEqual(classInstance.PublicFieldValue,
                            deserializedValue.PublicFieldValue);
            Assert.AreEqual(classInstance.GetPrivateFieldValue(),
                            deserializedValue.GetPrivateFieldValue());
            Assert.AreEqual(classInstance.InternalFieldValue,
                            deserializedValue.InternalFieldValue);
            Assert.AreEqual(classInstance.PublicPropertyValue,
                            deserializedValue.PublicPropertyValue);
            Assert.AreEqual(classInstance.GetPrivatePropertyValue(),
                            deserializedValue.GetPrivatePropertyValue());
            Assert.AreEqual(classInstance.InternalPropertyValue,
                            deserializedValue.InternalPropertyValue);
        }

        [TestMethod]
        //[ExpectedException(typeof(ObjectExistsInCurrentSerializationGraphException))]
        public void SerializeCircularReference()
        {
            var instance1 = new CircularReference { Id = 1 };
            var instance2 = new CircularReference { Id = 2 };
            instance1.Child = instance2;
            instance2.Parent = instance1;

            var bytes = Serializer.Serialize(instance1);
            var deserializedValue = Deserializer.Deserialize<CircularReference>(bytes);

            Assert.IsTrue(ReferenceEquals(deserializedValue,
                                          deserializedValue.Child.Parent));

            Assert.AreEqual(1, deserializedValue.Id);
            Assert.AreEqual(2, deserializedValue.Child.Id);
            Assert.AreEqual(1, deserializedValue.Child.Parent.Id);
        }

        [TestMethod]
        public void SerializeTrackMultipleReference()
        {
            var instance = new ClassWithGenericInt(123);
            var list = new List<ClassWithGenericInt>
                        {
                            instance,
                            instance
                        };

            var bytes = Serializer.Serialize(list);
            var deserializedValue = Deserializer.Deserialize<List<ClassWithGenericInt>>(bytes);

            Assert.AreEqual(list.Count,
                            deserializedValue.Count);
            CollectionAssert.AreEquivalent(list,
                                            deserializedValue);
        }

        [TestMethod]
        public void SerializeTrackSamePrimitiveMultipleTimes()
        {
            // this case exists to make sure that the ReferenceWatcher only tracks classes

            var instance = 3;
            var list = new List<int>
                        {
                            instance,
                            instance
                        };

            var bytes = Serializer.Serialize(list);
            var deserializedValue = Deserializer.Deserialize<List<int>>(bytes);

            Assert.AreEqual(list.Count,
                            deserializedValue.Count);
            CollectionAssert.AreEquivalent(list,
                                            deserializedValue);
        }

        [TestMethod]
        public void SerializeClassWithoutSerializableAttribute()
        {
            var instance = new ClassWithoutSerializableAttribute
            {
                PublicPropertyValue = 4
            };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithoutSerializableAttribute>(bytes);

            Assert.AreEqual(instance.PublicPropertyValue,
                            deserializedValue.PublicPropertyValue);
        }

        [TestMethod]
        public void SerializeClassWithGenericBase()
        {
            var instance = new ClassWithGenericInt()
            {
                Value = 4
            };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithGenericInt>(bytes);

            Assert.AreEqual(instance.Value,
                            deserializedValue.Value);
        }

        [TestMethod]
        public void SerializeGenericClass()
        {
            var instance = new GenericBaseClass<int>()
            {
                Value = 4
            };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<GenericBaseClass<int>>(bytes);

            Assert.AreEqual(instance.Value,
                            deserializedValue.Value);
        }

        [TestMethod]
        public void SerializeDictionaryStringObject()
        {
            var instance = new Dictionary<string, object>()
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<Dictionary<string, object>>(bytes);

            Assert.AreEqual(instance.Count,
                            deserializedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            deserializedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                deserializedValue[kvp.Key]);
            }
        }

        [TestMethod]
        public void SerializeDictionaryIntString()
        {
            var instance = new Dictionary<int, string>()
            {
                {1, "Value1"},
                {2, "Value2"},
                {3, "Value3"}
            };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<Dictionary<int, string>>(bytes);

            Assert.AreEqual(instance.Count,
                            deserializedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            deserializedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                deserializedValue[kvp.Key]);
            }
        }

        [TestMethod]
        public void SerializeDictionaryGuidString()
        {
            var instance = new Dictionary<Guid, string>()
            {
                {Guid.NewGuid(), "Value1"},
                {Guid.NewGuid(), "Value2"},
                {Guid.NewGuid(), "Value3"}
            };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<Dictionary<Guid, string>>(bytes);

            Assert.AreEqual(instance.Count,
                            deserializedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            deserializedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                deserializedValue[kvp.Key]);
            }
        }

        [TestMethod]
        public void SerializeCustomDictionary()
        {
            var instance = new CustomDictionary
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<CustomDictionary>(bytes);

            Assert.AreEqual(instance.Count,
                            deserializedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            deserializedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                deserializedValue[kvp.Key]);
            }
        }

        [TestMethod]
        public void SerializeWrappedCustomDictionary()
        {
            var instance = new Wrapper<CustomDictionary>
                           {
                               Value = new CustomDictionary
                                       {
                                           {"Key1", 123},
                                           {"Key2", "abc"},
                                           {"Key3", new ClassWithGenericInt(3)},
                                       }
                           };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<Wrapper<CustomDictionary>>(bytes);

            Assert.AreEqual(instance.Value.Count,
                            deserializedValue.Value.Count);
            CollectionAssert.AreEquivalent(instance.Value.Keys,
                                            deserializedValue.Value.Keys);
            foreach (var kvp in instance.Value)
            {
                Assert.AreEqual(kvp.Value,
                                deserializedValue.Value[kvp.Key]);
            }
        }

        [TestMethod]
        public void SerializeCustomDictionaryWithComparer()
        {
            var instance = new CustomDictionary(StringComparer.CurrentCultureIgnoreCase)
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<CustomDictionary>(bytes);

            Assert.AreEqual(instance.Comparer.GetType(),
                            deserializedValue.Comparer.GetType());
            Assert.AreEqual(instance.Count,
                            deserializedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            deserializedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                deserializedValue[kvp.Key]);
            }
        }

        [TestMethod]
        public void SerializeDictionaryWithCustomComparer()
        {
            var instance = new Dictionary<StructForTesting, object>(new StructForTestingComparer())
            {
                {new StructForTesting { Value = 1 }, 123},
                {new StructForTesting { Value = 2 }, "abc"},
                {new StructForTesting { Value = 3 }, new ClassWithGenericInt(3) },
            };


            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<Dictionary<StructForTesting, object>>(bytes);

            Assert.AreEqual(instance.Comparer.GetType(),
                            deserializedValue.Comparer.GetType());
            Assert.AreEqual(instance.Count,
                            deserializedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            deserializedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                deserializedValue[kvp.Key]);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MissingConstructorException))]
        public void SerializeCustomDictionaryWithoutPublicParameterlessConstructor()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructor<object, object>()
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
                {"Key4", null },
            };

            Serializer.Serialize(instance);
        }

        [TestMethod]
        public void SerializeCustomDictionaryWithAdditionalPropertiesWithCallback()
        {
            var instance = new CustomDictionaryWithAdditionalPropertiesWithOverridingOnDeserializedCallback
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };
            instance.SomeProperty = 849;

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<CustomDictionaryWithAdditionalPropertiesWithOverridingOnDeserializedCallback>(bytes);

            Assert.AreEqual(instance.SomeProperty,
                            deserializedValue.SomeProperty);
            Assert.AreEqual(instance.Count,
                            deserializedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            deserializedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                deserializedValue[kvp.Key]);
            }
        }

        [TestMethod]
        public void SerializeCustomDictionaryWithAdditionalPropertiesWithoutCallback()
        {
            var instance = new CustomDictionaryWithAdditionalPropertiesWithoutOverridingOnDeserializedCallback
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };
            instance.SomeProperty = 849;

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<CustomDictionaryWithAdditionalPropertiesWithoutOverridingOnDeserializedCallback>(bytes);

            Assert.AreEqual(0,
                            deserializedValue.SomeProperty); // default value for property
            Assert.AreEqual(instance.Count,
                            deserializedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            deserializedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                deserializedValue[kvp.Key]);
            }
        }

        [TestMethod]
        public void SerializeCustomDictionaryWithDictionaryProperty()
        {
            var instance = new CustomDictionaryWithDictionaryProperty<string, object>
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };
            instance.SwitchedDictionary = new CustomDictionaryWithAdditionalPropertiesAndGenerics<object, string>
            {
                { 123, "Key1"},
                { "abc", "Key2"},
                { new ClassWithGenericInt(3), "Key3" },
            };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<CustomDictionaryWithDictionaryProperty<string, object>>(bytes);

            CompareDictionaries(instance,
                                deserializedValue);
            CompareDictionaries(instance.SwitchedDictionary,
                                deserializedValue.SwitchedDictionary);
        }

        private static void CompareDictionaries<TKey, TValue>(Dictionary<TKey, TValue> instance, Dictionary<TKey, TValue> deserializedValue)
        {
            Assert.AreEqual(instance.Count,
                            deserializedValue.Count);
            Assert.AreEqual(instance.Comparer.GetType(),
                            deserializedValue.Comparer.GetType());
            CollectionAssert.AreEquivalent(instance.Keys,
                                           deserializedValue.Keys);
            foreach (var key in instance.Keys)
            {
                if (instance[key] != null
                    && instance[key].GetType() == typeof(object))
                {
                    Assert.IsTrue(deserializedValue[key] != null && deserializedValue[key].GetType() == typeof(object));
                }
                else if (instance[key] != null
                    && instance[key].GetType() == typeof(ClassWithoutSerializableAttribute))
                {
                    Assert.AreEqual(((ClassWithoutSerializableAttribute)(object)instance[key]).PublicPropertyValue,
                                    ((ClassWithoutSerializableAttribute)(object)deserializedValue[key]).PublicPropertyValue);
                }
                else
                {
                    Assert.AreEqual(instance[key],
                                    deserializedValue[key]);
                }
            }
        }

        [TestMethod]
        public void SerializeCustomDictionaryWithAdditionalPropertiesAndGenerics()
        {
            var instance = new CustomDictionaryWithAdditionalPropertiesAndGenerics<string, int>
            {
                {"Key1", 123},
                {"Key2", 456},
                {"Key3", 789},
            };
            instance.SomeProperty = 849;

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<CustomDictionaryWithAdditionalPropertiesAndGenerics<string, int>>(bytes);

            Assert.AreEqual(instance.SomeProperty,
                            deserializedValue.SomeProperty);
            Assert.AreEqual(instance.Count,
                            deserializedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            deserializedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                deserializedValue[kvp.Key]);
            }
        }

        [TestMethod]
        public void SerializeListWithMultipleTypes()
        {
            var list = new List<IHierarchy>
            {
                new ChildIntHierarchy(123),
                new ChildStringHierarchy("abc"),
            };

            var bytes = Serializer.Serialize(list);
            var deserializedValue = Deserializer.Deserialize<List<IHierarchy>>(bytes);

            Assert.AreEqual(list.Count,
                            deserializedValue.Count);
            Assert.AreEqual(list.OfType<ChildIntHierarchy>().First().Value,
                            deserializedValue.OfType<ChildIntHierarchy>().First().Value);
            Assert.AreEqual(list.OfType<ChildStringHierarchy>().First().Value,
                            deserializedValue.OfType<ChildStringHierarchy>().First().Value);
        }

        [TestMethod]
        public void SerializeObjectWithListAsIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 } };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithIEnumerable<int>>(bytes);

            Assert.AreEqual(instance.Items.Count(),
                            deserializedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            deserializedValue.Items.ToList());
        }

        [TestMethod]
        public void SerializeObjectWithHashsetAsIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int>
            {
                Items = new HashSet<int> { 1, 2, 3 }
            };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithIEnumerable<int>>(bytes);

            Assert.AreEqual(instance.Items.Count(),
                            deserializedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            deserializedValue.Items.ToList());
        }

        [TestMethod]
        public void SerializeHashSetWithEqualityComparer()
        {
            var instance = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "a", "b", "C" };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<HashSet<string>>(bytes);

            Assert.AreEqual(instance.Comparer.GetType(),
                            deserializedValue.Comparer.GetType());
            Assert.AreEqual(instance.Count(),
                            deserializedValue.Count());
            CollectionAssert.AreEquivalent(instance.ToList(),
                                            deserializedValue.ToList());
        }

        [TestMethod]
        public void SerializeObjectWithEnumProperty()
        {
            var instance = new GenericBaseClass<EnumForTesting> { Value = EnumForTesting.Two };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<GenericBaseClass<EnumForTesting>>(bytes);

            Assert.AreEqual(instance.Value,
                            deserializedValue.Value);
        }

        [TestMethod]
        public void SerializeHashtable()
        {
            var instance = new Hashtable
                            {
                                {1, 2},
                                {"a", "b"},
                            };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<Hashtable>(bytes);

            Assert.AreEqual(instance.Count,
                            deserializedValue.Count);
            CollectionAssert.AreEquivalent(instance,
                                            deserializedValue);
        }

        [TestMethod]
        public void SerializeHashtableWithEqualityComparer()
        {
            var instance = new Hashtable(StringComparer.CurrentCultureIgnoreCase)
                            {
                                {"e", 2},
                                {"a", "b"},
                            };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<Hashtable>(bytes);

            Assert.IsNotNull(GetHashTableComparer(instance));
            Assert.IsNotNull(GetHashTableComparer(deserializedValue));
            Assert.AreEqual(GetHashTableComparer(instance).GetType(),
                            GetHashTableComparer(deserializedValue).GetType());
            Assert.AreEqual(instance.Count,
                            deserializedValue.Count);
            CollectionAssert.AreEquivalent(instance,
                                            deserializedValue);
        }

        private object GetHashTableComparer(Hashtable ht)
        {
            if (ht == null)
            {
                return null;
            }

            return ht.GetType()
                     .GetProperty("EqualityComparer",
                                  BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.Instance)
                     .GetValue(ht);
        }

        [TestMethod]
        public void SerializeObjectWithArrayAsIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new[] { 1, 2, 3 } };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithIEnumerable<int>>(bytes);

            Assert.AreEqual(instance.Items.Count(),
                            deserializedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            deserializedValue.Items.ToList());
        }

        [TestMethod]
        public void SerializeObjectWithDistinctIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 1, 2, 3 }.Distinct() };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithIEnumerable<int>>(bytes);

            Assert.AreEqual(instance.Items.Count(),
                            deserializedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            deserializedValue.Items.ToList());
        }

        [TestMethod]
        public void SerializeObjectWithWhereIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Where(x => x > 1) };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithIEnumerable<int>>(bytes);

            Assert.AreEqual(instance.Items.Count(),
                            deserializedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            deserializedValue.Items.ToList());
        }

        [TestMethod]
        public void SerializeObjectWithOrderByIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.OrderBy(x => x) };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithIEnumerable<int>>(bytes);

            Assert.AreEqual(instance.Items.Count(),
                            deserializedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            deserializedValue.Items.ToList());
        }

        [TestMethod]
        public void SerializeObjectWithDefaultIfEmptyIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.DefaultIfEmpty(123) };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithIEnumerable<int>>(bytes);

            Assert.AreEqual(instance.Items.Count(),
                            deserializedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            deserializedValue.Items.ToList());
        }

        [TestMethod]
        public void SerializeObjectWithExceptIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Except(new[] { 2 }) };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithIEnumerable<int>>(bytes);

            Assert.AreEqual(instance.Items.Count(),
                            deserializedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            deserializedValue.Items.ToList());
        }

        [TestMethod]
        public void SerializeObjectWithUnionIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Union(new[] { 4 }) };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithIEnumerable<int>>(bytes);

            Assert.AreEqual(instance.Items.Count(),
                            deserializedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            deserializedValue.Items.ToList());
        }

        [TestMethod]
        public void SerializeObjectWithIntersectIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Intersect(new[] { 2 }) };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithIEnumerable<int>>(bytes);

            Assert.AreEqual(instance.Items.Count(),
                            deserializedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            deserializedValue.Items.ToList());
        }

        [TestMethod]
        public void SerializeObjectWithOfTypeIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.OfType<int>() };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithIEnumerable<int>>(bytes);

            Assert.AreEqual(instance.Items.Count(),
                            deserializedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            deserializedValue.Items.ToList());
        }

        [TestMethod]
        public void SerializeObjectWithSkipByIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Skip(1) };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithIEnumerable<int>>(bytes);

            Assert.AreEqual(instance.Items.Count(),
                            deserializedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            deserializedValue.Items.ToList());
        }

        [TestMethod]
        public void SerializeObjectWithTakeByIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Take(1) };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithIEnumerable<int>>(bytes);

            Assert.AreEqual(instance.Items.Count(),
                            deserializedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            deserializedValue.Items.ToList());
        }

        [TestMethod]
        public void SerializeObjectWithSelectByIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Select(x => x * 2) };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithIEnumerable<int>>(bytes);

            Assert.AreEqual(instance.Items.Count(),
                            deserializedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            deserializedValue.Items.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void SerializeSimpleFunc()
        {
            var testData = new List<int> { 1, 2, 3, 4, 5, 6 };
            System.Func<int, bool> instance = x => x > 3;

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<System.Func<int, bool>>(bytes);

            Assert.IsNotNull(deserializedValue);

            Assert.AreEqual(testData.Count(x => instance(x)),
                            testData.Count(x => deserializedValue(x)));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void SerializeSimpleExpression()
        {
            var testData = new List<int> { 1, 2, 3, 4, 5, 6 };

            Expression<System.Func<int, bool>> instance = x => x > 3;

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<Expression<System.Func<int, bool>>>(bytes);

            Assert.IsNotNull(deserializedValue);

            Assert.AreEqual(testData.Count(instance.Compile()),
                            testData.Count(deserializedValue.Compile()));
            Assert.Fail("we should not allow serialization of Expression");
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void SerializeAnonymousObject()
        {
            var instance = new { Property1 = "hello", Property2 = 123 };
            var bytes = Serializer.Serialize(instance);
            dynamic o = Deserializer.Deserialize<object>(bytes);
            Assert.AreEqual(instance.Property1, o.Property1);
            Assert.AreEqual(instance.Property2, o.Property2);


            Assert.Fail("we should not allow serialization of anonymous types");
        }

        [TestMethod]
        public void SerializeArray()
        {
            var instance = new int[] { 123, 456 };
            var bytes = Serializer.Serialize(instance);
            int[] o = Deserializer.Deserialize<int[]>(bytes);
            CollectionAssert.AreEqual(instance, o);
        }

        [TestMethod]
        public void SerializeArrayWithNullablePrimitive()
        {
            var instance = new int?[] { 123, null, 456 };
            var bytes = Serializer.Serialize(instance);
            int?[] o = Deserializer.Deserialize<int?[]>(bytes);
            CollectionAssert.AreEqual(instance, o);
        }

        [TestMethod]
        public void SerializeArrayObject()
        {
            var instance = new ClassWithoutSerializableAttribute[] { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var bytes = Serializer.Serialize(instance);
            ClassWithoutSerializableAttribute[] o = Deserializer.Deserialize<ClassWithoutSerializableAttribute[]>(bytes);
            Assert.AreEqual(2, o.Length);
            Assert.AreEqual(123, o[0].PublicPropertyValue);
            Assert.AreEqual(456, o[1].PublicPropertyValue);
        }

        [TestMethod]
        public void SerializeArrayOfClassWithGenericInt()
        {
            var obj = new ClassWithGenericInt(123);
            var instance = new ClassWithGenericInt[] { obj, null };

            var bytes = Serializer.Serialize(instance);
            var o = Deserializer.Deserialize<ClassWithGenericInt[]>(bytes);

            instance.ShouldAllBeEquivalentTo(o);
        }

        [TestMethod]
        public void SerializeArrayObjectHierarchy()
        {
            var instance = new SomeBaseClass[] { new ClassWithGenericInt(123), new ClassWithGenericDouble(3.38D) };
            var bytes = Serializer.Serialize(instance);
            SomeBaseClass[] o = Deserializer.Deserialize<SomeBaseClass[]>(bytes);
            Assert.AreEqual(2, o.Length);
            Assert.AreEqual(123, (o[0] as ClassWithGenericInt).Value);
            Assert.AreEqual(3.38D, (o[1] as ClassWithGenericDouble).Value);
        }

        [TestMethod]
        public void SerializeSameObjectMultipleTimeInArray()
        {
            var obj123 = new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 };
            var instance = new ClassWithoutSerializableAttribute[] { obj123, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 }, obj123 };
            var bytes = Serializer.Serialize(instance);
            ClassWithoutSerializableAttribute[] o = Deserializer.Deserialize<ClassWithoutSerializableAttribute[]>(bytes);
            Assert.AreEqual(3, o.Length);
            Assert.AreEqual(123, o[0].PublicPropertyValue);
            Assert.AreEqual(456, o[1].PublicPropertyValue);
            Assert.AreEqual(123, o[2].PublicPropertyValue);
            Assert.IsTrue(ReferenceEquals(o[0], o[2]));
        }

        [TestMethod]
        public void SerializeTwoDimensionalArray()
        {
            var instance = new int[3, 4];
            instance[0, 0] = 0;
            instance[0, 1] = 1;
            instance[0, 2] = 2;
            instance[0, 3] = 3;
            instance[1, 0] = 4;
            instance[1, 1] = 5;
            instance[1, 2] = 6;
            instance[1, 3] = 7;
            instance[2, 0] = 8;
            instance[2, 1] = 9;
            instance[2, 2] = 10;
            instance[2, 3] = 11;

            var bytes = Serializer.Serialize(instance);
            var o = Deserializer.Deserialize<int[,]>(bytes);
            instance.Should().BeEquivalentTo(o);
        }

        [TestMethod]
        public void SerializeMultiDimensionalArray()
        {
            var instance = CreateMultiDimensionalArray<int>(8);
            SeedArray(instance);

            var bytes = Serializer.Serialize(instance);
            var o = Deserializer.Deserialize<Array>(bytes);
            instance.Should().BeEquivalentTo(o);
        }

        [TestMethod]
        public void SerializeMultiDimensionalArrayOfObjects()
        {
            var instance = CreateMultiDimensionalArray<Object>(2);
            instance.SetValue(new object(), new[] { 0, 0 });
            instance.SetValue(new object(), new[] { 1, 0 });

            var bytes = Serializer.Serialize(instance);
            var o = Deserializer.Deserialize<Array>(bytes);

            Assert.IsTrue(o.GetValue(new[] { 0, 0 }).GetType() == typeof(object));
            Assert.IsNull(o.GetValue(new[] { 0, 1 }));
            Assert.IsTrue(o.GetValue(new[] { 1, 0 }).GetType() == typeof(object));
            Assert.IsNull(o.GetValue(new[] { 1, 1 }));
        }

        [TestMethod]
        public void SerializeMultiDimensionalArrayOfClass()
        {
            var instance = CreateMultiDimensionalArray<ClassWithGenericInt>(2);
            var obj = new ClassWithGenericInt(123);
            instance.SetValue(obj, new[] { 0, 0 });
            instance.SetValue(new ClassWithGenericInt(456), new[] { 1, 0 });
            instance.SetValue(obj, new[] { 1, 1 });

            var bytes = Serializer.Serialize(instance);
            var o = Deserializer.Deserialize<Array>(bytes);
            instance.Should().BeEquivalentTo(o);
            Assert.AreEqual(456, ((ClassWithGenericInt)o.GetValue(1, 0)).Value);
            Assert.IsTrue(ReferenceEquals(o.GetValue(0, 0), o.GetValue(1, 1)));
        }

        private void SeedArray(Array array)
        {
            Random rnd = new Random();

            int[] indices = new int[array.Rank];
            for (int i = 0; i < ((array.Rank * 2) ^ 10); i++)
            {
                for (int j = 0; j < array.Rank; j++)
                {
                    indices[j] = rnd.Next(0, array.GetLength(j));
                }

                array.SetValue(rnd.Next(), indices);
            }
        }

        private Array CreateMultiDimensionalArray<T>(int numberofDimensions)
        {
            Random rnd = new Random();

            int[] lengths = new int[numberofDimensions];
            for (var i = 0; i < lengths.Length; i++)
            {
                lengths[i] = rnd.Next(2, 5);
            }

            Array arr = Array.CreateInstance(typeof(T), lengths);
            return arr;
        }


        [TestMethod]
        public void SerializeJaggedArrayObject()
        {
            var instance = new object[2][];
            instance[0] = new object[] { new object(), null };
            instance[1] = new object[] { new object(), null };

            var bytes = Serializer.Serialize(instance);
            var o = Deserializer.Deserialize<Array>(bytes) as object[][];

            Assert.IsTrue(o[0][0].GetType() == typeof(object));
            Assert.IsNull(o[0][1]);
            Assert.IsTrue(o[1][0].GetType() == typeof(object));
            Assert.IsNull(o[1][1]);
        }


        [TestMethod]
        public void SerializeJaggedArrayClass()
        {
            var obj = new ClassWithGenericInt(123);
            var instance = new ClassWithGenericInt[2][];
            instance[0] = new ClassWithGenericInt[] { obj, null };
            //instance[0] = new ClassWithGenericInt[] { obj, new ClassWithGenericInt(4563) };
            instance[1] = new ClassWithGenericInt[] { new ClassWithGenericInt(456), obj };

            var bytes = Serializer.Serialize(instance);
            var o = Deserializer.Deserialize<ClassWithGenericInt[][]>(bytes);

            instance.ShouldAllBeEquivalentTo(o);

            Assert.AreEqual(456, ((ClassWithGenericInt)o[1][0]).Value);
            Assert.IsTrue(ReferenceEquals(o[0][0], o[1][1]));

            instance = new ClassWithGenericInt[2][];
            instance[0] = new ClassWithGenericInt[] { obj, new ClassWithGenericInt(4563) };
            instance[1] = new ClassWithGenericInt[] { new ClassWithGenericInt(456), obj };

            bytes = Serializer.Serialize(instance);
            o = Deserializer.Deserialize<ClassWithGenericInt[][]>(bytes);

            instance.ShouldAllBeEquivalentTo(o);
        }


        [TestMethod]
        public void SerializeJaggedArrayClassInheritance()
        {
            var obj = new ClassWithGenericInt(123);
            var instance = new SomeBaseClass[2][];
            instance[0] = new SomeBaseClass[] { obj, new ClassWithGenericDouble(123.3D) };
            instance[1] = new SomeBaseClass[] { new ClassWithGenericInt(456), obj };

            var bytes = Serializer.Serialize(instance);
            var o = Deserializer.Deserialize<SomeBaseClass[][]>(bytes);


            Assert.AreEqual(123, ((ClassWithGenericInt)o[0][0]).Value);
            Assert.AreEqual(123.3D, ((ClassWithGenericDouble)o[0][1]).Value);
            Assert.AreEqual(456, ((ClassWithGenericInt)o[1][0]).Value);
            Assert.IsTrue(ReferenceEquals(o[0][0], o[1][1]));
        }


        [TestMethod]
        public void SerializeNullJaggedArray()
        {
            int[][] nullArray = null;
            var instance = new Wrapper<int[][]> { Value = nullArray };
            var bytes = Serializer.Serialize(instance);
            var o = Deserializer.Deserialize<Wrapper<int[][]>>(bytes);

            Assert.IsNull(o.Value);
        }

        [TestMethod]
        public void SerializeThreeDimensionJaggedArray()
        {
            int[][][] instance = new [] { new int[2][], new int[3][] };
            instance[0][0] = new int[] { 123, 238 };
            var bytes = Serializer.Serialize(instance);
            var o = Deserializer.Deserialize<int[][][]>(bytes);

            instance.ShouldAllBeEquivalentTo(o);
        }

        [TestMethod]
        public void SerializeEmptyJaggedArray()
        {
            int[][] instance = new int[2][];
            var bytes = Serializer.Serialize(instance);
            var o = Deserializer.Deserialize<int[][]> (bytes);
            instance.ShouldAllBeEquivalentTo(o);
        }

        [TestMethod]
        public void SerializeJaggedArray()
        {
            var instance = new int[][] { new int[] { 123, 238 }, new int[] { 456, 546, 784 } };
            var bytes = Serializer.Serialize(instance);
            int[][] o = Deserializer.Deserialize<int[][]>(bytes);

            Assert.AreEqual(instance.Length, o.Length);
            instance.ShouldAllBeEquivalentTo(o);
        }

        [TestMethod]
        public void TrackArrayInMultipleObjects()
        {
            int[] arr = new[] { 1, 2, 3 };
            var instance1 = new CircularReference { Id = 1 };
            var instance2 = new CircularReference { Id = 2, Ids = arr };
            var instance3 = new CircularReference { Id = 3, Ids = arr };
            instance1.Child = instance2;
            instance2.Child = instance3;

            var bytes = Serializer.Serialize(instance1);
            var deserializedValue = Deserializer.Deserialize<CircularReference>(bytes);

            CollectionAssert.AreEqual(arr, deserializedValue.Child.Ids);
            Assert.IsTrue(ReferenceEquals(deserializedValue.Child.Ids, deserializedValue.Child.Child.Ids));
        }

        [TestMethod]
        public void SerializeClassWithDynamicObject()
        {
            ClassWithDynamicProperty instance = new ClassWithDynamicProperty { Value = 123 };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithDynamicProperty>(bytes);

            Assert.IsNotNull(deserializedValue);

            Assert.AreEqual(instance.Value,
                            deserializedValue.Value);
        }

        [TestMethod]
        [ExpectedException(typeof(TypeNotFoundException))]
        public void DeserializeUnknownType()
        {
            ClassWithDynamicProperty instance = new ClassWithDynamicProperty { Value = 123 };
            var bytes = Serializer.Serialize(instance);
            bytes[25] = 54;

            var deserializedValue = Deserializer.Deserialize<ClassWithDynamicProperty>(bytes);
        }

        [TestMethod]
        [ExpectedException(typeof(TypeWasModifiedSinceSerializationException))]
        public void DeserializeTypeThatWasModified()
        {
            ClassWithDynamicProperty instance = new ClassWithDynamicProperty { Value = 123 };
            var bytes = Serializer.Serialize(instance);

            var needle = SerializedTypeResolver.GetShortNameFromType(typeof(ClassWithDynamicProperty));
            var index = System.Text.Encoding.UTF8.GetString(bytes).IndexOf(needle);

            // this is a hackish way to change the hashcode of a serialized object
            // if the way/order (currently TypeName + Hash) that an object is serialized changes the line below will need to be modified to target a byte of the hashcode
            bytes[index + needle.Length + 1] = (bytes[index + needle.Length + 1] == 255) ? (byte)0 : (byte)(bytes[index + needle.Length] + 1); // change the hashcode to something invalid

            var deserializedValue = Deserializer.Deserialize<ClassWithDynamicProperty>(bytes);
        }

        [TestMethod]
        public void SerializeExpandoObject()
        {
            dynamic instance = new ExpandoObject();
            instance.Property1 = 123;
            instance.Property2 = "abc";
            instance.Property3 = new ClassWithGenericInt(349);
            instance.Property4 = new object();
            instance.Property5 = null;

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ExpandoObject>(bytes);

            Assert.IsNotNull(deserializedValue);

            Assert.AreEqual(instance.Property1,
                            deserializedValue.Property1);
            Assert.AreEqual(instance.Property2,
                            deserializedValue.Property2);
            Assert.AreEqual(instance.Property3,
                            deserializedValue.Property3);
            Assert.IsTrue(deserializedValue.Property4.GetType() == typeof(object));
            Assert.IsNull(deserializedValue.Property5);

        }

        [TestMethod]
        public void SerializeEmptyExpandoObject()
        {
            dynamic instance = new ExpandoObject();

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ExpandoObject>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.AreEqual(0, (deserializedValue as IDictionary<string, object>).Count);
        }

        [TestMethod]
        public void SerializeNullExpandoObject()
        {
            ExpandoObject instance = null;

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ExpandoObject>(bytes);

            Assert.IsNull(deserializedValue);

            bytes = Serializer.Serialize(new Wrapper<ExpandoObject> { Value = instance });
            var wrappedDeserializedValue = Deserializer.Deserialize<Wrapper<ExpandoObject>>(bytes);

            Assert.IsNull(wrappedDeserializedValue.Value);
        }

        [TestMethod]
        public void SerializeTypedQueue()
        {
            var instance = new Queue<int>();
            instance.Enqueue(1);
            instance.Enqueue(2);
            instance.Enqueue(3);

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<Queue<int>>(bytes);

            Assert.IsNotNull(deserializedValue);

            Assert.AreEqual(instance.Count,
                            deserializedValue.Count);
            CollectionAssert.AreEquivalent(instance.ToArray(),
                                            deserializedValue.ToArray());
        }

        [TestMethod]
        public void SerializeUntypedQueue()
        {
            var instance = new Queue();
            instance.Enqueue(1);
            instance.Enqueue(2);
            instance.Enqueue("abc");
            instance.Enqueue(new ClassWithGenericInt(123));

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<Queue>(bytes);

            Assert.IsNotNull(deserializedValue);

            Assert.AreEqual(instance.Count,
                            deserializedValue.Count);
            CollectionAssert.AreEquivalent(instance.ToArray(),
                                            deserializedValue.ToArray());
        }

        [TestMethod]
        public void SerializeTypedStack()
        {
            var instance = new Stack<int>();
            instance.Push(1);
            instance.Push(2);
            instance.Push(3);

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<Stack<int>>(bytes);

            Assert.IsNotNull(deserializedValue);

            Assert.AreEqual(instance.Count,
                            deserializedValue.Count);
            CollectionAssert.AreEquivalent(instance.ToArray(),
                                            deserializedValue.ToArray());
        }

        [TestMethod]
        public void SerializeUntypedStack()
        {
            var instance = new Stack();
            instance.Push(1);
            instance.Push(2);
            instance.Push("abc");
            instance.Push(new ClassWithGenericInt(123));

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<Stack>(bytes);

            Assert.IsNotNull(deserializedValue);

            Assert.AreEqual(instance.Count,
                            deserializedValue.Count);
            CollectionAssert.AreEquivalent(instance.ToArray(),
                                            deserializedValue.ToArray());
        }

        [TestMethod]
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
        public void SerializeEnumEqualityComparer()
        {
            var instance = new Dictionary<EnumForTesting, int> { { EnumForTesting.One, 1 }, { EnumForTesting.Two, 2 } };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<Dictionary<EnumForTesting, int>>(bytes);

            Assert.IsNotNull(deserializedValue);

            CompareDictionaries(instance,
                                deserializedValue);
        }

        [TestMethod]
        public void SerializeAnObject()
        {
            var instance = new object();

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<object>(bytes);

            Assert.IsNotNull(deserializedValue);
        }

        [TestMethod]
        public void SerializeAClassWithANullObjectProperty()
        {
            var instance = new ClassWithObjectProperty();

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithObjectProperty>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.IsNull(deserializedValue.Obj);
        }

        [TestMethod]
        public void SerializeAClassWithANotNullObjectProperty()
        {
            var instance = new ClassWithObjectProperty { Obj = new object() };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithObjectProperty>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.IsNotNull(deserializedValue.Obj);
        }

        [TestMethod]
        public void SerializeAClassWithAnBoxedInt()
        {
            var instance = new ClassWithObjectProperty { Obj = 123 };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithObjectProperty>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.AreEqual(123, (int)deserializedValue.Obj);
        }

        [TestMethod]
        public void SerializeIList()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IList> { Value = list };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<GenericBaseClass<IList>>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.IsNotNull(deserializedValue.Value);
            var dList = deserializedValue.Value as List<ClassWithoutSerializableAttribute>;

            Assert.AreEqual(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.IsNull(dList[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [TestMethod]
        public void SerializeICollection()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<ICollection> { Value = list };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<GenericBaseClass<ICollection>>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.IsNotNull(deserializedValue.Value);
            var dList = deserializedValue.Value as List<ClassWithoutSerializableAttribute>;

            Assert.AreEqual(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.IsNull(dList[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [TestMethod]
        public void SerializeIQueryableContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IQueryable> { Value = list.AsQueryable() };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<GenericBaseClass<IQueryable>>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.IsNotNull(deserializedValue.Value);
            var dList = deserializedValue.Value.Cast<ClassWithoutSerializableAttribute>().ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.IsNull(dList[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [TestMethod]
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
        public void SerializeListOfIQueryableContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new List<GenericBaseClass<IQueryable>> { new GenericBaseClass<IQueryable> { Value = list.AsQueryable() } };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<List<GenericBaseClass<IQueryable>>>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.AreEqual(1, deserializedValue.Count);
            Assert.IsNotNull(deserializedValue[0].Value);
            var dList = deserializedValue[0].Value.Cast<ClassWithoutSerializableAttribute>().ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.IsNull(dList[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [TestMethod]
        public void SerializeArrayOfIQueryableContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IQueryable>[] { new GenericBaseClass<IQueryable> { Value = list.AsQueryable() } };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<GenericBaseClass<IQueryable>[]>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.AreEqual(1, deserializedValue.Length);
            Assert.IsNotNull(deserializedValue[0].Value);
            var dList = deserializedValue[0].Value.Cast<ClassWithoutSerializableAttribute>().ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.IsNull(dList[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [TestMethod]
        public void SerializeListOfMultipleObjects()
        {
            var instance = new List<object> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new CircularReference { Id = 456 }, 1234 };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<List<object>>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.AreEqual(4, deserializedValue.Count);

            instance.ShouldAllBeEquivalentTo(deserializedValue);
        }

        [TestMethod]
        public void SerializeEmptyList()
        {
            var instance = new List<object>();

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<List<object>>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.AreEqual(0, deserializedValue.Count);

            instance.ShouldAllBeEquivalentTo(deserializedValue);
        }

        [TestMethod]
        public void SerializeNullList()
        {
            List<object> instance = null;

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<List<object>>(bytes);

            Assert.IsNull(deserializedValue);

            bytes = Serializer.Serialize(new Wrapper<List<object>> { Value = instance });
            var wrappedDeserializedValue = Deserializer.Deserialize<Wrapper<List<object>>>(bytes);

            Assert.IsNull(wrappedDeserializedValue.Value);
        }

        [TestMethod]
        public void SerializeArrayOfMultipleObjects()
        {
            var instance = new object[] { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new CircularReference { Id = 456 }, 1234 };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<object[]>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.AreEqual(4, deserializedValue.Length);

            instance.ShouldAllBeEquivalentTo(deserializedValue);
        }

        [TestMethod]
        public void SerializeEmptyArray()
        {
            var instance = new object[0];

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<object[]>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.AreEqual(0, deserializedValue.Length);

            instance.ShouldAllBeEquivalentTo(deserializedValue);
        }

        [TestMethod]
        public void SerializeNullArray()
        {
            object[] instance = null;

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<object[]>(bytes);

            Assert.IsNull(deserializedValue);

            bytes = Serializer.Serialize(new Wrapper<object[]> { Value = instance });
            var wrappedDeserializedValue = Deserializer.Deserialize<Wrapper<object[]>>(bytes);

            Assert.IsNull(wrappedDeserializedValue.Value);
        }

        [TestMethod]
        public void SerializeListOfIQueryableDirectly()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            List<IQueryable<ClassWithoutSerializableAttribute>> instance = new List<IQueryable<ClassWithoutSerializableAttribute>> { list.AsQueryable() };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<List<IQueryable<ClassWithoutSerializableAttribute>>>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.AreEqual(1, deserializedValue.Count);
            var deserializedArray = deserializedValue[0].ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, deserializedArray[0].PublicPropertyValue);
            Assert.IsNull(deserializedArray[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, deserializedArray[2].PublicPropertyValue);
        }

        [TestMethod]
        public void SerializeArrayOfIQueryableDirectly()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            IQueryable<ClassWithoutSerializableAttribute>[] instance = new IQueryable<ClassWithoutSerializableAttribute>[] { list.AsQueryable() };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<IQueryable<ClassWithoutSerializableAttribute>[]>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.AreEqual(1, deserializedValue.Length);
            var deserializedArray = deserializedValue[0].ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, deserializedArray[0].PublicPropertyValue);
            Assert.IsNull(deserializedArray[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, deserializedArray[2].PublicPropertyValue);
        }

        [TestMethod]
        public void SerializeNullIQueryableContainedInAClass()
        {
            var instance = new GenericBaseClass<IQueryable> { Value = null };

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<GenericBaseClass<IQueryable>>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.IsNull(deserializedValue.Value);
        }

        [TestMethod]
        public void SerializeObjectWithISerializable()
        {
            var instance = new Dictionary<string, object>();
            instance.Add("Key1", "A");
            instance.Add("Key2", "B");
            instance.Add("Key3", 123);
            instance.Add("Key4", null);
            instance.Add("Key5", new object());
            instance.Add("Key6", new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 });

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<Dictionary<string, object>>(bytes);

            CompareDictionaries(instance,
                                deserializedValue);
        }

        [TestMethod]
        public void SerializeClassWithNonSerializableProperty()
        {
            var instance = new ClassWithNonSerializableField();
            instance.SerializableProperty = 839;
            instance.NonSerializableProperty = 33534;

            var bytes = Serializer.Serialize(instance);
            var deserializedValue = Deserializer.Deserialize<ClassWithNonSerializableField>(bytes);

            Assert.AreEqual(instance.SerializableProperty, deserializedValue.SerializableProperty);
            Assert.AreEqual(0, deserializedValue.NonSerializableProperty);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public unsafe void SerializingPointerTypeIsNotSupported()
        {
            int[] a = new int[5] { 10, 20, 30, 40, 50 };
            fixed (int* p = &a[0])
            {
                var instance = new ClassWithPointer();
                instance.Value = p;
                Serializer.Serialize(instance);
            }
        }

        [TestMethod]
        public void KrakenSerializerShouldWork()
        {
            var instance = new GenericBaseClass<IQueryable> { Value = null };
            
            var bytes = KrakenSerializer.Serialize(instance);
            var deserializedValue = KrakenSerializer.Deserialize<GenericBaseClass<IQueryable>>(bytes);

            Assert.IsNotNull(deserializedValue);
            Assert.IsNull(deserializedValue.Value);
        }

        //[TestMethod]
        //[ExpectedException(typeof(NotSupportedException))]
        //public void SerializingAStreamIsNotSupported()
        //{
        //    using (var sr = new StringReader("abc"))
        //    {
        //        var bytes = Serializer.Serialize(sr);
        //    }
        //}

        //[TestMethod]
        //public void z_AdditionalTestsToImplements()
        //{
            
        //    Assert.Fail();
        //}
    }
}
