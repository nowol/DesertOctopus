﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using DesertOctopus.Cloning;
using DesertOctopus.Exceptions;
using DesertOctopus.Serialization;
using DesertOctopus.Tests.TestObjects;
using DesertOctopus.Utilities;
using FluentAssertions;
using SerializerTests.TestObjects;
using Xunit;
using Xunit.Abstractions;

namespace DesertOctopus.Tests
{
    public abstract class BaseDuplicationTest
    {
        public abstract T Duplicate<T>(T obj);

        private readonly ITestOutputHelper _output;

        public BaseDuplicationTest(ITestOutputHelper output)
        {
            _output = output;
        }

#if FALSE
        static uint EncodeZigZag32(int n)
        {
            return (uint)((n << 1) ^ (n >> 31));
        }
        static ulong EncodeZigZag64(long n)
        {
            return (ulong)((n << 1) ^ (n >> 63));
        }
        static ulong ReadVarint64(Stream stream)
        {
            long result = 0;
            int offset = 0;

            for (; offset < 64; offset += 7)
            {
                int b = stream.ReadByte();
                if (b == -1)
                    throw new EndOfStreamException();

                result |= ((long)(b & 0x7f)) << offset;

                if ((b & 0x80) == 0)
                    return (ulong)result;
            }

            throw new InvalidDataException();
        }


        [Fact]
        public unsafe void TestPrimitiveeeeee()
        {
            //using (var ms = new MemoryStream())
            //{
            //    uint value = EncodeZigZag32(int.MinValue);

            //    for (; value >= 0x80u; value >>= 7)
            //        ms.WriteByte((byte)(value | 0x80u));

            //    ms.WriteByte((byte)value);

            //    ms.Position = 0;

            //    ReadVarint32(ms);
            //}


            //var outputStream = Expression.Parameter(typeof(Stream), "outputStream");
            //var objToSerialize = Expression.Parameter(typeof(int), "objToSerialize");
            //var objTracking = Expression.Parameter(typeof(SerializerObjectTracker), "objTracking");

            //var expr = PrimitiveHelpers.WriteInt32(outputStream, objToSerialize, objTracking);
            //var write = Expression.Lambda<Action<Stream, int, SerializerObjectTracker>>(expr, outputStream, objToSerialize, objTracking).Compile();


            //var inputStream = Expression.Parameter(typeof(Stream), "inputStream");
            //var objTracker = Expression.Parameter(typeof(DeserializerObjectTracker), "objTracker");
            //var exprRead = PrimitiveHelpers.ReadInt32(inputStream, objTracker);
            //var read = Expression.Lambda<Func<Stream, DeserializerObjectTracker, int>>(exprRead, inputStream, objTracker).Compile();


            ////

            //var inputObj = Expression.Parameter(typeof(int));
            //var zigEnc = PrimitiveHelpers.EncodeZigZag32(inputObj);
            //var zigE = Expression.Lambda<Func<int, uint>>(zigEnc, inputObj).Compile();

            //var inputObj2 = Expression.Parameter(typeof(uint));
            //var zigDec = PrimitiveHelpers.DecodeZigZag32(inputObj2);
            //var zigD = Expression.Lambda<Func<uint, int>>(zigDec, inputObj2).Compile();


            //for (int i = int.MinValue; i < int.MaxValue; i++)
            //{
            //    //uint z = zigE(i);
            //    //int zz = zigD(z);


            //    using (var ms = new MemoryStream())
            //    {
            //        write(ms, i, new SerializerObjectTracker());
            //        ms.Position = 0;

            //        int d = read(ms, new DeserializerObjectTracker());

            //        Assert.Equal(i, d);
            //    }
            //}

            var d = 5456465.564D;
            long longValue = *(long*)&d;


            using (var ms = new MemoryStream())
            {
                ulong value = EncodeZigZag64(longValue);

                for (; value >= 0x80u; value >>= 7)
                    ms.WriteByte((byte)(value | 0x80u));

                ms.WriteByte((byte)value);

                ms.Position = 0;

                var lll = ReadVarint64(ms);
            }


            var outputStream = Expression.Parameter(typeof(Stream), "outputStream");
            var objToSerialize = Expression.Parameter(typeof(long), "objToSerialize");
            var objTracking = Expression.Parameter(typeof(SerializerObjectTracker), "objTracking");

            var expr = PrimitiveHelpers.WriteInt64(outputStream, objToSerialize, objTracking);
            var write = Expression.Lambda<Action<Stream, long, SerializerObjectTracker>>(expr, outputStream, objToSerialize, objTracking).Compile();


            var inputStream = Expression.Parameter(typeof(Stream), "inputStream");
            var objTracker = Expression.Parameter(typeof(DeserializerObjectTracker), "objTracker");
            var exprRead = PrimitiveHelpers.ReadInt64(inputStream, objTracker);
            var read = Expression.Lambda<Func<Stream, DeserializerObjectTracker, long>>(exprRead, inputStream, objTracker).Compile();


            //

            var inputObj = Expression.Parameter(typeof(long));
            var zigEnc = PrimitiveHelpers.EncodeZigZag64(inputObj);
            var zigE = Expression.Lambda<Func<long, ulong>>(zigEnc, inputObj).Compile();

            var inputObj2 = Expression.Parameter(typeof(ulong));
            var zigDec = PrimitiveHelpers.DecodeZigZag64(inputObj2);
            var zigD = Expression.Lambda<Func<ulong, long>>(zigDec, inputObj2).Compile();


            for (long i = long.MinValue; i < long.MaxValue; i++)
            {
                ulong z = zigE(i);
                long zz = zigD(z);

                i = longValue;

                using (var ms = new MemoryStream())
                {
                    write(ms, i, new SerializerObjectTracker());
                    ms.Position = 0;

                    long dd = read(ms, new DeserializerObjectTracker());

                    Assert.Equal(i, dd);
                }
            }

            //9218868437227405311

        }

        static uint ReadVarint32(Stream stream)
        {
            int result = 0;
            int offset = 0;

            for (; offset < 32; offset += 7)
            {
                int b = stream.ReadByte();
                if (b == -1)
                    throw new EndOfStreamException();

                result |= (b & 0x7f) << offset;

                if ((b & 0x80) == 0)
                    return (uint)result;
            }

            throw new InvalidDataException();
        }


        static void WriteVarint32(System.IO.Stream stream, uint value)
        {
           
        }
#endif

