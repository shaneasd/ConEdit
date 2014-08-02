using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    class CallbackSet<T> : ISet<T>
    {
        HashSet<T> m_data = new HashSet<T>();

        public bool Add(T item)
        {
            throw new NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return m_data.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return m_data.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return m_data.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return m_data.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return m_data.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return m_data.SetEquals(other);
        }

        public bool Contains(T item)
        {
            return m_data.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_data.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return m_data.Count; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_data.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_data.GetEnumerator();
        }
    }
}
