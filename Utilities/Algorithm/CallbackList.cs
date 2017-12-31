using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    /// <summary>
    /// List object that triggers callbacks whenever its content is modified. Distinct from
    /// System.Collections.ObjectModel.ObservableCollection in that most of its callbacks
    /// trigger before modification rather than after.
    /// </summary>
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

        /// <summary>
        /// Triggered before an element is inserted
        /// </summary>
        public event Action<T> Inserting;
        /// <summary>
        /// Triggered after an element has been inserted (but before the generic Modified event)
        /// </summary>
        public event Action<T> Inserted;

        /// <summary>
        /// Triggered before an element is removed.
        /// </summary>
        public event Action<T> Removing;
        public event Action Clearing;

        public event Action Modified;

        public void Insert(int index, T item)
        {
            Inserting.Execute(item);
            m_base.Insert(index, item);
            Inserted.Execute(item);
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
                Removing.Execute(m_base[index]);
                Inserting.Execute(value);
                m_base[index] = value;
                Inserted.Execute(value);
                Modified.Execute();
            }
        }

        public void Add(T item)
        {
            Inserting.Execute(item);
            m_base.Add(item);
            Inserted.Execute(item);
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
                Removing.Execute(item);
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

        public int Count => m_base.Count;

        public bool IsReadOnly => false;

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
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));
            foreach (var e in elements)
            {
                Add(e);
            }
        }

        public void RemoveRange(IEnumerable<T> elements)
        {
            if (elements == null)
                throw new ArgumentNullException(nameof(elements));
            foreach (var e in elements)
            {
                Remove(e);
            }
        }
    }
}
