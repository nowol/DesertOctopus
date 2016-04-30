using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using DesertOctopus.Serialization.Helpers;

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

            var expressions = new List<Expression>();

            expressions.Add(Expression.Assign(length, PrimitiveHelpers.ReadInt32(inputStream)));
            expressions.Add(Expression.Assign(i, Expression.Constant(0)));
            expressions.Add(Expression.Assign(newInstance, Expression.New(typeof(ExpandoObject))));
            expressions.Add(Expression.Assign(destDict, Expression.Convert(newInstance, dictType)));


            var loopExpressions = new List<Expression>();
            loopExpressions.Add(Expression.Assign(key, PrimitiveHelpers.ReadString(inputStream)));
            loopExpressions.Add(Deserializer.GetReadClassExpression(inputStream, objTracking, value, typeExpr, typeName, typeHashCode, deserializer, typeof(object)));
            loopExpressions.Add(Expression.Call(destDict, dictType.GetMethod("Add", new []{ typeof(string), typeof(object)}), key, value));
            loopExpressions.Add(Expression.Assign(i, Expression.Add(i, Expression.Constant(1))));


            var cond = Expression.LessThan(i, length);
            var loopBody = Expression.Block(loopExpressions);

            var breakLabel = Expression.Label("breakLabel");
            var loop = Expression.Loop(Expression.IfThenElse(cond,
                                                             loopBody,
                                                             Expression.Break(breakLabel)
                                                            ),
                                        breakLabel);

            expressions.Add(loop);
            expressions.Add(newInstance);

            return Expression.Block(expressions);
        }
    }
}
