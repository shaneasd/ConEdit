using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public struct NotNullable
    {
        public static NotNullable<T> Make<T>(T value)
        {
            return new NotNullable<T>(value);
        }
    }

    public struct NotNullable<T>
    {
        private T Value;

        public override bool Equals(object obj)
        {
            if (obj is NotNullable<T>)
            {
                return object.Equals(((NotNullable<T>)obj).Value, Value);
            }
            return false;
        }

        public static bool operator ==(NotNullable<T> a, NotNullable<T> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(NotNullable<T> a, NotNullable<T> b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        internal NotNullable(T value)
        {
            Value = value;
        }

        public static implicit operator T(NotNullable<T> a)
        {
            return a.Value;
        }
    }
}
