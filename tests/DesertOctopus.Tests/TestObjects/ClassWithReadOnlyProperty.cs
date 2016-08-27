﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesertOctopus.Tests.TestObjects
{
    public class ClassWithReadOnlyProperty<T>
    {
        private readonly T _value;

        public T Value
        {
            get { return _value; }
        }

        public ClassWithReadOnlyProperty(T value)
        {
            _value = value;
        }
    }

    public class ClassWithCSharp6StyleReadOnlyProperty<T>
    {
        public T Value { get; }

        public ClassWithCSharp6StyleReadOnlyProperty(T value)
        {
            Value = value;
        }
    }
}
