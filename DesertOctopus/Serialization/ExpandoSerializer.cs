using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DesertOctopus.Utilities;

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
            var getEnumeratorMethodInfo = IEnumerableMIH.GetEnumerator<string, object>();
            var enumeratorMethod = Expression.Call(Expression.Convert(objToSerialize, enumerableType), getEnumeratorMethodInfo);

            var loopBodyCargo = new EnumerableLoopBodyCargo<string, object>();
            loopBodyCargo.EnumeratorType = typeof(IEnumerator<KeyValuePair<string, object>>);
            loopBodyCargo.KvpType = typeof(KeyValuePair<string, object>);

            var preLoopActions = new List<Expression>();
            preLoopActions.Add(PrimitiveHelpers.WriteInt32(outputStream, Expression.Property(Expression.Convert(objToSerialize, typeof(ICollection<KeyValuePair<string, object>>)), ICollectionMIH.Count<KeyValuePair<string, object>>())));

            var notTrackedExpressions = new List<Expression>();
            notTrackedExpressions.Add(Expression.Call(objTracking, SerializerObjectTrackerMIH.TrackObject(), objToSerialize));
            notTrackedExpressions.Add(EnumerableLoopHelper.GenerateEnumeratorLoop<string, object, IEnumerator<KeyValuePair<string, object>>>(variables,
                                                                                                                                             EnumerableLoopHelper.GetStringToSomethingWriter(outputStream, objTracking),
                                                                                                                                             enumeratorMethod,
                                                                                                                                             preLoopActions,
                                                                                                                                             loopBodyCargo));

            return Serializer.GenerateNullTrackedOrUntrackedExpression(outputStream,
                                                                       objToSerialize,
                                                                       objTracking,
                                                                       notTrackedExpressions,
                                                                       variables);
        }
    }
}