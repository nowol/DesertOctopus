using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using DesertOctopus.Utilities;

namespace DesertOctopus.Serialization
{
    /// <summary>
    /// Helper class to handle jagged array serialization
    /// </summary>
    internal static class JaggedArraySerializer
    {
        /// <summary>
        /// Generates an expression tree to handle jagged array serialization
        /// </summary>
        /// <param name="type">Type of array</param>
        /// <param name="elementType">Type of the elements contained in the array</param>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="objToSerialize">Object to serialize</param>
        /// <param name="objTracking">Reference tracker</param>
        /// <returns>An expression tree to handle jagged array serialization</returns>
        internal static Expression GenerateJaggedArray(Type type,
                                                       Type elementType,
                                                       ParameterExpression outputStream,
                                                       ParameterExpression objToSerialize,
                                                       ParameterExpression objTracking)
        {
            List<Expression> expressions = new List<Expression>();
            List<Expression> notTrackedExpressions = new List<Expression>();
            List<ParameterExpression> variables = new List<ParameterExpression>();

            var trackedObjectPosition = Expression.Parameter(typeof(int?), "trackedObjectPosition");
            var arr = Expression.Parameter(type, "arr");

            variables.Add(trackedObjectPosition);
            variables.Add(arr);

            notTrackedExpressions.Add(Expression.Call(objTracking, SerializerObjectTrackerMih.TrackObject(), objToSerialize));
            notTrackedExpressions.Add(Expression.Assign(arr, Expression.Convert(objToSerialize, type)));
            notTrackedExpressions.AddRange(WriteJaggedArray(elementType, variables, outputStream, arr, objTracking));

            return Serializer.GenerateNullTrackedOrUntrackedExpression(outputStream,
                                                                        objToSerialize,
                                                                        objTracking,
                                                                        notTrackedExpressions,
                                                                        variables);
        }

        private static IEnumerable<Expression> WriteJaggedArray(Type elementType,
                                                                List<ParameterExpression> variables,
                                                                ParameterExpression outputStream,
                                                                ParameterExpression arr,
                                                                ParameterExpression objTracking)
        {
            var item = Expression.Parameter(elementType, "item");
            var serializer = Expression.Parameter(typeof(Action<Stream, object, SerializerObjectTracker>), "serializer");
            var itemAsObj = Expression.Parameter(typeof(object), "itemAsObj");
            var typeExpr = Expression.Parameter(typeof(Type), "typeExpr");
            var i = Expression.Parameter(typeof(int), "i");
            var length = Expression.Parameter(typeof(int), "length");
            variables.Add(typeExpr);
            variables.Add(itemAsObj);
            variables.Add(serializer);
            variables.Add(item);
            variables.Add(length);
            variables.Add(i);

            var expressions = new List<Expression>();
            expressions.Add(Expression.Assign(length, Expression.Property(arr, "Length")));
            expressions.Add(PrimitiveHelpers.WriteInt32(outputStream, length));
            expressions.Add(Expression.Assign(i, Expression.Constant(0)));

            Debug.Assert(!elementType.IsPrimitive && !elementType.IsValueType && elementType != typeof(string), "Type cannot be a primitive");

            var loopExpressions = new List<Expression>();
            loopExpressions.Add(Expression.Assign(item, Expression.ArrayAccess(arr, i)));   // uh?
            loopExpressions.Add(Expression.Assign(item, Expression.Convert(Expression.Call(arr, ArrayMih.GetValue(), i), elementType)));
            loopExpressions.Add(Serializer.GetWriteClassTypeExpression(outputStream, objTracking, item, itemAsObj, typeExpr, serializer, elementType));
            loopExpressions.Add(Expression.Assign(i, Expression.Add(i, Expression.Constant(1))));

            var cond = Expression.LessThan(i, length);
            var loopBody = Expression.Block(loopExpressions);

            var breakLabel = Expression.Label("breakLabel");
            var loop = Expression.Loop(Expression.IfThenElse(cond,
                                                             loopBody,
                                                             Expression.Break(breakLabel)),
                                       breakLabel);

            expressions.Add(loop);

            return expressions;
        }
    }
}
