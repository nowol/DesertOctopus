using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using DesertOctopus.Cloning;
using DesertOctopus.Exceptions;
using SerializerTests.TestObjects;
using Xunit;

namespace DesertOctopus.Tests
{
    public class ExceptionsTest
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void TypeWasModifiedSinceSerializationExceptionShouldInitializeItsMessage()
        {
            var ex = new TypeWasModifiedSinceSerializationException("abc");
            Assert.Equal("abc", ex.Message);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void TypeWasModifiedSinceSerializationExceptionShouldInitializeItsMessageWithSerializationContext()
        {
            var ex = new TypeWasModifiedSinceSerializationException("abc");
            var clonedEx = ObjectCloner.Clone(ex);
            Assert.Equal("abc", clonedEx.Message);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void InvalidSerializationVersionExceptionShouldInitializeItsMessageWithSerializationContext()
        {
            var ex = new InvalidSerializationVersionException("abc");
            var clonedEx = ObjectCloner.Clone(ex);
            Assert.Equal("abc", clonedEx.Message);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void MissingConstructorExceptionShouldInitializeItsMessageWithSerializationContext()
        {
            var ex = new MissingConstructorException("abc");
            var clonedEx = ObjectCloner.Clone(ex);
            Assert.Equal("abc", clonedEx.Message);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void TypeNotFoundExceptionShouldInitializeItsMessageWithSerializationContext()
        {
            var ex = new TypeNotFoundException("abc");
            var clonedEx = ObjectCloner.Clone(ex);
            Assert.Equal("abc", clonedEx.Message);
        }
    }
}
