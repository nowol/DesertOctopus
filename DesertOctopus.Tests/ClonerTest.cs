using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using DesertOctopus.Exceptions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerializerTests.TestObjects;

namespace DesertOctopus.Tests
{
    [TestClass]
    public class ClonerTest
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
        public void CloneTuple()
        {
            PrimitiveTestSuite<Tuple<int, string>>(new Tuple<int, string>(1, "a"), new Tuple<int, string>(2, "b"));
            PrimitiveTestSuite<Tuple<string, int>>(new Tuple<string, int>("a", 1), new Tuple<string, int>("b", 2));
            PrimitiveTestSuite<Tuple<int, string, bool>>(new Tuple<int, string, bool>(1, "a", true), new Tuple<int, string, bool>(2, "b", false));
        }

        [TestMethod]
        public void CloneUtcDateTime()
        {
            var instance = new Wrapper<DateTime> { Value = DateTime.UtcNow };
            var clonedValue = Cloning.ObjectCloner.Clone(instance);
            Assert.IsNotNull(clonedValue.Value);
            Assert.AreEqual(instance.Value, clonedValue.Value);
            Assert.AreEqual(instance.Value.Kind, clonedValue.Value.Kind);
        }

        [TestMethod]
        public void CloneNullPrimitiveArray()
        {
            int[] nullArray = null;
            var clonedValue = Cloning.ObjectCloner.Clone(nullArray);
            Assert.IsNull(clonedValue);
        }

        [TestMethod]
        public void CloneEmptyObjectArray()
        {
            Object[] emptyArray = new Object[0];
            var clonedValue = Cloning.ObjectCloner.Clone(emptyArray);
            Assert.IsNotNull(clonedValue);
            Assert.AreEqual(0, clonedValue.Length);
        }

        [TestMethod]
        public void CloneNullObjectArray()
        {
            Object[] nullArray = null;
            var clonedValue = Cloning.ObjectCloner.Clone(nullArray);
            Assert.IsNull(clonedValue);
        }

        [TestMethod]
        public void CloneEmptyPrimitiveArray()
        {
            int[] emptyArray = new int[0];
            var clonedValue = Cloning.ObjectCloner.Clone(emptyArray);
            Assert.IsNotNull(clonedValue);
            Assert.AreEqual(0, clonedValue.Length);
        }

