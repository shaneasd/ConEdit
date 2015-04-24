using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    /// <summary>
    /// Implements a fixed length immutable set of unique elements.
    /// The expectation is that the number of elements would be resonably small such that a HashSet would be overkill
    /// </summary>
    public class ReadonlySet<T> : IEnumerable<T>
    {
        private readonly T[] m_values;
        public ReadonlySet(params T[] values)
            : this((IEnumerable<T>)values)
        {
        }
        public ReadonlySet(IEnumerable<T> values)
        {
            m_values = values.Distinct().ToArray();
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var a in m_values)
                yield return a;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            var other = obj as ReadonlySet<T>;
            if (other != null)
            {
                if (other.m_values.Length != m_values.Length)
                    return false;
                foreach (var v in m_values)
                {
                    if (!other.m_values.Contains(v))
                        return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            //Note that this hashing is commutative
            int hash = 0;
            foreach (var v in m_values)
            {
                hash ^= v.GetHashCode();
            }
            return hash;
        }
    }
}
