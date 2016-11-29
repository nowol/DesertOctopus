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
        /// <param name="type">Type of <paramref name="objToSerialize"/></param>
        /// <param name="variables">Global variables for the expression tree</param>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="objToSerialize">Object to serialize</param>
        /// <param name="objTracking">Reference tracker</param>
        /// <returns>An expression tree to handle ExpandoObject serialization</returns>
        public static Expression GenerateExpandoObjectExpression(Type type,
                                                                 List<ParameterExpression> variables,
                                                                 ParameterExpression outputStream,
                                                                 ParameterExpression objToSerialize,
                                                                 ParameterExpression objTracking)
        {
            var enumerableType = typeof(IEnumerable<KeyValuePair<string, object>>);
            var getEnumeratorMethodInfo = IEnumerableMih.GetEnumerator<string, object>();
            var enumeratorMethod = Expression.Call(Expression.Convert(objToSerialize, enumerableType), getEnumeratorMethodInfo);

            var loopBodyCargo = new EnumerableLoopBodyCargo();
            loopBodyCargo.EnumeratorType = typeof(IEnumerator<KeyValuePair<string, object>>);
            loopBodyCargo.KvpType = typeof(KeyValuePair<string, object>);

            var preLoopActions = new List<Expression>();
            preLoopActions.Add(PrimitiveHelpers.WriteInt32(outputStream, Expression.Property(Expression.Convert(objToSerialize, typeof(ICollection<KeyValuePair<string, object>>)), CollectionMih.Count<KeyValuePair<string, object>>()), objTracking));

            var notTrackedExpressions = new List<Expression>();
            notTrackedExpressions.Add(Expression.Call(objTracking, SerializerObjectTrackerMih.TrackObject(), objToSerialize));
            notTrackedExpressions.Add(EnumerableLoopHelper.GenerateEnumeratorLoop(variables,
                                                                                  EnumerableLoopHelper.GetStringToSomethingWriter(outputStream, objTracking),
                                                                                  enumeratorMethod,
                                                                                  preLoopActions,
                                                                                  loopBodyCargo));

            return Serializer.GenerateNullTrackedOrUntrackedExpression(type,
                                                                       outputStream,
                                                                       objToSerialize,
                                                                       objTracking,
                                                                       notTrackedExpressions,
                                                                       variables);
        }
    }
}