//#define INLINE_PRIMITIVE_METHOD

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using DesertOctopus.Utilities;

namespace DesertOctopus.Serialization
{
    /// <summary>
    /// Helper class to handle primitive types serialization
    /// </summary>
    internal static class PrimitiveHelpers
    {
        private static Expression WriteByteArray(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            var variables = new List<ParameterExpression>();
            var length = Expression.Parameter(typeof(int), "length");
            var i = Expression.Parameter(typeof(int), "i");

            variables.Add(length);
            variables.Add(i);

            var breakLabel = Expression.Label("breakLabel");
            var cond = Expression.LessThan(i, length);
            var loopBody = Expression.Block(WriteByte(outputStream, Expression.ArrayAccess(Expression.Convert(obj, typeof(byte[])), i), objTracker),
                                            Expression.Assign(i, Expression.Increment(i)));

            return Expression.Block(variables,
                                    Expression.Assign(length, Expression.Property(Expression.Convert(obj, typeof(byte[])), "Length")),
                                    Expression.Assign(i, Expression.Constant(0, typeof(int))),
                                    WriteInt32(outputStream, length, objTracker),
                                    Expression.Loop(Expression.IfThenElse(cond,
                                                                          loopBody,
                                                                          Expression.Break(breakLabel)),
                                                    breakLabel));
        }

        private static Expression ReadByteArray(ParameterExpression inputStream, ParameterExpression objTracker)
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
            var loopBody = Expression.Block(Expression.Assign(Expression.ArrayAccess(arr, i), Expression.Convert(ReadByte(inputStream, objTracker), typeof(byte))),
                                            Expression.Assign(i, Expression.Increment(i)));

            return Expression.Block(variables,
                                    Expression.Assign(length, ReadInt32(inputStream, objTracker)),
                                    Expression.Assign(i, Expression.Constant(0, typeof(int))),
                                    Expression.Assign(arr, Expression.NewArrayBounds(typeof(byte), length)),
                                    Expression.Loop(Expression.IfThenElse(cond,
                                                                          loopBody,
                                                                          Expression.Break(breakLabel)),
                                                    breakLabel),
                                    arr);
        }

#if !INLINE_PRIMITIVE_METHOD