        [Fact]
        [Trait("Category", "Unit")]
        public void TestPrimitives()
        {
            PrimitiveTestSuite<double>(double.MaxValue, double.MinValue, 1.1D, -1.1D, 0);
            PrimitiveTestSuite<UInt64>(ulong.MaxValue, ulong.MinValue, 65000, 0);
            PrimitiveTestSuite<Int64>(long.MaxValue, long.MinValue, -32767, 32767, 0);


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

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateWrappedString()
        {
            var instance = new GenericBaseClass<string>();
            instance.Value = "abc";
            var duplicate = Duplicate(instance);
            Assert.Equal(instance.Value,
                            duplicate.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateWrappedInt32()
        {
            var instance = new GenericBaseClass<int>();
            instance.Value = 32;
            var duplicate = Duplicate(instance);
            Assert.Equal(instance.Value,
                            duplicate.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateTuple()
        {
            PrimitiveTestSuite<Tuple<int, string>>(new Tuple<int, string>(1, "a"), new Tuple<int, string>(2, "b"));
            PrimitiveTestSuite<Tuple<string, int>>(new Tuple<string, int>("a", 1), new Tuple<string, int>("b", 2));
            PrimitiveTestSuite<Tuple<int, string, bool>>(new Tuple<int, string, bool>(1, "a", true), new Tuple<int, string, bool>(2, "b", false));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void SerializeUtcDateTime()
        {
            var instance = new Wrapper<DateTime> { Value = DateTime.UtcNow };
            var deserializedValue = Duplicate<Wrapper<DateTime>>(instance);
            Assert.Equal(instance.Value, deserializedValue.Value);
            Assert.Equal(instance.Value.Kind, deserializedValue.Value.Kind);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void SerializeDateTime()
        {
            var instance = new Wrapper<DateTime> { Value = DateTime.Now };
            var deserializedValue = Duplicate<Wrapper<DateTime>>(instance);
            Assert.Equal(instance.Value, deserializedValue.Value);
            Assert.Equal(instance.Value.Kind, deserializedValue.Value.Kind);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateNullPrimitiveArray()
        {
            int[] nullArray = null;
            var duplicatedValue = Duplicate(nullArray);
            Assert.Null(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateEmptyObjectArray()
        {
            Object[] emptyArray = new Object[0];
            var duplicatedValue = Duplicate(emptyArray);
            Assert.NotNull(duplicatedValue);
            Assert.Empty(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateNullObjectArray()
        {
            Object[] nullArray = null;
            var duplicatedValue = Duplicate(nullArray);
            Assert.Null(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateEmptyPrimitiveArray()
        {
            int[] emptyArray = new int[0];
            var duplicatedValue = Duplicate(emptyArray);
            Assert.NotNull(duplicatedValue);
            Assert.Empty(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectArrayWithNullValues()
        {
            var array = new ClassWithoutSerializableAttribute[] { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var duplicatedValue = Duplicate(array);
            Assert.Equal(array.Length, duplicatedValue.Length);
            Assert.Equal(array[0].PublicPropertyValue, duplicatedValue[0].PublicPropertyValue);
            Assert.Null(duplicatedValue[1]);
            Assert.Equal(array[2].PublicPropertyValue, duplicatedValue[2].PublicPropertyValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateNullPrimitiveList()
        {
            List<int> nullList = null;
            var duplicatedValue = Duplicate(nullList);
            Assert.Null(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateEmptyObjectList()
        {
            List<Object> emptyList = new List<Object>();
            var duplicatedValue = Duplicate(emptyList);
            Assert.NotNull(duplicatedValue);
            Assert.Empty(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateNullObjectList()
        {
            List<object> nullList = null;
            var duplicatedValue = Duplicate(nullList);
            Assert.Null(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateEmptyPrimitiveList()
        {
            List<int> emptyList = new List<int>();
            var duplicatedValue = Duplicate(emptyList);
            Assert.NotNull(duplicatedValue);
            Assert.Empty(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectListWithNullValues()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var duplicatedValue = Duplicate(list);
            Assert.Equal(list.Count, duplicatedValue.Count);
            Assert.Equal(list[0].PublicPropertyValue, duplicatedValue[0].PublicPropertyValue);
            Assert.Null(duplicatedValue[1]);
            Assert.Equal(list[2].PublicPropertyValue, duplicatedValue[2].PublicPropertyValue);
        }

        protected void PrimitiveTestSuite<T>(params T[] values)
        {
            foreach (var value in values)
            {
                DuplicatePrimitiveValue(value);
                DuplicateWrappedValue(value);
                DuplicateArrayOfOne(value);
                DuplicateListOfOne(value);
            }
            DuplicateArrayValue<T>(values);
            DuplicateList<T>(values);
            DuplicateReadOnly<T>(values.First());
            DuplicateCSharp6StyleReadOnly<T>(values.First());
            DuplicateWrappedPrimitive<T>(values.First());
        }

        private void DuplicateWrappedPrimitive<T>(T value)
        {
            var instance = new GenericBaseClass<T>();
            instance.Value = value;
            var duplicatedValue = Duplicate(instance);

            _output.WriteLine("Type: " + typeof(T));
            Assert.Equal(instance.Value,
                         duplicatedValue.Value);
        }

        private void DuplicatePrimitiveValue<T>(T instance)
        {
            var duplicatedValue = Duplicate(instance);

            _output.WriteLine("Type: " + typeof(T));
            Assert.Equal(instance,
                         duplicatedValue);
        }

        private void DuplicateReadOnly<T>(T value)
        {
            var instance = new ClassWithReadOnlyProperty<T>(value);
            var duplicatedValue = Duplicate(instance);

            _output.WriteLine("Type: " + typeof(T));
            Assert.Equal(instance.Value,
                         duplicatedValue.Value);
        }

        private void DuplicateCSharp6StyleReadOnly<T>(T value)
        {
            var instance = new ClassWithCSharp6StyleReadOnlyProperty<T>(value);
            var duplicatedValue = Duplicate(instance);

            _output.WriteLine("Type: " + typeof(T));
            Assert.Equal(instance.Value,
                         duplicatedValue.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateReadOnlyObjectProperty()
        {
            var instance = new ClassWithReadOnlyProperty<ClassWithGenericInt>(new ClassWithGenericInt { Value = 38 });
            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Value,
                         duplicatedValue.Value);
        }

        private void DuplicateWrappedValue<T>(T value)
        {
            var wrappedObject = new Wrapper<T>
            {
                Value = value
            };

            Type targetType = typeof(T);
            var duplicatedValue = Duplicate(wrappedObject);

            _output.WriteLine("Type: " + typeof(T));
            Assert.Equal(wrappedObject.Value,
                            duplicatedValue.Value);
        }

        private void DuplicateArrayValue<T>(T[] value)
        {
            var duplicatedValue = Duplicate(value);

            _output.WriteLine("Array Type: " + typeof(T));
            Assert.Equal(value,
                         duplicatedValue);
        }

        private void DuplicateArrayOfOne<T>(T value)
        {
            var array = new T[1];
            array[0] = value;

            var duplicatedValue = Duplicate(array);

            _output.WriteLine("Type: " + typeof(T));
            Assert.NotNull(duplicatedValue);
            Assert.Equal(array,
                         duplicatedValue);
        }

        private void DuplicateListOfOne<T>(T value)
        {
            var list = new List<T>
                        {
                            value
                        };

            var duplicatedValue = Duplicate(list);

            Assert.Equal(list,
                         duplicatedValue);
        }

        private void DuplicateList<T>(T[] value)
        {
            var list = value.ToList();

            var duplicatedValue = Duplicate(list);

            Assert.Equal(list,
                         duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateStruct()
        {
            var instance = new Wrapper<StructForTesting> { Value = new StructForTesting { Value = 1 } };
            var duplicatedValue = Duplicate(instance);
            Assert.Equal(instance.Value, duplicatedValue.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateShortString()
        {
            var duplicatedValue = Duplicate("abc");
            //Assert.Equal(5, bytes.Length);
            Assert.Equal("abc", duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateMediumString()
        {
            var str = RandomString(3000);
            var duplicatedValue = Duplicate(str);
            Assert.Equal(str, duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateLongString()
        {
            var str = RandomString(100000);
            var duplicatedValue = Duplicate(str);
            Assert.Equal(str, duplicatedValue);
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [Fact]
        [Trait("Category", "Unit")]
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

            Assert.Equal(classInstance.PublicFieldValue,
                            duplicatedValue.PublicFieldValue);
            Assert.Equal(classInstance.GetPrivateFieldValue(),
                            duplicatedValue.GetPrivateFieldValue());
            Assert.Equal(classInstance.InternalFieldValue,
                            duplicatedValue.InternalFieldValue);
            Assert.Equal(classInstance.PublicPropertyValue,
                            duplicatedValue.PublicPropertyValue);
            Assert.Equal(classInstance.GetPrivatePropertyValue(),
                            duplicatedValue.GetPrivatePropertyValue());
            Assert.Equal(classInstance.InternalPropertyValue,
                            duplicatedValue.InternalPropertyValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        //[ExpectedException(typeof(ObjectExistsInCurrentSerializationGraphException))]
        public void DuplicateCircularReference()
        {
            var instance1 = new CircularReference { Id = 1 };
            var instance2 = new CircularReference { Id = 2 };
            instance1.Child = instance2;
            instance2.Parent = instance1;

            var duplicatedValue = Duplicate(instance1);

            Assert.True(ReferenceEquals(duplicatedValue,
                                          duplicatedValue.Child.Parent));

            Assert.Equal(1, duplicatedValue.Id);
            Assert.Equal(2, duplicatedValue.Child.Id);
            Assert.Equal(1, duplicatedValue.Child.Parent.Id);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateTrackMultipleReference()
        {
            var instance = new ClassWithGenericInt(123);
            var list = new List<ClassWithGenericInt>
                        {
                            instance,
                            instance
                        };

            var duplicatedValue = Duplicate(list);

            Assert.Equal(list.Count,
                            duplicatedValue.Count);
            Assert.Equal(list,
                         duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateTrackMultipleCustomDictionaryReferenceStringInt()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructor<string, int>
                           {
                               { "potato", 2 }
                           };
            var list = new List<CustomDictionaryWithoutSerializationConstructor<string, int>>
                        {
                            instance,
                            instance
                        };

            var duplicatedValue = Duplicate(list);

            Assert.Equal(list.Count,
                            duplicatedValue.Count);
            Assert.Equal(2,
                            duplicatedValue.Count);
            Assert.Single(duplicatedValue[0]);
            Assert.Equal(2,
                            duplicatedValue[0]["potato"]);
            Assert.True(ReferenceEquals(duplicatedValue[0],
                                          duplicatedValue[1]));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateTrackMultipleCustomDictionaryReferenceIntInt()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructor<int, int>
                           {
                               { 123, 2 }
                           };
            var list = new List<CustomDictionaryWithoutSerializationConstructor<int, int>>
                        {
                            instance,
                            instance
                        };

            var duplicatedValue = Duplicate(list);

            Assert.Equal(list.Count,
                            duplicatedValue.Count);
            Assert.Equal(2,
                            duplicatedValue.Count);
            Assert.Single(duplicatedValue[0]);
            Assert.Equal(2,
                            duplicatedValue[0][123]);
            Assert.True(ReferenceEquals(duplicatedValue[0],
                                          duplicatedValue[1]));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateTrackMultipleCustomDictionaryReferenceStringClass()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructor<string, ClassWithGenericInt>()
            {
                {"Key1", new ClassWithGenericInt(1)},
                {"Key2", new ClassWithGenericInt(2)},
                {"Key3", null}
            };
            var list = new List<CustomDictionaryWithoutSerializationConstructor<string, ClassWithGenericInt>>
                        {
                            instance,
                            instance
                        };

            var duplicatedValue = Duplicate(list);

            Assert.Equal(list.Count,
                            duplicatedValue.Count);
            Assert.Equal(2,
                            duplicatedValue.Count);
            Assert.Equal(3,
                            duplicatedValue[0].Count);
            Assert.Equal(new ClassWithGenericInt(1),
                            duplicatedValue[0]["Key1"]);
            Assert.Equal(new ClassWithGenericInt(2),
                            duplicatedValue[0]["Key2"]);
            Assert.Null(duplicatedValue[0]["Key3"]);
            Assert.True(ReferenceEquals(duplicatedValue[0],
                                          duplicatedValue[1]));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateTrackMultipleCustomDictionaryReferenceClassClass()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructor<ClassWithGenericInt, ClassWithGenericInt>()
            {
                {new ClassWithGenericInt(1), new ClassWithGenericInt(3)},
                {new ClassWithGenericInt(2), new ClassWithGenericInt(1)}
            };
            var list = new List<CustomDictionaryWithoutSerializationConstructor<ClassWithGenericInt, ClassWithGenericInt>>
                        {
                            instance,
                            instance
                        };

            var duplicatedValue = Duplicate(list);

            Assert.Equal(list.Count,
                            duplicatedValue.Count);
            Assert.Equal(2,
                            duplicatedValue.Count);
            Assert.Equal(2,
                            duplicatedValue[0].Count);
            Assert.Equal(new ClassWithGenericInt(3),
                            duplicatedValue[0][new ClassWithGenericInt(1)]);
            Assert.Equal(new ClassWithGenericInt(1),
                            duplicatedValue[0][new ClassWithGenericInt(2)]);
            Assert.True(ReferenceEquals(duplicatedValue[0],
                                          duplicatedValue[1]));
            Assert.True(ReferenceEquals(duplicatedValue[0].Keys.Single(x => x.Value == new ClassWithGenericInt(1).Value),
                                          duplicatedValue[1][new ClassWithGenericInt(2)]));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateTrackMultipleCustomDictionaryReferenceCustomComparer()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructor<StructForTesting, int>
                           {
                               { new StructForTesting { Value = 3 }, 2 }
                           };
            var list = new List<CustomDictionaryWithoutSerializationConstructor<StructForTesting, int>>
                        {
                            instance,
                            instance
                        };

            var duplicatedValue = Duplicate(list);

            Assert.Equal(list.Count,
                            duplicatedValue.Count);
            Assert.Equal(2,
                            duplicatedValue.Count);
            Assert.Single(duplicatedValue[0]);
            Assert.Equal(2,
                            duplicatedValue[0][new StructForTesting { Value = 3 }]);
            Assert.True(ReferenceEquals(duplicatedValue[0],
                                          duplicatedValue[1]));
            Assert.Equal(instance.Comparer.GetType(),
                            duplicatedValue[0].Comparer.GetType());
            Assert.True(ReferenceEquals(duplicatedValue[0].Comparer,
                                          duplicatedValue[1].Comparer));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateTrackMultipleCustomDictionaryReferenceStringIntWihoutComparerConstructor()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructorWithoutComparerConstructor<string, int>
                           {
                               { "potato", 2 }
                           };
            var list = new List<CustomDictionaryWithoutSerializationConstructorWithoutComparerConstructor<string, int>>
                        {
                            instance,
                            instance
                        };

            var duplicatedValue = Duplicate(list);

            Assert.Equal(list.Count,
                            duplicatedValue.Count);
            Assert.Equal(2,
                            duplicatedValue.Count);
            Assert.Single(duplicatedValue[0]);
            Assert.Equal(2,
                            duplicatedValue[0]["potato"]);
            Assert.True(ReferenceEquals(duplicatedValue[0],
                                          duplicatedValue[1]));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateTrackMultipleCustomDictionaryReferenceIntIntWihoutComparerConstructor()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructorWithoutComparerConstructor<int, int>
                           {
                               { 123, 2 }
                           };
            var list = new List<CustomDictionaryWithoutSerializationConstructorWithoutComparerConstructor<int, int>>
                        {
                            instance,
                            instance
                        };

            var duplicatedValue = Duplicate(list);

            Assert.Equal(list.Count,
                            duplicatedValue.Count);
            Assert.Equal(2,
                            duplicatedValue.Count);
            Assert.Single(duplicatedValue[0]);
            Assert.Equal(2,
                            duplicatedValue[0][123]);
            Assert.True(ReferenceEquals(duplicatedValue[0],
                                          duplicatedValue[1]));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateTrackMultipleCustomDictionaryReferenceStringClassWihoutComparerConstructor()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructorWithoutComparerConstructor<string, ClassWithGenericInt>()
            {
                {"Key1", new ClassWithGenericInt(1)},
                {"Key2", new ClassWithGenericInt(2)},
                {"Key3", null}
            };
            var list = new List<CustomDictionaryWithoutSerializationConstructorWithoutComparerConstructor<string, ClassWithGenericInt>>
                        {
                            instance,
                            instance
                        };

            var duplicatedValue = Duplicate(list);

            Assert.Equal(list.Count,
                            duplicatedValue.Count);
            Assert.Equal(2,
                            duplicatedValue.Count);
            Assert.Equal(3,
                            duplicatedValue[0].Count);
            Assert.Equal(new ClassWithGenericInt(1),
                            duplicatedValue[0]["Key1"]);
            Assert.Equal(new ClassWithGenericInt(2),
                            duplicatedValue[0]["Key2"]);
            Assert.Null(duplicatedValue[0]["Key3"]);
            Assert.True(ReferenceEquals(duplicatedValue[0],
                                          duplicatedValue[1]));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateTrackMultipleCustomDictionaryReferenceClassClassWihoutComparerConstructor()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructorWithoutComparerConstructor<ClassWithGenericInt, ClassWithGenericInt>()
            {
                {new ClassWithGenericInt(1), new ClassWithGenericInt(3)},
                {new ClassWithGenericInt(2), new ClassWithGenericInt(1)}
            };
            var list = new List<CustomDictionaryWithoutSerializationConstructorWithoutComparerConstructor<ClassWithGenericInt, ClassWithGenericInt>>
                        {
                            instance,
                            instance
                        };

            var duplicatedValue = Duplicate(list);

            Assert.Equal(list.Count,
                            duplicatedValue.Count);
            Assert.Equal(2,
                            duplicatedValue.Count);
            Assert.Equal(2,
                            duplicatedValue[0].Count);
            Assert.Equal(new ClassWithGenericInt(3),
                            duplicatedValue[0][new ClassWithGenericInt(1)]);
            Assert.Equal(new ClassWithGenericInt(1),
                            duplicatedValue[0][new ClassWithGenericInt(2)]);
            Assert.True(ReferenceEquals(duplicatedValue[0],
                                          duplicatedValue[1]));
            Assert.True(ReferenceEquals(duplicatedValue[0].Keys.Single(x => x.Value == new ClassWithGenericInt(1).Value),
                                          duplicatedValue[1][new ClassWithGenericInt(2)]));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateTrackMultipleCustomDictionaryReferenceCustomComparerWihoutComparerConstructor()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructorWithoutComparerConstructor<StructForTesting, int>
                           {
                               { new StructForTesting { Value = 3 }, 2 }
                           };
            var list = new List<CustomDictionaryWithoutSerializationConstructorWithoutComparerConstructor<StructForTesting, int>>
                        {
                            instance,
                            instance
                        };

            var duplicatedValue = Duplicate(list);

            Assert.Equal(list.Count,
                            duplicatedValue.Count);
            Assert.Equal(2,
                            duplicatedValue.Count);
            Assert.Single(duplicatedValue[0]);
            Assert.Equal(2,
                            duplicatedValue[0][new StructForTesting { Value = 3 }]);
            Assert.True(ReferenceEquals(duplicatedValue[0],
                                          duplicatedValue[1]));
            Assert.Equal(instance.Comparer.GetType(),
                            duplicatedValue[0].Comparer.GetType());
            Assert.True(ReferenceEquals(duplicatedValue[0].Comparer,
                                          duplicatedValue[1].Comparer));
        }

        [Fact]
        [Trait("Category", "Unit")]
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

            Assert.Equal(list.Count,
                            duplicatedValue.Count);
            Assert.Equal(list,
                         duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateClassWithoutSerializableAttribute()
        {
            var instance = new ClassWithoutSerializableAttribute
            {
                PublicPropertyValue = 4
            };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.PublicPropertyValue,
                            duplicatedValue.PublicPropertyValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateClassWithGenericBase()
        {
            var instance = new ClassWithGenericInt()
            {
                Value = 4
            };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Value,
                            duplicatedValue.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateGenericClass()
        {
            var instance = new GenericBaseClass<int>()
            {
                Value = 4
            };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Value,
                            duplicatedValue.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateDictionaryStringObject()
        {
            var instance = new Dictionary<string, object>()
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateDictionaryObjectKeyAndNullableIntValue()
        {
            var instance = new Dictionary<object, int?>()
            {
                {new ClassWithGenericInt(1), 1},
                {new ClassWithGenericInt(2), 2},
                {new ClassWithGenericInt(3), null}
            };

            var duplicatedValue = Duplicate(instance);

            CompareDictionaries(instance,
                                duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateDictionaryObjectObject()
        {
            var instance = new Dictionary<object, object>()
            {
                {new ClassWithGenericInt(1), 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateDictionaryIntString()
        {
            var instance = new Dictionary<int, string>()
            {
                {1, "Value1"},
                {2, "Value2"},
                {3, "Value3"}
            };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateWrappedNullDictionary()
        {
            var instance = new GenericBaseClass<Dictionary<int, string>>();
            var duplicatedValue = Duplicate(instance);

            Assert.Null(duplicatedValue.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateDictionaryGuidString()
        {
            var instance = new Dictionary<Guid, string>()
            {
                {Guid.NewGuid(), "Value1"},
                {Guid.NewGuid(), "Value2"},
                {Guid.NewGuid(), "Value3"}
            };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateCustomDictionary()
        {
            var instance = new CustomDictionary
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        private class TypeValueCargo
        {
            public Type Type { get; set; }
            public ICollection Values { get; set; }
        }

        [Fact]
        [Trait("Category", "Unit")]
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
            var vk = (TKey[])valuesForKey;
            var rnd = new Random();
            var instance = new Dictionary<TKey, TValue>();
            foreach (var key in vk)
            {
                instance.Add((TKey)key, valuesForValues[rnd.Next(0, valuesForValues.Length)]);
            }

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectKeyDictionary()
        {
            var instance = new Dictionary<object, int>();
            instance.Add(new ClassWithGenericInt(3), 1);
            instance.Add(new ClassWithGenericInt(5), 4);

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateEmptyDictionary()
        {
            var instance = new Dictionary<string, int>();

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
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

            Assert.Equal(instance.Value.Count,
                            duplicatedValue.Value.Count);
            Assert.Equal(instance.Value.Keys,
                         duplicatedValue.Value.Keys);
            foreach (var kvp in instance.Value)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue.Value[kvp.Key]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateCustomDictionaryWithComparer()
        {
            var instance = new CustomDictionary(StringComparer.CurrentCultureIgnoreCase)
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Comparer.GetType(),
                            duplicatedValue.Comparer.GetType());
            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateDictionaryWithCustomComparer()
        {
            var instance = new Dictionary<StructForTesting, object>(new StructForTestingComparer())
            {
                {new StructForTesting { Value = 1 }, 123},
                {new StructForTesting { Value = 2 }, "abc"},
                {new StructForTesting { Value = 3 }, new ClassWithGenericInt(3) },
            };


            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Comparer.GetType(),
                            duplicatedValue.Comparer.GetType());
            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateCustomDictionaryStringKeyAndStringValue()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructor<string, string>()
            {
                {"Key1", "123"},
                {"Key2", "abc"},
                {"Key3", null}
            };
            instance.SomeProperty = Guid.NewGuid().ToString();

            var duplicatedValue = Duplicate(instance);
            Assert.Equal(instance.SomeProperty,
                           duplicatedValue.SomeProperty);

            CompareDictionaries(instance,
                                duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateCustomDictionaryStringKeyAndClassValue()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructor<string, ClassWithGenericInt>()
            {
                {"Key1", new ClassWithGenericInt(1)},
                {"Key2", new ClassWithGenericInt(2)},
                {"Key3", null}
            };
            instance.SomeProperty = Guid.NewGuid().ToString();

            var duplicatedValue = Duplicate(instance);
            Assert.Equal(instance.SomeProperty,
                           duplicatedValue.SomeProperty);

            CompareDictionaries(instance,
                                duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateCustomDictionaryClassKeyAndStringValue()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructor<ClassWithGenericInt, string>()
            {
                {new ClassWithGenericInt(1), "Key1"},
                {new ClassWithGenericInt(2), "Key2"}
            };
            instance.SomeProperty = Guid.NewGuid().ToString();

            var duplicatedValue = Duplicate(instance);
            Assert.Equal(instance.SomeProperty,
                           duplicatedValue.SomeProperty);

            CompareDictionaries(instance,
                                duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateCustomDictionaryClassKeyAndClassValue()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructor<ClassWithGenericInt, ClassWithGenericInt>()
            {
                {new ClassWithGenericInt(1), new ClassWithGenericInt(3)},
                {new ClassWithGenericInt(2), new ClassWithGenericInt(4)}
            };
            instance.SomeProperty = Guid.NewGuid().ToString();

            var duplicatedValue = Duplicate(instance);
            Assert.Equal(instance.SomeProperty,
                           duplicatedValue.SomeProperty);

            CompareDictionaries(instance,
                                duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateCustomDictionaryObjectKeyAndNullableIntValue()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructor<object, int?>()
            {
                {new ClassWithGenericInt(1), 1},
                {new ClassWithGenericInt(2), 2},
                {new ClassWithGenericInt(3), null}
            };
            instance.SomeProperty = Guid.NewGuid().ToString();

            var duplicatedValue = Duplicate(instance);
            Assert.Equal(instance.SomeProperty,
                           duplicatedValue.SomeProperty);

            CompareDictionaries(instance,
                                duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateCustomDictionaryObjectKeyAndObjectValue()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructor<object, object>()
            {
                {new ClassWithGenericInt(1), new ClassWithGenericInt(4)},
                {new ClassWithGenericInt(2), new ClassWithGenericInt(5)},
                {new ClassWithGenericInt(3), null}
            };
            instance.SomeProperty = Guid.NewGuid().ToString();

            var duplicatedValue = Duplicate(instance);
            Assert.Equal(instance.SomeProperty,
                           duplicatedValue.SomeProperty);

            CompareDictionaries(instance,
                                duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateCustomDictionaryWithoutSerializationConstructor()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructor<object, object>()
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
                {"Key4", null },
            };

            instance.SomeProperty = Guid.NewGuid().ToString();

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.SomeProperty,
                           duplicatedValue.SomeProperty);
            Assert.Equal(instance.Comparer.GetType(),
                           duplicatedValue.Comparer.GetType());
            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
            Assert.False(ReferenceEquals(instance["Key3"], duplicatedValue["Key3"]));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateCustomDictionaryWithoutSerializationConstructorAndCustomComparer()
        {
            var key3 = new StructForTesting { Value = 3 };
            var instance = new CustomDictionaryWithoutSerializationConstructor<StructForTesting, object>(new StructForTestingComparer())
            {
                {new StructForTesting { Value = 1 }, 123},
                {new StructForTesting { Value = 2 }, "abc"},
                {key3, new ClassWithGenericInt(3) },
            };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Comparer.GetType(),
                            duplicatedValue.Comparer.GetType());
            Assert.False(ReferenceEquals(instance.Comparer, duplicatedValue.Comparer));
            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }

            Assert.False(ReferenceEquals(instance[key3], duplicatedValue[key3]));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicatePrimitiveCustomDictionaryWithoutSerializationConstructor()
        {
            var instance = new CustomDictionaryWithoutSerializationConstructor<string, string>()
            {
                {"Key1", "123"},
                {"Key2", "abc"},
                {"Key4", null },
            };

            instance.SomeProperty = Guid.NewGuid().ToString();

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Comparer.GetType(),
                            duplicatedValue.Comparer.GetType());
            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
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

            Assert.Equal(instance.SomeProperty,
                            duplicatedValue.SomeProperty);
            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
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

            Assert.Equal(0,
                            duplicatedValue.SomeProperty); // default value for property
            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
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

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateCustomDictionaryWithNullDictionaryProperty()
        {
            var instance = new CustomDictionaryWithDictionaryProperty<string, object>
            {
                {"Key1", 123},
                {"Key2", "abc"},
                {"Key3", new ClassWithGenericInt(3) },
            };

            var duplicatedValue = Duplicate(instance);

            CompareDictionaries(instance,
                                duplicatedValue);
            Assert.Null(instance.SwitchedDictionary);
        }

        private static void CompareDictionaries<TKey, TValue>(Dictionary<TKey, TValue> instance, Dictionary<TKey, TValue> duplicatedValue)
        {
            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Comparer.GetType(),
                            duplicatedValue.Comparer.GetType());
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var key in instance.Keys)
            {
                if (instance[key] != null
                    && instance[key].GetType() == typeof(object))
                {
                    Assert.True(duplicatedValue[key] != null && duplicatedValue[key].GetType() == typeof(object));
                }
                else if (instance[key] != null
                    && instance[key].GetType() == typeof(ClassWithoutSerializableAttribute))
                {
                    Assert.Equal(((ClassWithoutSerializableAttribute)(object)instance[key]).PublicPropertyValue,
                                    ((ClassWithoutSerializableAttribute)(object)duplicatedValue[key]).PublicPropertyValue);
                }
                else
                {
                    Assert.Equal(instance[key],
                                    duplicatedValue[key]);
                }
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
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

            Assert.Equal(instance.SomeProperty,
                            duplicatedValue.SomeProperty);
            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateCustomDictionaryWithOfObjectString()
        {
            var instance = new CustomDictionaryWithAdditionalPropertiesAndGenerics<object, string>
            {
                { 123, "Key1"},
                { "abc", "Key2"},
                { new ClassWithGenericInt(3), "Key3" },
            };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.SomeProperty,
                            duplicatedValue.SomeProperty);
            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateEmptyCustomDictionaryWithAdditionalPropertiesAndGenerics()
        {
            var instance = new CustomDictionaryWithAdditionalPropertiesAndGenerics<string, int>();
            instance.SomeProperty = 849;

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.SomeProperty,
                            duplicatedValue.SomeProperty);
            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.Keys,
                         duplicatedValue.Keys);
            foreach (var kvp in instance)
            {
                Assert.Equal(kvp.Value,
                                duplicatedValue[kvp.Key]);
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateListWithMultipleTypes()
        {
            var list = new List<IHierarchy>
            {
                new ChildIntHierarchy(123),
                new ChildStringHierarchy("abc"),
            };

            var duplicatedValue = Duplicate(list);

            Assert.Equal(list.Count,
                            duplicatedValue.Count);
            Assert.Equal(list.OfType<ChildIntHierarchy>().First().Value,
                            duplicatedValue.OfType<ChildIntHierarchy>().First().Value);
            Assert.Equal(list.OfType<ChildStringHierarchy>().First().Value,
                            duplicatedValue.OfType<ChildStringHierarchy>().First().Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectWithListAsIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 } };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            Assert.Equal(instance.Items.ToList(),
                         duplicatedValue.Items.ToList());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectWithHashsetAsIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int>
            {
                Items = new HashSet<int> { 1, 2, 3 }
            };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            Assert.Equal(instance.Items.ToList(),
                         duplicatedValue.Items.ToList());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateHashSetWithEqualityComparer()
        {
            var instance = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "a", "b", "C" };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Comparer.GetType(),
                            duplicatedValue.Comparer.GetType());
            Assert.Equal(instance.Count(),
                            duplicatedValue.Count());
            Assert.Equal(instance.ToList(),
                         duplicatedValue.ToList());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectWithEnumProperty()
        {
            var instance = new GenericBaseClass<EnumForTestingInt32> { Value = EnumForTestingInt32.Two };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Value,
                            duplicatedValue.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateHashtable()
        {
            var instance = new Hashtable
                            {
                                {1, 2},
                                {"a", "b"},
                            };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance,
                         duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateHashtableWithEqualityComparer()
        {
            var instance = new Hashtable(StringComparer.CurrentCultureIgnoreCase)
                            {
                                {"e", 2},
                                {"a", "b"},
                            };

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(GetHashTableComparer(instance));
            Assert.NotNull(GetHashTableComparer(duplicatedValue));
            Assert.Equal(GetHashTableComparer(instance).GetType(),
                            GetHashTableComparer(duplicatedValue).GetType());
            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance,
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

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectWithArrayAsIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new[] { 1, 2, 3 } };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            Assert.Equal(instance.Items.ToList(),
                         duplicatedValue.Items.ToList());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectWithDistinctIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 1, 2, 3 }.Distinct() };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            Assert.Equal(instance.Items.ToList(),
                         duplicatedValue.Items.ToList());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectWithWhereIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Where(x => x > 1) };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            Assert.Equal(instance.Items.ToList(),
                         duplicatedValue.Items.ToList());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectWithOrderByIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.OrderBy(x => x) };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            Assert.Equal(instance.Items.ToList(),
                         duplicatedValue.Items.ToList());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectWithDefaultIfEmptyIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.DefaultIfEmpty(123) };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            Assert.Equal(instance.Items.ToList(),
                         duplicatedValue.Items.ToList());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectWithExceptIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Except(new[] { 2 }) };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            Assert.Equal(instance.Items.ToList(),
                         duplicatedValue.Items.ToList());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectWithUnionIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Union(new[] { 4 }) };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            Assert.Equal(instance.Items.ToList(),
                         duplicatedValue.Items.ToList());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectWithIntersectIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Intersect(new[] { 2 }) };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            Assert.Equal(instance.Items.ToList(),
                         duplicatedValue.Items.ToList());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectWithOfTypeIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.OfType<int>() };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            Assert.Equal(instance.Items.ToList(),
                         duplicatedValue.Items.ToList());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectWithSkipByIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Skip(1) };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            Assert.Equal(instance.Items.ToList(),
                         duplicatedValue.Items.ToList());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectWithTakeByIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Take(1) };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            Assert.Equal(instance.Items.ToList(),
                         duplicatedValue.Items.ToList());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateObjectWithSelectByIEnumerable()
        {
            var instance = new ClassWithIEnumerable<int> { Items = new List<int> { 1, 2, 3 }.Select(x => x * 2) };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Items.Count(),
                            duplicatedValue.Items.Count());
            Assert.Equal(instance.Items.ToList(),
                         duplicatedValue.Items.ToList());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateSimpleFunc()
        {
            var testData = new List<int> { 1, 2, 3, 4, 5, 6 };
            System.Func<int, bool> instance = x => x > 3;

            Assert.Throws<NotSupportedException>(() => Duplicate(instance));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateSimpleExpression()
        {
            var testData = new List<int> { 1, 2, 3, 4, 5, 6 };

            Expression<System.Func<int, bool>> instance = x => x > 3;

            Assert.Throws<NotSupportedException>(() => Duplicate(instance));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateAnonymousObject()
        {
            var instance = new { Property1 = "hello", Property2 = 123 };
            Assert.Throws<NotSupportedException>(() => Duplicate(instance));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateArray()
        {
            var instance = new int[] { 123, 456 };
            var duplicatedValue = Duplicate(instance);
            Assert.Equal(instance, duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateArrayWithNullablePrimitive()
        {
            var instance = new int?[] { 123, null, 456 };
            var duplicatedValue = Duplicate(instance);
            Assert.Equal(instance, duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateArrayObject()
        {
            var instance = new ClassWithoutSerializableAttribute[] { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var duplicatedValue = Duplicate(instance);
            Assert.Equal(2, duplicatedValue.Length);
            Assert.Equal(123, duplicatedValue[0].PublicPropertyValue);
            Assert.Equal(456, duplicatedValue[1].PublicPropertyValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateArrayOfClassWithGenericInt()
        {
            var obj = new ClassWithGenericInt(123);
            var instance = new ClassWithGenericInt[] { obj, null };

            var duplicatedValue = Duplicate(instance);

            instance.Should().BeEquivalentTo(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateArrayObjectHierarchy()
        {
            var instance = new SomeBaseClass[] { new ClassWithGenericInt(123), new ClassWithGenericDouble(3.38D) };
            var duplicatedValue = Duplicate(instance);
            Assert.Equal(2, duplicatedValue.Length);
            Assert.Equal(123, (duplicatedValue[0] as ClassWithGenericInt).Value);
            Assert.Equal(3.38D, (duplicatedValue[1] as ClassWithGenericDouble).Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateSameObjectMultipleTimeInArray()
        {
            var obj123 = new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 };
            var instance = new ClassWithoutSerializableAttribute[] { obj123, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 }, obj123 };
            var duplicatedValue = Duplicate(instance);
            Assert.Equal(3, duplicatedValue.Length);
            Assert.Equal(123, duplicatedValue[0].PublicPropertyValue);
            Assert.Equal(456, duplicatedValue[1].PublicPropertyValue);
            Assert.Equal(123, duplicatedValue[2].PublicPropertyValue);
            Assert.True(ReferenceEquals(duplicatedValue[0], duplicatedValue[2]));
        }

        [Fact]
        [Trait("Category", "Unit")]
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

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateMultiDimensionalArray()
        {
            var instance = CreateMultiDimensionalArray<int>(8);
            SeedArray(instance);

            var duplicatedValue = Duplicate(instance);
            instance.Should().BeEquivalentTo(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateMultiDimensionalArrayOfObjects()
        {
            var instance = CreateMultiDimensionalArray<Object>(2);
            instance.SetValue(new object(), new[] { 0, 0 });
            instance.SetValue(new object(), new[] { 1, 0 });

            var duplicatedValue = Duplicate(instance);

            Assert.True(duplicatedValue.GetValue(new[] { 0, 0 }).GetType() == typeof(object));
            Assert.Null(duplicatedValue.GetValue(new[] { 0, 1 }));
            Assert.True(duplicatedValue.GetValue(new[] { 1, 0 }).GetType() == typeof(object));
            Assert.Null(duplicatedValue.GetValue(new[] { 1, 1 }));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateMultiDimensionalArrayOfClass()
        {
            var instance = CreateMultiDimensionalArray<ClassWithGenericInt>(2);
            var obj = new ClassWithGenericInt(123);
            instance.SetValue(obj, new[] { 0, 0 });
            instance.SetValue(new ClassWithGenericInt(456), new[] { 1, 0 });
            instance.SetValue(obj, new[] { 1, 1 });

            var duplicatedValue = Duplicate(instance);
            instance.Should().BeEquivalentTo(duplicatedValue);
            Assert.Equal(456, ((ClassWithGenericInt)duplicatedValue.GetValue(1, 0)).Value);
            Assert.True(ReferenceEquals(duplicatedValue.GetValue(0, 0), duplicatedValue.GetValue(1, 1)));
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


        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateJaggedArrayObject()
        {
            var instance = new object[2][];
            instance[0] = new object[] { new object(), null };
            instance[1] = new object[] { new object(), null };

            var duplicatedValue = Duplicate(instance);

            Assert.True(duplicatedValue[0][0].GetType() == typeof(object));
            Assert.Null(duplicatedValue[0][1]);
            Assert.True(duplicatedValue[1][0].GetType() == typeof(object));
            Assert.Null(duplicatedValue[1][1]);
        }


        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateJaggedArrayClass()
        {
            var obj = new ClassWithGenericInt(123);
            var instance = new ClassWithGenericInt[2][];
            instance[0] = new ClassWithGenericInt[] { obj, null };
            //instance[0] = new ClassWithGenericInt[] { obj, new ClassWithGenericInt(4563) };
            instance[1] = new ClassWithGenericInt[] { new ClassWithGenericInt(456), obj };

            var duplicatedValue = Duplicate(instance);

            instance.Should().BeEquivalentTo(duplicatedValue);

            Assert.Equal(456, ((ClassWithGenericInt)duplicatedValue[1][0]).Value);
            Assert.True(ReferenceEquals(duplicatedValue[0][0], duplicatedValue[1][1]));

            instance = new ClassWithGenericInt[2][];
            instance[0] = new ClassWithGenericInt[] { obj, new ClassWithGenericInt(4563) };
            instance[1] = new ClassWithGenericInt[] { new ClassWithGenericInt(456), obj };

            duplicatedValue = Duplicate(instance);

            instance.Should().BeEquivalentTo(duplicatedValue);
        }


        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateJaggedArrayClassInheritance()
        {
            var obj = new ClassWithGenericInt(123);
            var instance = new SomeBaseClass[2][];
            instance[0] = new SomeBaseClass[] { obj, new ClassWithGenericDouble(123.3D) };
            instance[1] = new SomeBaseClass[] { new ClassWithGenericInt(456), obj };

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(123, ((ClassWithGenericInt)duplicatedValue[0][0]).Value);
            Assert.Equal(123.3D, ((ClassWithGenericDouble)duplicatedValue[0][1]).Value);
            Assert.Equal(456, ((ClassWithGenericInt)duplicatedValue[1][0]).Value);
            Assert.True(ReferenceEquals(duplicatedValue[0][0], duplicatedValue[1][1]));
        }


        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateNullJaggedArray()
        {
            int[][] nullArray = null;
            var instance = new Wrapper<int[][]> { Value = nullArray };
            var duplicatedValue = Duplicate(instance);

            Assert.Null(duplicatedValue.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateThreeDimensionJaggedArray()
        {
            int[][][] instance = new[] { new int[2][], new int[3][] };
            instance[0][0] = new int[] { 123, 238 };
            var duplicatedValue = Duplicate(instance);

            instance.Should().BeEquivalentTo(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateEmptyJaggedArray()
        {
            int[][] instance = new int[2][];
            var duplicatedValue = Duplicate(instance);
            instance.Should().BeEquivalentTo(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateJaggedArray()
        {
            var instance = new int[][] { new int[] { 123, 238 }, new int[] { 456, 546, 784 }, null };
            var duplicatedValue = Duplicate(instance);
            Assert.Equal(instance.Length, duplicatedValue.Length);
            instance.Should().BeEquivalentTo(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void TrackArrayInMultipleObjects()
        {
            int[] arr = new[] { 1, 2, 3 };
            var instance1 = new CircularReference { Id = 1 };
            var instance2 = new CircularReference { Id = 2, Ids = arr };
            var instance3 = new CircularReference { Id = 3, Ids = arr };
            instance1.Child = instance2;
            instance2.Child = instance3;

            var duplicatedValue = Duplicate(instance1);

            Assert.Equal(arr, duplicatedValue.Child.Ids);
            Assert.True(ReferenceEquals(duplicatedValue.Child.Ids, duplicatedValue.Child.Child.Ids));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateClassWithDynamicObject()
        {
            ClassWithDynamicProperty instance = new ClassWithDynamicProperty { Value = 123 };

            var duplicatedValue = Duplicate(instance);
            Assert.NotNull(duplicatedValue);
            Assert.Equal(instance.Value, duplicatedValue.Value);
        }
        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateExpandoObject()
        {
            dynamic instance = new ExpandoObject();
            instance.Property1 = 123;
            instance.Property2 = "abc";
            instance.Property3 = new ClassWithGenericInt(349);
            instance.Property4 = new object();
            instance.Property5 = null;

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);

            Assert.Equal(instance.Property1,
                            duplicatedValue.Property1);
            Assert.Equal(instance.Property2,
                            duplicatedValue.Property2);
            Assert.Equal(instance.Property3,
                            duplicatedValue.Property3);
            Assert.True(duplicatedValue.Property4.GetType() == typeof(object));
            Assert.Null(duplicatedValue.Property5);

        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateEmptyExpandoObject()
        {
            dynamic instance = new ExpandoObject();

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
            Assert.Equal(0, (duplicatedValue as IDictionary<string, object>).Count);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateNullExpandoObject()
        {
            ExpandoObject instance = null;

            var duplicatedValue = Duplicate(instance);

            Assert.Null(duplicatedValue);

            var wrappedduplicatedValue2 = Duplicate(new Wrapper<ExpandoObject> { Value = instance });

            Assert.Null(wrappedduplicatedValue2.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateTypedQueue()
        {
            var instance = new Queue<int>();
            instance.Enqueue(1);
            instance.Enqueue(2);
            instance.Enqueue(3);

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);

            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.ToArray(),
                         duplicatedValue.ToArray());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateUntypedQueue()
        {
            var instance = new Queue();
            instance.Enqueue(1);
            instance.Enqueue(2);
            instance.Enqueue("abc");
            instance.Enqueue(new ClassWithGenericInt(123));

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);

            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.ToArray(),
                         duplicatedValue.ToArray());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateTypedStack()
        {
            var instance = new Stack<int>();
            instance.Push(1);
            instance.Push(2);
            instance.Push(3);

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);

            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.ToArray(),
                         duplicatedValue.ToArray());
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateUntypedStack()
        {
            var instance = new Stack();
            instance.Push(1);
            instance.Push(2);
            instance.Push("abc");
            instance.Push(new ClassWithGenericInt(123));

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);

            Assert.Equal(instance.Count,
                            duplicatedValue.Count);
            Assert.Equal(instance.ToArray(),
                         duplicatedValue.ToArray());
        }


        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateEnumEqualityComparer()
        {
            var instance = new Dictionary<EnumForTestingInt32, int> { { EnumForTestingInt32.One, 1 }, { EnumForTestingInt32.Two, 2 } };

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);

            CompareDictionaries(instance,
                                duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateAnObject()
        {
            var instance = new object();

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateAClassWithANullObjectProperty()
        {
            var instance = new ClassWithObjectProperty();

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
            Assert.Null(duplicatedValue.Obj);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateAClassWithANotNullObjectProperty()
        {
            var instance = new ClassWithObjectProperty { Obj = new object() };

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
            Assert.NotNull(duplicatedValue.Obj);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateAClassWithAnBoxedInt()
        {
            var instance = new ClassWithObjectProperty { Obj = 123 };

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
            Assert.Equal(123, (int)duplicatedValue.Obj);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateIList()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IList> { Value = list };

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
            Assert.NotNull(duplicatedValue.Value);
            var dList = duplicatedValue.Value as List<ClassWithoutSerializableAttribute>;

            Assert.Equal(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.Null(dList[1]);
            Assert.Equal(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateICollection()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<ICollection> { Value = list };

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
            Assert.NotNull(duplicatedValue.Value);
            var dList = duplicatedValue.Value as List<ClassWithoutSerializableAttribute>;

            Assert.Equal(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.Null(dList[1]);
            Assert.Equal(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateIQueryableContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IQueryable> { Value = list.AsQueryable() };

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
            Assert.NotNull(duplicatedValue.Value);
            var dList = duplicatedValue.Value.Cast<ClassWithoutSerializableAttribute>().ToArray();

            Assert.Equal(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.Null(dList[1]);
            Assert.Equal(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateGenericIQueryableContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IQueryable<ClassWithoutSerializableAttribute>> { Value = list.AsQueryable() };

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
            Assert.NotNull(duplicatedValue.Value);
            var dList = duplicatedValue.Value.Cast<ClassWithoutSerializableAttribute>().ToArray();

            Assert.Equal(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.Null(dList[1]);
            Assert.Equal(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }


        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateListOfIQueryableContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new List<GenericBaseClass<IQueryable>> { new GenericBaseClass<IQueryable> { Value = list.AsQueryable() } };

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
            Assert.Single(duplicatedValue);
            Assert.NotNull(duplicatedValue[0].Value);
            var dList = duplicatedValue[0].Value.Cast<ClassWithoutSerializableAttribute>().ToArray();

            Assert.Equal(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.Null(dList[1]);
            Assert.Equal(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateArrayOfIQueryableContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IQueryable>[] { new GenericBaseClass<IQueryable> { Value = list.AsQueryable() } };

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
            Assert.Single(duplicatedValue);
            Assert.NotNull(duplicatedValue[0].Value);
            var dList = duplicatedValue[0].Value.Cast<ClassWithoutSerializableAttribute>().ToArray();

            Assert.Equal(list[0].PublicPropertyValue, dList[0].PublicPropertyValue);
            Assert.Null(dList[1]);
            Assert.Equal(list[2].PublicPropertyValue, dList[2].PublicPropertyValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateListOfMultipleObjects()
        {
            var instance = new List<object> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new CircularReference { Id = 456 }, 1234 };

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
            Assert.Equal(4, duplicatedValue.Count);

            instance.Should().BeEquivalentTo(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateEmptyList()
        {
            var instance = new List<object>();

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
            Assert.Empty(duplicatedValue);

            instance.Should().BeEquivalentTo(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateNullList()
        {
            List<object> instance = null;

            var duplicatedValue = Duplicate(instance);

            Assert.Null(duplicatedValue);

            var wrappedduplicatedValue = Duplicate(new Wrapper<List<object>> { Value = instance });

            Assert.Null(wrappedduplicatedValue.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateArrayOfMultipleObjects()
        {
            var instance = new object[] { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new CircularReference { Id = 456 }, 1234 };

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
            Assert.Equal(4, duplicatedValue.Length);

            instance.Should().BeEquivalentTo(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateEmptyArray()
        {
            var instance = new object[0];

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
            Assert.Empty(duplicatedValue);

            instance.Should().BeEquivalentTo(duplicatedValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateNullArray()
        {
            object[] instance = null;

            var duplicatedValue = Duplicate(instance);

            Assert.Null(duplicatedValue);

            var wrappedduplicatedValue = Duplicate(new Wrapper<object[]> { Value = instance });

            Assert.Null(wrappedduplicatedValue.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateListOfIQueryableDirectly()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            List<IQueryable<ClassWithoutSerializableAttribute>> instance = new List<IQueryable<ClassWithoutSerializableAttribute>> { list.AsQueryable() };

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
            Assert.Single(duplicatedValue);
            var deserializedArray = duplicatedValue[0].ToArray();

            Assert.Equal(list[0].PublicPropertyValue, deserializedArray[0].PublicPropertyValue);
            Assert.Null(deserializedArray[1]);
            Assert.Equal(list[2].PublicPropertyValue, deserializedArray[2].PublicPropertyValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateArrayOfIQueryableDirectly()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            IQueryable<ClassWithoutSerializableAttribute>[] instance = new IQueryable<ClassWithoutSerializableAttribute>[] { list.AsQueryable() };

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
            Assert.Single(duplicatedValue);
            var deserializedArray = duplicatedValue[0].ToArray();

            Assert.Equal(list[0].PublicPropertyValue, deserializedArray[0].PublicPropertyValue);
            Assert.Null(deserializedArray[1]);
            Assert.Equal(list[2].PublicPropertyValue, deserializedArray[2].PublicPropertyValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateJaggedArrayOfIQueryableDirectly()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, null, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new IQueryable<ClassWithoutSerializableAttribute>[1][];
            instance[0] = new IQueryable<ClassWithoutSerializableAttribute>[3];

            instance[0][2] = list.AsQueryable();

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.Rank, duplicatedValue.Rank);
            Assert.Equal(instance[0].Length, duplicatedValue[0].Length);

            Assert.NotNull(duplicatedValue);
            Assert.Single(duplicatedValue);
            var deserializedArray = duplicatedValue[0][2].ToArray();

            Assert.Equal(list[0].PublicPropertyValue, deserializedArray[0].PublicPropertyValue);
            Assert.Null(deserializedArray[1]);
            Assert.Equal(list[2].PublicPropertyValue, deserializedArray[2].PublicPropertyValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateNullIQueryableContainedInAClass()
        {
            var instance = new GenericBaseClass<IQueryable> { Value = null };

            var duplicatedValue = Duplicate(instance);

            Assert.NotNull(duplicatedValue);
            Assert.Null(duplicatedValue.Value);
        }

        [Fact]
        [Trait("Category", "Unit")]
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

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateClassWithNonSerializableProperty()
        {
            var instance = new ClassWithNonSerializableField();
            instance.SerializableProperty = 839;
            instance.NonSerializableProperty = 33534;

            var duplicatedValue = Duplicate(instance);

            Assert.Equal(instance.SerializableProperty, duplicatedValue.SerializableProperty);
            Assert.Equal(0, duplicatedValue.NonSerializableProperty);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public unsafe void DuplicatePointerTypeIsNotSupported()
        {
            int[] a = new int[5] { 10, 20, 30, 40, 50 };
            fixed (int* p = &a[0])
            {
                var instance = new ClassWithPointer();
                instance.Value = p;
                Assert.Throws<NotSupportedException>(() => Duplicate(instance));
            }
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateAutoInitializeList()
        {
            var instance = new ClassWithInitializedList();
            instance.Values = null;
            var duplicatedValue = Duplicate(instance);
            Assert.Null(duplicatedValue.Values);
        }

        [Fact]
        [Trait("Category", "Unit")]
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

            Assert.Equal(3, duplicatedList.Count);
            dynamic duplicatedValue = duplicatedList[0];

            Assert.Equal(eo.Property1, duplicatedValue.Property1);
            Assert.Equal(eo.Property2, duplicatedValue.Property2);
            Assert.Equal(eo.Property3, duplicatedValue.Property3);
            Assert.True(duplicatedValue.Property4.GetType() == typeof(object));
            Assert.Null(duplicatedValue.Property5);
            Assert.True(ReferenceEquals(duplicatedList[0], duplicatedList[2]));
        }

        [Fact]
        [Trait("Category", "Unit")]
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

            Assert.Equal(3, duplicatedValue.Count);
            Dictionary<string, object> deserializedDict = (Dictionary<string, object>)duplicatedValue[0];

            Assert.Equal(dict["Property1"], deserializedDict["Property1"]);
            Assert.Equal(dict["Property2"], deserializedDict["Property2"]);
            Assert.Equal(dict["Property3"], deserializedDict["Property3"]);
            Assert.True(deserializedDict["Property4"].GetType() == typeof(object));
            Assert.Equal(dict["Property5"], deserializedDict["Property5"]);
            Assert.True(ReferenceEquals(duplicatedValue[0], duplicatedValue[2]));
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void DuplicateGroupByContainedInAClass()
        {
            var list = new List<ClassWithoutSerializableAttribute> { new ClassWithoutSerializableAttribute { PublicPropertyValue = 123 }, new ClassWithoutSerializableAttribute { PublicPropertyValue = 456 } };
            var instance = new GenericBaseClass<IEnumerable<IGrouping<int, ClassWithoutSerializableAttribute>>> { Value = list.GroupBy(x => x.PublicPropertyValue) };

            Assert.Throws<NotSupportedException>(() => Duplicate(instance));
        }
    }
}
