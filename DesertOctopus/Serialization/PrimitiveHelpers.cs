using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace DesertOctopus.Serialization
{
    internal static class PrimitiveHelpers
    {
        private static MethodInfo GetStreamWriteByteMethod()
        {
            return typeof(System.IO.Stream).GetMethod("WriteByte");
        }

        private static MethodInfo GetStreamWriteMethod()
        {
            return typeof(System.IO.Stream).GetMethod("Write");
        }

        private static MethodInfo GetStreamReadByteMethod()
        {
            return typeof(System.IO.Stream).GetMethod("ReadByte");
        }

        private static MethodInfo GetStreamReadMethod()
        {
            return typeof(System.IO.Stream).GetMethod("Read");
        }

        private static MethodInfo GetLongFromDoubleMethod()
        {
            return typeof(PrimitiveHelpers).GetMethod("GetLongFromDouble", BindingFlags.Static | BindingFlags.NonPublic);
        }

        private static MethodInfo GetDoubleFromLongMethod()
        {
            return typeof(PrimitiveHelpers).GetMethod("GetDoubleFromLong", BindingFlags.Static | BindingFlags.NonPublic);
        }

        private static MethodInfo GetUintFromSingleMethod()
        {
            return typeof(PrimitiveHelpers).GetMethod("GetUintFromSingle", BindingFlags.Static | BindingFlags.NonPublic);
        }

        private static MethodInfo GetSingleFromUintMethod()
        {
            return typeof(PrimitiveHelpers).GetMethod("GetSingleFromUint", BindingFlags.Static | BindingFlags.NonPublic);
        }

        private static Expression WriteByteArray(ParameterExpression outputStream, Expression obj)
        {
            var variables = new List<ParameterExpression>();
            var length = Expression.Parameter(typeof(int), "length");
            var i = Expression.Parameter(typeof(int), "i");

            variables.Add(length);
            variables.Add(i);

            var breakLabel = Expression.Label("breakLabel");
            var cond = Expression.LessThan(i, length);
            var loopBody = Expression.Block(
                                            WriteByte(outputStream, Expression.ArrayAccess(Expression.Convert(obj, typeof(byte[])), i)),
                                            Expression.Assign(i, Expression.Increment(i))
                                            );


            return Expression.Block(variables, 
                                    Expression.Assign(length, Expression.Property(Expression.Convert(obj, typeof(byte[])), "Length")),
                                    WriteInt32(outputStream, length),
                                    Expression.Loop(Expression.IfThenElse(cond, 
                                                                          loopBody, 
                                                                          Expression.Break(breakLabel)), 
                                                    breakLabel));
        }

        private static Expression ReadByteArray(ParameterExpression inputStream)
        {
            var variables = new List<ParameterExpression>();
            var arr = Expression.Parameter(typeof(byte[]), "arr");
            var length = Expression.Parameter(typeof(int), "length");
            var i = Expression.Parameter(typeof(int), "i");

            variables.Add(length);
            variables.Add(i);
            variables.Add(arr);

            var breakLabel = Expression.Label("breakLabel");
            var cond = Expression.LessThan(i, length);
            var loopBody = Expression.Block(
                                            Expression.Assign(Expression.ArrayAccess(arr, i), Expression.Convert(ReadByte(inputStream), typeof(byte))),
                                            Expression.Assign(i, Expression.Increment(i))
                                            );

            return Expression.Block(variables, 
                                    Expression.Assign(length, ReadInt32(inputStream)),
                                    Expression.Assign(arr, Expression.NewArrayBounds(typeof(byte), length)),
                                    Expression.Loop(Expression.IfThenElse(cond, 
                                                                          loopBody, 
                                                                          Expression.Break(breakLabel)), 
                                                    breakLabel),
                                    arr);
        }

        private static Expression WriteIntegerNumberPrimitive(ParameterExpression outputStream, Expression obj, int numberOfBytes, Type expectedType)
        {
            var tmp = Expression.Parameter(expectedType, "tmp");
            var expressions = new List<Expression>();

            expressions.Add(Expression.Assign(tmp, Expression.Convert(obj, expectedType)));

            for (int bits = numberOfBytes * 8 - 8; bits >= 8; bits -= 8)
            {
                expressions.Add(Expression.Call(outputStream, GetStreamWriteByteMethod(), Expression.Convert(Expression.And(Expression.RightShift(tmp, Expression.Constant(bits)), Expression.Convert(Expression.Constant(0xFFu), expectedType)), typeof(byte))));
            }
            expressions.Add(Expression.Call(outputStream, GetStreamWriteByteMethod(), Expression.Convert(Expression.And(tmp, Expression.Convert(Expression.Constant(0xFFu), expectedType)), typeof(byte))));
            return Expression.Block(new[] { tmp }, expressions);
            //return Expression.Empty();
        }

        private static Expression ReadIntegerNumberPrimitive(ParameterExpression inputStream, Expression numberOfBytes, Type expectedType)
        {
            var tmp = Expression.Parameter(expectedType, "tmp");
            var i = Expression.Parameter(typeof(Int32), "i");
            var expressions = new List<Expression>();
            expressions.Add(Expression.Assign(tmp, Expression.Default(expectedType)));
            expressions.Add(Expression.Assign(i, Expression.Constant(0)));
            

            // todo remove loop and 'hard code' the read bytes stuff?

            var breakLabel = Expression.Label("breakLabel");
            var loopBody = Expression.Block(Expression.Assign(tmp,

                                                              Expression.Or(
                                                                            Expression.LeftShift(tmp, Expression.Constant(8)),
                                                                            Expression.Convert(
                                                                                    ReadByte(inputStream)
                                                                                    //, typeof(byte))
                                                                                    , expectedType)
                                                                            )
                                                                )
                                                              //Expression.LeftShift(Expression.Or(tmp, Expression.Convert(ReadByte(inputStream), typeof(UInt32))), Expression.Constant(8))
                                                              //Expression.LeftShift(Expression.Or(tmp, Expression.Convert(ReadByte(inputStream), typeof(UInt32))), Expression.Constant(8))
                                                              //Expression.LeftShift(Expression.Or(tmp, Expression.Convert(ReadByte(inputStream), typeof(UInt32))), Expression.Constant(8))
                                                              ,
                                                              Expression.Assign(i, Expression.Add(i, Expression.Constant(1)))
                                            );


            var loop = Expression.Loop(Expression.IfThenElse(Expression.LessThan(i, Expression.Convert(numberOfBytes, typeof(Int32))),
                                                             loopBody,
                                                             Expression.Break(breakLabel)
                                                            ),
                                                            breakLabel);

            expressions.Add(loop);
            expressions.Add(tmp);



            /*

            System.IO.Stream ms;
            int tmp2 = 0;

            int i = 0;
            while (i < numberOfBytes)
            {
                byte tmpByte = (byte)ms.ReadByte();

                tmp2 = (tmp2 << 8) | tmpByte;
                tmp2 = (tmp2 | tmpByte) << 8;

            }
            */


            return Expression.Block(new[] { tmp, i }, expressions);
        }


        [System.Security.SecuritySafeCritical] 
        private static unsafe long GetLongFromDouble(double value)
        {
            long longValue = *(long*) &value;
            return longValue;
        }

        [System.Security.SecuritySafeCritical] 
        private static unsafe double GetDoubleFromLong(long value)
        {
            double doubleValue = *(double*) &value;
            return doubleValue;
        }

        [System.Security.SecuritySafeCritical] 
        private static unsafe uint GetUintFromSingle(float value)
        {
            uint uintValue = *(uint*) &value;
            return uintValue;
        }

        [System.Security.SecuritySafeCritical] 
        private static unsafe float GetSingleFromUint(uint value)
        {
            float singleValue = *(float*) &value;
            return singleValue;
        }


        #region byte

        public static Expression WriteNullableBool(ParameterExpression outputStream, Expression obj)
        {
            return WriteNullable(outputStream, obj, typeof(bool?), WriteBool);
        }

        public static Expression ReadNullableBool(ParameterExpression inputStream)
        {
            return ReadNullable(inputStream, typeof(bool?), ReadBool);
        }

        public static Expression WriteBool(ParameterExpression outputStream, Expression obj)
        {
            return WriteIntegerNumberPrimitive(outputStream, Expression.Condition(Expression.IsTrue(obj), Expression.Constant((byte)1), Expression.Constant((byte)0)), sizeof(byte), typeof(byte));
        }

        public static Expression ReadBool(ParameterExpression inputStream)
        {
            return Expression.Condition(Expression.Equal(ReadByte(inputStream), Expression.Constant(1)), Expression.Constant(true), Expression.Constant(false));
        }
        
        #endregion

        #region byte

        public static Expression WriteNullableByte(ParameterExpression outputStream, Expression obj)
        {
            return WriteNullable(outputStream, obj, typeof(byte?), WriteByte);
        }

        public static Expression ReadNullableByte(ParameterExpression inputStream)
        {
            return ReadNullable(inputStream, typeof(byte?), ReadByte);
        }

        public static Expression WriteByte(ParameterExpression outputStream, Expression obj)
        {
            return WriteIntegerNumberPrimitive(outputStream, obj, sizeof(byte), typeof(byte));
        }

        public static Expression ReadByte(ParameterExpression inputStream)
        {
            return Expression.Call(inputStream, GetStreamReadByteMethod());
        }

        public static Expression WriteNullableSByte(ParameterExpression outputStream, Expression obj)
        {
            return WriteNullable(outputStream, obj, typeof(sbyte?), WriteSByte);
        }

        public static Expression ReadNullableSByte(ParameterExpression inputStream)
        {
            return ReadNullable(inputStream, typeof(sbyte?), ReadSByte);
        }

        public static Expression WriteSByte(ParameterExpression outputStream, Expression obj)
        {
            return WriteIntegerNumberPrimitive(outputStream, obj, sizeof(sbyte), typeof(sbyte));
        }

        public static Expression ReadSByte(ParameterExpression inputStream)
        {
            return Expression.Convert(Expression.Call(inputStream, GetStreamReadByteMethod()), typeof(sbyte));
        }

        #endregion

        #region Int16

        public static Expression WriteNullableInt16(ParameterExpression outputStream, Expression obj)
        {
            return WriteNullable(outputStream, obj, typeof(short?), WriteInt16);
        }

        public static Expression ReadNullableInt16(ParameterExpression inputStream)
        {
            return ReadNullable(inputStream, typeof(short?), ReadInt16);
        }

        public static Expression WriteInt16(ParameterExpression outputStream, Expression obj)
        {
            return WriteIntegerNumberPrimitive(outputStream, obj, sizeof(short), typeof(short));
        }

        public static Expression ReadInt16(ParameterExpression inputStream)
        {
            return ReadIntegerNumberPrimitive(inputStream, Expression.Constant(sizeof(short)), typeof(short));
        }

        public static Expression WriteNullableUInt16(ParameterExpression outputStream, Expression obj)
        {
            return WriteNullable(outputStream, obj, typeof(ushort?), WriteUInt16);
        }

        public static Expression ReadNullableUInt16(ParameterExpression inputStream)
        {
            return ReadNullable(inputStream, typeof(ushort?), ReadUInt16);
        }

        public static Expression WriteUInt16(ParameterExpression outputStream, Expression obj)
        {
            return WriteIntegerNumberPrimitive(outputStream, obj, sizeof(ushort), typeof(ushort));
        }

        public static Expression ReadUInt16(ParameterExpression inputStream)
        {
            return ReadIntegerNumberPrimitive(inputStream, Expression.Constant(sizeof(ushort)), typeof(ushort));
        }

        #endregion
        
        #region Int32
        
        public static Expression WriteNullableInt32(ParameterExpression outputStream, Expression obj)
        {
            return WriteNullable(outputStream, obj, typeof(int?), WriteInt32);
        }

        public static Expression ReadNullableInt32(ParameterExpression inputStream)
        {
            return ReadNullable(inputStream, typeof(int?), ReadInt32);
        }

        public static Expression WriteInt32(ParameterExpression outputStream, Expression obj)
        {
            return WriteIntegerNumberPrimitive(outputStream, obj, sizeof(int), typeof(int));
        }

        public static Expression ReadInt32(ParameterExpression inputStream)
        {
            return ReadIntegerNumberPrimitive(inputStream, Expression.Constant(sizeof(int)), typeof(int));
        }

        public static Expression WriteNullableUInt32(ParameterExpression outputStream, Expression obj)
        {
            return WriteNullable(outputStream, obj, typeof(uint?), WriteUInt32);
        }

        public static Expression ReadNullableUInt32(ParameterExpression inputStream)
        {
            return ReadNullable(inputStream, typeof(uint?), ReadUInt32);
        }

        public static Expression WriteUInt32(ParameterExpression outputStream, Expression obj)
        {
            return WriteIntegerNumberPrimitive(outputStream, obj, sizeof(uint), typeof(uint));
        }

        public static Expression ReadUInt32(ParameterExpression inputStream)
        {
            return ReadIntegerNumberPrimitive(inputStream, Expression.Constant(sizeof(uint)), typeof(uint));
        }

        #endregion

        #region Int64
        
        public static Expression WriteNullableInt64(ParameterExpression outputStream, Expression obj)
        {
            return WriteNullable(outputStream, obj, typeof(long?), WriteInt64);
        }

        public static Expression ReadNullableInt64(ParameterExpression inputStream)
        {
            return ReadNullable(inputStream, typeof(long?), ReadInt64);
        }

        public static Expression WriteInt64(ParameterExpression outputStream, Expression obj)
        {
            return WriteIntegerNumberPrimitive(outputStream, obj, sizeof(long), typeof(long));
        }
        
        public static Expression ReadInt64(ParameterExpression inputStream)
        {
            return ReadIntegerNumberPrimitive(inputStream, Expression.Constant(sizeof(long)), typeof(long));
        }

        public static Expression WriteNullableUInt64(ParameterExpression outputStream, Expression obj)
        {
            return WriteNullable(outputStream, obj, typeof(ulong?), WriteUInt64);
        }

        public static Expression ReadNullableUInt64(ParameterExpression inputStream)
        {
            return ReadNullable(inputStream, typeof(ulong?), ReadUInt64);
        }

        public static Expression WriteUInt64(ParameterExpression outputStream, Expression obj)
        {
            return WriteIntegerNumberPrimitive(outputStream, obj, sizeof(ulong), typeof(long));
        }

        public static Expression ReadUInt64(ParameterExpression inputStream)
        {
            return ReadIntegerNumberPrimitive(inputStream, Expression.Constant(sizeof(ulong)), typeof(ulong));
        }

        #endregion

        #region Decimal

        public static Expression WriteNullableDecimal(ParameterExpression outputStream, Expression obj)
        {
            return WriteNullable(outputStream, obj, typeof(decimal?), WriteDecimal);
        }

        public static Expression ReadNullableDecimal(ParameterExpression inputStream)
        {
            return ReadNullable(inputStream, typeof(decimal?), ReadDecimal);
        }

        public static Expression WriteDecimal(ParameterExpression outputStream, Expression obj)
        {
            var variables = new List<ParameterExpression>();
            var bits = Expression.Parameter(typeof(int[]), "bits");
            var low = Expression.Parameter(typeof(ulong), "ulong");
            var mid = Expression.Parameter(typeof(ulong), "mid");
            var lowmid = Expression.Parameter(typeof(ulong), "lowmid");
            var high = Expression.Parameter(typeof(uint), "high");
            var scale = Expression.Parameter(typeof(uint), "scale");
            var sign = Expression.Parameter(typeof(uint), "sign");
            var scaleSign = Expression.Parameter(typeof(uint), "scaleSign");

            variables.Add(bits);
            variables.Add(low);
            variables.Add(mid);
            variables.Add(lowmid);
            variables.Add(high);
            variables.Add(scale);
            variables.Add(sign);
            variables.Add(scaleSign);
            

            return Expression.Block(variables,
                                    Expression.Assign(bits, Expression.Call(typeof(Decimal).GetMethod("GetBits", BindingFlags.Static | BindingFlags.Public), Expression.Convert(obj, typeof(Decimal)))),
                                    Expression.Assign(low, Expression.Convert(Expression.ArrayAccess(bits, Expression.Constant(0)) , typeof(ulong))),
                                    Expression.Assign(mid, Expression.LeftShift(Expression.Convert(Expression.ArrayAccess(bits, Expression.Constant(1)) , typeof(ulong)), Expression.Constant(32))),
                                    Expression.Assign(lowmid, Expression.Or(low, mid)),
                                    Expression.Assign(high, Expression.Convert(Expression.ArrayAccess(bits, Expression.Constant(2)), typeof(uint))),
                                    Expression.Assign(scale, Expression.And(Expression.RightShift(Expression.Convert(Expression.ArrayAccess(bits, Expression.Constant(3)), typeof(uint)), Expression.Constant(15)), Expression.Convert(Expression.Constant(0x01fe), typeof(uint)))),
                                    Expression.Assign(sign, Expression.RightShift(Expression.Convert(Expression.ArrayAccess(bits, Expression.Constant(3)), typeof(uint)), Expression.Constant(31))),
                                    Expression.Assign(scaleSign, Expression.Or(scale, sign)),

                                    WriteUInt64(outputStream, lowmid),
                                    WriteUInt32(outputStream, high),
                                    WriteUInt32(outputStream, scaleSign)
                                    );





            //int[] bits = Decimal.GetBits(value);

            //ulong low = (uint)bits[0];
            //ulong mid = ((ulong)(uint)bits[1]) << 32;
            //ulong lowmid = low | mid;

            //uint high = (uint)bits[2];

            //uint scale = ((uint)bits[3] >> 15) & 0x01fe;
            //uint sign = ((uint)bits[3]) >> 31;
            //uint scaleSign = scale | sign;

            //WritePrimitive(stream, lowmid);
            //WritePrimitive(stream, high);
            //WritePrimitive(stream, scaleSign);
        }

        public static Expression ReadDecimal(ParameterExpression inputStream)
        {
            var variables = new List<ParameterExpression>();

            var lowmid = Expression.Parameter(typeof(ulong), "lowmid");
            var high = Expression.Parameter(typeof(uint), "high");
            var scaleSign = Expression.Parameter(typeof(uint), "scaleSign");
            var scale = Expression.Parameter(typeof(int), "scale");
            var sign = Expression.Parameter(typeof(int), "sign");
            var arr = Expression.Parameter(typeof(int[]), "arr"); // todo: we could improve performance by initializing the array once per serialization

            variables.Add(lowmid);
            variables.Add(high);
            variables.Add(scaleSign);
            variables.Add(scale);
            variables.Add(sign);
            variables.Add(arr);

            return Expression.Block(variables,
                                    Expression.Assign(lowmid, ReadUInt64(inputStream)),
                                    Expression.Assign(high, ReadUInt32(inputStream)),
                                    Expression.Assign(scaleSign, ReadUInt32(inputStream)),
                                    
                                    Expression.Assign(scale, Expression.Convert(Expression.LeftShift(Expression.And(scaleSign, Expression.Convert(Expression.Constant(~1), typeof(uint))), Expression.Constant(15)), typeof(int))),
                                    Expression.Assign(sign, Expression.Convert(Expression.LeftShift(Expression.And(scaleSign, Expression.Convert(Expression.Constant(1), typeof(uint))), Expression.Constant(31)), typeof(int))),

                                    Expression.Assign(arr, Expression.NewArrayBounds(typeof(int), Expression.Constant(4))),

                                    Expression.Assign(Expression.ArrayAccess(arr, Expression.Constant(0)), Expression.Convert(lowmid, typeof(int))),
                                    Expression.Assign(Expression.ArrayAccess(arr, Expression.Constant(1)), Expression.Convert(Expression.RightShift(lowmid, Expression.Constant(32)), typeof(int))),
                                    Expression.Assign(Expression.ArrayAccess(arr, Expression.Constant(2)), Expression.Convert(high, typeof(int))),
                                    Expression.Assign(Expression.ArrayAccess(arr, Expression.Constant(3)), Expression.Or(scale, sign)),

                                    Expression.New(typeof(Decimal).GetConstructor(new Type[] { typeof(int[]) }), arr)
                                    );

            //ulong lowmid;
            //uint high, scaleSign;

            //ReadPrimitive(stream, out lowmid);
            //ReadPrimitive(stream, out high);
            //ReadPrimitive(stream, out scaleSign);

            //int scale = (int)((scaleSign & ~1) << 15);
            //int sign = (int)((scaleSign & 1) << 31);

            //var arr = s_decimalBitsArray;
            //if (arr == null)
            //    arr = s_decimalBitsArray = new int[4];

            //arr[0] = (int)lowmid;
            //arr[1] = (int)(lowmid >> 32);
            //arr[2] = (int)high;
            //arr[3] = scale | sign;

            //value = new Decimal(arr);
        }

        #endregion

        #region double

        public static Expression WriteNullableDouble(ParameterExpression outputStream, Expression obj)
        {
            return WriteNullable(outputStream, obj, typeof(double?), WriteDouble);
        }

        public static Expression ReadNullableDouble(ParameterExpression inputStream)
        {
            return ReadNullable(inputStream, typeof(double?), ReadDouble);
        }

        public static Expression WriteDouble(ParameterExpression outputStream, Expression obj)
        {
            var convertedExpr = Expression.Call(GetLongFromDoubleMethod(), obj);

            return WriteIntegerNumberPrimitive(outputStream, convertedExpr, sizeof(long), typeof(long));
        }

        public static Expression ReadDouble(ParameterExpression inputStream)
        {
            var longValue = ReadIntegerNumberPrimitive(inputStream, Expression.Constant(sizeof(long)), typeof(long));
            return Expression.Call(GetDoubleFromLongMethod(), longValue);
        }

        #endregion

        #region single
        
        public static Expression WriteNullableSingle(ParameterExpression outputStream, Expression obj)
        {
            return WriteNullable(outputStream, obj, typeof(float?), WriteSingle);
        }

        public static Expression ReadNullableSingle(ParameterExpression inputStream)
        {
            return ReadNullable(inputStream, typeof(float?), ReadSingle);
        }

        public static Expression WriteSingle(ParameterExpression outputStream, Expression obj)
        {
            var convertedExpr = Expression.Call(GetUintFromSingleMethod(), obj);

            return WriteIntegerNumberPrimitive(outputStream, convertedExpr, sizeof(uint), typeof(uint));
        }

        public static Expression ReadSingle(ParameterExpression inputStream)
        {
            var longValue = ReadIntegerNumberPrimitive(inputStream, Expression.Constant(sizeof(uint)), typeof(uint));
            return Expression.Call(GetSingleFromUintMethod(), longValue);
        }

        #endregion

        #region Char
        
        public static Expression WriteNullableChar(ParameterExpression outputStream, Expression obj)
        {
            return WriteNullable(outputStream, obj, typeof(char?), WriteChar);
        }

        public static Expression ReadNullableChar(ParameterExpression inputStream)
        {
            return ReadNullable(inputStream, typeof(char?), ReadChar);
        }

        public static Expression WriteChar(ParameterExpression outputStream, Expression obj)
        {
            return WriteIntegerNumberPrimitive(outputStream, Expression.Convert(obj, typeof(short)), sizeof(short), typeof(short));
        }

        public static Expression ReadChar(ParameterExpression inputStream)
        {
            return Expression.Convert(ReadIntegerNumberPrimitive(inputStream, Expression.Constant(sizeof(short)), typeof(short)), typeof(char));
        }

        #endregion

        #region DateTime

        public static Expression WriteNullableDateTime(ParameterExpression outputStream, Expression obj)
        {
            return WriteNullable(outputStream, obj, typeof(DateTime?), WriteDateTime);
        }

        public static Expression ReadNullableDateTime(ParameterExpression inputStream)
        {
            return ReadNullable(inputStream, typeof(DateTime?), ReadDateTime);
        }

        public static Expression WriteDateTime(ParameterExpression outputStream, Expression obj)
        {
            return WriteIntegerNumberPrimitive(outputStream, Expression.Call(Expression.Convert(obj, typeof(DateTime)), typeof(DateTime).GetMethod("ToBinary")) , sizeof(long), typeof(long));
        }

        public static Expression ReadDateTime(ParameterExpression inputStream)
        {
            return Expression.Call(typeof(DateTime).GetMethod("FromBinary", BindingFlags.Static | BindingFlags.Public),
                                   ReadIntegerNumberPrimitive(inputStream, Expression.Constant(sizeof(long)), typeof(long)));
        }

        #endregion

        #region TimeSpan

        public static Expression WriteNullableTimeSpan(ParameterExpression outputStream, Expression obj)
        {
            return WriteNullable(outputStream, obj, typeof(TimeSpan?), WriteTimeSpan);
        }

        public static Expression ReadNullableTimeSpan(ParameterExpression inputStream)
        {
            return ReadNullable(inputStream, typeof(TimeSpan?), ReadTimeSpan);
        }

        public static Expression WriteTimeSpan(ParameterExpression outputStream, Expression obj)
        {
            return WriteIntegerNumberPrimitive(outputStream, Expression.Property(Expression.Convert(obj, typeof(TimeSpan)), typeof(TimeSpan).GetProperty("Ticks")), sizeof(long), typeof(long));
        }

        public static Expression ReadTimeSpan(ParameterExpression inputStream)
        {
            return Expression.Call(typeof(TimeSpan).GetMethod("FromTicks", BindingFlags.Static | BindingFlags.Public),
                                   ReadIntegerNumberPrimitive(inputStream, Expression.Constant(sizeof(long)), typeof(long)));
        }

        #endregion

        #region BigInteger

        public static Expression WriteNullableBigInteger(ParameterExpression outputStream, Expression obj)
        {
            return WriteNullable(outputStream, obj, typeof(BigInteger?), WriteBigInteger);
        }

        public static Expression ReadNullableBigInteger(ParameterExpression inputStream)
        {
            return ReadNullable(inputStream, typeof(BigInteger?), ReadBigInteger);
        }

        public static Expression WriteBigInteger(ParameterExpression outputStream, Expression obj)
        {
            return WriteByteArray(outputStream, Expression.Call(Expression.Convert(obj, typeof(BigInteger)), typeof(BigInteger).GetMethod("ToByteArray")));
        }

        public static Expression ReadBigInteger(ParameterExpression inputStream)
        {
            return Expression.New(typeof(BigInteger).GetConstructor(new []{ typeof(byte[])}), 
                                  ReadByteArray(inputStream));
        }

        #endregion



        #region nullable

        private static Expression WriteNullable(ParameterExpression outputStream, Expression obj, Type nullableType, Func<ParameterExpression, Expression, Expression> primitiveWriter)
        {
            var underlyingType = Nullable.GetUnderlyingType(nullableType);

            return Expression.IfThenElse(Expression.Equal(obj, Expression.Constant(null)),
                                         WriteByte(outputStream, Expression.Constant((byte)0)),
                                         Expression.Block(
                                                            WriteByte(outputStream, Expression.Constant((byte)1)),
                                                            primitiveWriter(outputStream, Expression.Convert(obj, underlyingType))
                                                            )
                                        );
        }

        private static Expression ReadNullable(ParameterExpression inputStream, Type nullableType, Func<ParameterExpression, Expression> primitiveReader)
        {
            var tmp = Expression.Parameter(nullableType, "tmp");
            return Expression.Block(new List<ParameterExpression> { tmp },
                                    Expression.IfThenElse(Expression.Equal(Expression.Convert(ReadByte(inputStream), typeof(byte)), Expression.Constant((byte)0)),
                                         Expression.Assign(tmp, Expression.Constant(null, nullableType)),
                                         Expression.Assign(tmp, Expression.Convert(primitiveReader(inputStream), nullableType))),
                                    tmp);
        }

        #endregion


        private static readonly ConstructorInfo EndOfStreamExceptionConstructor = typeof(EndOfStreamException).GetConstructor(new Type[0]);


        #region string


        public static Expression ReadString(ParameterExpression inputStream)
        {
            var expressions = new List<Expression>();
            var variables = new List<ParameterExpression>();
            var encoding = Expression.Parameter(typeof(UTF8Encoding), "encoding");
            var nbBytes = Expression.Parameter(typeof(byte), "nbBytes");
            var length = Expression.Parameter(typeof(Int32), "length");
            var buffer = Expression.Parameter(typeof(byte[]), "buffer");
            var i = Expression.Parameter(typeof(int), "i");
            var r = Expression.Parameter(typeof(int), "r");
            var newInstance = Expression.Parameter(typeof(string), "newInstance");

            variables.Add(encoding);
            variables.Add(nbBytes);
            variables.Add(length);
            variables.Add(buffer);
            variables.Add(i);
            variables.Add(r);
            variables.Add(newInstance);

            // expressions.Add(Expression.Assign(nbBytes, Expression.Convert(ReadByte(inputStream), typeof(byte))));

            expressions.Add(Expression.Assign(i, Expression.Constant(0)));
            expressions.Add(Expression.Assign(r, Expression.Constant(0)));
            //expressions.Add(Expression.Assign(nbBytes, Expression.Convert(ReadByte(inputStream), typeof(byte))));
            //expressions.Add(Expression.Assign(length, Expression.Convert(ReadIntegerNumberPrimitive(inputStream, nbBytes, typeof(Int32)), typeof(Int32))));
            expressions.Add(Expression.Assign(length, Expression.Convert(ReadInt32(inputStream), typeof(Int32))));
            expressions.Add(Expression.Assign(buffer, Expression.NewArrayBounds(typeof(byte), length)));

            expressions.Add(Expression.Assign(encoding, Expression.New(typeof(UTF8Encoding).GetConstructor(new[] { typeof(bool), typeof(bool) }), Expression.Constant(false), Expression.Constant(true))));
            //expressions.Add(Expression.Assign(length, Expression.Call(encoding, typeof(UTF8Encoding).GetMethod("GetByteCount", new[] { typeof(string) }), Expression.Convert(obj, typeof(string)))));



            var loopBody = Expression.Block(
                                            Expression.Assign(r, Expression.Call(inputStream, GetStreamReadMethod(), buffer, i, Expression.Convert(Expression.Subtract(length, i), typeof(int)))),
                                            Expression.IfThenElse(Expression.Equal(r, Expression.Constant(0)),
                                                                    Expression.Throw(Expression.New(EndOfStreamExceptionConstructor)),
                                                                    Expression.Assign(i, Expression.Add(i, r))
                                                                  )
                                            );


            var breakLabel = Expression.Label("breakLabel");

            var loop = Expression.Loop(Expression.IfThenElse(Expression.LessThan(i, length),
                                                             loopBody,
                                                             Expression.Break(breakLabel)
                                                            ),
                                                            breakLabel);

            expressions.Add(loop);
            expressions.Add(Expression.Assign(newInstance, Expression.Convert(Expression.Call(encoding, typeof(UTF8Encoding).GetMethod("GetString", new Type[] { typeof(byte[]) }), buffer), typeof(string))));

            return Expression.Block(variables, 
                                    Expression.Block(Expression.IfThenElse(Expression.Equal(Expression.Convert(ReadByte(inputStream), typeof(byte)), Expression.Constant((byte)0)),
                                                                           Expression.Assign(newInstance, Expression.Constant(null, typeof(string))),
                                                                           Expression.Block(expressions))
                                                        ,
                                                        newInstance)
                                                );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe int GetByteCount(string value)
        {
            if (value == null)
            {
                return 0;
            }

            fixed (char* ptr = value)
            {
            	return System.Text.Encoding.UTF8.GetByteCount(ptr, value.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteString(Stream stream, string value)
        {
            var encoding = System.Text.Encoding.UTF8;
            //var length = GetByteCount(value);
            //var bytes = encoding.GetBytes(value);

            //stream.Write(bytes, 0, bytes.Length);

            //using (var s = new StreamWriter(stream, Encoding.UTF8, 1024, true))
            //{
            //    s.Write(value);
            //}

            var bytes = System.Text.Encoding.UTF8.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);


        }

        public static Expression WriteString(ParameterExpression outputStream, Expression obj)
        {
            // format: number of bytes for length | length | value

            var expressions = new List<Expression>();
            var variables = new List<ParameterExpression>();
            var encoding = Expression.Parameter(typeof(Encoding), "encoding");
            var length = Expression.Parameter(typeof(Int32), "length");
            var buffer = Expression.Parameter(typeof(byte[]), "buffer");

            variables.Add(encoding);
            variables.Add(length);
            variables.Add(buffer);

            //var writeLength = Expression.IfThenElse(Expression.LessThanOrEqual(length, Expression.Constant((Int32)byte.MaxValue)),
            //                                        Expression.Block(
            //                                                        WriteByte(outputStream, Expression.Constant((byte)1)),
            //                                                        WriteByte(outputStream, Expression.Convert(length, typeof(byte)))
            //                                                        ),
            //                                        Expression.IfThenElse(Expression.LessThanOrEqual(length, Expression.Constant((Int32)short.MaxValue)),
            //                                                            Expression.Block(
            //                                                                            WriteByte(outputStream, Expression.Constant((byte)2)),
            //                                                                            WriteInt16(outputStream, length)
            //                                                                            ),
            //                                                            Expression.Block(
            //                                                                                WriteByte(outputStream, Expression.Constant((byte)4)),
            //                                                                                WriteInt32(outputStream, length)
            //                                                                            ))
            //                                            );
            var writeLength = WriteInt32(outputStream, length);

            Expression stringNotNull = Expression.Block(variables,
                                                        Expression.Assign(encoding, Expression.Constant(System.Text.Encoding.UTF8)),
                                                        //Expression.Assign(encoding, Expression.New(typeof(UTF8Encoding).GetConstructor(new[] { typeof(bool), typeof(bool) }), Expression.Constant(false), Expression.Constant(true))),
                                                        Expression.Assign(length, Expression.Call(encoding, typeof(Encoding).GetMethod("GetByteCount", new[] { typeof(string) }), Expression.Convert(obj, typeof(string)))),

                                                        WriteByte(outputStream, Expression.Constant((byte)1)),
                                                        writeLength,

                                                        Expression.Assign(buffer, Expression.NewArrayBounds(typeof(byte), length)),
                                                        Expression.Call(encoding, typeof(Encoding).GetMethod("GetBytes", new Type[] { typeof(string), typeof(int), typeof(int), typeof(byte[]), typeof(int) }),
                                                                         Expression.Convert(obj, typeof(string)),
                                                                         Expression.Constant(0),
                                                                         Expression.Property(Expression.Convert(obj, typeof(string)), "Length"),
                                                                         buffer,
                                                                         Expression.Constant(0)),
                                                        Expression.Call(outputStream, GetStreamWriteMethod(), buffer, Expression.Constant(0), length)
                                                        );

            expressions.Add(Expression.IfThenElse(Expression.Equal(obj, Expression.Constant(null)),
                                                  WriteByte(outputStream, Expression.Constant((byte)0)),
                                                  stringNotNull
                                                  )
                            );


            //expressions.Add(
            //                Expression.Call( typeof(PrimitiveHelpers).GetMethod("WriteString", BindingFlags.Static | BindingFlags.NonPublic), outputStream, Expression.Convert(obj, typeof(string)))
            //                );



            return Expression.Block(variables, expressions);
        }

        #endregion


    }
}
