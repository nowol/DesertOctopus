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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerializerTests.TestObjects;

namespace DesertOctopus.Tests
{
    [TestClass]
    public class TrackerTest
    {
        [TestMethod]
        [TestCategory("Unit")]
        public void TrackingNullObjectShouldTrackNothing()
        {
            var tracker = new SerializerObjectTracker();
            tracker.TrackObject(null);
            Assert.AreEqual(0, tracker.NumberOfTrackedObjects);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void GettingTheIndexOfANullObjectShouldReturnNull()
        {
            var tracker = new SerializerObjectTracker();
            Assert.IsFalse(tracker.GetTrackedObjectIndex(null).HasValue);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ObjectClonerReferenceTrackerShouldReturnNullIfItDoesNotContainsObject()
        {
            var tracker = new ObjectClonerReferenceTracker();
            Assert.IsNull(tracker.GetEquivalentTargetObject(23));
        }
    }
}
