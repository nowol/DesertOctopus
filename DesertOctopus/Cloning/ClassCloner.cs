using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                        //var assignExpr = Expression.IfThenElse(Expression.IsTrue(Expression.Call(IQueryableMIH.IsGenericIQueryableType(), Expression.Constant(field.FieldType))),
                        //                                       Expression.Assign(cloneField, callCopyExpr ),
                        //                                       Expression.Assign(cloneField, callCopyExpr));

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
                            Type enumerableInterface = IQueryableCloner.GetInterfaceType(field.FieldType, typeof(IEnumerable<>));
                            var genericArgumentType = enumerableInterface.GetGenericArguments()[0];


                            var copy = CallCopyExpression(Expression.Call(SerializerMIH.PrepareObjectForSerialization(), sourceField), refTrackerParam);

                            //var m = Expression.Call(typeof(Queryable), "AsQueryable", new Type[] { genericArgumentType }, Expression.Convert(copy, typeof(IEnumerable<>).MakeGenericType(genericArgumentType)));
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
        
        internal static Expression CallCopyExpression(Expression item, ParameterExpression refTrackerParam)
        {
            /*
             var t = item.GetType();
             if (ObjectCloner.IsEnumeratingType(t))
             {
                 var arr = item.ToArray();
                 var arrClone = CloneImpl(arr);

             }
             
             */



            var typeExpr = Expression.Call(item, ObjectMIH.GetTypeMethod());
            var generateTypeExpr = Expression.Call(ObjectClonerMIH.CloneImpl(), typeExpr);
            return Expression.Call(generateTypeExpr, FuncMIH.CloneMethodInvoke(), Expression.Convert(item, typeof(object)), refTrackerParam);
        }
        
        public static Expression GetCloneClassTypeExpression(ParameterExpression refTrackerParam,
                                                             Expression item,
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