        #region VARINT

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static uint EncodeZigZag32(int n)
        {
            return (uint)((n << 1) ^ (n >> 31));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static ulong EncodeZigZag64(long n)
        {
            return (ulong)((n << 1) ^ (n >> 63));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int DecodeZigZag32(uint n)
        {
            return (int)(n >> 1) ^ -(int)(n & 1);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static long DecodeZigZag64(ulong n)
        {
            return (long)(n >> 1) ^ -(long)(n & 1);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static ulong ReadVarint64(Stream stream)
        {
            long result = 0;
            int offset = 0;

            for (; offset < 64; offset += 7)
            {
                int b = stream.ReadByte();
                if (b == -1)
                {
                    throw new EndOfStreamException();
                }

                result |= ((long)(b & 0x7f)) << offset;

                if ((b & 0x80) == 0)
                {
                    return (ulong)result;
                }
            }

            throw new InvalidDataException();
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static void WriteVarint64(Stream stream, ulong value)
        {
            for (; value >= 0x80u; value >>= 7)
            {
                stream.WriteByte((byte)(value | 0x80u));
            }

            stream.WriteByte((byte)value);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static uint ReadVarint32(Stream stream)
        {
            int result = 0;
            int offset = 0;

            for (; offset < 32; offset += 7)
            {
                int b = stream.ReadByte();
                if (b == -1)
                {
                    throw new EndOfStreamException();
                }

                result |= (b & 0x7f) << offset;

                if ((b & 0x80) == 0)
                {
                    return (uint)result;
                }
            }

            throw new InvalidDataException();
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static void WriteVarint32(Stream stream,
                                          uint value)
        {
            for (; value >= 0x80u; value >>= 7)
            {
                stream.WriteByte((byte)(value | 0x80u));
            }

            stream.WriteByte((byte)value);
        }

        #endregion

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static void WriteSignedInt64(Stream outputStream, long obj)
        {
            WriteVarint64(outputStream, EncodeZigZag64(obj));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static long ReadSignedInt64(Stream inputStream)
        {
            return DecodeZigZag64(ReadVarint64(inputStream));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static void WriteSignedInt32(Stream outputStream, int obj)
        {
            WriteVarint32(outputStream, EncodeZigZag32(obj));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int ReadSignedInt32(Stream inputStream)
        {
            return DecodeZigZag32(ReadVarint32(inputStream));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static void WriteUShortImpl(Stream outputStream, ushort obj)
        {
            outputStream.WriteByte((byte)obj);
            outputStream.WriteByte((byte)(obj >> 8));
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static ushort ReadUShortImpl(Stream inputStream)
        {
            uint value = (uint)inputStream.ReadByte();
            value |= (uint)inputStream.ReadByte() << 8;

            return (ushort)value;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static void WriteByteImpl(Stream outputStream, byte obj)
        {
            outputStream.WriteByte((byte)obj);
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static byte ReadByteImpl(Stream inputStream)
        {
            return (byte)inputStream.ReadByte();
        }

        private static Expression WriteIntegerNumberPrimitive(ParameterExpression outputStream, Expression obj, Expression objTracker, int numberOfBytes, Type expectedType)
        {
            if (expectedType == typeof(byte)
                || expectedType == typeof(sbyte))
            {
                var mi = typeof(PrimitiveHelpers).GetMethod(nameof(WriteByteImpl), BindingFlags.NonPublic | BindingFlags.Static);
                return Expression.Call(mi, outputStream, Expression.Convert(obj, typeof(byte)));
            }
            else if (expectedType == typeof(short)
                     || expectedType == typeof(ushort))
            {
                var mi = typeof(PrimitiveHelpers).GetMethod(nameof(WriteUShortImpl), BindingFlags.NonPublic | BindingFlags.Static);
                return Expression.Call(mi, outputStream, Expression.Convert(obj, typeof(ushort)));
            }
            else if (expectedType == typeof(int))
            {
                var mi = typeof(PrimitiveHelpers).GetMethod(nameof(WriteSignedInt32), BindingFlags.NonPublic | BindingFlags.Static);
                return Expression.Call(mi, outputStream, obj);
            }
            else if (expectedType == typeof(uint))
            {
                var mi = typeof(PrimitiveHelpers).GetMethod(nameof(WriteVarint32), BindingFlags.NonPublic | BindingFlags.Static);
                return Expression.Call(mi, outputStream, obj);
            }
            else if (expectedType == typeof(long))
            {
                var mi = typeof(PrimitiveHelpers).GetMethod(nameof(WriteSignedInt64), BindingFlags.NonPublic | BindingFlags.Static);
                return Expression.Call(mi, outputStream, obj);
            }
            else if (expectedType == typeof(ulong))
            {
                var mi = typeof(PrimitiveHelpers).GetMethod(nameof(WriteVarint64), BindingFlags.NonPublic | BindingFlags.Static);
                return Expression.Call(mi, outputStream, obj);
            }
            else
            {
                throw new InvalidOperationException("Unhandled type: " + expectedType);
            }
        }

        private static Expression ReadIntegerNumberPrimitive(ParameterExpression inputStream, ParameterExpression objTracker, int numberOfBytes, Type expectedType)
        {
            if (expectedType == typeof(byte)
               || expectedType == typeof(sbyte))
            {
                var mi = typeof(PrimitiveHelpers).GetMethod(nameof(ReadByteImpl), BindingFlags.NonPublic | BindingFlags.Static);
                return Expression.Convert(Expression.Call(mi, inputStream), expectedType);
            }
            else if (expectedType == typeof(short)
                     || expectedType == typeof(ushort))
            {
                var mi = typeof(PrimitiveHelpers).GetMethod(nameof(ReadUShortImpl), BindingFlags.NonPublic | BindingFlags.Static);
                return Expression.Convert(Expression.Call(mi, inputStream), expectedType);
            }
            else if (expectedType == typeof(int))
            {
                var mi = typeof(PrimitiveHelpers).GetMethod(nameof(ReadSignedInt32), BindingFlags.NonPublic | BindingFlags.Static);
                return Expression.Call(mi, inputStream);
            }
            else if (expectedType == typeof(uint))
            {
                var mi = typeof(PrimitiveHelpers).GetMethod(nameof(ReadVarint32), BindingFlags.NonPublic | BindingFlags.Static);
                return Expression.Call(mi, inputStream);
            }
            else if (expectedType == typeof(long))
            {
                var mi = typeof(PrimitiveHelpers).GetMethod(nameof(ReadSignedInt64), BindingFlags.NonPublic | BindingFlags.Static);
                return Expression.Call(mi, inputStream);
            }
            else if (expectedType == typeof(ulong))
            {
                var mi = typeof(PrimitiveHelpers).GetMethod(nameof(ReadVarint64), BindingFlags.NonPublic | BindingFlags.Static);
                return Expression.Call(mi, inputStream);
            }
            else
            {
                throw new InvalidOperationException("Unhandled type: " + expectedType);
            }
        }
#else

        #region VARINT

        private static Expression WriteVarint32(ParameterExpression outputStream, Expression obj)
        {
            return WriteVarint(outputStream,
                               obj,
                               Expression.Constant(0x80u, typeof(uint)),
                               typeof(uint));
        }

        private static Expression WriteVarint64(ParameterExpression outputStream, Expression obj)
        {
            return WriteVarint(outputStream,
                               obj,
                               Expression.Constant((ulong)0x80u, typeof(ulong)),
                               typeof(ulong));
        }

        private static Expression WriteVarint(ParameterExpression outputStream, Expression obj, Expression marker, Type tmpType)
        {
            var expressions = new List<Expression>();
            var variables = new List<ParameterExpression>();
            var breakLabel = Expression.Label("breakLabel");
            var tmp = Expression.Parameter(tmpType, "tmp");

            variables.Add(tmp);
            expressions.Add(Expression.Assign(tmp, Expression.Convert(obj, tmpType)));

            var cond = Expression.GreaterThanOrEqual(tmp, marker);
            var loopBody = Expression.Block(Expression.Call(outputStream, StreamMih.WriteByte(), Expression.Convert(Expression.Or(tmp, marker), typeof(byte))),
                                            Expression.RightShiftAssign(tmp, Expression.Constant(7)));

            expressions.Add(Expression.Loop(Expression.IfThenElse(cond,
                                                                  loopBody,
                                                                  Expression.Break(breakLabel)),
                                            breakLabel));
            expressions.Add(Expression.Call(outputStream, StreamMih.WriteByte(), Expression.Convert(tmp, typeof(byte))));

            return Expression.Block(variables, expressions);
        }

        private static Expression ReadVarint32(ParameterExpression inputStream,
                                               ParameterExpression objTracker)
        {
            return ReadVarint(inputStream, objTracker, 4 * 8, typeof(int), Expression.Constant(0, typeof(int)));
        }

        private static Expression ReadVarint64(ParameterExpression inputStream,
                                               ParameterExpression objTracker)
        {
            return ReadVarint(inputStream, objTracker, 8 * 8, typeof(long), Expression.Constant((long)0, typeof(long)));
        }

        private static Expression ReadVarint(ParameterExpression inputStream, ParameterExpression objTracker, int max, Type tmpType, Expression defaultValue)
        {
            var variables = new List<ParameterExpression>();

            var result = Expression.Parameter(tmpType, "result");
            var offset = Expression.Parameter(typeof(int), "offset");
            var b = Expression.Parameter(typeof(int), "b");
            var breakLabel = Expression.Label("breakLabel" + Guid.NewGuid().ToString("N"));

            variables.Add(result);
            variables.Add(offset);
            variables.Add(b);

            var cond = Expression.LessThan(offset, Expression.Constant(max));
            var loopBody = Expression.Block(Expression.Assign(b, ReadByte(inputStream, objTracker)),
                                           Expression.IfThen(Expression.Equal(b, Expression.Constant(-1)),
                                                             Expression.Throw(Expression.New(EndOfStreamExceptionMih.Constructor()))),

                                           Expression.OrAssign(result,
                                                               Expression.LeftShift(Expression.Convert(Expression.And(b, Expression.Constant(0x7f)),
                                                                                                       tmpType),
                                                                                    offset)),

                                           Expression.IfThen(Expression.Equal(Expression.And(b, Expression.Constant(0x80)), Expression.Constant(0)),
                                                             Expression.Break(breakLabel)),
                                           Expression.Assign(offset, Expression.Add(offset, Expression.Constant(7))));


            return Expression.Block(variables,
                                    Expression.Assign(result, defaultValue),
                                    Expression.Assign(offset, Expression.Constant(0)),
                                    Expression.Loop(Expression.IfThenElse(cond,
                                                                          loopBody,
                                                                          Expression.Break(breakLabel)),
                                                    breakLabel),
                                    result);
        }

        internal static Expression EncodeZigZag32(Expression obj)
        {
            // original algorithm: (uint)((n << 1) ^ (n >> 31));
            var variables = new List<ParameterExpression>();
            var tmp = Expression.Parameter(typeof(int), "tmp");
            variables.Add(tmp);

            return Expression.Block(variables,
                                    Expression.Assign(tmp, Expression.Convert(obj, typeof(int))),
                                    Expression.Convert(Expression.ExclusiveOr(Expression.LeftShift(tmp, Expression.Constant(1)),
                                                                              Expression.RightShift(tmp, Expression.Constant(63))),
                                                       typeof(uint)));
        }

        internal static Expression EncodeZigZag64(Expression obj)
        {
            // original algorithm: (ulong)((n << 1) ^ (n >> 63));
            var variables = new List<ParameterExpression>();
            var tmp = Expression.Parameter(typeof(long), "tmp");
            variables.Add(tmp);

            return Expression.Block(variables,
                                    Expression.Assign(tmp, Expression.Convert(obj, typeof(long))),
                                    Expression.Convert(Expression.ExclusiveOr(Expression.LeftShift(tmp, Expression.Constant(1)),
                                                                              Expression.RightShift(tmp, Expression.Constant(63))),
                                                       typeof(ulong)));
        }

        internal static Expression DecodeZigZag32(Expression obj)
        {
            // original algorithm: (int)(n >> 1) ^ -(int)(n & 1);
            var variables = new List<ParameterExpression>();
            var tmp = Expression.Parameter(typeof(uint), "tmp");
            variables.Add(tmp);

            return Expression.Convert(Expression.Block(variables,
                                                       Expression.Assign(tmp, Expression.Convert(obj, typeof(uint))),
                                           Expression.ExclusiveOr(Expression.Convert(Expression.RightShift(tmp, Expression.Constant(1)), typeof(int)),
                                                                  Expression.Negate(Expression.Convert(Expression.And(tmp, Expression.Constant((uint)1, typeof(uint))), typeof(int))))),
                                      typeof(int));
        }

        internal static Expression DecodeZigZag64(Expression obj)
        {
            // original algorithm: (long)(n >> 1) ^ -(long)(n & 1);
            var variables = new List<ParameterExpression>();
            var tmp = Expression.Parameter(typeof(ulong), "tmp");
            variables.Add(tmp);

            return Expression.Convert(Expression.Block(variables,
                                                       Expression.Assign(tmp, Expression.Convert(obj, typeof(ulong))),
                                           Expression.ExclusiveOr(Expression.Convert(Expression.RightShift(tmp, Expression.Constant(1)), typeof(long)),
                                                                  Expression.Negate(Expression.Convert(Expression.And(tmp, Expression.Constant((ulong)1, typeof(ulong))), typeof(long))))),
                                      typeof(long));
        }

        #endregion

        private static Expression WriteIntegerNumberPrimitive(ParameterExpression outputStream, Expression obj, Expression objTracker, int numberOfBytes, Type expectedType)
        {
            if (expectedType == typeof(byte)
                || expectedType == typeof(sbyte)
                || expectedType == typeof(short)
                || expectedType == typeof(ushort))
            {
                var tmp = Expression.Parameter(expectedType, "tmp");
                var expressions = new List<Expression>();

                expressions.Add(Expression.Assign(tmp, Expression.Convert(obj, expectedType)));

                for (int bits = (numberOfBytes * 8) - 8; bits >= 8; bits -= 8)
                {
                    expressions.Add(Expression.Call(outputStream, StreamMih.WriteByte(), Expression.Convert(Expression.And(Expression.RightShift(tmp, Expression.Constant(bits)), Expression.Convert(Expression.Constant(0xFFu), expectedType)), typeof(byte))));
                }

                expressions.Add(Expression.Call(outputStream, StreamMih.WriteByte(), Expression.Convert(Expression.And(tmp, Expression.Convert(Expression.Constant(0xFFu), expectedType)), typeof(byte))));
                return Expression.Block(new[] { tmp }, expressions);
            }
            else if (expectedType == typeof(int))
            {
                return WriteVarint32(outputStream, EncodeZigZag32(obj));
            }
            else if (expectedType == typeof(uint))
            {
                return WriteVarint32(outputStream, obj);
            }
            else if (expectedType == typeof(long))
            {
                return WriteVarint64(outputStream, EncodeZigZag64(obj));
            }
            else if (expectedType == typeof(ulong))
            {
                return WriteVarint64(outputStream, obj);
            }
            else
            {
                throw new InvalidOperationException("Unhandled type: " + expectedType);
            }
        }

        private static Expression ReadIntegerNumberPrimitive(ParameterExpression inputStream, ParameterExpression objTracker, int numberOfBytes, Type expectedType)
        {
            if (expectedType == typeof(byte)
                   || expectedType == typeof(sbyte)
                   || expectedType == typeof(short)
                   || expectedType == typeof(ushort))
            {
                var expressions = new List<Expression>();
                var tmp = Expression.Parameter(expectedType, "tmp");
                expressions.Add(Expression.Assign(tmp, Expression.Convert(Expression.Constant(0), expectedType)));

                for (int i = 0; i < numberOfBytes; i++)
                {
                    expressions.Add(Expression.Assign(tmp, Expression.Or(Expression.LeftShift(tmp, Expression.Constant(8)), Expression.Convert(ReadByte(inputStream, objTracker), expectedType))));
                }

                expressions.Add(tmp);

                return Expression.Block(new[] { tmp }, expressions);
            }
            else if (expectedType == typeof(int))
            {
                return Expression.Convert(DecodeZigZag32(ReadVarint32(inputStream, objTracker)), expectedType);
            }
            else if (expectedType == typeof(uint))
            {
                return Expression.Convert(ReadVarint32(inputStream, objTracker), expectedType);
            }
            else if (expectedType == typeof(long))
            {
                return Expression.Convert(DecodeZigZag64(ReadVarint64(inputStream, objTracker)), expectedType);
            }
            else if (expectedType == typeof(ulong))
            {
                return Expression.Convert(ReadVarint64(inputStream, objTracker), expectedType);
            }
            else
            {
                throw new InvalidOperationException("Unhandled type: " + expectedType);
            }
        }
#endif

        [System.Security.SecuritySafeCritical]
        internal static unsafe ulong GetLongFromDouble(double value)
        {
            ulong longValue = *(ulong*)&value;
            return longValue;
        }

        [System.Security.SecuritySafeCritical]
        internal static unsafe double GetDoubleFromLong(ulong value)
        {
            double doubleValue = *(double*)&value;
            return doubleValue;
        }

        [System.Security.SecuritySafeCritical]
        internal static unsafe uint GetUintFromSingle(float value)
        {
            uint uintValue = *(uint*)&value;
            return uintValue;
        }

        [System.Security.SecuritySafeCritical]
        internal static unsafe float GetSingleFromUint(uint value)
        {
            float singleValue = *(float*)&value;
            return singleValue;
        }

#region byte

        /// <summary>
        /// Generates an expression to handle nullable boolean serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable boolean serialization</returns>
        public static Expression WriteNullableBool(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteNullable(outputStream, obj, objTracker, typeof(bool?), WriteBool);
        }

        /// <summary>
        /// Generates an expression to handle nullable boolean deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable boolean deserialization</returns>
        public static Expression ReadNullableBool(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadNullable(inputStream, objTracker, typeof(bool?), ReadBool);
        }

        /// <summary>
        /// Generates an expression to handle boolean serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle boolean serialization</returns>
        public static Expression WriteBool(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteIntegerNumberPrimitive(outputStream, Expression.Condition(Expression.IsTrue(obj), Expression.Constant(SerializerObjectTracker.Value1), Expression.Constant(SerializerObjectTracker.Value0)), objTracker, sizeof(byte), typeof(byte));
        }

        /// <summary>
        /// Generates an expression to handle boolean deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle boolean deserialization</returns>
        public static Expression ReadBool(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return Expression.Condition(Expression.Equal(ReadByte(inputStream, objTracker), Expression.Constant(1)), Expression.Constant(true), Expression.Constant(false));
        }

#endregion

#region byte?

        /// <summary>
        /// Generates an expression to handle nullable byte serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable byte serialization</returns>
        public static Expression WriteNullableByte(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteNullable(outputStream, obj, objTracker, typeof(byte?), WriteByte);
        }

        /// <summary>
        /// Generates an expression to handle nullable byte deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable byte deserialization</returns>
        public static Expression ReadNullableByte(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadNullable(inputStream, objTracker, typeof(byte?), ReadByte);
        }

        /// <summary>
        /// Generates an expression to handle byte serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle byte serialization</returns>
        public static Expression WriteByte(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteIntegerNumberPrimitive(outputStream, obj, objTracker, sizeof(byte), typeof(byte));
        }

        /// <summary>
        /// Generates an expression to handle byte deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle byte deserialization</returns>
        public static Expression ReadByte(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return Expression.Call(inputStream, StreamMih.ReadByte());
        }

        /// <summary>
        /// Generates an expression to handle nullable sbyte serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable sbyte serialization</returns>
        public static Expression WriteNullableSByte(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteNullable(outputStream, obj, objTracker, typeof(sbyte?), WriteSByte);
        }

        /// <summary>
        /// Generates an expression to handle nullable sbyte deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable sbyte deserialization</returns>
        public static Expression ReadNullableSByte(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadNullable(inputStream, objTracker, typeof(sbyte?), ReadSByte);
        }

        /// <summary>
        /// Generates an expression to handle sbyte serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle sbyte serialization</returns>
        public static Expression WriteSByte(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteIntegerNumberPrimitive(outputStream, obj, objTracker, sizeof(sbyte), typeof(sbyte));
        }

        /// <summary>
        /// Generates an expression to handle sbyte deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle sbyte deserialization</returns>
        public static Expression ReadSByte(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return Expression.Convert(Expression.Call(inputStream, StreamMih.ReadByte()), typeof(sbyte));
        }

#endregion

#region Int16

        /// <summary>
        /// Generates an expression to handle nullable int16 serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable int16 serialization</returns>
        public static Expression WriteNullableInt16(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteNullable(outputStream, obj, objTracker, typeof(short?), WriteInt16);
        }

        /// <summary>
        /// Generates an expression to handle nullable int16 deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable int16 deserialization</returns>
        public static Expression ReadNullableInt16(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadNullable(inputStream, objTracker, typeof(short?), ReadInt16);
        }

        /// <summary>
        /// Generates an expression to handle int16 serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable boolean serialization</returns>
        public static Expression WriteInt16(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteIntegerNumberPrimitive(outputStream, obj, objTracker, sizeof(short), typeof(short));
        }

        /// <summary>
        /// Generates an expression to handle int16 deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle int16 deserialization</returns>
        public static Expression ReadInt16(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadIntegerNumberPrimitive(inputStream, objTracker, sizeof(short), typeof(short));
        }

        /// <summary>
        /// Generates an expression to handle nullable uint16 serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable uint16 serialization</returns>
        public static Expression WriteNullableUInt16(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteNullable(outputStream, obj, objTracker, typeof(ushort?), WriteUInt16);
        }

        /// <summary>
        /// Generates an expression to handle nullable uint16 deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable uint16 deserialization</returns>
        public static Expression ReadNullableUInt16(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadNullable(inputStream, objTracker, typeof(ushort?), ReadUInt16);
        }

        /// <summary>
        /// Generates an expression to handle uint16 serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle uint16 serialization</returns>
        public static Expression WriteUInt16(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteIntegerNumberPrimitive(outputStream, obj, objTracker, sizeof(ushort), typeof(ushort));
        }

        /// <summary>
        /// Generates an expression to handle uint16 deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle uint16 deserialization</returns>
        public static Expression ReadUInt16(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadIntegerNumberPrimitive(inputStream, objTracker, sizeof(ushort), typeof(ushort));
        }

#endregion

#region Int32

        /// <summary>
        /// Generates an expression to handle nullable int32 serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable int32 serialization</returns>
        public static Expression WriteNullableInt32(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteNullable(outputStream, obj, objTracker, typeof(int?), WriteInt32);
        }

        /// <summary>
        /// Generates an expression to handle nullable int32 deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable int32 deserialization</returns>
        public static Expression ReadNullableInt32(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadNullable(inputStream, objTracker, typeof(int?), ReadInt32);
        }

        /// <summary>
        /// Generates an expression to handle int32 serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle int32 serialization</returns>
        public static Expression WriteInt32(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteIntegerNumberPrimitive(outputStream, obj, objTracker, sizeof(int), typeof(int));
        }

        /// <summary>
        /// Generates an expression to handle int32 deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle int32 deserialization</returns>
        public static Expression ReadInt32(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadIntegerNumberPrimitive(inputStream, objTracker, sizeof(int), typeof(int));
        }

        /// <summary>
        /// Generates an expression to handle nullable uint32 serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable uint32 serialization</returns>
        public static Expression WriteNullableUInt32(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteNullable(outputStream, obj, objTracker, typeof(uint?), WriteUInt32);
        }

        /// <summary>
        /// Generates an expression to handle nullable uint32 deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable uint32 deserialization</returns>
        public static Expression ReadNullableUInt32(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadNullable(inputStream, objTracker, typeof(uint?), ReadUInt32);
        }

        /// <summary>
        /// Generates an expression to handle uint32 serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle uint32 serialization</returns>
        public static Expression WriteUInt32(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteIntegerNumberPrimitive(outputStream, obj, objTracker, sizeof(uint), typeof(uint));
        }

        /// <summary>
        /// Generates an expression to handle uint32 deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle uint32 deserialization</returns>
        public static Expression ReadUInt32(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadIntegerNumberPrimitive(inputStream, objTracker, sizeof(uint), typeof(uint));
        }

#endregion

#region Int64

        /// <summary>
        /// Generates an expression to handle nullable int64 serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable int64 serialization</returns>
        public static Expression WriteNullableInt64(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteNullable(outputStream, obj, objTracker, typeof(long?), WriteInt64);
        }

        /// <summary>
        /// Generates an expression to handle nullable int64 deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable int64 deserialization</returns>
        public static Expression ReadNullableInt64(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadNullable(inputStream, objTracker, typeof(long?), ReadInt64);
        }

        /// <summary>
        /// Generates an expression to handle int64 serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle int64 serialization</returns>
        public static Expression WriteInt64(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteIntegerNumberPrimitive(outputStream, obj, objTracker, sizeof(long), typeof(long));
        }

        /// <summary>
        /// Generates an expression to handle int64 deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle int64 deserialization</returns>
        public static Expression ReadInt64(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadIntegerNumberPrimitive(inputStream, objTracker, sizeof(long), typeof(long));
        }

        /// <summary>
        /// Generates an expression to handle nullable uint64 serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable uint64 serialization</returns>
        public static Expression WriteNullableUInt64(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteNullable(outputStream, obj, objTracker, typeof(ulong?), WriteUInt64);
        }

        /// <summary>
        /// Generates an expression to handle nullable uint64 deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable uint64 deserialization</returns>
        public static Expression ReadNullableUInt64(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadNullable(inputStream, objTracker, typeof(ulong?), ReadUInt64);
        }

        /// <summary>
        /// Generates an expression to handle uint64 serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle uint64 serialization</returns>
        public static Expression WriteUInt64(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteIntegerNumberPrimitive(outputStream, obj, objTracker, sizeof(ulong), typeof(ulong));
        }

        /// <summary>
        /// Generates an expression to handle uint64 deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle uint64 deserialization</returns>
        public static Expression ReadUInt64(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadIntegerNumberPrimitive(inputStream, objTracker, sizeof(ulong), typeof(ulong));
        }

#endregion

#region Decimal

        /// <summary>
        /// Generates an expression to handle nullable decimal serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable decimal serialization</returns>
        public static Expression WriteNullableDecimal(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteNullable(outputStream, obj, objTracker, typeof(decimal?), WriteDecimal);
        }

        /// <summary>
        /// Generates an expression to handle nullable decimal deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable decimal deserialization</returns>
        public static Expression ReadNullableDecimal(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadNullable(inputStream, objTracker, typeof(decimal?), ReadDecimal);
        }

        /// <summary>
        /// Generates an expression to handle decimal serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle decimal serialization</returns>
        public static Expression WriteDecimal(ParameterExpression outputStream, Expression obj, Expression objTracker)
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
                                    Expression.Assign(bits, Expression.Call(DecimalMih.GetBits(), Expression.Convert(obj, typeof(decimal)))),
                                    Expression.Assign(low, Expression.Convert(Expression.ArrayAccess(bits, Expression.Constant(0)), typeof(ulong))),
                                    Expression.Assign(mid, Expression.LeftShift(Expression.Convert(Expression.ArrayAccess(bits, Expression.Constant(1)), typeof(ulong)), Expression.Constant(32))),
                                    Expression.Assign(lowmid, Expression.Or(low, mid)),
                                    Expression.Assign(high, Expression.Convert(Expression.ArrayAccess(bits, Expression.Constant(2)), typeof(uint))),
                                    Expression.Assign(scale, Expression.And(Expression.RightShift(Expression.Convert(Expression.ArrayAccess(bits, Expression.Constant(3)), typeof(uint)), Expression.Constant(15)), Expression.Convert(Expression.Constant(0x01fe), typeof(uint)))),
                                    Expression.Assign(sign, Expression.RightShift(Expression.Convert(Expression.ArrayAccess(bits, Expression.Constant(3)), typeof(uint)), Expression.Constant(31))),
                                    Expression.Assign(scaleSign, Expression.Or(scale, sign)),
                                    WriteUInt64(outputStream, lowmid, objTracker),
                                    WriteUInt32(outputStream, high, objTracker),
                                    WriteUInt32(outputStream, scaleSign, objTracker));
        }

        /// <summary>
        /// Generates an expression to handle decimal deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object trakcer</param>
        /// <returns>An expression to handle decimal deserialization</returns>
        public static Expression ReadDecimal(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            var variables = new List<ParameterExpression>();

            var lowmid = Expression.Parameter(typeof(ulong), "lowmid");
            var high = Expression.Parameter(typeof(uint), "high");
            var scaleSign = Expression.Parameter(typeof(uint), "scaleSign");
            var scale = Expression.Parameter(typeof(int), "scale");
            var sign = Expression.Parameter(typeof(int), "sign");

            variables.Add(lowmid);
            variables.Add(high);
            variables.Add(scaleSign);
            variables.Add(scale);
            variables.Add(sign);

            var tempArray = Expression.Property(objTracker, DeserializerObjectTrackerMih.DecimalArray());

            return Expression.Block(variables,
                                    Expression.Assign(lowmid, ReadUInt64(inputStream, objTracker)),
                                    Expression.Assign(high, ReadUInt32(inputStream, objTracker)),
                                    Expression.Assign(scaleSign, ReadUInt32(inputStream, objTracker)),
                                    Expression.Assign(scale, Expression.Convert(Expression.LeftShift(Expression.And(scaleSign, Expression.Convert(Expression.Constant(~1), typeof(uint))), Expression.Constant(15)), typeof(int))),
                                    Expression.Assign(sign, Expression.Convert(Expression.LeftShift(Expression.And(scaleSign, Expression.Convert(Expression.Constant(1), typeof(uint))), Expression.Constant(31)), typeof(int))),
                                    Expression.Assign(Expression.ArrayAccess(tempArray, Expression.Constant(0)), Expression.Convert(lowmid, typeof(int))),
                                    Expression.Assign(Expression.ArrayAccess(tempArray, Expression.Constant(1)), Expression.Convert(Expression.RightShift(lowmid, Expression.Constant(32)), typeof(int))),
                                    Expression.Assign(Expression.ArrayAccess(tempArray, Expression.Constant(2)), Expression.Convert(high, typeof(int))),
                                    Expression.Assign(Expression.ArrayAccess(tempArray, Expression.Constant(3)), Expression.Or(scale, sign)),
                                    Expression.New(typeof(decimal).GetConstructor(new Type[] { typeof(int[]) }), tempArray));
        }

#endregion

#region double

        /// <summary>
        /// Generates an expression to handle nullable double serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable double serialization</returns>
        public static Expression WriteNullableDouble(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteNullable(outputStream, obj, objTracker, typeof(double?), WriteDouble);
        }

        /// <summary>
        /// Generates an expression to handle nullable double deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable double deserialization</returns>
        public static Expression ReadNullableDouble(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadNullable(inputStream, objTracker, typeof(double?), ReadDouble);
        }

        /// <summary>
        /// Generates an expression to handle double serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle double serialization</returns>
        public static Expression WriteDouble(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            var convertedExpr = Expression.Call(PrimitiveHelpersMih.GetLongFromDouble(), obj);

            return WriteIntegerNumberPrimitive(outputStream, convertedExpr, objTracker, sizeof(ulong), typeof(ulong));
        }

        /// <summary>
        /// Generates an expression to handle double deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle double deserialization</returns>
        public static Expression ReadDouble(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            var longValue = ReadIntegerNumberPrimitive(inputStream, objTracker, sizeof(ulong), typeof(ulong));
            return Expression.Call(PrimitiveHelpersMih.GetDoubleFromLong(), longValue);
        }

#endregion

#region single

        /// <summary>
        /// Generates an expression to handle nullable single serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable single serialization</returns>
        public static Expression WriteNullableSingle(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteNullable(outputStream, obj, objTracker, typeof(float?), WriteSingle);
        }

        /// <summary>
        /// Generates an expression to handle nullable single deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable single deserialization</returns>
        public static Expression ReadNullableSingle(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadNullable(inputStream, objTracker, typeof(float?), ReadSingle);
        }

        /// <summary>
        /// Generates an expression to handle single serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle single serialization</returns>
        public static Expression WriteSingle(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            var convertedExpr = Expression.Call(PrimitiveHelpersMih.GetUintFromSingle(), obj);

            return WriteIntegerNumberPrimitive(outputStream, convertedExpr, objTracker, sizeof(uint), typeof(uint));
        }

        /// <summary>
        /// Generates an expression to handle single deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle single deserialization</returns>
        public static Expression ReadSingle(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            var unitValue = ReadIntegerNumberPrimitive(inputStream, objTracker, sizeof(uint), typeof(uint));
            return Expression.Call(PrimitiveHelpersMih.GetSingleFromUint(), unitValue);
        }

#endregion

#region Char

        /// <summary>
        /// Generates an expression to handle nullable char serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable char serialization</returns>
        public static Expression WriteNullableChar(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteNullable(outputStream, obj, objTracker, typeof(char?), WriteChar);
        }

        /// <summary>
        /// Generates an expression to handle nullable char deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable char deserialization</returns>
        public static Expression ReadNullableChar(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadNullable(inputStream, objTracker, typeof(char?), ReadChar);
        }

        /// <summary>
        /// Generates an expression to handle char serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle char serialization</returns>
        public static Expression WriteChar(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteIntegerNumberPrimitive(outputStream, Expression.Convert(obj, typeof(short)), objTracker, sizeof(short), typeof(short));
        }

        /// <summary>
        /// Generates an expression to handle char deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle char deserialization</returns>
        public static Expression ReadChar(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return Expression.Convert(ReadIntegerNumberPrimitive(inputStream, objTracker, sizeof(short), typeof(short)), typeof(char));
        }

#endregion

#region DateTime

        /// <summary>
        /// Generates an expression to handle nullable DateTime serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable DateTime serialization</returns>
        public static Expression WriteNullableDateTime(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteNullable(outputStream, obj, objTracker, typeof(DateTime?), WriteDateTime);
        }

        /// <summary>
        /// Generates an expression to handle nullable DateTime deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable DateTime deserialization</returns>
        public static Expression ReadNullableDateTime(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadNullable(inputStream, objTracker, typeof(DateTime?), ReadDateTime);
        }

        /// <summary>
        /// Generates an expression to handle DateTime serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle DateTime serialization</returns>
        public static Expression WriteDateTime(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteIntegerNumberPrimitive(outputStream, Expression.Call(Expression.Convert(obj, typeof(DateTime)), DateTimeMih.ToBinary()), objTracker, sizeof(long), typeof(long));
        }

        /// <summary>
        /// Generates an expression to handle DateTime deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle DateTime deserialization</returns>
        public static Expression ReadDateTime(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return Expression.Call(DateTimeMih.FromBinary(),
                                   ReadIntegerNumberPrimitive(inputStream, objTracker, sizeof(long), typeof(long)));
        }

#endregion

#region TimeSpan

        /// <summary>
        /// Generates an expression to handle nullable TimeSpan serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable TimeSpan serialization</returns>
        public static Expression WriteNullableTimeSpan(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteNullable(outputStream, obj, objTracker, typeof(TimeSpan?), WriteTimeSpan);
        }

        /// <summary>
        /// Generates an expression to handle nullable TimeSpan deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable TimeSpan deserialization</returns>
        public static Expression ReadNullableTimeSpan(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadNullable(inputStream, objTracker, typeof(TimeSpan?), ReadTimeSpan);
        }

        /// <summary>
        /// Generates an expression to handle TimeSpan serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle TimeSpan serialization</returns>
        public static Expression WriteTimeSpan(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteIntegerNumberPrimitive(outputStream, Expression.Property(Expression.Convert(obj, typeof(TimeSpan)), typeof(TimeSpan).GetProperty("Ticks")), objTracker, sizeof(long), typeof(long));
        }

        /// <summary>
        /// Generates an expression to handle TimeSpan deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle TimeSpan deserialization</returns>
        public static Expression ReadTimeSpan(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return Expression.Call(TimeSpanMih.FromTicks(),
                                   ReadIntegerNumberPrimitive(inputStream, objTracker, sizeof(long), typeof(long)));
        }

#endregion

#region BigInteger

        /// <summary>
        /// Generates an expression to handle nullable BigInteger serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable BigInteger serialization</returns>
        public static Expression WriteNullableBigInteger(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteNullable(outputStream, obj, objTracker, typeof(BigInteger?), WriteBigInteger);
        }

        /// <summary>
        /// Generates an expression to handle nullable BigInteger deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle nullable BigInteger deserialization</returns>
        public static Expression ReadNullableBigInteger(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return ReadNullable(inputStream, objTracker, typeof(BigInteger?), ReadBigInteger);
        }

        /// <summary>
        /// Generates an expression to handle BigInteger serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle BigInteger serialization</returns>
        public static Expression WriteBigInteger(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            return WriteByteArray(outputStream, Expression.Call(Expression.Convert(obj, typeof(BigInteger)), BigIntegerMih.ToByteArray()), objTracker);
        }

        /// <summary>
        /// Generates an expression to handle BigInteger deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle BigInteger deserialization</returns>
        public static Expression ReadBigInteger(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            return Expression.New(BigIntegerMih.Constructor(),
                                  ReadByteArray(inputStream, objTracker));
        }

#endregion

#region nullable

        private static Expression WriteNullable(ParameterExpression outputStream, Expression obj, Expression objTracker, Type nullableType, Func<ParameterExpression, Expression, Expression, Expression> primitiveWriter)
        {
            var underlyingType = Nullable.GetUnderlyingType(nullableType);

            return Expression.IfThenElse(Expression.Equal(obj, Expression.Constant(null)),
                                         WriteByte(outputStream, Expression.Constant(SerializerObjectTracker.Value0), objTracker),
                                         Expression.Block(WriteByte(outputStream, Expression.Constant(SerializerObjectTracker.Value1), objTracker),
                                                          primitiveWriter(outputStream, Expression.Convert(obj, underlyingType), objTracker)));
        }

        private static Expression ReadNullable(ParameterExpression inputStream, ParameterExpression objTracker, Type nullableType, Func<ParameterExpression, ParameterExpression, Expression> primitiveReader)
        {
            var tmp = Expression.Parameter(nullableType, "tmp");
            return Expression.Block(new List<ParameterExpression> { tmp },
                                    Expression.IfThenElse(Expression.Equal(Expression.Convert(ReadByte(inputStream, objTracker), typeof(byte)), Expression.Constant(SerializerObjectTracker.Value0)),
                                         Expression.Assign(tmp, Expression.Constant(null, nullableType)),
                                         Expression.Assign(tmp, Expression.Convert(primitiveReader(inputStream, objTracker), nullableType))),
                                    tmp);
        }

#endregion

#region string

#if !INLINE_PRIMITIVE_METHOD

        /// <summary>
        /// Generates an expression to handle string deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle string deserialization</returns>
        public static Expression ReadString(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            var mi = typeof(PrimitiveHelpers).GetMethod(nameof(ReadStringImpl), BindingFlags.NonPublic | BindingFlags.Static);

            return Expression.Call(mi,
                                   inputStream,
                                   objTracker);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string ReadStringImpl(Stream inputStream, DeserializerObjectTracker objTracker)
        {
            if (inputStream.ReadByte() == SerializerObjectTracker.Value0)
            {
                return null;
            }

            int length = ReadSignedInt32(inputStream);
            int i = 0;
            int r = 0;

            objTracker.EnsureBufferSize(length);
            var encoding = new UTF8Encoding(false, true);

            while (i < length)
            {
                r = inputStream.Read(objTracker.Buffer, i, length - i);

                if (r == 0)
                {
                    throw new EndOfStreamException();
                }

                i += r;
            }

            return encoding.GetString(objTracker.Buffer, 0, length);
        }

        /// <summary>
        /// Generates an expression to handle string serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle string serialization</returns>
        public static Expression WriteString(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            var mi = typeof(PrimitiveHelpers).GetMethod(nameof(WriteStringImpl), BindingFlags.NonPublic | BindingFlags.Static);

            return Expression.Call(mi,
                                   outputStream,
                                   obj,
                                   objTracker);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WriteStringImpl(Stream outputStream,
                                         string obj,
                                         SerializerObjectTracker objTracker)
        {
            if (obj == null)
            {
                outputStream.WriteByte(SerializerObjectTracker.Value0);
                return;
            }

            outputStream.WriteByte(SerializerObjectTracker.Value1);

            var encoding = System.Text.Encoding.UTF8;
            int length = encoding.GetByteCount(obj);
            WriteSignedInt32(outputStream, length);

            objTracker.EnsureBufferSize(length);

            encoding.GetBytes(obj,
                              0,
                              obj.Length,
                              objTracker.Buffer,
                              0);

            outputStream.Write(objTracker.Buffer,
                               0,
                               length);
        }


#else
        /// <summary>
        /// Generates an expression to handle string deserialization
        /// </summary>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle string deserialization</returns>
        public static Expression ReadString(ParameterExpression inputStream, ParameterExpression objTracker)
        {
            var expressions = new List<Expression>();
            var variables = new List<ParameterExpression>();
            var encoding = Expression.Parameter(typeof(UTF8Encoding), "encoding");
            var nbBytes = Expression.Parameter(typeof(byte), "nbBytes");
            var length = Expression.Parameter(typeof(int), "length");
            var i = Expression.Parameter(typeof(int), "i");
            var r = Expression.Parameter(typeof(int), "r");
            var newInstance = Expression.Parameter(typeof(string), "newInstance");
            var buffer = Expression.Property(objTracker, nameof(SerializerObjectTracker.Buffer));

            variables.Add(encoding);
            variables.Add(nbBytes);
            variables.Add(length);
            variables.Add(i);
            variables.Add(r);
            variables.Add(newInstance);

            expressions.Add(Expression.Assign(i, Expression.Constant(0)));
            expressions.Add(Expression.Assign(r, Expression.Constant(0)));
            expressions.Add(Expression.Assign(length, Expression.Convert(ReadInt32(inputStream, objTracker), typeof(int))));
            expressions.Add(Expression.Call(objTracker, DeserializerObjectTrackerMih.EnsureBufferSize(), length));
            expressions.Add(Expression.Assign(encoding, Expression.New(typeof(UTF8Encoding).GetConstructor(new[] { typeof(bool), typeof(bool) }), Expression.Constant(false), Expression.Constant(true))));

            var loopBody = Expression.Block(Expression.Assign(r, Expression.Call(inputStream, StreamMih.Read(), buffer, i, Expression.Convert(Expression.Subtract(length, i), typeof(int)))),
                                            Expression.IfThenElse(Expression.Equal(r, Expression.Constant(0)),
                                                                    Expression.Throw(Expression.New(EndOfStreamExceptionMih.Constructor())),
                                                                    Expression.Assign(i, Expression.Add(i, r))));

            var breakLabel = Expression.Label("breakLabel");

            var loop = Expression.Loop(Expression.IfThenElse(Expression.LessThan(i, length),
                                                             loopBody,
                                                             Expression.Break(breakLabel)),
                                       breakLabel);
            expressions.Add(loop);
            expressions.Add(Expression.Assign(newInstance, Expression.Convert(Expression.Call(encoding, Utf8EncodingMih.GetStringResuableBuffer(), buffer, Expression.Constant(0), length), typeof(string))));

            return Expression.Block(variables,
                                    Expression.Block(Expression.IfThenElse(Expression.Equal(Expression.Convert(ReadByte(inputStream, objTracker), typeof(byte)), Expression.Constant(SerializerObjectTracker.Value0)),
                                                                           Expression.Assign(newInstance, Expression.Constant(null, typeof(string))),
                                                                           Expression.Block(expressions)),
                                                     newInstance));
        }

        /// <summary>
        /// Generates an expression to handle string serialization
        /// </summary>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="obj">Object to serialize</param>
        /// <param name="objTracker">Object tracker</param>
        /// <returns>An expression to handle string serialization</returns>
        public static Expression WriteString(ParameterExpression outputStream, Expression obj, Expression objTracker)
        {
            var expressions = new List<Expression>();
            var variables = new List<ParameterExpression>();
            var encoding = Expression.Parameter(typeof(Encoding), "encoding");
            var length = Expression.Parameter(typeof(int), "length");
            var writeLength = WriteInt32(outputStream, length, objTracker);
            var buffer = Expression.Property(objTracker, nameof(SerializerObjectTracker.Buffer));

            variables.Add(encoding);
            variables.Add(length);

            Expression stringNotNull = Expression.Block(variables,
                                                        Expression.Assign(encoding, Expression.Constant(System.Text.Encoding.UTF8)),
                                                        Expression.Assign(length, Expression.Call(encoding, EncodingMih.GetByteCount(), Expression.Convert(obj, typeof(string)))),
                                                        WriteByte(outputStream, Expression.Constant(SerializerObjectTracker.Value1), objTracker),
                                                        writeLength,
                                                        Expression.Call(objTracker, SerializerObjectTrackerMih.EnsureBufferSize(), length),
                                                        Expression.Call(encoding,
                                                                        EncodingMih.GetBytes(),
                                                                        Expression.Convert(obj, typeof(string)),
                                                                        Expression.Constant(0),
                                                                        Expression.Property(Expression.Convert(obj, typeof(string)), nameof(String.Length)),
                                                                        buffer,
                                                                        Expression.Constant(0)),
                                                        Expression.Call(outputStream, StreamMih.Write(), buffer, Expression.Constant(0), length));

            expressions.Add(Expression.IfThenElse(Expression.Equal(obj, Expression.Constant(null)),
                                                  WriteByte(outputStream, Expression.Constant(SerializerObjectTracker.Value0), objTracker),
                                                  stringNotNull));

            return Expression.Block(variables, expressions);
        }
#endif

#endregion
    }
}
