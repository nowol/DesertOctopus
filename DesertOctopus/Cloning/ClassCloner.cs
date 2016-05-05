using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using DesertOctopus.ObjectCloner;
using DesertOctopus.Utilities;
using DesertOctopus.Utilities.MethodInfoHelpers;

namespace DesertOctopus.Cloning
{
    internal static class ClassCloner
    {
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

            var expr = Expression.Block(copyExpressions);

            //return Expression.IfThenElse(Expression.Call(refTrackerParam, ObjectClonerReferenceTrackerMIH.IsSourceObjectTracked(), source),
            //                                            Expression.Assign(clone, Expression.Convert(Expression.Call(refTrackerParam, ObjectClonerReferenceTrackerMIH.GetEquivalentTargetObject(), source), sourceType)),
            //                                            expr);

            return ObjectCloner.GenerateNullTrackedOrUntrackedExpression(source,
                                                                         clone,
                                                                         sourceType,
                                                                         refTrackerParam,
                                                                         Expression.Block(copyExpressions));

        }

        internal static void GenerateCopyFieldsExpressions(IEnumerable<FieldInfo> fields,
                                                          ParameterExpression source,
                                                          ParameterExpression clone,
                                                          List<Expression> expressions,
                                                          ParameterExpression refTrackerParam)
        {
            foreach (var field in fields)
            {
                if (field.FieldType.IsPrimitive || field.FieldType.IsValueType || (field.FieldType == typeof(string)))
                {
                    if (field.IsInitOnly)
                    {
                        expressions.Add(Expression.Call(CopyReadOnlyFieldMethodInfo.GetMethodInfo(),
                                                        Expression.Constant(field),
                                                        Expression.Convert(Expression.Field(source, field), typeof(object)),
                                                        clone));
                    }
                    else
                    {
                        var from = Expression.Field(source, field);
                        var to = Expression.Field(clone, field);
                        expressions.Add(Expression.Assign(to, from));
                    }
                }
                else
                {
                    var callCopyExpression = CallCopyExpression(Expression.Field(source, field), refTrackerParam);

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

                        var conditionalExpression = Expression.IfThenElse(Expression.NotEqual(Expression.Field(source, field), Expression.Constant(null)),
                                                                          assignExpr(callCopyExpression),
                                                                          assignNullExpr);


                        expressions.Add(ObjectCloner.GenerateNullTrackedOrUntrackedExpression(Expression.Field(source, field),
                                                                        Expression.Field(clone, field),
                                                                        field.FieldType,
                                                                        refTrackerParam,
                                                                        conditionalExpression,
                                                                        trackedExpression: assignExpr)); // todo refactor this horrible method 
                    }
                    else
                    {
                        var assignExpr = Expression.Assign(Expression.Field(clone, field),
                                                  Expression.Convert(callCopyExpression, field.FieldType));
                        var conditionalExpression = Expression.IfThenElse(Expression.NotEqual(Expression.Field(source, field), Expression.Constant(null)),
                                                                          assignExpr,
                                                                          Expression.Assign(Expression.Field(clone, field), Expression.Constant(null, field.FieldType)));
                        expressions.Add(ObjectCloner.GenerateNullTrackedOrUntrackedExpression(Expression.Field(source, field),
                                                                         Expression.Field(clone, field),
                                                                         field.FieldType,
                                                                         refTrackerParam,
                                                                         conditionalExpression));
                    }
                }
            }
        }

        internal static Expression CallCopyExpression(Expression item, ParameterExpression refTrackerParam)
        {
            //return Expression.Block(
                                    
            //                        );



            var typeExpr = Expression.Call(item, ObjectMIH.GetTypeMethod());
            var generateTypeExpr = Expression.Call(ObjectClonerMIH.CloneImpl(), typeExpr);
            return Expression.Call(generateTypeExpr, FuncMIH.CloneMethodInvoke(), Expression.Convert(item, typeof(object)), refTrackerParam);
        }
        
        public static Expression GetCloneClassTypeExpression(ParameterExpression refTrackerParam,
                                                             ParameterExpression item,
                                                             ParameterExpression clonedItem,
                                                             Type cloneType,
                                                             Expression cloner)
        {

            //return Expression.IfThenElse(Expression.Equal(item, Expression.Constant(null)),
            //                             Expression.Assign(clonedItem, Expression.Constant(null, cloneType)),
            //                             Expression.Block(Expression.Assign(cloner, Expression.Call(ObjectClonerMIH.CloneImpl(), Expression.Call(item, ObjectMIH.GetTypeMethod()))),
            //                                              Expression.Assign(clonedItem, Expression.Convert(Expression.Invoke(cloner, item, refTrackerParam), cloneType))));

            return Expression.IfThenElse(Expression.Equal(item, Expression.Constant(null)),
                                         Expression.Assign(clonedItem, Expression.Constant(null, cloneType)),
                                         Expression.Assign(clonedItem, Expression.Convert(CallCopyExpression(item, refTrackerParam), cloneType)));

            //return Expression.Block(Expression.Assign(item, Expression.Call(SerializerMIH.PrepareObjectForSerialization(), item)),
            //                 Expression.Assign(cloner, Expression.Call(ObjectClonerMIH.CloneImpl(), Expression.Call(item, ObjectMIH.GetTypeMethod()))),
            //                 Expression.Assign(clonedItem, Expression.Convert(Expression.Invoke(cloner, item, refTrackerParam), cloneType)));
        }
    }
}