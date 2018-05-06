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
using SerializerTests.TestObjects;
using Xunit;

namespace DesertOctopus.Tests
{
    public class InternalSerializationStuffTest
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void GetFieldsShouldIncludeBaseClass()
        {
            var fields = InternalSerializationStuff.GetFields(typeof(CustomDictionaryWithAdditionalPropertiesWithoutOverridingOnDeserializedCallback));
            fields.Select(x => x.Name)
                  .ToArray()
                  .Should()
                  .BeEquivalentTo(new[]
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

        [Fact]
        [Trait("Category", "Unit")]
        public void GetFieldShouldExcludeSpecifiedBaseClass()
        {
            var fields = InternalSerializationStuff.GetFields(typeof(CustomDictionaryWithAdditionalPropertiesWithoutOverridingOnDeserializedCallback),
                                                              typeof(Dictionary<string, object>));
            fields.Select(x => x.Name)
                  .ToArray()
                  .Should()
                  .BeEquivalentTo(new[]
                                  {
                                      "<SomeProperty>k__BackingField"
                                  });
        }
    }
}