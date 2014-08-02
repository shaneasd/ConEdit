using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class QuickLookupCollection<T, U> : IEnumerable<T>
    {
        private readonly Dictionary<U, T> m_data = new Dictionary<U, T>();
        private readonly Func<T, U> m_keySelector;

        public QuickLookupCollection(Func<T, U> keySelector)
        {
            m_keySelector = keySelector;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_data.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_data.Values.GetEnumerator();
        }

        public T this[U key]
        {
            get { return m_data[key]; }
        }

        public void Add(T value)
        {
            m_data.Add(m_keySelector(value), value);
        }
    }
}
