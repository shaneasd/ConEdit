using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class DictionaryOfLists<TKey, TValue>
    {
        Dictionary<TKey, List<TValue>> m_data = new Dictionary<TKey, List<TValue>>();
        public List<TValue> this[TKey key]
        {
            get
            {
                if (!m_data.ContainsKey(key))
                    m_data[key] = new List<TValue>();
                return m_data[key];
            }
        }
    }
}
