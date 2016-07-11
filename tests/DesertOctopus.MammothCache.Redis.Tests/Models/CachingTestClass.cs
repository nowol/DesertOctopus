﻿using System;
using System.Linq;

namespace DesertOctopus.MammothCache.Redis.Tests.Models
{
    public class CachingTestClass
    {
        public Guid Value { get; set; }
        public bool[] ByteArray = new bool[0];

        public CachingTestClass()
        {
            Value = Guid.NewGuid();
        }
    }
}