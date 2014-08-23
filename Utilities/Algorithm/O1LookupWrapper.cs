using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class O1LookupWrapper<T, Key>
    {
        Dictionary<Key, T> m_data;
        public T this[Key key]
        {
            get { return m_data[key]; }
        }

        public O1LookupWrapper(CallbackList<T> data, Func<T, Key> keySelector)
        {
            m_data = data.ToDictionary(keySelector);
            data.Inserting += a => m_data[keySelector(a)] = a;
            data.Removing += (a) => m_data.Remove(keySelector(a));
            data.Clearing += () => m_data.Clear();
        }
    }
}
