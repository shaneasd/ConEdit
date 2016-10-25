using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class SharedResource<T> : Disposable where T : IDisposable
    {
        private T m_wrapped;
        private Count m_count;

        class Count
        {
            int m_count = 1;
            object m_lockObj = new object();
            public void Increment()
            {
                lock (m_lockObj)
                {
                    m_count++;
                }
            }
            public bool Decrement()
            {
                lock (m_lockObj)
                {
                    m_count--;
                    return m_count == 0;
                }
            }
        }

        private SharedResource(SharedResource<T> toCopy)
        {
            m_count = toCopy.m_count;
            m_count.Increment();
            m_wrapped = toCopy.m_wrapped;
        }

        public SharedResource(Func<T> wrap)
        {
            m_wrapped = wrap();
            m_count = new Count();
        }

        public SharedResource<T> Copy()
        {
            return new SharedResource<T>(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_count.Decrement())
                    m_wrapped.Dispose();
            }
        }
    }
}
