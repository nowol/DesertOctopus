using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Polyfills
{
    internal static class ReflectionHelper
    {
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static ConstructorInfo GetConstructor(this Type type,
                                                     Type[] types,
                                                     bool? isFamilyOrAssembly = null)
        {
            foreach (var ctor in type.GetTypeInfo()
                                     .DeclaredConstructors)
            {
                if (isFamilyOrAssembly == true
                    && !ctor.IsFamilyOrAssembly)
                {
                    continue;
                }

                var ctorParams = ctor.GetParameters();
                if (ctorParams.Length == types.Length)
                {
                    bool sameParams = true;
                    for (int i = 0; i < ctorParams.Length && sameParams; i++)
                    {
                        if (ctorParams[i].ParameterType != types[i])
                        {
                            sameParams = false;
                        }
                    }

                    if (sameParams)
                    {
                        return ctor;
                    }
                }
            }

            return null;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static PropertyInfo GetProperty(this Type type,
                                               string propertyName,
                                               bool? isStatic = null,
                                               bool isPublic = true)
        {
            var targetType = type;

            do
            {
                var propInfo = type.GetTypeInfo().GetDeclaredProperty(propertyName);
                if (propInfo != null)
                {
                    bool staticCond = isStatic == null || ((propInfo.GetMethod != null && isStatic == propInfo.GetMethod.IsStatic) || (propInfo.SetMethod != null && isStatic == propInfo.SetMethod.IsStatic));
                    bool publicCond = (propInfo.GetMethod != null && isPublic == propInfo.GetMethod.IsPublic) || (propInfo.SetMethod != null && isPublic == propInfo.SetMethod.IsPublic);

                    bool ok = staticCond && publicCond;

                    if (ok)
                    {
                        return propInfo;
                    }
                }

                targetType = targetType.GetTypeInfo().BaseType;

            } while (targetType != null && targetType != typeof(object));

            Debug.Assert(false, "Could not find property " + propertyName + " on type " + type);

            return null;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static MethodInfo GetMethod(this Type type,
                                           string methodName,
                                           bool? isStatic = null,
                                           bool isPublic = true,
                                           Type[] parameters = null)
        {
            var targetType = type;

            do
            {
                var methodInfo = type.GetTypeInfo().GetDeclaredMethod(methodName);
                if (methodInfo != null
                    && AreEquals(methodInfo.GetParameters(), parameters))
                {
                    bool staticCond = isStatic == null || isStatic == methodInfo.IsStatic;
                    bool publicCond = isPublic == methodInfo.IsPublic;

                    bool ok = staticCond && publicCond;

                    if (ok)
                    {
                        return methodInfo;
                    }
                }

                targetType = targetType.GetTypeInfo().BaseType;

            } while (targetType != null && targetType != typeof(object));

            Debug.Assert(false, "Could not find method " + methodName + " on type " + type);

            return null;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static bool AreEquals(ParameterInfo[] types1,
                                      Type[] types2)
        {
            if (types1.Length != types2.Length)
            {
                return false;
            }

            for (int i = 0; i < types1.Length; i++)
            {
                if (types1[i].ParameterType != types2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
