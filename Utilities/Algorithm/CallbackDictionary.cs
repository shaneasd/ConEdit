using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class CallbackDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        Dictionary<TKey, TValue> m_data = new Dictionary<TKey, TValue>();

        public event Action<TKey, TValue> Inserting;
        public event Action<TKey, TValue> Removing;
        public event Action Clearing;
        public event Action Modified;

        public void Add(TKey key, TValue value)
        {
            Inserting.Execute(key, value);
            m_data.Add(key, value);
            Modified.Execute();
        }

        public bool ContainsKey(TKey key)
        {
            return m_data.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get { return m_data.Keys; }
        }

        public bool Remove(TKey key)
        {
            if (!ContainsKey(key))
                return false;
            Removing.Execute(key, this[key]);
            bool result = m_data.Remove(key);
            Modified.Execute();
            return result;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return m_data.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values
        {
            get { return m_data.Values; }
        }

        public TValue this[TKey key]
        {
            get
            {
                return m_data[key];
            }
            set
            {
                if (ContainsKey(key))
                {
                    Removing.Execute(key, this[key]);
                    m_data.Remove(key);
                }
                Inserting.Execute(key, value);
                m_data[key] = value;
                Modified.Execute();
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            Clearing.Execute();
            m_data.Clear();
            Modified.Execute();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return m_data.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            (m_data as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
