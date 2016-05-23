using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SerializerTests.TestObjects
{
    [Serializable]
    public class CustomDictionary : Dictionary<string, object>
    {
        public CustomDictionary()
            : base()
        {

        }
        public CustomDictionary(IEqualityComparer<string> comparer)
            : base(comparer)
        {
        }

        protected CustomDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class CustomDictionaryWithAdditionalPropertiesWithoutOverridingOnDeserializedCallback : Dictionary<string, object>
    {
        public int SomeProperty { get; set; }

        public CustomDictionaryWithAdditionalPropertiesWithoutOverridingOnDeserializedCallback()
            : base()
        {

        }

        protected CustomDictionaryWithAdditionalPropertiesWithoutOverridingOnDeserializedCallback(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }

    [Serializable]
    public class CustomDictionaryWithAdditionalPropertiesWithOverridingOnDeserializedCallback : Dictionary<string, object>
    {
        private readonly SerializationInfo _info;
        public int SomeProperty { get; set; }

        public CustomDictionaryWithAdditionalPropertiesWithOverridingOnDeserializedCallback()
            : base()
        {

        }

        protected CustomDictionaryWithAdditionalPropertiesWithOverridingOnDeserializedCallback(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _info = info; // this is not the correct way to store the SerializationInfo but it's good enough for a unit test
        }

        public override void GetObjectData(SerializationInfo info,
                                           StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SomeProperty", SomeProperty);
        }

        public override void OnDeserialization(object sender)
        {
            base.OnDeserialization(sender);

            SomeProperty = _info.GetInt32("SomeProperty");
        }
    }

    [Serializable]
    public class CustomDictionaryWithAdditionalPropertiesAndGenerics<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private readonly SerializationInfo _info;
        public int SomeProperty { get; set; }

        public CustomDictionaryWithAdditionalPropertiesAndGenerics()
        {
        }
        protected CustomDictionaryWithAdditionalPropertiesAndGenerics(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _info = info; // this is not the correct way to store the SerializationInfo but it's good enough for a unit test
        }

        public override void GetObjectData(SerializationInfo info,
                                           StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SomeProperty", SomeProperty);
        }

        public override void OnDeserialization(object sender)
        {
            base.OnDeserialization(sender);

            SomeProperty = _info.GetInt32("SomeProperty");
        }
    }

    [Serializable]
    public class CustomDictionaryWithoutSerializationConstructor<TKey, TValue> : Dictionary<TKey, TValue>
    {
    }

    [Serializable]
    public class CustomDictionaryWithDictionaryProperty<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private readonly SerializationInfo _info;
        public Dictionary<TValue, TKey> SwitchedDictionary { get; set; }

        public CustomDictionaryWithDictionaryProperty()
        {
        }
        protected CustomDictionaryWithDictionaryProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _info = info; // this is not the correct way to store the SerializationInfo but it's good enough for a unit test
        }

        public override void GetObjectData(SerializationInfo info,
                                           StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SwitchedDictionary", SwitchedDictionary);
        }

        public override void OnDeserialization(object sender)
        {
            base.OnDeserialization(sender);

            SwitchedDictionary = _info.GetValue("SwitchedDictionary", typeof(Dictionary<TValue, TKey>)) as Dictionary<TValue, TKey>;
        }
    }

    [Serializable]
    public class StructForTestingComparer : IEqualityComparer<StructForTesting>
    {
        public bool Equals(StructForTesting x, StructForTesting y)
        {
            return x.Value == y.Value;
        }

        public int GetHashCode(StructForTesting obj)
        {
            return obj.Value.GetHashCode();
        }
    }
}
