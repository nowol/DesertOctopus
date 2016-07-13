using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DesertOctopus.Cloning;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for IQueryableCloner MethodInfo
    /// </summary>
    internal static class IQueryableMIH
    {
        /// <summary>
        /// Calls IQueryableCloner.IsGenericIQueryableType
        /// </summary>
        /// <returns>The method info for IQueryableCloner.IsGenericIQueryableType</returns>
        public static MethodInfo IsGenericIQueryableType()
        {
            return typeof(IQueryableCloner).GetMethod(nameof(IQueryableCloner.IsGenericIQueryableType), new[] { typeof(Type) });
        }

        /// <summary>
        /// Calls IQueryableCloner.ConvertToNonGenericQueryable
        /// </summary>
        /// <returns>The method info for IQueryableCloner.ConvertToNonGenericQueryable</returns>
        public static MethodInfo ConvertToNonGenericQueryable()
        {
            return typeof(IQueryableCloner).GetMethod(nameof(IQueryableCloner.ConvertToNonGenericQueryable), BindingFlags.Static | BindingFlags.NonPublic, null, CallingConventions.Any, new[] { typeof(object) }, new ParameterModifier[0]);
        }
    }
}
