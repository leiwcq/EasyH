using System;

namespace EasyH.Net.Protocols
{
    internal class CycQueue<T> : IDisposable
    {
        private T[] _bucket;
        private readonly int _capacity = 4;
        private int _head;
        private int _tail;

        public int Length => GetLength();

        public int Capacity => _capacity;

        public T[] Array => _bucket;

        public CycQueue(int capacity)
        {
            if (capacity < 4) capacity = 4;
            _capacity = capacity;
            _bucket = new T[capacity];
        }

        public void Clear()
        {
            _head = _tail = 0;
        }

        private int GetLength()
        {
            return (_tail - _head + _capacity) % _capacity;
        }

        public bool IsEmpty ()
        {
            return _tail == _head;
            //return length == 0;
        }

        public bool IsFull()
        {
            return (_tail + 1) % _capacity == _head;
            //return length == capacity;
        }

        public bool EnQueue(T value)
        {
            if (IsFull()) return false;
            _bucket[_tail] = value;
            _tail = (_tail+1) % _capacity;
            return true;
        }

        public T DeQueue()
        {
            if (IsEmpty()) return default(T);
            T v = _bucket[_head];
            _head = (_head+1) % _capacity;
            return v;
        }

        public T[] DeRange(int size)
        {
            if (size > Length) return null;
            T[] array = new T[size];
            int index = 0;
            while (size > 0)
            {
                if (IsEmpty()) return null;

                array[index] = _bucket[_head];
                _head = (_head + 1) % _capacity;
                --size;
                ++index;
            }
            return array;
        }

        public void Clear(int size)
        {
            int len = size <= Length ? size : Length;

            while (len > 0)
            {
                if (IsEmpty()) break;

                _head = (_head + 1) % _capacity;
                --len;
            }
        }

        public int DeSearchIndex(T value,int offset)
        {
            if (offset > Length) return -1;

            if (offset > 0)
            {
                _head = (_head + offset) % _capacity;
            }
            while (Length > 0)
            {
                if (IsEmpty()) return -1;
                if (value.Equals(_bucket[_head])) return _head;

                _head = (_head + 1) % _capacity;
            }
            return -1;
        }

        public int PeekIndex(T value, int offset)
        {
            if (offset > Length) return -1;

            int h = (_head + offset) % _capacity;
            if (_bucket[h].Equals(value)) return h;
            return -1;
        }

        public void Dispose()
        {
            if (_bucket != null)
                _bucket = null;
        }
    }
}
