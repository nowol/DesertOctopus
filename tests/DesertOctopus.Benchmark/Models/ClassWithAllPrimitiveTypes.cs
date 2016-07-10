using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Benchmark.Models
{
    [Serializable]
    public class ClassWithAllPrimitiveTypes
    {
        public ClassWithAllPrimitiveTypes()
        {
            ByteValue = 3;
            SByteValue = 3;
            NByteValue = 3;
            SNByteValue = 3;
            ShortValue = 3;
            UShortValue = 3;
            NShortValue = 3;
            UNShortValue = 3;
            IntValue = 3;
            UIntValue = 3;
            NIntValue = 3;
            UNIntValue = 3;
            LongValue = 3;
            ULongValue = 3;
            NLongValue = 3;
            UNLongValue = 3;
            DoubleValue = 3;
            NDoubleValue = 3;
            FloatValue = 3;
            NFloatValue = 3;
            DecimalValue = 3;
            NDecimalValue = 3;
            CharValue = 'a';
            NCharValue = 'z';
            BoolValue = true;
            NBoolValue = true;
            DateTimeValue = DateTime.UtcNow;
            NDateTimeValue = DateTime.UtcNow;
            TimeSpanValue = TimeSpan.FromSeconds(3);
            NTimeSpanValue = TimeSpan.FromSeconds(3);
            BigIntegerValue = new BigInteger(3892);
            NBigIntegerValue = new BigInteger(3892);

            // not primitives
            //StringValue = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            //DictValue = new Dictionary<int, int> { {1, 2}, {2, 2}, {3, 2}, {4, 2}, };
        }

        //public Dictionary<int,int> DictValue { get; set; }
        //public string StringValue { get; set; }

        public byte ByteValue { get; set; }
        public sbyte SByteValue { get; set; }
        public byte? NByteValue { get; set; }
        public sbyte? SNByteValue { get; set; }
        public short ShortValue { get; set; }
        public ushort UShortValue { get; set; }
        public short? NShortValue { get; set; }
        public ushort? UNShortValue { get; set; }
        public int IntValue { get; set; }
        public uint UIntValue { get; set; }
        public int? NIntValue { get; set; }
        public uint? UNIntValue { get; set; }
        public long LongValue { get; set; }
        public ulong ULongValue { get; set; }
        public long? NLongValue { get; set; }
        public ulong? UNLongValue { get; set; }
        public double DoubleValue { get; set; }
        public double? NDoubleValue { get; set; }
        public float FloatValue { get; set; }
        public float? NFloatValue { get; set; }
        public decimal DecimalValue { get; set; }
        public decimal? NDecimalValue { get; set; }
        public char CharValue { get; set; }
        public char? NCharValue { get; set; }
        public bool BoolValue { get; set; }
        public bool? NBoolValue { get; set; }
        public DateTime DateTimeValue { get; set; }
        public DateTime? NDateTimeValue { get; set; }
        public TimeSpan TimeSpanValue { get; set; }
        public TimeSpan? NTimeSpanValue { get; set; }
        public BigInteger BigIntegerValue { get; set; }
        public BigInteger? NBigIntegerValue { get; set; }
    }
}
