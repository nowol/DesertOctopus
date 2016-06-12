using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using DesertOctopus.Cloning;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class that defines some constants
    /// </summary>
    internal static class InternalSerializationStuff
    {
        /// <summary>
        /// Version of the serialization engine
        /// </summary>
        public const short Version = 1;

        /// <summary>
        /// Type of serialized object
        /// </summary>
        public enum SerializationType : byte
        {
            /// <summary>
            /// Value/Primitive type
            /// </summary>
            ValueType = 0,

            /// <summary>
            /// Reference type
            /// </summary>
            Class = 1
        }

        public static readonly ConcurrentDictionary<Type, FieldInfo[]> FieldsForType = new ConcurrentDictionary<Type, FieldInfo[]>();

        /// <summary>
        /// Gets the fields of a type that can be serialized
        /// </summary>
        /// <param name="type">Type to analyze</param>
        /// <returns>The fields of a type</returns>
        public static FieldInfo[] GetFields(Type type)
        {
            return FieldsForType.GetOrAdd(type,
                                          t =>
                                          {
                                              var fields = new List<FieldInfo>();
                                              var targetType = t;

                                              do
                                              {
                                                  fields.AddRange(targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                                                                                       | BindingFlags.DeclaredOnly)
                                                                            .Where(fi => (fi.Attributes & FieldAttributes.NotSerialized) == 0));
                                                  targetType = targetType.BaseType;
                                              }
                                              while (targetType != null);

                                              return fields.OrderBy(f => f.Name,
                                                                    StringComparer.Ordinal)
                                                           .ToArray();
                                          });
        }

        /// <summary>
        /// Throws an exception if the type is not supported for serialization or cloning
        /// </summary>
        /// <param name="type">Type to analyze</param>
        public static void ValidateSupportedTypes(Type type)
        {
            if (typeof(Expression).IsAssignableFrom(type))
            {
                throw new NotSupportedException(type.ToString());
            }

            if (typeof(Delegate).IsAssignableFrom(type))
            {
                throw new NotSupportedException(type.ToString());
            }

            if (type.IsPointer)
            {
                throw new NotSupportedException($"Pointer types such as {type} are not suported");
            }

            if (InternalSerializationStuff.GetFields(type).Any(x => x.FieldType.IsPointer))
            {
                throw new NotSupportedException($"Type {type} cannot contains fields that are pointers.");
            }

            if (type == typeof(IQueryable))
            {
                throw new NotSupportedException(type.ToString());
            }

            if (type == typeof(IEnumerable))
            {
                throw new NotSupportedException(type.ToString());
            }

            var enumerableType = IQueryableCloner.GetInterfaceType(type, typeof(IEnumerable<>));
            if (enumerableType != null)
            {
                var genericArgument = enumerableType.GetGenericArguments()[0];
                if (genericArgument.IsGenericType
                    && genericArgument.GetGenericTypeDefinition() == typeof(IGrouping<,>))
                {
                    throw new NotSupportedException(type.ToString());
                }
            }

            if (Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                     && type.IsGenericType && type.Name.Contains("AnonymousType")
                     && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase)
                        ||
                        type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase))
                    && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic)
            {
                throw new NotSupportedException(type.ToString());
            }

            if (!type.IsArray
                && type.Namespace != null
                && (type.Namespace.StartsWith("System.") || type.Namespace.StartsWith("Microsoft."))
                && type.GetCustomAttribute<SerializableAttribute>() == null
                && type != typeof(ExpandoObject)
                && type != typeof(BigInteger))
            {
                throw new NotSupportedException(type.ToString());
            }
        }

        internal static Expression TraceWriteLine(Expression value)
        {
            return Expression.Call(typeof(System.Diagnostics.Trace).GetMethod("WriteLine", BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Any, new[] { typeof(string) }, new ParameterModifier[0]), value);
        }
    }
}