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
using SerializerTests.TestObjects;
using Xunit;

namespace DesertOctopus.Tests
{
    public class TrackerTest
    {
        [Fact]
        [Trait("Category", "Unit")]
        public void TrackingNullObjectShouldTrackNothing()
        {
            var tracker = new SerializerObjectTracker();
            tracker.TrackObject(null);
            Assert.Equal(0, tracker.NumberOfTrackedObjects);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void GettingTheIndexOfANullObjectShouldReturnNull()
        {
            var tracker = new SerializerObjectTracker();
            Assert.False(tracker.GetTrackedObjectIndex(null).HasValue);
        }

        [Fact]
        [Trait("Category", "Unit")]
        public void ObjectClonerReferenceTrackerShouldReturnNullIfItDoesNotContainsObject()
        {
            var tracker = new ObjectClonerReferenceTracker();
            Assert.Null(tracker.GetEquivalentTargetObject(23));
        }
    }
}
