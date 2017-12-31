using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class LazyReverseEnumerable<T> : IEnumerable<T>
    {
        private IEnumerable<T> m_data;
        public LazyReverseEnumerable(IEnumerable<T> data)
        {
            m_data = data;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_data.Reverse().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_data.Reverse().GetEnumerator();
        }
    }

    public class LinkedListReverseEnumerable<T> : IEnumerable<T>
    {
        private LinkedList<T> m_data;
        public LinkedListReverseEnumerable(LinkedList<T> data)
        {
            m_data = data;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new LinkedListReverseEnumerator<T>(m_data);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new LinkedListReverseEnumerator<T>(m_data);
        }
    }

    public class LinkedListReverseEnumerator<T> : Disposable, IEnumerator<T>
    {
        private LinkedListNode<T> m_current = null;
        private LinkedList<T> m_data;
        public LinkedListReverseEnumerator(LinkedList<T> data)
        {
            m_data = data;
        }

        public T Current => m_current.Value;

        protected override void Dispose(bool disposing)
        {
        }

        object System.Collections.IEnumerator.Current => m_current.Value;

        public bool MoveNext()
        {
            if (m_current == null)
            {
                m_current = m_data.Last;
            }
            else
            {
                m_current = m_current.Previous;
            }
            return m_current != null;
        }

        public void Reset()
        {
            m_current = null;
        }
    }

    public interface IEnumerableReversible<T> : IEnumerable<T>
    {
        IEnumerable<T> Reverse();
    }

    public static class EnumerableReversible
    {
        public static EnumerableReversible<T> Empty<T>()
        {
            return new EnumerableReversible<T>(Enumerable.Empty<T>(), Enumerable.Empty<T>());
        }
        public static EnumerableReversible<T> Make<T>(IEnumerable<T> data)
        {
            if (data is LinkedList<T> linkedList)
            {
                return Make(linkedList);
            }
            else
            {
                if (data is EnumerableReversible<T> enumerableReversible)
                    return Make(enumerableReversible);
                else
                    return new EnumerableReversible<T>(data, new LazyReverseEnumerable<T>(data)); //The standard buffered way, but lazy.
            }
        }

        public static EnumerableReversible<T> Make<T>(LinkedList<T> data)
        {
            return new EnumerableReversible<T>(data, new LinkedListReverseEnumerable<T>(data as LinkedList<T>));
        }

        public static EnumerableReversible<T> Make<T>(EnumerableReversible<T> data)
        {
            return EnumerableReversible<T>.Make(data);
        }
    }

    public class EnumerableReversible<T> : IEnumerableReversible<T>
    {
        public static EnumerableReversible<T> Make(EnumerableReversible<T> data)
        {
            var a = data as EnumerableReversible<T>;
            return new EnumerableReversible<T>(a.m_reverse, a.m_forward);
        }

        private IEnumerable<T> m_forward;
        private IEnumerable<T> m_reverse;

        public EnumerableReversible(IEnumerable<T> forward, IEnumerable<T> reverse)
        {
            m_forward = forward;
            m_reverse = reverse;
        }

        public IEnumerable<T> Reverse()
        {
            return m_reverse;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_forward.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_forward.GetEnumerator();
        }
    }
}
