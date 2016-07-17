using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using DesertOctopus.Cloning;
using DesertOctopus.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerializerTests.TestObjects;

namespace DesertOctopus.Tests
{
    [TestClass]
    public class ExceptionsTest
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void TypeWasModifiedSinceSerializationExceptionShouldInitializeItsMessage()
        {
            var ex = new TypeWasModifiedSinceSerializationException("abc");
            Assert.AreEqual("abc", ex.Message);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TypeWasModifiedSinceSerializationExceptionShouldInitializeItsMessageWithSerializationContext()
        {
            var ex = new TypeWasModifiedSinceSerializationException("abc");
            var clonedEx = ObjectCloner.Clone(ex);
            Assert.AreEqual("abc", clonedEx.Message);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void InvalidSerializationVersionExceptionShouldInitializeItsMessageWithSerializationContext()
        {
            var ex = new InvalidSerializationVersionException("abc");
            var clonedEx = ObjectCloner.Clone(ex);
            Assert.AreEqual("abc", clonedEx.Message);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void MissingConstructorExceptionShouldInitializeItsMessageWithSerializationContext()
        {
            var ex = new MissingConstructorException("abc");
            var clonedEx = ObjectCloner.Clone(ex);
            Assert.AreEqual("abc", clonedEx.Message);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TypeNotFoundExceptionShouldInitializeItsMessageWithSerializationContext()
        {
            var ex = new TypeNotFoundException("abc");
            var clonedEx = ObjectCloner.Clone(ex);
            Assert.AreEqual("abc", clonedEx.Message);
        }
    }
}
