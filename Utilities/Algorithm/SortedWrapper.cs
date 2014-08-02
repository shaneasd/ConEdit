using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class SortedWrapper<T> : IEnumerableReversible<T> where T : class
    {
        private LinkedList<T> m_sortedData;
        public SortedWrapper(CallbackList<T> data)
        {
            m_sortedData = new LinkedList<T>(data);
            data.Inserting += a => m_sortedData.AddLast(a);
            data.Removing += a => m_sortedData.Remove(a);
            data.Clearing += () => m_sortedData.Clear();
        }

        public SortedWrapper(IEnumerable<T> data, Action<Action<T>> inserting, Action<Action<T>> removing, Action<Action> clearing)
        {
            m_sortedData = new LinkedList<T>(data);
            inserting(a => m_sortedData.AddLast(a));
            removing(a => m_sortedData.Remove(a));
            clearing(() => m_sortedData.Clear());
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_sortedData.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_sortedData.GetEnumerator();
        }

        public void BringToFront(IEnumerable<T> elements)
        {
            LinkedListNode<T> last = null;
            for (var i = m_sortedData.First; i != null; i = i.Next)
            {
                if (elements.Contains(i.Value))
                {
                    m_sortedData.Remove(i);
                    if (last == null)
                        last = m_sortedData.AddFirst(i.Value);
                    else
                        last = m_sortedData.AddAfter(last, i.Value);
                }
            }
        }

        public IEnumerable<T> Reverse()
        {
            return new LinkedListReverseEnumerable<T>(m_sortedData);
        }
    }
}