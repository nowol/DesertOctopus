using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using DesertOctopus.Utilities;

namespace DesertOctopus.Serialization
{
    internal static class ExpandoDeserializer
    {
        internal static Expression GenerateExpandoObjectExpression(Type type,
                                                                   List<ParameterExpression> variables,
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
            var deserializer = Expression.Parameter(typeof(Func<Stream, List<object>, object>), "deserializer");
            var newInstance = Expression.Parameter(type, "newInstance");
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
            loopExpressions.Add(Expression.Call(destDict, DictionaryMIH.Add<string, object>(), key, value));
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
            notTrackedExpressions.Add(Expression.Call(objTracking, ListMIH.ObjectListAdd(), newInstance));

            notTrackedExpressions.Add(loop);
            //notTrackedExpressions.Add(newInstance);

            return Deserializer.GenerateNullTrackedOrUntrackedExpression(type,
                                                                         inputStream,
                                                                         objTracking,
                                                                         newInstance,
                                                                         notTrackedExpressions,
                                                                         trackType,
                                                                         variables);
        }
    }
}
