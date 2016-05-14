using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Utilities
{
    internal class ObjectCleaner
    {
        internal static object PrepareObjectForSerialization(object objToPrepare)
        {
            var enumerableValue = objToPrepare as IEnumerable;
            if (enumerableValue != null)
            {
                var objectType = objToPrepare.GetType();
                if (objectType.IsArray
                    || typeof(IList).IsAssignableFrom(objectType)
                    || typeof(ICollection).IsAssignableFrom(objectType))
                {
                    return objToPrepare;
                }

                if (IsEnumeratingType(enumerableValue))
                {
                    Type itemType = typeof(object);

                    var enumerableInterface = objectType.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                    if (enumerableInterface != null)
                    {
                        itemType = enumerableInterface.GetGenericArguments()[0];
                    }

                    var converter = SerializerMIH.ConvertEnumerableToArray(itemType);
                    return converter.Invoke(null,
                                            new object[]
                                            {
                                                enumerableValue
                                            });
                }
            }


            return objToPrepare;
        }

        internal static bool IsEnumeratingType(IEnumerable enumerableValue)
        {
            if (enumerableValue == null)
            {
                return false;
            }
            var type = enumerableValue.GetType();
            return IsEnumeratingType(type);
        }

        internal static bool IsEnumeratingType(Type type)
        {
            return type.DeclaringType == typeof(System.Linq.Enumerable)
                   || (!String.IsNullOrWhiteSpace(type.Namespace) && type.Namespace.StartsWith("System.Linq"));
        }
    }
}
