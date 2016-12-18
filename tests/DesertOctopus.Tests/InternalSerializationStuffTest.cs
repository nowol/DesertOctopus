using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using DesertOctopus.Cloning;
using DesertOctopus.Exceptions;
using DesertOctopus.Utilities;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerializerTests.TestObjects;

namespace DesertOctopus.Tests
{
    [TestClass]
    public class InternalSerializationStuffTest
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void GetFieldsShouldIncludeBaseClass()
        {
            var fields = InternalSerializationStuff.GetFields(typeof(CustomDictionaryWithAdditionalPropertiesWithoutOverridingOnDeserializedCallback));
            fields.Select(x => x.Name)
                  .ToArray()
                  .ShouldAllBeEquivalentTo(new[]
                                           {
                                               "<SomeProperty>k__BackingField",
                                               "_syncRoot",
                                               "buckets",
                                               "comparer",
                                               "count",
                                               "entries",
                                               "freeCount",
                                               "freeList",
                                               "keys",
                                               "values",
                                               "version"
                                           });
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GetFieldShouldExcludeSpecifiedBaseClass()
        {
            var fields = InternalSerializationStuff.GetFields(typeof(CustomDictionaryWithAdditionalPropertiesWithoutOverridingOnDeserializedCallback),
                                                              typeof(Dictionary<string, object>));
            fields.Select(x => x.Name)
                  .ToArray()
                  .ShouldAllBeEquivalentTo(new[]
                                           {
                                               "<SomeProperty>k__BackingField"
                                           });
        }
    }
}