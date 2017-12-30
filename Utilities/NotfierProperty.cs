using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class NotifierProperty<T>
    {
        private T m_value;
        private WeakEvent<Changed<T>> m_changed = new WeakEvent<Changed<T>>();
        public WeakEvent<Changed<T>> Changed => m_changed;
        public NotifierProperty(T value)
        {
            m_value = value;
        }

        public T Value
        {
            get
            {
                return m_value;
            }
            set
            {
                T old = m_value;
                if (!object.Equals(m_value, value))
                {
                    m_value = value;
                    Changed.Execute(Utilities.Changed.Create(old, m_value));
                }
            }
        }
    }

    public static class Changed
    {
        public static Changed<T> Create<T>(T from, T to)
        {
            return new Changed<T>(from, to);
        }
    }

    public struct Changed<T>
    {
        public T From { get; private set; }
        public T To { get; private set; }
        public Changed(T from, T to)
        {
            From = from;
            To = to;
        }
    }
}
