using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using DesertOctopus.Utilities;
using DesertOctopus.Utilities.MethodInfoHelpers;

namespace DesertOctopus.Cloning
{
    /// <summary>
    /// Helper class for array expression trees
    /// </summary>
    internal static class ArrayCloner
    {
        /// <summary>
        /// Generate an expression tree for arrays
        /// </summary>
        /// <param name="variables">Global variables for the expression tree</param>
        /// <param name="source">Source object</param>
        /// <param name="clone">Clone object</param>
        /// <param name="sourceType">Type of the source object</param>
        /// <param name="refTrackerParam">Reference tracker</param>
        /// <returns>Expression tree to clone arrays</returns>
        public static Expression GenerateArrayExpression(List<ParameterExpression> variables,
                                                         ParameterExpression source,
                                                         ParameterExpression clone,
                                                         Type sourceType,
                                                         ParameterExpression refTrackerParam)
        {
            var elementType = sourceType.GetElementType();
            if (elementType.IsPrimitive || elementType.IsValueType || (elementType == typeof(string)))
            {
                return Expression.Block(Expression.Assign(clone, Expression.Convert(Expression.Call(Expression.Convert(source, typeof(Array)), ArrayMih.Clone()), sourceType)),
                                        Expression.Call(refTrackerParam, ObjectClonerReferenceTrackerMih.Track(), source, clone));
            }

            if (elementType.IsArray)
            {
                return GenerateJaggedArray(variables, source, clone, sourceType, refTrackerParam);
            }
            else
            {
                return GenerateArrayOfKnownDimension(variables, source, clone, sourceType, elementType, refTrackerParam);
            }
        }

        private static Expression GenerateArrayOfKnownDimension(List<ParameterExpression> variables,
                                                                ParameterExpression source,
                                                                ParameterExpression clone,
                                                                Type sourceType,
                                                                Type elementType,
                                                                ParameterExpression refTrackerParam)
        {
            var i = Expression.Parameter(typeof(int), "i");
            var lengths = Expression.Parameter(typeof(int[]), "lengths");
            var rank = sourceType.GetArrayRank();

            variables.Add(i);
            variables.Add(lengths);

            List<Expression> notTrackedExpressions = new List<Expression>();

            notTrackedExpressions.Add(Expression.IfThen(Expression.GreaterThanOrEqual(Expression.Constant(rank), Expression.Constant(255)),
                                                        Expression.Throw(Expression.New(NotSupportedExceptionMih.ConstructorString(), Expression.Constant("Array with more than 255 dimensions are not supported")))));
            notTrackedExpressions.Add(Expression.Assign(lengths, Expression.Call(CreateArrayMethodInfo.GetCreateArrayMethodInfo(typeof(int)), Expression.Constant(rank))));
            notTrackedExpressions.AddRange(PopulateDimensionalArrayLength(source, i, lengths, rank));
            notTrackedExpressions.Add(Expression.Assign(clone, Expression.Convert(Expression.Call(ArrayMih.CreateInstance(), Expression.Constant(elementType), lengths), sourceType)));
            notTrackedExpressions.Add(Expression.Call(refTrackerParam, ObjectClonerReferenceTrackerMih.Track(), source, clone));
            notTrackedExpressions.AddRange(GenerateCopyDimensionalArray(source, clone, sourceType, variables, lengths, rank, refTrackerParam));

            return ObjectCloner.GenerateNullTrackedOrUntrackedExpression(source,
                                                                         clone,
                                                                         sourceType,
                                                                         refTrackerParam,
                                                                         Expression.Block(notTrackedExpressions));
        }