        [TestMethod]
        public void CloneObjectArrayWithNullValues()
        {
            var array = new ClassWithoutSerializableAttribute[] { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var clonedValue = Cloning.ObjectCloner.Clone(array);
            Assert.AreEqual(array.Length, clonedValue.Length);
            Assert.AreEqual(array[0].PublicPropertyValue, clonedValue[0].PublicPropertyValue);
            Assert.IsNull(clonedValue[1]);
            Assert.AreEqual(array[2].PublicPropertyValue, clonedValue[2].PublicPropertyValue);
        }

        [TestMethod]
        public void CloneNullPrimitiveList()
        {
            List<int> nullList = null;
            var clonedValue = Cloning.ObjectCloner.Clone(nullList);
            Assert.IsNull(clonedValue);
        }

        [TestMethod]
        public void CloneEmptyObjectList()
        {
            List<Object> emptyList = new List<Object>();
            var clonedValue = Cloning.ObjectCloner.Clone(emptyList);
            Assert.IsNotNull(clonedValue);
            Assert.AreEqual(0, clonedValue.Count);
        }

        [TestMethod]
        public void CloneNullObjectList()
        {
            List<object> nullList = null;
            var clonedValue = Cloning.ObjectCloner.Clone(nullList);
            Assert.IsNull(clonedValue);
        }

        [TestMethod]
        public void CloneEmptyPrimitiveList()
        {
            List<int> emptyList = new List<int>();
            var clonedValue = Cloning.ObjectCloner.Clone(emptyList);
            Assert.IsNotNull(clonedValue);
            Assert.AreEqual(0, clonedValue.Count);
        }

        [TestMethod]
        public void CloneObjectListWithNullValues()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var clonedValue = Cloning.ObjectCloner.Clone(list);
            Assert.AreEqual(list.Count, clonedValue.Count);
            Assert.AreEqual(list[0].PublicPropertyValue, clonedValue[0].PublicPropertyValue);
            Assert.IsNull(clonedValue[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, clonedValue[2].PublicPropertyValue);
        }

        public void PrimitiveTestSuite<T>(params T[] values)
        {
            foreach (var value in values)
            {
                CloneWrappedValue(value);
                CloneArrayOfOne(value);
                CloneListOfOne(value);
            }
            CloneArray<T>(values);
            CloneList<T>(values);
        }

        private static void CloneWrappedValue<T>(T value)
        {
            var wrappedObject = new Wrapper<T>
            {
                Value = value
            };

            Type targetType = typeof(T);
            var clonedValue = Cloning.ObjectCloner.Clone(wrappedObject);

            Assert.AreEqual(wrappedObject.Value,
                            clonedValue.Value,
                            string.Format("Type {0} does not have the same value after being deserialized.",
                                            targetType));

        }

        private static void CloneArray<T>(T[] value)
        {
            var clonedValue = Cloning.ObjectCloner.Clone(value);

            CollectionAssert.AreEquivalent(value,
                                           clonedValue);
        }

        private static void CloneArrayOfOne<T>(T value)
        {
            var array = new T[1];
            array[0] = value;

            var clonedValue = Cloning.ObjectCloner.Clone(array);
            Assert.IsNotNull(clonedValue, "Type: " + typeof(T));
            CollectionAssert.AreEquivalent(array,
                                            clonedValue, "Type: " + typeof(T));
        }

        private static void CloneListOfOne<T>(T value)
        {
            var list = new List<T>
                        {
                            value
                        };

            var clonedValue = Cloning.ObjectCloner.Clone(list);

            CollectionAssert.AreEquivalent(list,
                                            clonedValue);
        }

        private static void CloneList<T>(T[] value)
        {
            var list = value.ToList();

            var clonedValue = Cloning.ObjectCloner.Clone(list);

            CollectionAssert.AreEquivalent(list,
                                            clonedValue);
        }

        [TestMethod]
        public void CloneStruct()
        {
            var instance = new Wrapper<StructForTesting> { Value = new StructForTesting { Value = 1 } };
            var clonedValue = Cloning.ObjectCloner.Clone(instance);
            Assert.AreEqual(instance.Value, clonedValue.Value);
        }

        [TestMethod]
        public void CloneShortString()
        {
            var clonedValue = Cloning.ObjectCloner.Clone("abc");
            //Assert.AreEqual(5, bytes.Length);
            Assert.AreEqual("abc", clonedValue);
        }

        [TestMethod]
        public void CloneMediumString()
        {
            var str = RandomString(3000);
            var clonedValue = Cloning.ObjectCloner.Clone(str);
            Assert.AreEqual(str, clonedValue);
        }

        [TestMethod]
        public void CloneLongString()
        {
            var str = RandomString(100000);
            var clonedValue = Cloning.ObjectCloner.Clone(str);
            Assert.AreEqual(str, clonedValue);
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        
        [TestMethod]
        public void CloneTestClass()
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

            var clonedValue = Cloning.ObjectCloner.Clone(classInstance);

            Assert.AreEqual(classInstance.PublicFieldValue,
                            clonedValue.PublicFieldValue);
            Assert.AreEqual(classInstance.GetPrivateFieldValue(),
                            clonedValue.GetPrivateFieldValue());
            Assert.AreEqual(classInstance.InternalFieldValue,
                            clonedValue.InternalFieldValue);
            Assert.AreEqual(classInstance.PublicPropertyValue,
                            clonedValue.PublicPropertyValue);
            Assert.AreEqual(classInstance.GetPrivatePropertyValue(),
                            clonedValue.GetPrivatePropertyValue());
            Assert.AreEqual(classInstance.InternalPropertyValue,
                            clonedValue.InternalPropertyValue);
        }

        [TestMethod]
        //[ExpectedException(typeof(ObjectExistsInCurrentSerializationGraphException))]
        public void CloneCircularReference()
        {
            var instance1 = new CircularReference { Id = 1 };
            var instance2 = new CircularReference { Id = 2 };
            instance1.Child = instance2;
            instance2.Parent = instance1;

            var clonedValue = Cloning.ObjectCloner.Clone(instance1);

            Assert.IsTrue(ReferenceEquals(clonedValue,
                                          clonedValue.Child.Parent));

            Assert.AreEqual(1, clonedValue.Id);
            Assert.AreEqual(2, clonedValue.Child.Id);
            Assert.AreEqual(1, clonedValue.Child.Parent.Id);
        }

        [TestMethod]
        public void CloneTrackMultipleReference()
        {
            var instance = new ClassWithGenericInt(123);
            var list = new List<ClassWithGenericInt>
                        {
                            instance,
                            instance
                        };

            var clonedValue = Cloning.ObjectCloner.Clone(list);

            Assert.AreEqual(list.Count,
                            clonedValue.Count);
            CollectionAssert.AreEquivalent(list,
                                            clonedValue);
        }

        [TestMethod]
        public void CloneTrackSamePrimitiveMultipleTimes()
        {
            // this case exists to make sure that the ReferenceWatcher only tracks classes

            var instance = 3;
            var list = new List<int>
                        {
                            instance,
                            instance
                        };

            var clonedValue = Cloning.ObjectCloner.Clone(list);

            Assert.AreEqual(list.Count,
                            clonedValue.Count);
            CollectionAssert.AreEquivalent(list,
                                            clonedValue);
        }

        [TestMethod]
        public void CloneClassWithoutSerializableAttribute()
        {
            var instance = new ClassWithoutSerializableAttribute
            {
                PublicPropertyValue = 4
            };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.PublicPropertyValue,
                            clonedValue.PublicPropertyValue);
        }

