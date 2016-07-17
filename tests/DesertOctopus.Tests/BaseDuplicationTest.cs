using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using DesertOctopus.Cloning;
using DesertOctopus.Exceptions;
using DesertOctopus.Serialization;
using DesertOctopus.Tests.TestObjects;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerializerTests.TestObjects;

namespace DesertOctopus.Tests
{
    [TestClass]
    public abstract class BaseDuplicationTest
    {
        public abstract T Duplicate<T>(T obj) where T : class;



        [TestMethod]
        [TestCategory("Unit")]
        public void TestPrimitives()
        {
            PrimitiveTestSuite<string>("S", "s", null);

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

            PrimitiveTestSuite<EnumForTestingInt32>(EnumForTestingInt32.One, EnumForTestingInt32.Two);
            PrimitiveTestSuite<EnumForTestingInt32?>(EnumForTestingInt32.One, EnumForTestingInt32.Two, null);

            PrimitiveTestSuite<EnumForTestingUint32>(EnumForTestingUint32.One, EnumForTestingUint32.Two);
            PrimitiveTestSuite<EnumForTestingUint32?>(EnumForTestingUint32.One, EnumForTestingUint32.Two, null);

            PrimitiveTestSuite<EnumForTestingInt16>(EnumForTestingInt16.One, EnumForTestingInt16.Two);
            PrimitiveTestSuite<EnumForTestingInt16?>(EnumForTestingInt16.One, EnumForTestingInt16.Two, null);

            PrimitiveTestSuite<EnumForTestingInt64>(EnumForTestingInt64.One, EnumForTestingInt64.Two);
            PrimitiveTestSuite<EnumForTestingInt64?>(EnumForTestingInt64.One, EnumForTestingInt64.Two, null);

            PrimitiveTestSuite<EnumForTestingUint64>(EnumForTestingUint64.One, EnumForTestingUint64.Two);
            PrimitiveTestSuite<EnumForTestingUint64?>(EnumForTestingUint64.One, EnumForTestingUint64.Two, null);

            PrimitiveTestSuite<EnumForTestingByte>(EnumForTestingByte.One, EnumForTestingByte.Two);
            PrimitiveTestSuite<EnumForTestingByte?>(EnumForTestingByte.One, EnumForTestingByte.Two, null);

            PrimitiveTestSuite<EnumForTestingSbyte>(EnumForTestingSbyte.One, EnumForTestingSbyte.Two);
            PrimitiveTestSuite<EnumForTestingSbyte?>(EnumForTestingSbyte.One, EnumForTestingSbyte.Two, null);

            PrimitiveTestSuite<EnumForTestingUint16>(EnumForTestingUint16.One, EnumForTestingUint16.Two);
            PrimitiveTestSuite<EnumForTestingUint16?>(EnumForTestingUint16.One, EnumForTestingUint16.Two, null);

        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateWrappedString()
        {
            var instance = new GenericBaseClass<string>();
            instance.Value = "abc";
            var duplicate = Duplicate(instance);
            Assert.AreEqual(instance.Value,
                            duplicate.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateTuple()
        {
            PrimitiveTestSuite<Tuple<int, string>>(new Tuple<int, string>(1, "a"), new Tuple<int, string>(2, "b"));
            PrimitiveTestSuite<Tuple<string, int>>(new Tuple<string, int>("a", 1), new Tuple<string, int>("b", 2));
            PrimitiveTestSuite<Tuple<int, string, bool>>(new Tuple<int, string, bool>(1, "a", true), new Tuple<int, string, bool>(2, "b", false));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void SerializeUtcDateTime()
        {
            var instance = new Wrapper<DateTime> { Value = DateTime.UtcNow };
            var deserializedValue = Duplicate<Wrapper<DateTime>>(instance);
            Assert.IsNotNull(deserializedValue.Value);
            Assert.AreEqual(instance.Value, deserializedValue.Value);
            Assert.AreEqual(instance.Value.Kind, deserializedValue.Value.Kind);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void SerializeDateTime()
        {
            var instance = new Wrapper<DateTime> { Value = DateTime.Now };
            var deserializedValue = Duplicate<Wrapper<DateTime>>(instance);
            Assert.IsNotNull(deserializedValue.Value);
            Assert.AreEqual(instance.Value, deserializedValue.Value);
            Assert.AreEqual(instance.Value.Kind, deserializedValue.Value.Kind);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateNullPrimitiveArray()
        {
            int[] nullArray = null;
            var duplicatedValue = Duplicate(nullArray);
            Assert.IsNull(duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateEmptyObjectArray()
        {
            Object[] emptyArray = new Object[0];
            var duplicatedValue = Duplicate(emptyArray);
            Assert.IsNotNull(duplicatedValue);
            Assert.AreEqual(0, duplicatedValue.Length);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateNullObjectArray()
        {
            Object[] nullArray = null;
            var duplicatedValue = Duplicate(nullArray);
            Assert.IsNull(duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateEmptyPrimitiveArray()
        {
            int[] emptyArray = new int[0];
            var duplicatedValue = Duplicate(emptyArray);
            Assert.IsNotNull(duplicatedValue);
            Assert.AreEqual(0, duplicatedValue.Length);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectArrayWithNullValues()
        {
            var array = new ClassWithoutSerializableAttribute[] { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var duplicatedValue = Duplicate(array);
            Assert.AreEqual(array.Length, duplicatedValue.Length);
            Assert.AreEqual(array[0].PublicPropertyValue, duplicatedValue[0].PublicPropertyValue);
            Assert.IsNull(duplicatedValue[1]);
            Assert.AreEqual(array[2].PublicPropertyValue, duplicatedValue[2].PublicPropertyValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateNullPrimitiveList()
        {
            List<int> nullList = null;
            var duplicatedValue = Duplicate(nullList);
            Assert.IsNull(duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateEmptyObjectList()
        {
            List<Object> emptyList = new List<Object>();
            var duplicatedValue = Duplicate(emptyList);
            Assert.IsNotNull(duplicatedValue);
            Assert.AreEqual(0, duplicatedValue.Count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateNullObjectList()
        {
            List<object> nullList = null;
            var duplicatedValue = Duplicate(nullList);
            Assert.IsNull(duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateEmptyPrimitiveList()
        {
            List<int> emptyList = new List<int>();
            var duplicatedValue = Duplicate(emptyList);
            Assert.IsNotNull(duplicatedValue);
            Assert.AreEqual(0, duplicatedValue.Count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectListWithNullValues()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var duplicatedValue = Duplicate(list);
            Assert.AreEqual(list.Count, duplicatedValue.Count);
            Assert.AreEqual(list[0].PublicPropertyValue, duplicatedValue[0].PublicPropertyValue);
            Assert.IsNull(duplicatedValue[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, duplicatedValue[2].PublicPropertyValue);
        }

        public void PrimitiveTestSuite<T>(params T[] values)
        {
            foreach (var value in values)
            {
                DuplicateWrappedValue(value);
                DuplicateArrayOfOne(value);
                DuplicateListOfOne(value);
            }
            DuplicateArray<T>(values);
            DuplicateList<T>(values);
            DuplicateReadOnly<T>(values.First());
            DuplicateWrappedPrimitive<T>(values.First());
        }

        private void DuplicateWrappedPrimitive<T>(T value)
        {
            var instance = new GenericBaseClass<T>();
            instance.Value = value;
            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Value,
                            duplicatedValue.Value,
                            string.Format("Type {0} does not have the same value after being deserialized.",
                                            typeof(T)));
        }

        private void DuplicateReadOnly<T>(T value)
        {
            var instance = new ClassWithReadOnlyProperty<T>(value);
            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Value,
                            duplicatedValue.Value,
                            string.Format("Type {0} does not have the same value after being deserialized.",
                                            typeof(T)));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateReadOnlyObjectProperty()
        {
            var instance = new ClassWithReadOnlyProperty<ClassWithGenericInt>(new ClassWithGenericInt { Value = 38 });
            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Value,
                            duplicatedValue.Value,
                            string.Format("Type {0} does not have the same value after being deserialized.",
                                            typeof(ClassWithReadOnlyProperty<ClassWithGenericInt>)));
        }

        private void DuplicateWrappedValue<T>(T value)
        {
            var wrappedObject = new Wrapper<T>
            {
                Value = value
            };

            Type targetType = typeof(T);
            var duplicatedValue = Duplicate(wrappedObject);

            Assert.AreEqual(wrappedObject.Value,
                            duplicatedValue.Value,
                            string.Format("Type {0} does not have the same value after being deserialized.",
                                            targetType));

        }

        private void DuplicateArray<T>(T[] value)
        {
            var duplicatedValue = Duplicate(value);

            CollectionAssert.AreEquivalent(value,
                                           duplicatedValue);
        }

        private void DuplicateArrayOfOne<T>(T value)
        {
            var array = new T[1];
            array[0] = value;

            var duplicatedValue = Duplicate(array);
            Assert.IsNotNull(duplicatedValue, "Type: " + typeof(T));
            CollectionAssert.AreEquivalent(array,
                                            duplicatedValue, "Type: " + typeof(T));
        }

        private void DuplicateListOfOne<T>(T value)
        {
            var list = new List<T>
                        {
                            value
                        };

            var duplicatedValue = Duplicate(list);

            CollectionAssert.AreEquivalent(list,
                                            duplicatedValue);
        }

        private void DuplicateList<T>(T[] value)
        {
            var list = value.ToList();

            var duplicatedValue = Duplicate(list);

            CollectionAssert.AreEquivalent(list,
                                            duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateStruct()
        {
            var instance = new Wrapper<StructForTesting> { Value = new StructForTesting { Value = 1 } };
            var duplicatedValue = Duplicate(instance);
            Assert.AreEqual(instance.Value, duplicatedValue.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateShortString()
        {
            var duplicatedValue = Duplicate("abc");
            //Assert.AreEqual(5, bytes.Length);
            Assert.AreEqual("abc", duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateMediumString()
        {
            var str = RandomString(3000);
            var duplicatedValue = Duplicate(str);
            Assert.AreEqual(str, duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateLongString()
        {
            var str = RandomString(100000);
            var duplicatedValue = Duplicate(str);
            Assert.AreEqual(str, duplicatedValue);
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateTestClass()
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

            var duplicatedValue = Duplicate(classInstance);

            Assert.AreEqual(classInstance.PublicFieldValue,
                            duplicatedValue.PublicFieldValue);
            Assert.AreEqual(classInstance.GetPrivateFieldValue(),
                            duplicatedValue.GetPrivateFieldValue());
            Assert.AreEqual(classInstance.InternalFieldValue,
                            duplicatedValue.InternalFieldValue);
            Assert.AreEqual(classInstance.PublicPropertyValue,
                            duplicatedValue.PublicPropertyValue);
            Assert.AreEqual(classInstance.GetPrivatePropertyValue(),
                            duplicatedValue.GetPrivatePropertyValue());
            Assert.AreEqual(classInstance.InternalPropertyValue,
                            duplicatedValue.InternalPropertyValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        //[ExpectedException(typeof(ObjectExistsInCurrentSerializationGraphException))]
        public void DuplicateCircularReference()
        {
            var instance1 = new CircularReference { Id = 1 };
            var instance2 = new CircularReference { Id = 2 };
            instance1.Child = instance2;
            instance2.Parent = instance1;

            var duplicatedValue = Duplicate(instance1);

            Assert.IsTrue(ReferenceEquals(duplicatedValue,
                                          duplicatedValue.Child.Parent));

            Assert.AreEqual(1, duplicatedValue.Id);
            Assert.AreEqual(2, duplicatedValue.Child.Id);
            Assert.AreEqual(1, duplicatedValue.Child.Parent.Id);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateTrackMultipleReference()
        {
            var instance = new ClassWithGenericInt(123);
            var list = new List<ClassWithGenericInt>
                        {
                            instance,
                            instance
                        };

            var duplicatedValue = Duplicate(list);

            Assert.AreEqual(list.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(list,
                                            duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateTrackSamePrimitiveMultipleTimes()
        {
            // this case exists to make sure that the ReferenceWatcher only tracks classes

            var instance = 3;
            var list = new List<int>
                        {
                            instance,
                            instance
                        };

            var duplicatedValue = Duplicate(list);

            Assert.AreEqual(list.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(list,
                                            duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateClassWithoutSerializableAttribute()
        {
            var instance = new ClassWithoutSerializableAttribute
            {
                PublicPropertyValue = 4
            };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.PublicPropertyValue,
                            duplicatedValue.PublicPropertyValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateClassWithGenericBase()
        {
            var instance = new ClassWithGenericInt()
            {
                Value = 4
            };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Value,
                            duplicatedValue.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateGenericClass()
        {
            var instance = new GenericBaseClass<int>()
            {
                Value = 4
            };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Value,
                            duplicatedValue.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateDictionaryStringObject()
        {
            var instance = new Dictionary<string, object>()
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateDictionaryIntString()
        {
            var instance = new Dictionary<int, string>()
            {
                {1, "Value1"},
                {2, "Value2"},
                {3, "Value3"}
            };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateWrappedNullDictionary()
        {
            var instance = new GenericBaseClass<Dictionary<int, string>>();
            var duplicatedValue = Duplicate(instance);

            Assert.IsNull(duplicatedValue.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateDictionaryGuidString()
        {
            var instance = new Dictionary<Guid, string>()
            {
                {Guid.NewGuid(), "Value1"},
                {Guid.NewGuid(), "Value2"},
                {Guid.NewGuid(), "Value3"}
            };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateCustomDictionary()
        {
            var instance = new CustomDictionary
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        private class TypeValueCargo
        {
            public Type Type { get; set; }
            public ICollection Values { get; set; }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateDictionaries()
        {
            var cargoes = new TypeValueCargo[]
                          {
                                new TypeValueCargo { Type = typeof(bool), Values = new bool[] { true, false } },
                                new TypeValueCargo { Type = typeof(byte), Values = new byte[] { byte.MinValue, byte.MaxValue, 3 } },
                                new TypeValueCargo { Type = typeof(sbyte), Values = new sbyte[] { sbyte.MinValue, sbyte.MaxValue, 3, 0, -3 } },
                                new TypeValueCargo { Type = typeof(Int16), Values = new Int16[] { Int16.MinValue, Int16.MaxValue, 3, 0 } },
                                new TypeValueCargo { Type = typeof(UInt16), Values = new UInt16[] { UInt16.MinValue, UInt16.MaxValue, 3 } },
                                new TypeValueCargo { Type = typeof(Int32), Values = new Int32[] { Int32.MinValue, Int32.MaxValue, 3, 0, -3 } },
                                new TypeValueCargo { Type = typeof(UInt32), Values = new UInt32[] { UInt32.MinValue, UInt32.MaxValue, 3 } },
                                new TypeValueCargo { Type = typeof(Int64), Values = new Int64[] { Int64.MinValue, Int64.MaxValue, 3, 0, -3 } },
                                new TypeValueCargo { Type = typeof(UInt64), Values = new UInt64[] { UInt64.MinValue, UInt64.MaxValue, 3 } },
                                new TypeValueCargo { Type = typeof(char), Values = new char[] { char.MinValue, char.MaxValue, 'a', 'z' } },
                                new TypeValueCargo { Type = typeof(double), Values = new double[] { double.MinValue, double.MaxValue, 3.34D, 0, -3.34D } },
                                new TypeValueCargo { Type = typeof(Decimal), Values = new Decimal[] { Decimal.MinValue, Decimal.MaxValue, 3.34M, 0, -3.34M } },
                                new TypeValueCargo { Type = typeof(Single), Values = new Single[] { Single.MinValue, Single.MaxValue, 3.34F, 0, -3.34F } },
                                new TypeValueCargo { Type = typeof(DateTime), Values = new DateTime[] { DateTime.MaxValue, DateTime.MinValue, DateTime.Now } },
                                new TypeValueCargo { Type = typeof(TimeSpan), Values = new TimeSpan[] { TimeSpan.MaxValue, TimeSpan.MinValue, TimeSpan.FromSeconds(30) } },
                                new TypeValueCargo { Type = typeof(BigInteger), Values = new BigInteger[] { BigInteger.MinusOne, BigInteger.One, BigInteger.Zero, 98, -1928 } },
                                new TypeValueCargo { Type = typeof(Tuple<int, string>), Values = new Tuple<int, string>[] { new Tuple<int, string>(1, "a"), new Tuple<int, string>(2, "b") } }
                          };

            var types = new[]
                        {

                            typeof(bool),
                            typeof(byte),
                            typeof(sbyte),
                            typeof(Int16),
                            typeof(UInt16),
                            typeof(Int32),
                            typeof(UInt32),
                            typeof(Int64),
                            typeof(UInt64),
                            typeof(char),
                            typeof(double),
                            typeof(Decimal),
                            typeof(Single),
                            typeof(DateTime),
                            typeof(TimeSpan),
                            typeof(BigInteger),
                            typeof(Tuple<int, string>)
                        };

            foreach (var keyType in types)
            {
                foreach (var valueType in types)
                {
                    var m = typeof(BaseDuplicationTest).GetMethod("DuplicateDictionary", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(keyType, valueType);
                    m.Invoke(this,
                             new object[]
                             {
                                cargoes.Single(x => x.Type == keyType).Values,
                                 cargoes.Single(x => x.Type == valueType).Values
                             });
                }
            }
        }

        private void DuplicateDictionary<TKey, TValue>(object valuesForKey, TValue[] valuesForValues)
        {
            var vk = (TKey[]) valuesForKey;
            var rnd = new Random();
            var instance = new Dictionary<TKey, TValue>();
            foreach (var key in vk)
            {
                instance.Add((TKey)key, valuesForValues[rnd.Next(0, valuesForValues.Length)]);
            }

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectKeyDictionary()
        {
            var instance = new Dictionary<object, int>();
            instance.Add(new ClassWithGenericInt(3), 1);
            instance.Add(new ClassWithGenericInt(5), 4);

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateEmptyDictionary()
        {
            var instance = new Dictionary<string, int>();

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateWrappedCustomDictionary()
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

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Value.Count,
                            duplicatedValue.Value.Count);
            CollectionAssert.AreEquivalent(instance.Value.Keys,
                                            duplicatedValue.Value.Keys);
            foreach (var kvp in instance.Value)
            {
                Assert.AreEqual(kvp.Value,
                                duplicatedValue.Value[kvp.Key]);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateCustomDictionaryWithComparer()
        {
            var instance = new CustomDictionary(StringComparer.CurrentCultureIgnoreCase)
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Comparer.GetType(),
                            duplicatedValue.Comparer.GetType());
            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateDictionaryWithCustomComparer()
        {
            var instance = new Dictionary<StructForTesting, object>(new StructForTestingComparer())
            {
                {new StructForTesting { Value = 1 }, 123},
                {new StructForTesting { Value = 2 }, "abc"},
                {new StructForTesting { Value = 3 }, new ClassWithGenericInt(3) },
            };


            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Comparer.GetType(),
                            duplicatedValue.Comparer.GetType());
            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(MissingConstructorException))]
        public void DuplicateCustomDictionaryWithoutPublicParameterlessConstructor()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructor<object, object>()
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
                {"Key4", null },
            };

            Duplicate(instance);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateCustomDictionaryWithAdditionalPropertiesWithCallback()
        {
            var instance = new CustomDictionaryWithAdditionalPropertiesWithOverridingOnDeserializedCallback
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };
            instance.SomeProperty = 849;

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.SomeProperty,
                            duplicatedValue.SomeProperty);
            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateCustomDictionaryWithAdditionalPropertiesWithoutCallback()
        {
            var instance = new CustomDictionaryWithAdditionalPropertiesWithoutOverridingOnDeserializedCallback
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };
            instance.SomeProperty = 849;

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(0,
                            duplicatedValue.SomeProperty); // default value for property
            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateCustomDictionaryWithDictionaryProperty()
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

            var duplicatedValue = Duplicate(instance);

            CompareDictionaries(instance,
                                duplicatedValue);
            CompareDictionaries(instance.SwitchedDictionary,
                                duplicatedValue.SwitchedDictionary);
        }

        private static void CompareDictionaries<TKey, TValue>(Dictionary<TKey, TValue> instance, Dictionary<TKey, TValue> duplicatedValue)
        {
            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            Assert.AreEqual(instance.Comparer.GetType(),
                            duplicatedValue.Comparer.GetType());
            CollectionAssert.AreEquivalent(instance.Keys,
                                           duplicatedValue.Keys);
            foreach (var key in instance.Keys)
            {
                if (instance[key] != null
                    && instance[key].GetType() == typeof(object))
                {
                    Assert.IsTrue(duplicatedValue[key] != null && duplicatedValue[key].GetType() == typeof(object));
                }
                else if (instance[key] != null
                    && instance[key].GetType() == typeof(ClassWithoutSerializableAttribute))
                {
                    Assert.AreEqual(((ClassWithoutSerializableAttribute)(object)instance[key]).PublicPropertyValue,
                                    ((ClassWithoutSerializableAttribute)(object)duplicatedValue[key]).PublicPropertyValue);
                }
                else
                {
                    Assert.AreEqual(instance[key],
                                    duplicatedValue[key]);
                }
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateCustomDictionaryWithAdditionalPropertiesAndGenerics()
        {
            var instance = new CustomDictionaryWithAdditionalPropertiesAndGenerics<string, int>
            {
                {"Key1", 123},
                {"Key2", 456},
                {"Key3", 789},
            };
            instance.SomeProperty = 849;

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.SomeProperty,
                            duplicatedValue.SomeProperty);
            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateEmptyCustomDictionaryWithAdditionalPropertiesAndGenerics()
        {
            var instance = new CustomDictionaryWithAdditionalPropertiesAndGenerics<string, int>();
            instance.SomeProperty = 849;

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.SomeProperty,
                            duplicatedValue.SomeProperty);
            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance.Keys,
                                            duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.AreEqual(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateListWithMultipleTypes()
        {
            var list = new List<IHierarchy>
            {
                new ChildIntHierarchy(123),
                new ChildStringHierarchy("abc"),
            };

            var duplicatedValue = Duplicate(list);

            Assert.AreEqual(list.Count,
                            duplicatedValue.Count);
            Assert.AreEqual(list.OfType<ChildIntHierarchy>().First().Value,
                            duplicatedValue.OfType<ChildIntHierarchy>().First().Value);
            Assert.AreEqual(list.OfType<ChildStringHierarchy>().First().Value,
                            duplicatedValue.OfType<ChildStringHierarchy>().First().Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectWithListAsIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 } };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            duplicatedValue.Items.ToList());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectWithHashsetAsIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int>
            {
                Items = new HashSet<int> { 1, 2, 3 }
            };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            duplicatedValue.Items.ToList());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateHashSetWithEqualityComparer()
        {
            var instance = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "a", "b", "C" };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Comparer.GetType(),
                            duplicatedValue.Comparer.GetType());
            Assert.AreEqual(instance.Count(),
                            duplicatedValue.Count());
            CollectionAssert.AreEquivalent(instance.ToList(),
                                            duplicatedValue.ToList());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectWithEnumProperty()
        {
            var instance = new GenericBaseClass<EnumForTestingInt32> { Value = EnumForTestingInt32.Two };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Value,
                            duplicatedValue.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateHashtable()
        {
            var instance = new Hashtable
                            {
                                {1, 2},
                                {"a", "b"},
                            };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance,
                                            duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateHashtableWithEqualityComparer()
        {
            var instance = new Hashtable(StringComparer.CurrentCultureIgnoreCase)
                            {
                                {"e", 2},
                                {"a", "b"},
                            };

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(GetHashTableComparer(instance));
            Assert.IsNotNull(GetHashTableComparer(duplicatedValue));
            Assert.AreEqual(GetHashTableComparer(instance).GetType(),
                            GetHashTableComparer(duplicatedValue).GetType());
            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance,
                                            duplicatedValue);
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
        [TestCategory("Unit")]
        public void DuplicateObjectWithArrayAsIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new[] { 1, 2, 3 } };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            duplicatedValue.Items.ToList());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectWithDistinctIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 1, 2, 3 }.Distinct() };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            duplicatedValue.Items.ToList());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectWithWhereIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Where(x => x > 1) };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            duplicatedValue.Items.ToList());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectWithOrderByIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.OrderBy(x => x) };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            duplicatedValue.Items.ToList());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectWithDefaultIfEmptyIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.DefaultIfEmpty(123) };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            duplicatedValue.Items.ToList());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectWithExceptIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Except(new[] { 2 }) };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            duplicatedValue.Items.ToList());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectWithUnionIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Union(new[] { 4 }) };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            duplicatedValue.Items.ToList());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectWithIntersectIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Intersect(new[] { 2 }) };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            duplicatedValue.Items.ToList());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectWithOfTypeIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.OfType<int>() };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            duplicatedValue.Items.ToList());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectWithSkipByIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Skip(1) };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            duplicatedValue.Items.ToList());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectWithTakeByIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Take(1) };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            duplicatedValue.Items.ToList());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectWithSelectByIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Select(x => x * 2) };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            CollectionAssert.AreEquivalent(instance.Items.ToList(),
                                            duplicatedValue.Items.ToList());
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(NotSupportedException))]
        public void DuplicateSimpleFunc()
        {
            var testData = new List<int> { 1, 2, 3, 4, 5, 6 };
            System.Func<int, bool> instance = x => x > 3;

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);

            Assert.AreEqual(testData.Count(x => instance(x)),
                            testData.Count(x => duplicatedValue(x)));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(NotSupportedException))]
        public void DuplicateSimpleExpression()
        {
            var testData = new List<int> { 1, 2, 3, 4, 5, 6 };

            Expression<System.Func<int, bool>> instance = x => x > 3;

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);

            Assert.AreEqual(testData.Count(instance.Compile()),
                            testData.Count(duplicatedValue.Compile()));
            Assert.Fail("we should not allow serialization of Expression");
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(NotSupportedException))]
        public void DuplicateAnonymousObject()
        {
            var instance = new { Property1 = "hello", Property2 = 123 };
            var duplicatedValue = Duplicate(instance);
            Assert.AreEqual(instance.Property1, duplicatedValue.Property1);
            Assert.AreEqual(instance.Property2, duplicatedValue.Property2);

            Assert.Fail("we should not allow serialization of anonymous types");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateArray()
        {
            var instance = new int[] { 123, 456 };
            var duplicatedValue = Duplicate(instance);
            CollectionAssert.AreEqual(instance, duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateArrayWithNullablePrimitive()
        {
            var instance = new int?[] { 123, null, 456 };
            var duplicatedValue = Duplicate(instance);
            CollectionAssert.AreEqual(instance, duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateArrayObject()
        {
            var instance = new ClassWithoutSerializableAttribute[] { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var duplicatedValue = Duplicate(instance);
            Assert.AreEqual(2, duplicatedValue.Length);
            Assert.AreEqual(123, duplicatedValue[0].PublicPropertyValue);
            Assert.AreEqual(456, duplicatedValue[1].PublicPropertyValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateArrayOfClassWithGenericInt()
        {
            var obj = new ClassWithGenericInt(123);
            var instance = new ClassWithGenericInt[] { obj, null };

            var duplicatedValue = Duplicate(instance);

            instance.ShouldAllBeEquivalentTo(duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateArrayObjectHierarchy()
        {
            var instance = new SomeBaseClass[] { new ClassWithGenericInt(123), new ClassWithGenericDouble(3.38D) };
            var duplicatedValue = Duplicate(instance);
            Assert.AreEqual(2, duplicatedValue.Length);
            Assert.AreEqual(123, (duplicatedValue[0] as ClassWithGenericInt).Value);
            Assert.AreEqual(3.38D, (duplicatedValue[1] as ClassWithGenericDouble).Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateSameObjectMultipleTimeInArray()
        {
            var obj123 = new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 };
            var instance = new ClassWithoutSerializableAttribute[] { obj123, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 }, obj123 };
            var duplicatedValue = Duplicate(instance);
            Assert.AreEqual(3, duplicatedValue.Length);
            Assert.AreEqual(123, duplicatedValue[0].PublicPropertyValue);
            Assert.AreEqual(456, duplicatedValue[1].PublicPropertyValue);
            Assert.AreEqual(123, duplicatedValue[2].PublicPropertyValue);
            Assert.IsTrue(ReferenceEquals(duplicatedValue[0], duplicatedValue[2]));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateTwoDimensionalArray()
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

            var duplicatedValue = Duplicate(instance);
            instance.Should().BeEquivalentTo(duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateMultiDimensionalArray()
        {
            var instance = CreateMultiDimensionalArray<int>(8);
            SeedArray(instance);

            var duplicatedValue = Duplicate(instance);
            instance.Should().BeEquivalentTo(duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateMultiDimensionalArrayOfObjects()
        {
            var instance = CreateMultiDimensionalArray<Object>(2);
            instance.SetValue(new object(), new[] { 0, 0 });
            instance.SetValue(new object(), new[] { 1, 0 });

            var duplicatedValue = Duplicate(instance);

            Assert.IsTrue(duplicatedValue.GetValue(new[] { 0, 0 }).GetType() == typeof(object));
            Assert.IsNull(duplicatedValue.GetValue(new[] { 0, 1 }));
            Assert.IsTrue(duplicatedValue.GetValue(new[] { 1, 0 }).GetType() == typeof(object));
            Assert.IsNull(duplicatedValue.GetValue(new[] { 1, 1 }));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateMultiDimensionalArrayOfClass()
        {
            var instance = CreateMultiDimensionalArray<ClassWithGenericInt>(2);
            var obj = new ClassWithGenericInt(123);
            instance.SetValue(obj, new[] { 0, 0 });
            instance.SetValue(new ClassWithGenericInt(456), new[] { 1, 0 });
            instance.SetValue(obj, new[] { 1, 1 });

            var duplicatedValue = Duplicate(instance);
            instance.Should().BeEquivalentTo(duplicatedValue);
            Assert.AreEqual(456, ((ClassWithGenericInt)duplicatedValue.GetValue(1, 0)).Value);
            Assert.IsTrue(ReferenceEquals(duplicatedValue.GetValue(0, 0), duplicatedValue.GetValue(1, 1)));
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
        [TestCategory("Unit")]
        public void DuplicateJaggedArrayObject()
        {
            var instance = new object[2][];
            instance[0] = new object[] { new object(), null };
            instance[1] = new object[] { new object(), null };

            var duplicatedValue = Duplicate(instance);

            Assert.IsTrue(duplicatedValue[0][0].GetType() == typeof(object));
            Assert.IsNull(duplicatedValue[0][1]);
            Assert.IsTrue(duplicatedValue[1][0].GetType() == typeof(object));
            Assert.IsNull(duplicatedValue[1][1]);
        }


        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateJaggedArrayClass()
        {
            var obj = new ClassWithGenericInt(123);
            var instance = new ClassWithGenericInt[2][];
            instance[0] = new ClassWithGenericInt[] { obj, null };
            //instance[0] = new ClassWithGenericInt[] { obj, new ClassWithGenericInt(4563) };
            instance[1] = new ClassWithGenericInt[] { new ClassWithGenericInt(456), obj };

            var duplicatedValue = Duplicate(instance);

            instance.ShouldAllBeEquivalentTo(duplicatedValue);

            Assert.AreEqual(456, ((ClassWithGenericInt)duplicatedValue[1][0]).Value);
            Assert.IsTrue(ReferenceEquals(duplicatedValue[0][0], duplicatedValue[1][1]));

            instance = new ClassWithGenericInt[2][];
            instance[0] = new ClassWithGenericInt[] { obj, new ClassWithGenericInt(4563) };
            instance[1] = new ClassWithGenericInt[] { new ClassWithGenericInt(456), obj };

            duplicatedValue = Duplicate(instance);

            instance.ShouldAllBeEquivalentTo(duplicatedValue);
        }


        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateJaggedArrayClassInheritance()
        {
            var obj = new ClassWithGenericInt(123);
            var instance = new SomeBaseClass[2][];
            instance[0] = new SomeBaseClass[] { obj, new ClassWithGenericDouble(123.3D) };
            instance[1] = new SomeBaseClass[] { new ClassWithGenericInt(456), obj };

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(123, ((ClassWithGenericInt)duplicatedValue[0][0]).Value);
            Assert.AreEqual(123.3D, ((ClassWithGenericDouble)duplicatedValue[0][1]).Value);
            Assert.AreEqual(456, ((ClassWithGenericInt)duplicatedValue[1][0]).Value);
            Assert.IsTrue(ReferenceEquals(duplicatedValue[0][0], duplicatedValue[1][1]));
        }


        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateNullJaggedArray()
        {
            int[][] nullArray = null;
            var instance = new Wrapper<int[][]> { Value = nullArray };
            var duplicatedValue = Duplicate(instance);

            Assert.IsNull(duplicatedValue.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateThreeDimensionJaggedArray()
        {
            int[][][] instance = new[] { new int[2][], new int[3][] };
            instance[0][0] = new int[] { 123, 238 };
            var duplicatedValue = Duplicate(instance);

            instance.ShouldAllBeEquivalentTo(duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateEmptyJaggedArray()
        {
            int[][] instance = new int[2][];
            var duplicatedValue = Duplicate(instance);
            instance.ShouldAllBeEquivalentTo(duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateJaggedArray()
        {
            var instance = new int[][] { new int[] { 123, 238 }, new int[] { 456, 546, 784 }, null };
            var duplicatedValue = Duplicate(instance);
            Assert.AreEqual(instance.Length, duplicatedValue.Length);
            instance.ShouldAllBeEquivalentTo(duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TrackArrayInMultipleObjects()
        {
            int[] arr = new[] { 1, 2, 3 };
            var instance1 = new CircularReference { Id = 1 };
            var instance2 = new CircularReference { Id = 2, Ids = arr };
            var instance3 = new CircularReference { Id = 3, Ids = arr };
            instance1.Child = instance2;
            instance2.Child = instance3;

            var duplicatedValue = Duplicate(instance1);

            CollectionAssert.AreEqual(arr, duplicatedValue.Child.Ids);
            Assert.IsTrue(ReferenceEquals(duplicatedValue.Child.Ids, duplicatedValue.Child.Child.Ids));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateClassWithDynamicObject()
        {
            ClassWithDynamicProperty instance = new ClassWithDynamicProperty { Value = 123 };

            var duplicatedValue = Duplicate(instance);
            Assert.IsNotNull(duplicatedValue);
            Assert.AreEqual(instance.Value, duplicatedValue.Value);
        }
        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateExpandoObject()
        {
            dynamic instance = new ExpandoObject();
            instance.Property1 = 123;
            instance.Property2 = "abc";
            instance.Property3 = new ClassWithGenericInt(349);
            instance.Property4 = new object();
            instance.Property5 = null;

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);

            Assert.AreEqual(instance.Property1,
                            duplicatedValue.Property1);
            Assert.AreEqual(instance.Property2,
                            duplicatedValue.Property2);
            Assert.AreEqual(instance.Property3,
                            duplicatedValue.Property3);
            Assert.IsTrue(duplicatedValue.Property4.GetType() == typeof(object));
            Assert.IsNull(duplicatedValue.Property5);

        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateEmptyExpandoObject()
        {
            dynamic instance = new ExpandoObject();

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
            Assert.AreEqual(0, (duplicatedValue as IDictionary<string, object>).Count);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateNullExpandoObject()
        {
            ExpandoObject instance = null;

            var duplicatedValue = Duplicate(instance);

            Assert.IsNull(duplicatedValue);

            var wrappedduplicatedValue2 = Duplicate(new Wrapper<ExpandoObject> { Value = instance });

            Assert.IsNull(wrappedduplicatedValue2.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateTypedQueue()
        {
            var instance = new Queue<int>();
            instance.Enqueue(1);
            instance.Enqueue(2);
            instance.Enqueue(3);

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);

            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance.ToArray(),
                                            duplicatedValue.ToArray());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateUntypedQueue()
        {
            var instance = new Queue();
            instance.Enqueue(1);
            instance.Enqueue(2);
            instance.Enqueue("abc");
            instance.Enqueue(new ClassWithGenericInt(123));

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);

            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance.ToArray(),
                                            duplicatedValue.ToArray());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateTypedStack()
        {
            var instance = new Stack<int>();
            instance.Push(1);
            instance.Push(2);
            instance.Push(3);

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);

            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance.ToArray(),
                                            duplicatedValue.ToArray());
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateUntypedStack()
        {
            var instance = new Stack();
            instance.Push(1);
            instance.Push(2);
            instance.Push("abc");
            instance.Push(new ClassWithGenericInt(123));

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);

            Assert.AreEqual(instance.Count,
                            duplicatedValue.Count);
            CollectionAssert.AreEquivalent(instance.ToArray(),
                                            duplicatedValue.ToArray());
        }


        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateEnumEqualityComparer()
        {
            var instance = new Dictionary<EnumForTestingInt32, int> { { EnumForTestingInt32.One, 1 }, { EnumForTestingInt32.Two, 2 } };

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);

            CompareDictionaries(instance,
                                duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateAnObject()
        {
            var instance = new object();

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateAClassWithANullObjectProperty()
        {
            var instance = new ClassWithObjectProperty();

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
            Assert.IsNull(duplicatedValue.Obj);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateAClassWithANotNullObjectProperty()
        {
            var instance = new ClassWithObjectProperty { Obj = new object() };

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
            Assert.IsNotNull(duplicatedValue.Obj);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateAClassWithAnBoxedInt()
        {
            var instance = new ClassWithObjectProperty { Obj = 123 };

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
            Assert.AreEqual(123, (int)duplicatedValue.Obj);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateIList()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IList> { Value = list };

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
            Assert.IsNotNull(duplicatedValue.Value);
            var dList = duplicatedValue.Value as List<ClassWithoutSerializableAttribute>;

            Assert.AreEqual(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.IsNull(dList[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateICollection()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<ICollection> { Value = list };

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
            Assert.IsNotNull(duplicatedValue.Value);
            var dList = duplicatedValue.Value as List<ClassWithoutSerializableAttribute>;

            Assert.AreEqual(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.IsNull(dList[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateIQueryableContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IQueryable> { Value = list.AsQueryable() };

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
            Assert.IsNotNull(duplicatedValue.Value);
            var dList = duplicatedValue.Value.Cast<ClassWithoutSerializableAttribute>().ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.IsNull(dList[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateGenericIQueryableContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IQueryable<ClassWithoutSerializableAttribute>> { Value = list.AsQueryable() };

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
            Assert.IsNotNull(duplicatedValue.Value);
            var dList = duplicatedValue.Value.Cast<ClassWithoutSerializableAttribute>().ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.IsNull(dList[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }


        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateListOfIQueryableContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new List<GenericBaseClass<IQueryable>> { new GenericBaseClass<IQueryable> { Value = list.AsQueryable() } };

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
            Assert.AreEqual(1, duplicatedValue.Count);
            Assert.IsNotNull(duplicatedValue[0].Value);
            var dList = duplicatedValue[0].Value.Cast<ClassWithoutSerializableAttribute>().ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.IsNull(dList[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateArrayOfIQueryableContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IQueryable>[] { new GenericBaseClass<IQueryable> { Value = list.AsQueryable() } };

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
            Assert.AreEqual(1, duplicatedValue.Length);
            Assert.IsNotNull(duplicatedValue[0].Value);
            var dList = duplicatedValue[0].Value.Cast<ClassWithoutSerializableAttribute>().ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.IsNull(dList[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateListOfMultipleObjects()
        {
            var instance = new List<object> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new CircularReference { Id = 456 }, 1234 };

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
            Assert.AreEqual(4, duplicatedValue.Count);

            instance.ShouldAllBeEquivalentTo(duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateEmptyList()
        {
            var instance = new List<object>();

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
            Assert.AreEqual(0, duplicatedValue.Count);

            instance.ShouldAllBeEquivalentTo(duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateNullList()
        {
            List<object> instance = null;

            var duplicatedValue = Duplicate(instance);

            Assert.IsNull(duplicatedValue);

            var wrappedduplicatedValue = Duplicate(new Wrapper<List<object>> { Value = instance });

            Assert.IsNull(wrappedduplicatedValue.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateArrayOfMultipleObjects()
        {
            var instance = new object[] { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new CircularReference { Id = 456 }, 1234 };

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
            Assert.AreEqual(4, duplicatedValue.Length);

            instance.ShouldAllBeEquivalentTo(duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateEmptyArray()
        {
            var instance = new object[0];

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
            Assert.AreEqual(0, duplicatedValue.Length);

            instance.ShouldAllBeEquivalentTo(duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateNullArray()
        {
            object[] instance = null;

            var duplicatedValue = Duplicate(instance);

            Assert.IsNull(duplicatedValue);

            var wrappedduplicatedValue = Duplicate(new Wrapper<object[]> { Value = instance });

            Assert.IsNull(wrappedduplicatedValue.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateListOfIQueryableDirectly()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            List<IQueryable<ClassWithoutSerializableAttribute>> instance = new List<IQueryable<ClassWithoutSerializableAttribute>> { list.AsQueryable() };

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
            Assert.AreEqual(1, duplicatedValue.Count);
            var deserializedArray = duplicatedValue[0].ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, deserializedArray[0].PublicPropertyValue);
            Assert.IsNull(deserializedArray[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, deserializedArray[2].PublicPropertyValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateArrayOfIQueryableDirectly()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            IQueryable<ClassWithoutSerializableAttribute>[] instance = new IQueryable<ClassWithoutSerializableAttribute>[] { list.AsQueryable() };

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
            Assert.AreEqual(1, duplicatedValue.Length);
            var deserializedArray = duplicatedValue[0].ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, deserializedArray[0].PublicPropertyValue);
            Assert.IsNull(deserializedArray[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, deserializedArray[2].PublicPropertyValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateJaggedArrayOfIQueryableDirectly()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new IQueryable<ClassWithoutSerializableAttribute>[1][];
            instance[0] = new IQueryable<ClassWithoutSerializableAttribute>[3];

            instance[0][2] = list.AsQueryable();

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.Rank, duplicatedValue.Rank);
            Assert.AreEqual(instance[0].Length, duplicatedValue[0].Length);

            Assert.IsNotNull(duplicatedValue);
            Assert.AreEqual(1, duplicatedValue.Length);
            var deserializedArray = duplicatedValue[0][2].ToArray();

            Assert.AreEqual(list[0].PublicPropertyValue, deserializedArray[0].PublicPropertyValue);
            Assert.IsNull(deserializedArray[1]);
            Assert.AreEqual(list[2].PublicPropertyValue, deserializedArray[2].PublicPropertyValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateNullIQueryableContainedInAClass()
        {
            var instance = new GenericBaseClass<IQueryable> { Value = null };

            var duplicatedValue = Duplicate(instance);

            Assert.IsNotNull(duplicatedValue);
            Assert.IsNull(duplicatedValue.Value);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateObjectWithISerializable()
        {
            var instance = new Dictionary<string, object>();
            instance.Add("Key1", "A");
            instance.Add("Key2", "B");
            instance.Add("Key3", 123);
            instance.Add("Key4", null);
            instance.Add("Key5", new object());
            instance.Add("Key6", new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 });

            var duplicatedValue = Duplicate(instance);

            CompareDictionaries(instance,
                                duplicatedValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateClassWithNonSerializableProperty()
        {
            var instance = new ClassWithNonSerializableField();
            instance.SerializableProperty = 839;
            instance.NonSerializableProperty = 33534;

            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(instance.SerializableProperty, duplicatedValue.SerializableProperty);
            Assert.AreEqual(0, duplicatedValue.NonSerializableProperty);
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(NotSupportedException))]
        public unsafe void DuplicatePointerTypeIsNotSupported()
        {
            int[] a = new int[5] { 10, 20, 30, 40, 50 };
            fixed (int* p = &a[0])
            {
                var instance = new ClassWithPointer();
                instance.Value = p;
                Duplicate(instance);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void DuplicateAutoInitializeList()
        {
            var instance = new ClassWithInitializedList();
            instance.Values = null;
            var duplicatedValue = Duplicate(instance);
            Assert.IsNull(duplicatedValue.Values);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TrackReferenceOfExpandoObject()
        {
            dynamic eo = new ExpandoObject();
            eo.Property1 = 123;
            eo.Property2 = "abc";
            eo.Property3 = new ClassWithGenericInt(349);
            eo.Property4 = new object();
            eo.Property5 = null;

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
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TrackReferenceOfDictionary()
        {
            var dict = new Dictionary<string, object>();
            dict.Add("Property1", 123);
            dict.Add("Property2", "abc");
            dict.Add("Property3", new ClassWithGenericInt(349));
            dict.Add("Property4", new object());
            dict.Add("Property5", null);

            var instance = new List<object> { dict, null, dict };
            var duplicatedValue = Duplicate(instance);

            Assert.AreEqual(3, duplicatedValue.Count);
            Dictionary<string, object> deserializedDict = (Dictionary<string, object>)duplicatedValue[0];

            Assert.AreEqual(dict["Property1"], deserializedDict["Property1"]);
            Assert.AreEqual(dict["Property2"], deserializedDict["Property2"]);
            Assert.AreEqual(dict["Property3"], deserializedDict["Property3"]);
            Assert.IsTrue(deserializedDict["Property4"].GetType() == typeof(object));
            Assert.AreEqual(dict["Property5"], deserializedDict["Property5"]);
            Assert.IsTrue(ReferenceEquals(duplicatedValue[0], duplicatedValue[2]));
        }

        [TestMethod]
        [TestCategory("Unit")]
        [ExpectedException(typeof(NotSupportedException))]
        public void DuplicateGroupByContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IEnumerable<IGrouping<int, ClassWithoutSerializableAttribute>>> { Value = list.GroupBy(x => x.PublicPropertyValue) };

            var bytes = Duplicate(instance);
        }
    }
}
