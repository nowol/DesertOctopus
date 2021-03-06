﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DesertOctopus.Serialization;

namespace DesertOctopus.Utilities
{
    /// <summary>
    /// Helper class for Deserializer MethodInfo
    /// </summary>
    internal static class DeserializerMih
    {
        /// <summary>
        /// Calls Deserializer.GetTypeDeserializer
        /// </summary>
        /// <returns>The method info for Deserializer.GetTypeDeserializer</returns>
        public static MethodInfo GetTypeToObjectDeserializer()
        {
            return typeof(Deserializer).GetMethod(nameof(Deserializer.GetTypeToObjectDeserializer), BindingFlags.Static | BindingFlags.NonPublic);
        }
    }
}
