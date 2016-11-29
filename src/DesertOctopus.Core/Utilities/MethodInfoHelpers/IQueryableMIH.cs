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
    internal static class IQueryableMih
    {
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
