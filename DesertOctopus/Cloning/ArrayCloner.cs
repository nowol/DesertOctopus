using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using DesertOctopus.ObjectCloner;
using DesertOctopus.Utilities;
using DesertOctopus.Utilities.MethodInfoHelpers;
using System.Diagnostics;

namespace DesertOctopus.Cloning
{
    internal static class ArrayCloner
    {
        public static Expression GenerateArrayExpression(List<ParameterExpression> variables,
                                                         ParameterExpression source,
                                                         ParameterExpression clone,
                                                         Type cloneType,
                                                         ParameterExpression refTrackerParam)
        {
            var elementType = cloneType.GetElementType();
            if (elementType.IsPrimitive || elementType.IsValueType || (elementType == typeof(string)))
            {
                return Expression.Block(Expression.Assign(clone, Expression.Convert(Expression.Call(Expression.Convert(source, typeof(Array)), ArrayMIH.Clone()), cloneType)),
                                        Expression.Call(refTrackerParam, ObjectClonerReferenceTrackerMIH.Track(), source, clone));
            }

            if (elementType.IsArray)
            {
                return GenerateJaggedArray(variables, source, clone, cloneType, refTrackerParam);
            }
            else
            {
                return GenerateArrayOfKnownDimension(variables, source, clone, cloneType, elementType, refTrackerParam);
            }
        }

        private static Expression GenerateArrayOfKnownDimension(List<ParameterExpression> variables,
                                                                ParameterExpression source,
                                                                ParameterExpression clone,
                                                                Type cloneType,
                                                                Type elementType,
                                                                ParameterExpression refTrackerParam)
        {
            var i = Expression.Parameter(typeof(int), "i");
            var lengths = Expression.Parameter(typeof(int[]), "lengths");
            //var sourceArray = Expression.Parameter(cloneType, "arr");
            var rank = cloneType.GetArrayRank();

            variables.Add(i);
            variables.Add(lengths);
            //variables.Add(sourceArray);

            List<Expression> notTrackedExpressions = new List<Expression>();

            notTrackedExpressions.Add(Expression.IfThen(Expression.GreaterThanOrEqual(Expression.Constant(rank), Expression.Constant(255)),
                                                        Expression.Throw(Expression.New(NotSupportedExceptionMIH.ConstructorString(), Expression.Constant("Array with more than 255 dimensions are not supported")))));
            notTrackedExpressions.Add(Expression.Assign(lengths, Expression.Call(CreateArrayMethodInfo.GetCreateArrayMethodInfo(typeof(int)), Expression.Constant(rank))));
            //notTrackedExpressions.Add(Expression.Assign(sourceArray, Expression.Convert(source, cloneType)));
            notTrackedExpressions.AddRange(PopulateDimensionalArrayLength(source, i, lengths, rank));
            notTrackedExpressions.Add(Expression.Assign(clone, Expression.Convert(Expression.Call(ArrayMIH.CreateInstance(), Expression.Constant(elementType), lengths), cloneType)));
            notTrackedExpressions.Add(Expression.Call(refTrackerParam, ObjectClonerReferenceTrackerMIH.Track(), source, clone));
            notTrackedExpressions.AddRange(GenerateCopyDimensionalArray(source, clone, cloneType, variables, i, lengths, rank, refTrackerParam));

            return ObjectCloner.GenerateNullTrackedOrUntrackedExpression(source,
                                                                         clone,
                                                                         cloneType,
                                                                         refTrackerParam,
                                                                         Expression.Block(notTrackedExpressions));







            //notTrackedExpressions.Add(Expression.Call(objTracking, SerializerObjectTrackerMIH.TrackObject(), objToSerialize));
            //notTrackedExpressions.Add(Expression.Assign(arr, Expression.Convert(objToSerialize, type)));
            //notTrackedExpressions.Add(Expression.Assign(lengths, Expression.Call(CreateArrayMethodInfo.GetCreateArrayMethodInfo(typeof(int)), Expression.Constant(rank))));
            //notTrackedExpressions.Add(Expression.Assign(i, Expression.Constant(0)));
            //notTrackedExpressions.AddRange(WriteDimensionalArrayLength(outputStream, i, arr, lengths, rank));
            //notTrackedExpressions.AddRange(WriteDimensionalArray(elementType, variables, outputStream, arr, rank, lengths, objTracking));

            //return Serializer.GenerateNullTrackedOrUntrackedExpression(outputStream,
            //                                                           objToSerialize,
            //                                                           objTracking,
            //                                                           notTrackedExpressions,
            //                                                           expressions,
            //                                                           variables);

            /*
             if (IsTracked)
                clone = tracked version
            else
            {
                if (rank >= 255)
                    throw exception
                clone = new ()
                track(source, clone)
                copy(source, clone)
            }
             */



            //expressions.Add(Expression.IfThenElse(Expression.Call(refTrackerParam, typeof(ObjectClonerReferenceTracker).GetMethod("IsSourceObjectTracked"), source),
            //                                    Expression.Assign(clone, Expression.Convert(Expression.Call(refTrackerParam, typeof(ObjectClonerReferenceTracker).GetMethod("GetEquivalentTargetObject"), source), sourceType)),

            //                                    expr)
            //                );






            //var arrayLengthExpr = Expression.Call(original, CreateArrayMethodInfo.GetArrayLengthMethod(), Expression.Constant(0));
            //var assignExpr = Expression.Assign(clone, Expression.Convert(Expression.Call(CreateArrayMethodInfo.GetCreateArrayMethodInfo(elementType), arrayLengthExpr), clonedType));
            //expressions.Add(assignExpr);

            //var index = Expression.Parameter(typeof(int));
            //var item = Expression.Parameter(elementType);
            //var breakLabel = Expression.Label("breakLabel");

            //var loopBody = Expression.Block(new[] { item },
            //                                Expression.Assign(item, Expression.ArrayAccess(original, index)),
            //                                Expression.IfThen(Expression.NotEqual(item, Expression.Constant(null)),
            //                                                    Expression.Assign(Expression.ArrayAccess(clone, index),
            //                                                                      Expression.Convert(CallCopyExpression(item, refTrackerParam), elementType))));

            //var loop = Expression.Block(new[] { index },
            //                            Expression.Assign(index, Expression.Constant(0)),
            //                            Expression.Loop(Expression.IfThenElse(Expression.LessThan(index, arrayLengthExpr),
            //                                                                  Expression.Block(loopBody,
            //                                                                                   Expression.AddAssign(index, Expression.Constant(1))),
            //                                                                  Expression.Break(breakLabel)),
            //                                            breakLabel));
            //expressions.Add(loop);

        }

