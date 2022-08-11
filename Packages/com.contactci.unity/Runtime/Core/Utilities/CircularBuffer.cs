using System;
using System.Runtime.CompilerServices;

namespace Maestro.Core.Utilities
{
    public class CircularBuffer<T>
    {
        private T[] _buffer;
        private int _start;
        private int _end;
        
        public int Count { get; private set; }
        public int Capacity { get; }
        public bool IsFull => Count == Capacity;

        public CircularBuffer(int capacity)
        {
            Capacity = capacity;
            _buffer = new T[capacity];
            _start = 0;
            _end = 0;
        }

        /// <summary>
        /// Inserts an item at the front of the buffer.
        /// Returns false if there is no room for the item.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool PushFront(T obj)
        {
            if (IsFull)
                return false;
            
            Decrement(ref _start);
            _buffer[_start] = obj;
            Count++;
            return true;
        }

        /// <summary>
        /// Returns the item at the front of the buffer without removing it.
        /// </summary>
        /// <returns></returns>
        public T PeekFront()
        {
            return _buffer[_start];
        }

        /// <summary>
        /// Inserts an item at the back of the buffer.
        /// Returns false if there is no room for the item.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool PushBack(T obj)
        {
            if (IsFull)
                return false;
            _buffer[_end] = obj;
            Increment(ref _end);
            Count++;
            return true;
        }

        /// <summary>
        /// Returns the item at the end of the buffer without removing it.
        /// </summary>
        /// <returns></returns>
        public T PeekBack()
        {
            if (_end == 0)
                return _buffer[Capacity - 1];
            return _buffer[_end - 1];
        }

        /// <summary>
        /// Returns the item at the front of the buffer and removes it from the buffer.
        /// </summary>
        /// <returns></returns>
        public T PopFront()
        {
            var ret = _buffer[_start];
            _buffer[_start] = default(T);
            Increment(ref _start);
            Count--;
            return ret;
        }

        /// <summary>
        /// Returns the item at the end of the buffer and removes it from the buffer.
        /// </summary>
        /// <returns></returns>
        public T PopBack()
        {
            Decrement(ref _end);
            var ret = _buffer[_end];
            _buffer[_end] = default(T);
            Count--;
            return ret;
        }

        /// <summary>
        /// Clears all items from the internal buffer.
        /// </summary>
        public void Clear()
        {
            _start = 0;
            _end = 0;
            Count = 0;
            Array.Clear(_buffer, 0, _buffer.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Decrement(ref int idx)
        {
            if (--idx < 0)
                idx = Capacity - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Increment(ref int idx)
        {
            if (++idx == Capacity)
                idx = 0;
        }
    }
}