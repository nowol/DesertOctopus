using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SerializerTests.TestObjects
{
    [Serializable]
    public class ClassWithISerializable : ISerializable, IDeserializationCallback
    {
        public int PropertyInt { get; set; }
        public int? PropertyNullableInt { get; set; }
        public double PropertyDouble { get; set; }
        public object PropertyObject { get; set; }
        public object PropertyObject2 { get; set; }
        public float PropertyFloat { get; set; }

        public int NumberOfTimesGetObjectDataWasCalled { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            NumberOfTimesGetObjectDataWasCalled++;

            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("PropertyInt", PropertyInt);
            info.AddValue("PropertyNullableInt", PropertyNullableInt);
            info.AddValue("PropertyDouble", PropertyDouble);
            info.AddValue("PropertyObject", PropertyObject);
            info.AddValue("PropertyObject2", PropertyObject2);
            // PropertyFloat is not copied to see that ISerializable was used
        }

        public void OnDeserialization(object sender)
        {
            //SerializationInfo serializationInfo;
            //HashHelpers.SerializationInfoTable.TryGetValue((object)this, out serializationInfo);
            //if (serializationInfo == null)
            //    return;
            //int int32_1 = serializationInfo.GetInt32("Version");
            //int int32_2 = serializationInfo.GetInt32("HashSize");
            //this.comparer = (IEqualityComparer<TKey>)serializationInfo.GetValue("Comparer", typeof(IEqualityComparer<TKey>));
            //if (int32_2 != 0)
            //{
            //    this.buckets = new int[int32_2];
            //    for (int index = 0; index < this.buckets.Length; ++index)
            //        this.buckets[index] = -1;
            //    this.entries = new Dictionary<TKey, TValue>.Entry[int32_2];
            //    this.freeList = -1;
            //    KeyValuePair<TKey, TValue>[] keyValuePairArray = (KeyValuePair<TKey, TValue>[])serializationInfo.GetValue("KeyValuePairs", typeof(KeyValuePair<TKey, TValue>[]));
            //    if (keyValuePairArray == null)
            //        ThrowHelper.ThrowSerializationException(ExceptionResource.Serialization_MissingKeys);
            //    for (int index = 0; index < keyValuePairArray.Length; ++index)
            //    {
            //        if ((object)keyValuePairArray[index].Key == null)
            //            ThrowHelper.ThrowSerializationException(ExceptionResource.Serialization_NullKey);
            //        this.Insert(keyValuePairArray[index].Key, keyValuePairArray[index].Value, true);
            //    }
            //}
            //else
            //    this.buckets = (int[])null;
            //this.version = int32_1;
            //HashHelpers.SerializationInfoTable.Remove((object)this);



        }
    }
}
