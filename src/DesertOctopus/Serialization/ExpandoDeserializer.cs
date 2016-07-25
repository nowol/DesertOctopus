using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using DesertOctopus.Utilities;

namespace DesertOctopus.Serialization
{
    /// <summary>
    /// Helper class to handle ExpandoObject deserialization
    /// </summary>
    internal static class ExpandoDeserializer
    {
        /// <summary>
        /// Generates an expression tree to deserialize an ExpandoObject
        /// </summary>
        /// <param name="variables">Global variables for the expression tree</param>
        /// <param name="inputStream">Stream to read from</param>
        /// <param name="objTracking">Reference tracker</param>
        /// <returns>An expression tree to deserialize an ExpandoObject</returns>
        internal static Expression GenerateExpandoObjectExpression(List<ParameterExpression> variables,
                                                                   ParameterExpression inputStream,
                                                                   ParameterExpression objTracking)
        {
            var dictType = typeof(IDictionary<string, object>);
            var length = Expression.Parameter(typeof(int), "length");
            var i = Expression.Parameter(typeof(int), "i");
            var key = Expression.Parameter(typeof(string), "key");
            var value = Expression.Parameter(typeof(object), "value");
            var typeName = Expression.Parameter(typeof(string), "typeName");
            var typeExpr = Expression.Parameter(typeof(TypeWithHashCode), "type");
            var deserializer = Expression.Parameter(typeof(Func<Stream, DeserializerObjectTracker, object>), "deserializer");
            var newInstance = Expression.Parameter(typeof(ExpandoObject), "newInstance");
            var destDict = Expression.Parameter(dictType, "destDict");
            var typeHashCode = Expression.Parameter(typeof(int), "typeHashCode");
            var trackType = Expression.Parameter(typeof(byte), "trackType");

            variables.Add(key);
            variables.Add(value);
            variables.Add(length);
            variables.Add(i);
            variables.Add(deserializer);
            variables.Add(typeName);
            variables.Add(typeExpr);
            variables.Add(newInstance);
            variables.Add(destDict);
            variables.Add(typeHashCode);
            variables.Add(trackType);

            var loopExpressions = new List<Expression>();
            loopExpressions.Add(Expression.Assign(key, Deserializer.GenerateStringExpression(inputStream, objTracking)));
            loopExpressions.Add(Deserializer.GetReadClassExpression(inputStream, objTracking, value, typeExpr, typeName, typeHashCode, deserializer, typeof(object)));
            loopExpressions.Add(Expression.Call(destDict, DictionaryMih.Add<string, object>(), key, value));
            loopExpressions.Add(Expression.Assign(i, Expression.Add(i, Expression.Constant(1))));

            var cond = Expression.LessThan(i, length);
            var loopBody = Expression.Block(loopExpressions);
            var breakLabel = Expression.Label("breakLabel");
            var loop = Expression.Loop(Expression.IfThenElse(cond,
                                                             loopBody,
                                                             Expression.Break(breakLabel)),
                                       breakLabel);

            var notTrackedExpressions = new List<Expression>();
            notTrackedExpressions.Add(Expression.Assign(length, PrimitiveHelpers.ReadInt32(inputStream)));
            notTrackedExpressions.Add(Expression.Assign(i, Expression.Constant(0)));
            notTrackedExpressions.Add(Expression.Assign(newInstance, Expression.New(typeof(ExpandoObject))));
            notTrackedExpressions.Add(Expression.Assign(destDict, Expression.Convert(newInstance, dictType)));
            notTrackedExpressions.Add(Expression.Call(objTracking, DeserializerObjectTrackerMih.TrackedObject(), newInstance));

            notTrackedExpressions.Add(loop);

            return Deserializer.GenerateNullTrackedOrUntrackedExpression(typeof(ExpandoObject),
                                                                         inputStream,
                                                                         objTracking,
                                                                         newInstance,
                                                                         notTrackedExpressions,
                                                                         trackType,
                                                                         variables);
        }
    }
}
