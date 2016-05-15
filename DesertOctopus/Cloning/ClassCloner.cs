using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DesertOctopus.Utilities;
using DesertOctopus.Utilities.MethodInfoHelpers;

namespace DesertOctopus.Cloning
{
    /// <summary>
    /// Helper class to clone classes
    /// </summary>
    internal static class ClassCloner
    {
        /// <summary>
        /// Generate an expression tree to clone a class
        /// </summary>
        /// <param name="variables">Global variables for the expression tree</param>
        /// <param name="sourceType">Type of the source object</param>
        /// <param name="source">Source object</param>
        /// <param name="clone">Clone object</param>
        /// <param name="refTrackerParam">Reference tracker</param>
        /// <returns>An expression tree that can clone a class</returns>
        internal static Expression GenerateClassExpressions(List<ParameterExpression> variables,
                                                            Type sourceType,
                                                            ParameterExpression source,
                                                            ParameterExpression clone,
                                                            ParameterExpression refTrackerParam)
        {
            var ctor = Expression.Call(FormatterServicesMIH.GetUninitializedObject(), Expression.Constant(sourceType));

            var copyExpressions = new List<Expression>();
            var fields = InternalSerializationStuff.GetFields(sourceType);
            copyExpressions.Add(Expression.Assign(clone, Expression.Convert(ctor, sourceType)));
            copyExpressions.Add(Expression.Call(refTrackerParam, ObjectClonerReferenceTrackerMIH.Track(), source, clone));
            GenerateCopyFieldsExpressions(fields, source, clone, copyExpressions, refTrackerParam);

            return ObjectCloner.GenerateNullTrackedOrUntrackedExpression(source,
                                                                         clone,
                                                                         sourceType,
                                                                         refTrackerParam,
                                                                         Expression.Block(copyExpressions));
        }

        private static void GenerateCopyFieldsExpressions(IEnumerable<FieldInfo> fields,
                                                          ParameterExpression source,
                                                          ParameterExpression clone,
                                                          List<Expression> expressions,
                                                          ParameterExpression refTrackerParam)
        {
            foreach (var field in fields)
            {
                var cloneField = Expression.Field(clone, field);
                var sourceField = Expression.Field(source, field);
                if (field.FieldType.IsPrimitive || field.FieldType.IsValueType || (field.FieldType == typeof(string)))
                {
                    if (field.IsInitOnly)
                    {
                        expressions.Add(Expression.Call(CopyReadOnlyFieldMethodInfo.GetMethodInfo(),
                                                        Expression.Constant(field),
                                                        Expression.Convert(sourceField, typeof(object)),
                                                        clone));
                    }
                    else
                    {
                        var from = sourceField;
                        var to = cloneField;
                        expressions.Add(Expression.Assign(to, from));
                    }
                }
                else
                {
                    if (field.IsInitOnly)
                    {
                        Func<Expression, Expression> assignExpr = exx => Expression.Call(CopyReadOnlyFieldMethodInfo.GetMethodInfo(),
                                                                         Expression.Constant(field),
                                                                         Expression.Convert(exx, typeof(object)),
                                                                        clone);

                        var assignNullExpr = Expression.Call(CopyReadOnlyFieldMethodInfo.GetMethodInfo(),
                                                             Expression.Constant(field),
                                                             Expression.Constant(null, typeof(object)),
                                                             clone);

                        var conditionalExpression = Expression.IfThenElse(Expression.NotEqual(sourceField, Expression.Constant(null)),
                                                                          assignExpr(CallCopyExpression(sourceField, refTrackerParam)),
                                                                          assignNullExpr);

                        expressions.Add(ObjectCloner.GenerateNullTrackedOrUntrackedExpression(sourceField,
                                                                        cloneField,
                                                                        field.FieldType,
                                                                        refTrackerParam,
                                                                        conditionalExpression,
                                                                        trackedExpression: assignExpr)); // todo refactor this horrible method
                    }
                    else
                    {
                        Expression assignExpr;

                        if (IQueryableCloner.IsGenericIQueryableType(field.FieldType))
                        {
                            Type queryableInterface = IQueryableCloner.GetInterfaceType(field.FieldType, typeof(IQueryable<>));
                            var genericArgumentType = queryableInterface.GetGenericArguments()[0];

                            var copy = CallCopyExpression(Expression.Call(SerializerMIH.PrepareObjectForSerialization(), sourceField), refTrackerParam);
                            var copy2 = Expression.Convert(copy, field.FieldType);

                            var m = Expression.Call(typeof(Queryable), "AsQueryable", new Type[] { genericArgumentType }, Expression.Convert(copy2, typeof(IEnumerable<>).MakeGenericType(genericArgumentType)));
                            assignExpr = Expression.Assign(cloneField, m);
                        }
                        else if (IEnumerableCloner.IsGenericIEnumerableType(field.FieldType))
                        {
                            var copy = CallCopyExpression(Expression.Call(SerializerMIH.PrepareObjectForSerialization(), sourceField), refTrackerParam);
                            assignExpr = Expression.Assign(cloneField, Expression.Convert(copy, field.FieldType));
                        }
                        else
                        {
                            assignExpr = Expression.Assign(cloneField, Expression.Convert(CallCopyExpression(sourceField, refTrackerParam), field.FieldType));
                        }

                        var conditionalExpression = Expression.IfThenElse(Expression.Equal(sourceField, Expression.Constant(null)),
                                                                          Expression.Assign(cloneField, Expression.Constant(null, field.FieldType)),
                                                                          assignExpr);
                        expressions.Add(ObjectCloner.GenerateNullTrackedOrUntrackedExpression(sourceField,
                                                                         cloneField,
                                                                         field.FieldType,
                                                                         refTrackerParam,
                                                                         conditionalExpression));
                    }
                }
            }
        }

        /// <summary>
        /// Generate an expression that call the clone implementation method
        /// </summary>
        /// <param name="source">Source object</param>
        /// <param name="refTrackerParam">Reference tracker</param>
        /// <returns>An expression that call the clone implementation method</returns>
        internal static Expression CallCopyExpression(Expression source, ParameterExpression refTrackerParam)
        {
            var typeExpr = Expression.Call(source, ObjectMIH.GetTypeMethod());
            var generateTypeExpr = Expression.Call(ObjectClonerMIH.CloneImpl(), typeExpr);
            return Expression.Call(generateTypeExpr, FuncMIH.CloneMethodInvoke(), Expression.Convert(source, typeof(object)), refTrackerParam);
        }

        /// <summary>
        /// Generate an expression tree that handle classes
        /// </summary>
        /// <param name="refTrackerParam">Reference tracker</param>
        /// <param name="source">Source object</param>
        /// <param name="clone">Clone object</param>
        /// <param name="sourceType">Type of the source object</param>
        /// <returns>An expression tree that handle classes</returns>
        public static Expression GetCloneClassTypeExpression(ParameterExpression refTrackerParam,
                                                             Expression source,
                                                             ParameterExpression clone,
                                                             Type sourceType)
        {
            return Expression.IfThenElse(Expression.Equal(source, Expression.Constant(null)),
                                         Expression.Assign(clone, Expression.Constant(null, sourceType)),
                                         Expression.Assign(clone, Expression.Convert(CallCopyExpression(source, refTrackerParam), sourceType)));
        }
    }
}