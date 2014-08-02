using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class GenericEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> m_compare;
        private readonly Func<T, int> m_hash;
        public GenericEqualityComparer(Func<T, T, bool> compare, Func<T, int> hash)
        {
            m_compare = compare;
            m_hash = hash;
        }

        public bool Equals(T x, T y)
        {
            return m_compare(x, y);
        }

        public int GetHashCode(T obj)
        {
            return m_hash(obj);
        }
    }
}