        private static IEnumerable<Expression> GenerateCopyDimensionalArray(ParameterExpression sourceArray,
                                                                            ParameterExpression cloneArray,
                                                                            Type cloneType,
                                                                            List<ParameterExpression> variables,
                                                                            ParameterExpression parameterExpression,
                                                                            ParameterExpression lengths,
                                                                            int rank,
                                                                            ParameterExpression refTrackerParam)
        {
            var elementType = cloneType.GetElementType();

            var item = Expression.Parameter(elementType, "item");
            var clonedItem = Expression.Parameter(elementType, "clonedItem");
            var cloner = Expression.Parameter(typeof(Func<object, ObjectClonerReferenceTracker, object>), "cloner");
            //var itemAsObj = Expression.Parameter(typeof(object), "itemAsObj");
            var typeExpr = Expression.Parameter(typeof(Type), "typeExpr");
            var indices = Expression.Parameter(typeof(int[]), "indices");
            variables.Add(typeExpr);
            //variables.Add(itemAsObj);
            variables.Add(cloner);
            variables.Add(item);
            variables.Add(indices);
            variables.Add(clonedItem);

            var expressions = new List<Expression>();
            expressions.Add(Expression.Assign(indices, Expression.Call(CreateArrayMethodInfo.GetCreateArrayMethodInfo(typeof(int)), Expression.Constant(rank))));

            Expression innerExpression;
            if (elementType.IsPrimitive || elementType.IsValueType || elementType == typeof(string))
            {
                var primitiveCloner = ObjectCloner.CloneImpl(elementType);
                var c = Expression.Invoke(Expression.Constant(primitiveCloner), Expression.Convert(item, typeof(object)), refTrackerParam);
                innerExpression = Expression.Call(cloneArray, ArrayMIH.SetValueRank(), Expression.Convert(c, typeof(object)), indices);
            }
            else
            {
                innerExpression = Expression.Block(ClassCloner.GetCloneClassTypeExpression(refTrackerParam, item, clonedItem, elementType, cloner),
                                                   Expression.Call(cloneArray, ArrayMIH.SetValueRank(), Expression.Convert(clonedItem, typeof(object)), indices));
            }

            Func<int, Expression, Expression> makeArrayLoop = (loopRank,
                                                               innerExpr) =>
            {
                var loopRankIndex = Expression.Parameter(typeof(int), "loopRankIndex" + loopRank);
                variables.Add(loopRankIndex);

                var loopExpressions = new List<Expression>();

                loopExpressions.Add(Expression.Assign(Expression.ArrayAccess(indices, Expression.Constant(loopRank)), loopRankIndex));
                loopExpressions.Add(Expression.Assign(item, Expression.Convert(Expression.Call(sourceArray, ArrayMIH.GetValueRank(), indices), elementType)));
                loopExpressions.Add(innerExpr);
                loopExpressions.Add(Expression.Assign(loopRankIndex, Expression.Add(loopRankIndex, Expression.Constant(1))));

                var cond = Expression.LessThan(loopRankIndex, Expression.ArrayIndex(lengths, Expression.Constant(loopRank)));
                var loopBody = Expression.Block(loopExpressions);

                var breakLabel = Expression.Label("breakLabel" + loopRank);
                var loop = Expression.Loop(Expression.IfThenElse(cond,
                                                                 loopBody,
                                                                 Expression.Break(breakLabel)
                                                                ),
                                            breakLabel);
                return Expression.Block(Expression.Assign(loopRankIndex, Expression.Constant(0)),
                                        loop);
            };

            for (int r = rank - 1; r >= 0; r--)
            {
                innerExpression = makeArrayLoop(r, innerExpression);
            }

            expressions.Add(innerExpression);

            return expressions;
        }

