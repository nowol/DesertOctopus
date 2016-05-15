using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DesertOctopus.Utilities;

namespace DesertOctopus.Serialization
{
    /// <summary>
    /// Helper class to handle ExpandoObject serialization
    /// </summary>
    internal static class ExpandoSerializer
    {
        /// <summary>
        /// Generates an expression tree to handle ExpandoObject serialization
        /// </summary>
        /// <param name="variables">Global variables for the expression tree</param>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="objToSerialize">Object to serialize</param>
        /// <param name="objTracking">Reference tracker</param>
        /// <returns>An expression tree to handle ExpandoObject serialization</returns>
        public static Expression GenerateExpandoObjectExpression(List<ParameterExpression> variables,
                                                                 ParameterExpression outputStream,
                                                                 ParameterExpression objToSerialize,
                                                                 ParameterExpression objTracking)
        {
            var enumerableType = typeof(IEnumerable<KeyValuePair<string, object>>);
            var getEnumeratorMethodInfo = IEnumerableMIH.GetEnumerator<string, object>();
            var enumeratorMethod = Expression.Call(Expression.Convert(objToSerialize, enumerableType), getEnumeratorMethodInfo);

            var loopBodyCargo = new EnumerableLoopBodyCargo();
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