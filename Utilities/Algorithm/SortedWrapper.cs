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

            int i = 0;
            foreach (var item in m_sortedData)
            {
                m_positions[item] = i;
                i++;
            }

            data.Inserting += a =>
            {
                m_sortedData.AddLast(a);
                m_positions[a] = m_sortedData.Count - 1;
            };
            data.Removing += (a) =>
            {
                m_sortedData.Remove(a);
                RegeneratePositions();
            };
            data.Clearing += () =>
            {
                m_sortedData.Clear();
                m_positions.Clear();
            };
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

            RegeneratePositions();
        }

        private void RegeneratePositions()
        {
            m_positions.Clear();
            int i = 0;
            foreach (var item in m_sortedData)
            {
                m_positions[item] = i;
                i++;
            }
        }

        Dictionary<T, int> m_positions = new Dictionary<T, int>();

        public IEnumerable<T> Reverse()
        {
            return new LinkedListReverseEnumerable<T>(m_sortedData);
        }

        /// <summary>
        /// positive if 'of' is in front of 'relativeTo'
        /// </summary>
        public int RelativePosition(T of, T relativeTo)
        {
            return m_positions[of] - m_positions[relativeTo];
        }
    }
}