        [TestMethod]
        public void CloneClassWithGenericBase()
        {
            var instance = new ClassWithGenericInt()
            {
                Value = 4
            };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Value,
                            clonedValue.Value);
        }

        [TestMethod]
        public void CloneGenericClass()
        {
            var instance = new GenericBaseClass<int>()
            {
                Value = 4
            };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Value,
                            clonedValue.Value);
        }

        [TestMethod]
        public void CloneDictionaryStringObject()
        {
            var instance = new Dictionary<string, object>()
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Count,
                            clonedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            clonedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                clonedValue[kvp.Key]);
            }
        }

        [TestMethod]
        public void CloneDictionaryIntString()
        {
            var instance = new Dictionary<int, string>()
            {
                {1, "Value1"},
                {2, "Value2"},
                {3, "Value3"}
            };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Count,
                            clonedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            clonedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                clonedValue[kvp.Key]);
            }
        }

        [TestMethod]
        public void CloneDictionaryGuidString()
        {
            var instance = new Dictionary<Guid, string>()
            {
                {Guid.NewGuid(), "Value1"},
                {Guid.NewGuid(), "Value2"},
                {Guid.NewGuid(), "Value3"}
            };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Count,
                            clonedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            clonedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                clonedValue[kvp.Key]);
            }
        }

        [TestMethod]
        public void CloneCustomDictionary()
        {
            var instance = new CustomDictionary
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Count,
                            clonedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            clonedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                clonedValue[kvp.Key]);
            }
        }

        [TestMethod]
        public void CloneWrappedCustomDictionary()
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

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Value.Count,
                            clonedValue.Value.Count);
            CollectionAssert.AreEquivalent(instance.Value.Keys,
                                            clonedValue.Value.Keys);
            foreach (var kvp in instance.Value)
            {
                Assert.AreEqual(kvp.Value,
                                clonedValue.Value[kvp.Key]);
            }
        }

        [TestMethod]
        public void CloneCustomDictionaryWithComparer()
        {
            var instance = new CustomDictionary(StringComparer.CurrentCultureIgnoreCase)
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Comparer.GetType(),
                            clonedValue.Comparer.GetType());
            Assert.AreEqual(instance.Count,
                            clonedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            clonedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                clonedValue[kvp.Key]);
            }
        }

        [TestMethod]
        public void CloneDictionaryWithCustomComparer()
        {
            var instance = new Dictionary<StructForTesting, object>(new StructForTestingComparer())
            {
                {new StructForTesting { Value = 1 }, 123},
                {new StructForTesting { Value = 2 }, "abc"},
                {new StructForTesting { Value = 3 }, new ClassWithGenericInt(3) },
            };


            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Comparer.GetType(),
                            clonedValue.Comparer.GetType());
            Assert.AreEqual(instance.Count,
                            clonedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            clonedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                clonedValue[kvp.Key]);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MissingConstructorException))]
        public void CloneCustomDictionaryWithoutPublicParameterlessConstructor()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructor<object, object>()
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
                {"Key4", null },
            };

            Cloning.ObjectCloner.Clone(instance);
        }

        [TestMethod]
        public void CloneCustomDictionaryWithAdditionalPropertiesWithCallback()
        {
            var instance = new CustomDictionaryWithAdditionalPropertiesWithOverridingOnDeserializedCallback
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };
            instance.SomeProperty = 849;

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.SomeProperty,
                            clonedValue.SomeProperty);
            Assert.AreEqual(instance.Count,
                            clonedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            clonedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                clonedValue[kvp.Key]);
            }
        }

        [TestMethod]
        public void CloneCustomDictionaryWithAdditionalPropertiesWithoutCallback()
        {
            var instance = new CustomDictionaryWithAdditionalPropertiesWithoutOverridingOnDeserializedCallback
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };
            instance.SomeProperty = 849;

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(0,
                            clonedValue.SomeProperty); // default value for property
            Assert.AreEqual(instance.Count,
                            clonedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            clonedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                clonedValue[kvp.Key]);
            }
        }

        [TestMethod]
        public void CloneCustomDictionaryWithDictionaryProperty()
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

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            CompareDictionaries(instance,
                                clonedValue);
            CompareDictionaries(instance.SwitchedDictionary,
                                clonedValue.SwitchedDictionary);
        }

        private static void CompareDictionaries<TKey, TValue>(Dictionary<TKey, TValue> instance, Dictionary<TKey, TValue> clonedValue)
        {
            Assert.AreEqual(instance.Count,
                            clonedValue.Count);
            Assert.AreEqual(instance.Comparer.GetType(),
                            clonedValue.Comparer.GetType());
            CollectionAssert.AreEquivalent(instance.Keys,
                                           clonedValue.Keys);
            foreach (var key in instance.Keys)
            {
                if (instance[key] != null
                    && instance[key].GetType() == typeof(object))
                {
                    Assert.IsTrue(clonedValue[key] != null && clonedValue[key].GetType() == typeof(object));
                }
                else if (instance[key] != null
                    && instance[key].GetType() == typeof(ClassWithoutSerializableAttribute))
                {
                    Assert.AreEqual(((ClassWithoutSerializableAttribute)(object)instance[key]).PublicPropertyValue,
                                    ((ClassWithoutSerializableAttribute)(object)clonedValue[key]).PublicPropertyValue);
                }
                else
                {
                    Assert.AreEqual(instance[key],
                                    clonedValue[key]);
                }
            }
        }

        [TestMethod]
        public void CloneCustomDictionaryWithAdditionalPropertiesAndGenerics()
        {
            var instance = new CustomDictionaryWithAdditionalPropertiesAndGenerics<string, int>
            {
                {"Key1", 123},
                {"Key2", 456},
                {"Key3", 789},
            };
            instance.SomeProperty = 849;

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.SomeProperty,
                            clonedValue.SomeProperty);
            Assert.AreEqual(instance.Count,
                            clonedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            clonedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                clonedValue[kvp.Key]);
            }
        }

        [TestMethod]
        public void CloneListWithMultipleTypes()
        {
            var list = new List<IHierarchy>
            {
                new ChildIntHierarchy(123),
                new ChildStringHierarchy("abc"),
            };

            var clonedValue = Cloning.ObjectCloner.Clone(list);

            Assert.AreEqual(list.Count,
                            clonedValue.Count);
            Assert.AreEqual(list.OfType<ChildIntHierarchy>().First().Value,
                            clonedValue.OfType<ChildIntHierarchy>().First().Value);
            Assert.AreEqual(list.OfType<ChildStringHierarchy>().First().Value,
                            clonedValue.OfType<ChildStringHierarchy>().First().Value);
        }

        [TestMethod]
        public void CloneObjectWithListAsIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 } };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Items.Count(),
                            clonedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            clonedValue.Items.ToList());
        }

        [TestMethod]
        public void CloneObjectWithHashsetAsIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int>
            {
                Items = new HashSet<int> { 1, 2, 3 }
            };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Items.Count(),
                            clonedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            clonedValue.Items.ToList());
        }

        [TestMethod]
        public void CloneHashSetWithEqualityComparer()
        {
            var instance = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "a", "b", "C" };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Comparer.GetType(),
                            clonedValue.Comparer.GetType());
            Assert.AreEqual(instance.Count(),
                            clonedValue.Count());
            CollectionAssert.AreEquivalent(instance.ToList(),
                                            clonedValue.ToList());
        }

        [TestMethod]
        public void CloneObjectWithEnumProperty()
        {
            var instance = new GenericBaseClass<EnumForTesting> { Value = EnumForTesting.Two };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Value,
                            clonedValue.Value);
        }

        [TestMethod]
        public void CloneHashtable()
        {
            var instance = new Hashtable
                            {
                                {1, 2},
                                {"a", "b"},
                            };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Count,
                            clonedValue.Count);
            CollectionAssert.AreEquivalent(instance,
                                            clonedValue);
        }

        [TestMethod]
        public void CloneHashtableWithEqualityComparer()
        {
            var instance = new Hashtable(StringComparer.CurrentCultureIgnoreCase)
                            {
                                {"e", 2},
                                {"a", "b"},
                            };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(GetHashTableComparer(instance));
            Assert.IsNotNull(GetHashTableComparer(clonedValue));
            Assert.AreEqual(GetHashTableComparer(instance).GetType(),
                            GetHashTableComparer(clonedValue).GetType());
            Assert.AreEqual(instance.Count,
                            clonedValue.Count);
            CollectionAssert.AreEquivalent(instance,
                                            clonedValue);
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
        public void CloneObjectWithArrayAsIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new[] { 1, 2, 3 } };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Items.Count(),
                            clonedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            clonedValue.Items.ToList());
        }

        [TestMethod]
        public void CloneObjectWithDistinctIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 1, 2, 3 }.Distinct() };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Items.Count(),
                            clonedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            clonedValue.Items.ToList());
        }

        [TestMethod]
        public void CloneObjectWithWhereIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Where(x => x > 1) };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Items.Count(),
                            clonedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            clonedValue.Items.ToList());
        }

        [TestMethod]
        public void CloneObjectWithOrderByIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.OrderBy(x => x) };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Items.Count(),
                            clonedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            clonedValue.Items.ToList());
        }

        [TestMethod]
        public void CloneObjectWithDefaultIfEmptyIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.DefaultIfEmpty(123) };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Items.Count(),
                            clonedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            clonedValue.Items.ToList());
        }

        [TestMethod]
        public void CloneObjectWithExceptIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Except(new[] { 2 }) };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Items.Count(),
                            clonedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            clonedValue.Items.ToList());
        }

        [TestMethod]
        public void CloneObjectWithUnionIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Union(new[] { 4 }) };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Items.Count(),
                            clonedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            clonedValue.Items.ToList());
        }

        [TestMethod]
        public void CloneObjectWithIntersectIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Intersect(new[] { 2 }) };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Items.Count(),
                            clonedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            clonedValue.Items.ToList());
        }

        [TestMethod]
        public void CloneObjectWithOfTypeIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.OfType<int>() };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Items.Count(),
                            clonedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            clonedValue.Items.ToList());
        }

        [TestMethod]
        public void CloneObjectWithSkipByIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Skip(1) };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Items.Count(),
                            clonedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            clonedValue.Items.ToList());
        }

        [TestMethod]
        public void CloneObjectWithTakeByIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Take(1) };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Items.Count(),
                            clonedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            clonedValue.Items.ToList());
        }

        [TestMethod]
        public void CloneObjectWithSelectByIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Select(x => x * 2) };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.Items.Count(),
                            clonedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            clonedValue.Items.ToList());
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CloneSimpleFunc()
        {
            var testData = new List<int> { 1, 2, 3, 4, 5, 6 };
            System.Func<int, bool> instance = x => x > 3;

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);

            Assert.AreEqual(testData.Count(x => instance(x)),
                            testData.Count(x => clonedValue(x)));
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CloneSimpleExpression()
        {
            var testData = new List<int> { 1, 2, 3, 4, 5, 6 };

            Expression<System.Func<int, bool>> instance = x => x > 3;

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);

            Assert.AreEqual(testData.Count(instance.Compile()),
                            testData.Count(clonedValue.Compile()));
            Assert.Fail("we should not allow serialization of Expression");
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CloneAnonymousObject()
        {
            var instance = new { Property1 = "hello", Property2 = 123 };
            var clonedValue = Cloning.ObjectCloner.Clone(instance);
            Assert.AreEqual(instance.Property1, clonedValue.Property1);
            Assert.AreEqual(instance.Property2, clonedValue.Property2);

            Assert.Fail("we should not allow serialization of anonymous types");
        }

        [TestMethod]
        public void CloneArray()
        {
            var instance = new int[] { 123, 456 };
            var clonedValue = Cloning.ObjectCloner.Clone(instance);
            CollectionAssert.AreEqual(instance, clonedValue);
        }

        [TestMethod]
        public void CloneArrayWithNullablePrimitive()
        {
            var instance = new int?[] { 123, null, 456 };
            var clonedValue = Cloning.ObjectCloner.Clone(instance);
            CollectionAssert.AreEqual(instance, clonedValue);
        }

        [TestMethod]
        public void CloneArrayObject()
        {
            var instance = new ClassWithoutSerializableAttribute[] { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var clonedValue = Cloning.ObjectCloner.Clone(instance);
            Assert.AreEqual(2, clonedValue.Length);
            Assert.AreEqual(123, clonedValue[0].PublicPropertyValue);
            Assert.AreEqual(456, clonedValue[1].PublicPropertyValue);
        }

        [TestMethod]
        public void CloneArrayOfClassWithGenericInt()
        {
            var obj = new ClassWithGenericInt(123);
            var instance = new ClassWithGenericInt[] { obj, null };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            instance.ShouldAllBeEquivalentTo(clonedValue);
        }

        [TestMethod]
        public void CloneArrayObjectHierarchy()
        {
            var instance = new SomeBaseClass[] { new ClassWithGenericInt(123), new ClassWithGenericDouble(3.38D) };
            var clonedValue = Cloning.ObjectCloner.Clone(instance);
            Assert.AreEqual(2, clonedValue.Length);
            Assert.AreEqual(123, (clonedValue[0] as ClassWithGenericInt).Value);
            Assert.AreEqual(3.38D, (clonedValue[1] as ClassWithGenericDouble).Value);
        }

        [TestMethod]
        public void CloneSameObjectMultipleTimeInArray()
        {
            var obj123 = new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 };
            var instance = new ClassWithoutSerializableAttribute[] { obj123, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 }, obj123 };
            var clonedValue = Cloning.ObjectCloner.Clone(instance);
            Assert.AreEqual(3, clonedValue.Length);
            Assert.AreEqual(123, clonedValue[0].PublicPropertyValue);
            Assert.AreEqual(456, clonedValue[1].PublicPropertyValue);
            Assert.AreEqual(123, clonedValue[2].PublicPropertyValue);
            Assert.IsTrue(ReferenceEquals(clonedValue[0], clonedValue[2]));
        }

        [TestMethod]
        public void CloneTwoDimensionalArray()
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

            var clonedValue = Cloning.ObjectCloner.Clone(instance);
            instance.Should().BeEquivalentTo(clonedValue);
        }

        [TestMethod]
        public void CloneMultiDimensionalArray()
        {
            var instance = CreateMultiDimensionalArray<int>(8);
            SeedArray(instance);

            var clonedValue = Cloning.ObjectCloner.Clone(instance);
            instance.Should().BeEquivalentTo(clonedValue);
        }

        [TestMethod]
        public void CloneMultiDimensionalArrayOfObjects()
        {
            var instance = CreateMultiDimensionalArray<Object>(2);
            instance.SetValue(new object(), new[] { 0, 0 });
            instance.SetValue(new object(), new[] { 1, 0 });

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsTrue(clonedValue.GetValue(new[] { 0, 0 }).GetType() == typeof(object));
            Assert.IsNull(clonedValue.GetValue(new[] { 0, 1 }));
            Assert.IsTrue(clonedValue.GetValue(new[] { 1, 0 }).GetType() == typeof(object));
            Assert.IsNull(clonedValue.GetValue(new[] { 1, 1 }));
        }

        [TestMethod]
        public void CloneMultiDimensionalArrayOfClass()
        {
            var instance = CreateMultiDimensionalArray<ClassWithGenericInt>(2);
            var obj = new ClassWithGenericInt(123);
            instance.SetValue(obj, new[] { 0, 0 });
            instance.SetValue(new ClassWithGenericInt(456), new[] { 1, 0 });
            instance.SetValue(obj, new[] { 1, 1 });

            var clonedValue = Cloning.ObjectCloner.Clone(instance);
            instance.Should().BeEquivalentTo(clonedValue);
            Assert.AreEqual(456, ((ClassWithGenericInt)clonedValue.GetValue(1, 0)).Value);
            Assert.IsTrue(ReferenceEquals(clonedValue.GetValue(0, 0), clonedValue.GetValue(1, 1)));
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
        public void CloneJaggedArrayObject()
        {
            var instance = new object[2][];
            instance[0] = new object[] { new object(), null };
            instance[1] = new object[] { new object(), null };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsTrue(clonedValue[0][0].GetType() == typeof(object));
            Assert.IsNull(clonedValue[0][1]);
            Assert.IsTrue(clonedValue[1][0].GetType() == typeof(object));
            Assert.IsNull(clonedValue[1][1]);
        }


        [TestMethod]
        public void CloneJaggedArrayClass()
        {
            var obj = new ClassWithGenericInt(123);
            var instance = new ClassWithGenericInt[2][];
            instance[0] = new ClassWithGenericInt[] { obj, null };
            //instance[0] = new ClassWithGenericInt[] { obj, new ClassWithGenericInt(4563) };
            instance[1] = new ClassWithGenericInt[] { new ClassWithGenericInt(456), obj };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            instance.ShouldAllBeEquivalentTo(clonedValue);

            Assert.AreEqual(456, ((ClassWithGenericInt)clonedValue[1][0]).Value);
            Assert.IsTrue(ReferenceEquals(clonedValue[0][0], clonedValue[1][1]));

            instance = new ClassWithGenericInt[2][];
            instance[0] = new ClassWithGenericInt[] { obj, new ClassWithGenericInt(4563) };
            instance[1] = new ClassWithGenericInt[] { new ClassWithGenericInt(456), obj };

            clonedValue = Cloning.ObjectCloner.Clone(instance);

            instance.ShouldAllBeEquivalentTo(clonedValue);
        }


        [TestMethod]
        public void CloneJaggedArrayClassInheritance()
        {
            var obj = new ClassWithGenericInt(123);
            var instance = new SomeBaseClass[2][];
            instance[0] = new SomeBaseClass[] { obj, new ClassWithGenericDouble(123.3D) };
            instance[1] = new SomeBaseClass[] { new ClassWithGenericInt(456), obj };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(123, ((ClassWithGenericInt)clonedValue[0][0]).Value);
            Assert.AreEqual(123.3D, ((ClassWithGenericDouble)clonedValue[0][1]).Value);
            Assert.AreEqual(456, ((ClassWithGenericInt)clonedValue[1][0]).Value);
            Assert.IsTrue(ReferenceEquals(clonedValue[0][0], clonedValue[1][1]));
        }


        [TestMethod]
        public void CloneNullJaggedArray()
        {
            int[][] nullArray = null;
            var instance = new Wrapper<int[][]> { Value = nullArray };
            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNull(clonedValue.Value);
        }

        [TestMethod]
        public void CloneThreeDimensionJaggedArray()
        {
            int[][][] instance = new [] { new int[2][], new int[3][] };
            instance[0][0] = new int[] { 123, 238 };
            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            instance.ShouldAllBeEquivalentTo(clonedValue);
        }

        [TestMethod]
        public void CloneEmptyJaggedArray()
        {
            int[][] instance = new int[2][];
            var clonedValue = Cloning.ObjectCloner.Clone(instance);
            instance.ShouldAllBeEquivalentTo(clonedValue);
        }

        [TestMethod]
        public void CloneJaggedArray()
        {
            var instance = new int[][] { new int[] { 123, 238 }, new int[] { 456, 546, 784 }, null };
            var clonedValue = Cloning.ObjectCloner.Clone(instance);
            Assert.AreEqual(instance.Length, clonedValue.Length);
            instance.ShouldAllBeEquivalentTo(clonedValue);
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

            var clonedValue = Cloning.ObjectCloner.Clone(instance1);

            CollectionAssert.AreEqual(arr, clonedValue.Child.Ids);
            Assert.IsTrue(ReferenceEquals(clonedValue.Child.Ids, clonedValue.Child.Child.Ids));
        }

        [TestMethod]
        public void CloneClassWithDynamicObject()
        {
            ClassWithDynamicProperty instance = new ClassWithDynamicProperty { Value = 123 };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);
            Assert.IsNotNull(clonedValue);
            Assert.AreEqual(instance.Value, clonedValue.Value);
        }

        //[TestMethod]
        //[ExpectedException(typeof(TypeNotFoundException))]
        //public void DeserializeUnknownType()
        //{
        //    ClassWithDynamicProperty instance = new ClassWithDynamicProperty { Value = 123 };
        //    var clonedValue = ObjectCloner.Clone(instance);
        //    bytes[25] = 54;

        //    var clonedValue = Deserializer.Deserialize<ClassWithDynamicProperty>(bytes);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(TypeWasModifiedSinceSerializationException))]
        //public void DeserializeTypeThatWasModified()
        //{
        //    ClassWithDynamicProperty instance = new ClassWithDynamicProperty { Value = 123 };
        //    var clonedValue = ObjectCloner.Clone(instance);

        //    var needle = SerializedTypeResolver.GetShortNameFromType(typeof(ClassWithDynamicProperty));
        //    var index = System.Text.Encoding.UTF8.GetString(bytes).IndexOf(needle);

        //    // this is a hackish way to change the hashcode of a serialized object
        //    // if the way/order (currently TypeName + Hash) that an object is serialized changes the line below will need to be modified to target a byte of the hashcode
        //    bytes[index + needle.Length + 1] = (bytes[index + needle.Length + 1] == 255) ? (byte)0 : (byte)(bytes[index + needle.Length] + 1); // change the hashcode to something invalid

        //    var clonedValue = Deserializer.Deserialize<ClassWithDynamicProperty>(bytes);
        //}

        [TestMethod]
        public void CloneExpandoObject()
        {
            dynamic instance = new ExpandoObject();
            instance.Property1 = 123;
            instance.Property2 = "abc";
            instance.Property3 = new ClassWithGenericInt(349);
            instance.Property4 = new object();
            instance.Property5 = null;

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);

            Assert.AreEqual(instance.Property1,
                            clonedValue.Property1);
            Assert.AreEqual(instance.Property2,
                            clonedValue.Property2);
            Assert.AreEqual(instance.Property3,
                            clonedValue.Property3);
            Assert.IsTrue(clonedValue.Property4.GetType() == typeof(object));
            Assert.IsNull(clonedValue.Property5);

        }

        [TestMethod]
        public void CloneEmptyExpandoObject()
        {
            dynamic instance = new ExpandoObject();

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
            Assert.AreEqual(0, (clonedValue as IDictionary<string, object>).Count);
        }

        [TestMethod]
        public void CloneNullExpandoObject()
        {
            ExpandoObject instance = null;

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNull(clonedValue);

            var wrappedclonedValue2 = Cloning.ObjectCloner.Clone(new Wrapper<ExpandoObject> { Value = instance });

            Assert.IsNull(wrappedclonedValue2.Value);
        }

        [TestMethod]
        public void CloneTypedQueue()
        {
            var instance = new Queue<int>();
            instance.Enqueue(1);
            instance.Enqueue(2);
            instance.Enqueue(3);

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);

            Assert.AreEqual(instance.Count,
                            clonedValue.Count);
            CollectionAssert.AreEquivalent(instance.ToArray(),
                                            clonedValue.ToArray());
        }

        [TestMethod]
        public void CloneUntypedQueue()
        {
            var instance = new Queue();
            instance.Enqueue(1);
            instance.Enqueue(2);
            instance.Enqueue("abc");
            instance.Enqueue(new ClassWithGenericInt(123));

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);

            Assert.AreEqual(instance.Count,
                            clonedValue.Count);
            CollectionAssert.AreEquivalent(instance.ToArray(),
                                            clonedValue.ToArray());
        }

        [TestMethod]
        public void CloneTypedStack()
        {
            var instance = new Stack<int>();
            instance.Push(1);
            instance.Push(2);
            instance.Push(3);

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);

            Assert.AreEqual(instance.Count,
                            clonedValue.Count);
            CollectionAssert.AreEquivalent(instance.ToArray(),
                                            clonedValue.ToArray());
        }

        [TestMethod]
        public void CloneUntypedStack()
        {
            var instance = new Stack();
            instance.Push(1);
            instance.Push(2);
            instance.Push("abc");
            instance.Push(new ClassWithGenericInt(123));

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);

            Assert.AreEqual(instance.Count,
                            clonedValue.Count);
            CollectionAssert.AreEquivalent(instance.ToArray(),
                                            clonedValue.ToArray());
        }

        [TestMethod]
        public void CloneInParallel()
        {
            Cloning.ObjectCloner.ClearTypeCache(); // empty the Type cache to start from a fresh state.

            for (int i = 0; i < 100; i++)
            {
                Parallel.For(0,
                             1000,
                             k =>
                             {
                                 var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 1, 2, 3 }.Distinct() };

                                 var clonedValue = Cloning.ObjectCloner.Clone(instance);

                                 Assert.AreEqual(instance.Items.Count(),
                                                 clonedValue.Items.Count());
                                 CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                                             clonedValue.Items.ToList());
                             });
            }
        }

        [TestMethod]
        public void CloneEnumEqualityComparer()
        {
            var instance = new Dictionary<EnumForTesting, int> { { EnumForTesting.One, 1 }, { EnumForTesting.Two, 2 } };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);

            CompareDictionaries(instance,
                                clonedValue);
        }

        [TestMethod]
        public void CloneAnObject()
        {
            var instance = new object();

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
        }

        [TestMethod]
        public void CloneAClassWithANullObjectProperty()
        {
            var instance = new ClassWithObjectProperty();

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
            Assert.IsNull(clonedValue.Obj);
        }

        [TestMethod]
        public void CloneAClassWithANotNullObjectProperty()
        {
            var instance = new ClassWithObjectProperty { Obj = new object() };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
            Assert.IsNotNull(clonedValue.Obj);
        }

        [TestMethod]
        public void CloneAClassWithAnBoxedInt()
        {
            var instance = new ClassWithObjectProperty { Obj = 123 };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
            Assert.AreEqual(123, (int)clonedValue.Obj);
        }

        [TestMethod]
        public void CloneIList()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IList> { Value = list };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
            Assert.IsNotNull(clonedValue.Value);
            var dList = clonedValue.Value as List<ClassWithoutSerializableAttribute>;

            Assert.AreEqual(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.IsNull(dList[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [TestMethod]
        public void CloneICollection()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<ICollection> { Value = list };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
            Assert.IsNotNull(clonedValue.Value);
            var dList = clonedValue.Value as List<ClassWithoutSerializableAttribute>;

            Assert.AreEqual(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.IsNull(dList[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [TestMethod]
        public void CloneIQueryableContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IQueryable> { Value = list.AsQueryable() };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
            Assert.IsNotNull(clonedValue.Value);
            var dList = clonedValue.Value.Cast<ClassWithoutSerializableAttribute>().ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.IsNull(dList[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [TestMethod]
        public void CloneIQueryableDirectly()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            IQueryable<ClassWithoutSerializableAttribute> instance = list.AsQueryable();

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
            Assert.IsNotNull(clonedValue);
            var deserializedArray = clonedValue.ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, deserializedArray[0].PublicPropertyValue);
            Assert.IsNull(deserializedArray[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, deserializedArray[2].PublicPropertyValue);
        }

        [TestMethod]
        public void CloneListOfIQueryableContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new List<GenericBaseClass<IQueryable>> { new GenericBaseClass<IQueryable> { Value = list.AsQueryable() } };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
            Assert.AreEqual(1, clonedValue.Count);
            Assert.IsNotNull(clonedValue[0].Value);
            var dList = clonedValue[0].Value.Cast<ClassWithoutSerializableAttribute>().ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.IsNull(dList[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [TestMethod]
        public void CloneArrayOfIQueryableContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IQueryable>[] { new GenericBaseClass<IQueryable> { Value = list.AsQueryable() } };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
            Assert.AreEqual(1, clonedValue.Length);
            Assert.IsNotNull(clonedValue[0].Value);
            var dList = clonedValue[0].Value.Cast<ClassWithoutSerializableAttribute>().ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.IsNull(dList[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [TestMethod]
        public void CloneListOfMultipleObjects()
        {
            var instance = new List<object> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new CircularReference { Id = 456 }, 1234 };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
            Assert.AreEqual(4, clonedValue.Count);

            instance.ShouldAllBeEquivalentTo(clonedValue);
        }

        [TestMethod]
        public void CloneEmptyList()
        {
            var instance = new List<object>();

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
            Assert.AreEqual(0, clonedValue.Count);

            instance.ShouldAllBeEquivalentTo(clonedValue);
        }

        [TestMethod]
        public void CloneNullList()
        {
            List<object> instance = null;

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNull(clonedValue);

            var wrappedclonedValue = Cloning.ObjectCloner.Clone(new Wrapper<List<object>> { Value = instance });

            Assert.IsNull(wrappedclonedValue.Value);
        }

        [TestMethod]
        public void CloneArrayOfMultipleObjects()
        {
            var instance = new object[] { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new CircularReference { Id = 456 }, 1234 };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
            Assert.AreEqual(4, clonedValue.Length);

            instance.ShouldAllBeEquivalentTo(clonedValue);
        }

        [TestMethod]
        public void CloneEmptyArray()
        {
            var instance = new object[0];

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
            Assert.AreEqual(0, clonedValue.Length);

            instance.ShouldAllBeEquivalentTo(clonedValue);
        }

        [TestMethod]
        public void CloneNullArray()
        {
            object[] instance = null;

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNull(clonedValue);

            var wrappedclonedValue = Cloning.ObjectCloner.Clone(new Wrapper<object[]> { Value = instance });

            Assert.IsNull(wrappedclonedValue.Value);
        }

        [TestMethod]
        public void CloneListOfIQueryableDirectly()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            List<IQueryable<ClassWithoutSerializableAttribute>> instance = new List<IQueryable<ClassWithoutSerializableAttribute>> { list.AsQueryable() };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
            Assert.AreEqual(1, clonedValue.Count);
            var deserializedArray = clonedValue[0].ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, deserializedArray[0].PublicPropertyValue);
            Assert.IsNull(deserializedArray[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, deserializedArray[2].PublicPropertyValue);
        }

        [TestMethod]
        public void CloneArrayOfIQueryableDirectly()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            IQueryable<ClassWithoutSerializableAttribute>[] instance = new IQueryable<ClassWithoutSerializableAttribute>[] { list.AsQueryable() };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
            Assert.AreEqual(1, clonedValue.Length);
            var deserializedArray = clonedValue[0].ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, deserializedArray[0].PublicPropertyValue);
            Assert.IsNull(deserializedArray[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, deserializedArray[2].PublicPropertyValue);
        }

        [TestMethod]
        public void CloneNullIQueryableContainedInAClass()
        {
            var instance = new GenericBaseClass<IQueryable> { Value = null };

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.IsNotNull(clonedValue);
            Assert.IsNull(clonedValue.Value);
        }

        [TestMethod]
        public void CloneObjectWithISerializable()
        {
            var instance = new Dictionary<string, object>();
            instance.Add("Key1", "A");
            instance.Add("Key2", "B");
            instance.Add("Key3", 123);
            instance.Add("Key4", null);
            instance.Add("Key5", new object());
            instance.Add("Key6", new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 });

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            CompareDictionaries(instance,
                                clonedValue);
        }

        [TestMethod]
        public void CloneClassWithNonSerializableProperty()
        {
            var instance = new ClassWithNonSerializableField();
            instance.SerializableProperty = 839;
            instance.NonSerializableProperty = 33534;

            var clonedValue = Cloning.ObjectCloner.Clone(instance);

            Assert.AreEqual(instance.SerializableProperty, clonedValue.SerializableProperty);
            Assert.AreEqual(0, clonedValue.NonSerializableProperty);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public unsafe void ClonePointerTypeIsNotSupported()
        {
            int[] a = new int[5] { 10, 20, 30, 40, 50 };
            fixed (int* p = &a[0])
            {
                var instance = new ClassWithPointer();
                instance.Value = p;
                Cloning.ObjectCloner.Clone(instance);
            }
        }

        [TestMethod]
        public void CloneAutoInitializeList()
        {
            var instance = new ClassWithInitializedList();
            instance.Values = null;
            var clone = Cloning.ObjectCloner.Clone(instance);
            Assert.IsNull(clone.Values);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CloneGroupByContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IEnumerable<IGrouping<int, ClassWithoutSerializableAttribute>>> { Value = list.GroupBy(x => x.PublicPropertyValue) };

            var bytes = Cloning.ObjectCloner.Clone(instance);
        }

        //[TestMethod]
        //public void KrakenSerializerShouldWork()
        //{
        //    var instance = new GenericBaseClass<IQueryable> { Value = null };

        //    var clonedValue = KrakenObjectCloner.Clone(instance);
        //    var clonedValue = KrakenSerializer.Deserialize<GenericBaseClass<IQueryable>>(bytes);

        //    Assert.IsNotNull(clonedValue);
        //    Assert.IsNull(clonedValue.Value);
        //}

        //[TestMethod]
        //public void StringShouldNotBeSerializedTwice()
        //{
        //    var str = RandomString(1000);
        //    var instance = new List<string> { str };
        //    var clonedValue = KrakenObjectCloner.Clone(instance);
        //    var clonedValue = KrakenSerializer.Deserialize<List<string>>(bytes);
        //    CollectionAssert.AreEqual(instance, clonedValue);

        //    instance = new List<string> { str, str };
        //    var clonedValueTwice = KrakenObjectCloner.Clone(instance);
        //    clonedValue = KrakenSerializer.Deserialize<List<string>>(bytesTwice);
        //    CollectionAssert.AreEqual(instance, clonedValue);

        //    Assert.IsTrue(bytes.Length + 500 > bytesTwice.Length);
        //}

        //[TestMethod]
        //[ExpectedException(typeof(NotSupportedException))]
        //public void SerializingAStreamIsNotSupported()
        //{
        //    using (var sr = new StringReader("abc"))
        //    {
        //        var clonedValue = ObjectCloner.Clone(sr);
        //    }
        //}

        //[TestMethod]
        //public void z_AdditionalTestsToImplements()
        //{

        //    Assert.Fail();
        //}
    }
}
