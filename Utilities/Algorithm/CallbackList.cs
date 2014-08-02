using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class CallbackList<T> : IList<T>
    {
        List<T> m_base;
        public CallbackList()
        {
            m_base = new List<T>();
        }

        public CallbackList(int capacity)
        {
            m_base = new List<T>(capacity);
        }

        public CallbackList(IEnumerable<T> collection)
        {
            m_base = new List<T>(collection);
        }

        public event Action<T> Inserting;
        public event Action<T> Removing;
        public event Action Clearing;

        public event Action Modified;

        public void Insert(int index, T item)
        {
            Inserting.Execute(item);
            m_base.Insert(index, item);
            Modified.Execute();
        }

        public void RemoveAt(int index)
        {
            Removing.Execute(this[index]);
            m_base.RemoveAt(index);
            Modified.Execute();
        }

        public T this[int index]
        {
            get
            {
                return m_base[index];
            }
            set
            {
                Removing.Execute( m_base[index]);
                Inserting.Execute( value);
                m_base[index] = value;
                Modified.Execute();
            }
        }

        public void Add(T item)
        {
            Inserting.Execute( item);
            m_base.Add(item);
            Modified.Execute();
        }

        public void Clear()
        {
            Clearing.Execute();
            m_base.Clear();
            Modified.Execute();
        }

        public bool Remove(T item)
        {
            if (m_base.Contains(item))
            {
                Removing.Execute( item);
                bool result = m_base.Remove(item);
                Modified.Execute();
                return result;
            }
            else
                return false;
        }

        public int IndexOf(T item)
        {
            return m_base.IndexOf(item);
        }

        public bool Contains(T item)
        {
            return m_base.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_base.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return m_base.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_base.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)m_base).GetEnumerator();
        }

        public void AddRange(IEnumerable<T> elements)
        {
            foreach (var e in elements)
            {
                Add(e);
            }
        }

        public void RemoveRange(IEnumerable<T> elements)
        {
            foreach (var e in elements)
            {
                Remove(e);
            }
        }
    }
}
