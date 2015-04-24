using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities.UI
{
    public static class ToStringWrapper
    {
        public static ToStringWrapper<T> Make<T>(T value, string toString)
        {
            return new ToStringWrapper<T>(value, toString);
        }
    }

    public class ToStringWrapper<T>
    {
        public readonly T Value;
        private readonly string m_string;
        public ToStringWrapper(T value, string toString)
        {
            Value = value;
            m_string = toString;
        }

        public override string ToString()
        {
            return m_string;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ToStringWrapper<T>;
            if (other != null)
                return other.Value.Equals(Value);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
