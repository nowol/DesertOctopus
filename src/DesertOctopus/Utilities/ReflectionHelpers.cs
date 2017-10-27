using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DesertOctopus.Exceptions;
using MissingMethodException = System.MissingMethodException;

namespace DesertOctopus.Utilities
{
    internal class ReflectionHelpers
    {
        public static ConstructorInfo GetPublicConstructor(Type targetType,
                                                           params Type[] parameterTypes)
        {
            var types = parameterTypes ?? new Type[0];

            var constructors = targetType.GetTypeInfo()
                                         .DeclaredConstructors;

            foreach (var constructorInfo in constructors)
            {
                if (constructorInfo.IsPublic)
                {
                    var constructorParameters = constructorInfo.GetParameters();
                    if (HasRequestedParameters(types, constructorParameters))
                    {
                        return constructorInfo;
                    }
                }
            }

            return null;
        }

        public static MethodInfo GetPublicMethod(Type targetType,
                                                 string methodName,
                                                 params Type[] parameterTypes)
        {
            return GetMethod(targetType,
                             methodName,
                             x => x.IsPublic,
                             parameterTypes);
        }

        public static MethodInfo GetPublicStaticMethod(Type targetType,
                                                       string methodName,
                                                       params Type[] parameterTypes)
        {
            return GetMethod(targetType,
                             methodName,
                             x => x.IsPublic && x.IsStatic,
                             parameterTypes);
        }

        public static MethodInfo GetNonPublicStaticMethod(Type targetType,
                                                          string methodName,
                                                          params Type[] parameterTypes)
        {
            return GetMethod(targetType,
                             methodName,
                             x => x.IsAssembly && x.IsStatic,
                             parameterTypes);
        }

        private static MethodInfo GetMethod(Type targetType,
                                            string methodName,
                                            Func<MethodInfo, bool> miPredicate,
                                            params Type[] parameterTypes)
        {
            var types = parameterTypes ?? new Type[0];

            var methods = targetType.GetTypeInfo().DeclaredMethods;

            foreach (var methodInfo in methods)
            {
                if (miPredicate(methodInfo) && methodInfo.Name == methodName)
                {
                    var methodParameters = methodInfo.GetParameters();
                    if (HasRequestedParameters(types, methodParameters))
                    {
                        return methodInfo;
                    }
                }
            }

            throw new MissingMethodException($"Method {methodName} was not found on type {targetType}.");
        }

        private static bool HasRequestedParameters(Type[] requestedTypes, ParameterInfo[] parameters)
        {
            if (parameters.Length == requestedTypes.Length)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType != requestedTypes[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}
