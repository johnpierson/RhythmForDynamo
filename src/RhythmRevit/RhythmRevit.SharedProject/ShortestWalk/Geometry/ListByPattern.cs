using System;
using System.Collections;
using System.Collections.Generic;

namespace ShortestWalk.Geometry
{
    internal class ListByPattern<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private readonly IList<T> _content;
        private readonly int _size;
        private readonly int _innerCount;

        public ListByPattern(IList<T> content, int length)
        {
            this._size = length >= 0 ? length : throw new ArgumentOutOfRangeException("Length is less than 0", nameof(length));
            this._content = content;
            this._innerCount = this._content.Count;
        }

        public int IndexOf(T item)
        {
            int num = this._content.IndexOf(item);
            return num < this._size ? num : -1;
        }

        public void Insert(int index, T item) => throw new NotSupportedException("This list is readonly.");

        public void RemoveAt(int index) => throw new NotSupportedException("This list is readonly.");

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= this._size)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return this._content[index % this._innerCount];
            }
        }

        T IList<T>.this[int index]
        {
            get => this[index];
            set => throw new NotSupportedException("This list is readonly.");
        }

        void ICollection<T>.Add(T item) => throw new NotSupportedException("This list is readonly.");

        void ICollection<T>.Clear() => throw new NotSupportedException("This list is readonly.");

        public bool Contains(T item) => this.IndexOf(item) != -1;

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "The array index must be larger than 0.");
            if (array.Rank != 1)
                throw new ArgumentException("Array rank must be 1, but this array is multi-dimensional.", nameof(array));
            if (arrayIndex >= array.Length || this._size + arrayIndex > array.Length)
                throw new ArgumentException(nameof(arrayIndex));
            for (int index = 0; index < this._size; ++index)
                array[arrayIndex++] = this[index];
        }

        public int Count => this._size;

        bool ICollection<T>.IsReadOnly => true;

        bool ICollection<T>.Remove(T item) => throw new NotSupportedException("This list is readonly.");

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < this._size; ++i)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => (IEnumerator)this.GetEnumerator();
    }
}