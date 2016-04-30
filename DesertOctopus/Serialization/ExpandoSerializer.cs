using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DesertOctopus.Serialization.Helpers;

namespace DesertOctopus.Serialization
{
    internal static class ExpandoSerializer
    {
        public static Expression GenerateExpandoObjectExpression(Type type,
                                                                 List<ParameterExpression> variables,
                                                                 ParameterExpression outputStream,
                                                                 ParameterExpression objToSerialize,
                                                                 ParameterExpression objTracking)
        {

            var enumerableType = typeof(IEnumerable<KeyValuePair<string, object>>);
            var getEnumeratorMethodInfo = enumerableType.GetMethod("GetEnumerator", new Type[0]);
            var enumeratorMethod = Expression.Call(Expression.Convert(objToSerialize, enumerableType), getEnumeratorMethodInfo);

            var loopBodyCargo = new EnumerableLoopBodyCargo<string, object>();
            loopBodyCargo.EnumeratorType = typeof(IEnumerator<KeyValuePair<string, object>>);
            loopBodyCargo.KvpType = typeof(KeyValuePair<string, object>);

            return EnumerableLoopHelper.GenerateEnumeratorLoop<string, object, IEnumerator<KeyValuePair<string, object>>>(type,
                                                                               variables,
                                                                               outputStream,
                                                                               objToSerialize,
                                                                               objTracking,
                                                                               EnumerableLoopHelper.GetStringToSomethingWriter(outputStream, objTracking),
                                                                               Expression.Property(Expression.Convert(objToSerialize, typeof(ICollection<KeyValuePair<string, object>>)), typeof(ICollection<KeyValuePair<string, object>>).GetProperty("Count")),
                                                                               enumeratorMethod,
                                                                               loopBodyCargo);
        }
    }
}