using System;
using System.Linq;
using System.Threading;

namespace DesertOctopus.MammothCache
{
    internal sealed class LongCounter
    {
        private readonly Semaphore _lockRoot = new Semaphore(1, 1);
        private long _value = 0;

        public void Set(long newValue)
        {
            _lockRoot.WaitOne();
            try
            {
                _value = newValue;
            }
            finally
            {
                _lockRoot.Release();
            }
        }

        public void Add(long value)
        {
            _lockRoot.WaitOne();
            try
            {
                _value += value;
            }
            finally
            {
                _lockRoot.Release();
            }
        }

        public void Substract(long value)
        {
            _lockRoot.WaitOne();
            try
            {
                _value -= value;
            }
            finally
            {
                _lockRoot.Release();
            }
        }

        public long Get()
        {
            _lockRoot.WaitOne();
            try
            {
                return _value;
            }
            finally
            {
                _lockRoot.Release();
            }
        }
    }
}