        private static IEnumerable<Expression> GenerateCopyDimensionalArray(ParameterExpression sourceArray,
                                                                            ParameterExpression cloneArray,
                                                                            Type sourceType,
                                                                            List<ParameterExpression> variables,
                                                                            ParameterExpression lengths,
                                                                            int rank,
                                                                            ParameterExpression refTrackerParam)
        {
            var elementType = sourceType.GetElementType();

            var item = Expression.Parameter(elementType, "item");
            var clonedItem = Expression.Parameter(elementType, "clonedItem");
            var typeExpr = Expression.Parameter(typeof(Type), "typeExpr");
            var indices = Expression.Parameter(typeof(int[]), "indices");
            variables.Add(typeExpr);
            variables.Add(item);
            variables.Add(indices);
            variables.Add(clonedItem);

            var expressions = new List<Expression>();
            expressions.Add(Expression.Assign(indices, Expression.Call(CreateArrayMethodInfo.GetCreateArrayMethodInfo(typeof(int)), Expression.Constant(rank))));

            Debug.Assert(!(elementType.IsPrimitive || elementType.IsValueType || elementType == typeof(string)), "This method is not made to handle primitive types");

            Expression innerExpression = Expression.Block(ClassCloner.GetCloneClassTypeExpression(refTrackerParam, item, clonedItem, elementType),
                                                          Expression.Call(cloneArray, ArrayMih.SetValueRank(), Expression.Convert(clonedItem, typeof(object)), indices));

            Func<int, Expression, Expression> makeArrayLoop = (loopRank,
                                                               innerExpr) =>
            {
                var loopRankIndex = Expression.Parameter(typeof(int), "loopRankIndex" + loopRank);
                variables.Add(loopRankIndex);

                var loopExpressions = new List<Expression>();

                loopExpressions.Add(Expression.Assign(Expression.ArrayAccess(indices, Expression.Constant(loopRank)), loopRankIndex));
                loopExpressions.Add(Expression.Assign(item, Expression.Convert(Expression.Call(sourceArray, ArrayMih.GetValueRank(), indices), elementType)));
                loopExpressions.Add(innerExpr);
                loopExpressions.Add(Expression.Assign(loopRankIndex, Expression.Add(loopRankIndex, Expression.Constant(1))));

                var cond = Expression.LessThan(loopRankIndex, Expression.ArrayIndex(lengths, Expression.Constant(loopRank)));
                var loopBody = Expression.Block(loopExpressions);

                var breakLabel = Expression.Label("breakLabel" + loopRank);
                var loop = Expression.Loop(Expression.IfThenElse(cond,
                                                                 loopBody,
                                                                 Expression.Break(breakLabel)),
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

            var length = Expression.Call(sourceArray, ArrayMih.GetLength(), i);
            loopExpressions.Add(Expression.Assign(Expression.ArrayAccess(lengths, i), length));
            loopExpressions.Add(Expression.Assign(i, Expression.Add(i, Expression.Constant(1))));

            var loopBody = Expression.Block(loopExpressions);
            var breakLabel = Expression.Label("breakLabelLength");
            var cond = Expression.LessThan(i, Expression.Constant(rank));
            var loop = Expression.Loop(Expression.IfThenElse(cond,
                                                             loopBody,
                                                             Expression.Break(breakLabel)),
                                       breakLabel);
            expressions.Add(loop);

            return expressions;
        }

        private static Expression GenerateJaggedArray(List<ParameterExpression> variables,
                                                      ParameterExpression source,
                                                      ParameterExpression clone,
                                                      Type sourceType,
                                                      ParameterExpression refTrackerParam)
        {
            var elementType = sourceType.GetElementType();

            var item = Expression.Parameter(elementType, "item");
            var clonedItem = Expression.Parameter(elementType, "item");
            var typeExpr = Expression.Parameter(typeof(Type), "typeExpr");
            var i = Expression.Parameter(typeof(int), "i");
            var length = Expression.Parameter(typeof(int), "length");

            variables.Add(typeExpr);
            variables.Add(clonedItem);
            variables.Add(item);
            variables.Add(length);
            variables.Add(i);

            var notTrackedExpressions = new List<Expression>();
            notTrackedExpressions.Add(Expression.Assign(length, Expression.Property(source, "Length")));
            notTrackedExpressions.Add(Expression.Assign(i, Expression.Constant(0)));
            notTrackedExpressions.Add(Expression.Assign(clone, Expression.Convert(Expression.New(sourceType.GetConstructor(new[] { typeof(int) }), length), sourceType)));
            notTrackedExpressions.Add(Expression.Call(refTrackerParam, ObjectClonerReferenceTrackerMih.Track(), source, clone));

            Debug.Assert(!elementType.IsPrimitive && !elementType.IsValueType && elementType != typeof(string), "Element type cannot be a primitive type");

            var loopExpressions = new List<Expression>();
            loopExpressions.Add(Expression.Assign(item, Expression.Convert(Expression.Call(source, ArrayMih.GetValue(), i), elementType)));
            loopExpressions.Add(ClassCloner.GetCloneClassTypeExpression(refTrackerParam, item, clonedItem, elementType));
            loopExpressions.Add(Expression.Call(clone, ArrayMih.SetValue(), Expression.Convert(clonedItem, typeof(object)), i));
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
                                                                         sourceType,
                                                                         refTrackerParam,
                                                                         Expression.Block(notTrackedExpressions));
        }
    }
}