using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class ExtraLazyEnumerable<T> : IEnumerable<T>
    {
        private Func<IEnumerable<T>> m_source;
        public ExtraLazyEnumerable(Func<IEnumerable<T>> source)
        {
            m_source = source;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_source().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_source().GetEnumerator();
        }
    }
}