        private static IEnumerable<Expression> PopulateDimensionalArrayLength(ParameterExpression sourceArray,
                                                                              ParameterExpression i,
                                                                              ParameterExpression lengths,
                                                                              int rank)
        {
            var loopExpressions = new List<Expression>();
            var expressions = new List<Expression>();
            expressions.Add(Expression.Assign(i, Expression.Constant(0)));

            var length = Expression.Call(sourceArray, ArrayMIH.GetLength(), i);
            loopExpressions.Add(Expression.Assign(Expression.ArrayAccess(lengths, i), length));
            loopExpressions.Add(Expression.Assign(i, Expression.Add(i, Expression.Constant(1))));

            var loopBody = Expression.Block(loopExpressions);
            var breakLabel = Expression.Label("breakLabelLength");
            var cond = Expression.LessThan(i, Expression.Constant(rank));
            var loop = Expression.Loop(Expression.IfThenElse(cond,
                                                             loopBody,
                                                             Expression.Break(breakLabel)
                                                            ),
                                       breakLabel);
            expressions.Add(loop);

            return expressions;
        }

        private static Expression GenerateJaggedArray(List<ParameterExpression> variables,
                                                      ParameterExpression source,
                                                      ParameterExpression clone,
                                                      Type cloneType,
                                                      ParameterExpression refTrackerParam)
        {
            var elementType = cloneType.GetElementType();

            //var sourceArray = Expression.Parameter(cloneType, "sourceArray");
            var item = Expression.Parameter(elementType, "item");
            var clonedItem = Expression.Parameter(elementType, "item");
            var cloner = Expression.Parameter(typeof(Func<object, ObjectClonerReferenceTracker, object>), "cloner");
            //var itemAsObj = Expression.Parameter(typeof(object), "itemAsObj");
            var typeExpr = Expression.Parameter(typeof(Type), "typeExpr");
            var i = Expression.Parameter(typeof(int), "i");
            var length = Expression.Parameter(typeof(int), "length");

            variables.Add(typeExpr);
            variables.Add(clonedItem);
            variables.Add(cloner);
            variables.Add(item);
            variables.Add(length);
            variables.Add(i);
            //variables.Add(source);

            var notTrackedExpressions = new List<Expression>();
            notTrackedExpressions.Add(Expression.Assign(length, Expression.Property(source, "Length")));
            notTrackedExpressions.Add(Expression.Assign(i, Expression.Constant(0)));
            notTrackedExpressions.Add(Expression.Assign(clone, Expression.Convert(Expression.New(cloneType.GetConstructor(new[] { typeof(int) }), length), cloneType)));
            notTrackedExpressions.Add(Expression.Call(refTrackerParam, ObjectClonerReferenceTrackerMIH.Track(), source, clone));

            Debug.Assert(!elementType.IsPrimitive && !elementType.IsValueType && elementType != typeof(string));

            var loopExpressions = new List<Expression>();
            loopExpressions.Add(Expression.Assign(item, Expression.Convert(Expression.Call(source, ArrayMIH.GetValue(), i), elementType)));
            loopExpressions.Add(ClassCloner.GetCloneClassTypeExpression(refTrackerParam, item, clonedItem, elementType, cloner));
            loopExpressions.Add(Expression.Call(clone, ArrayMIH.SetValue(), Expression.Convert(clonedItem, typeof(object)), i));
            loopExpressions.Add(Expression.Assign(i, Expression.Add(i, Expression.Constant(1))));


            var cond = Expression.LessThan(i, length);
            var loopBody = Expression.Block(loopExpressions);

            var breakLabel = Expression.Label("breakLabel");
            var loop = Expression.Loop(Expression.IfThenElse(cond,
                                                             loopBody,
                                                             Expression.Break(breakLabel)),
                                       breakLabel);

            notTrackedExpressions.Add(loop);


            return ObjectCloner.GenerateNullTrackedOrUntrackedExpression(source,
                                                                         clone,
                                                                         cloneType,
                                                                         refTrackerParam,
                                                                         Expression.Block(notTrackedExpressions));
        }
    }
}