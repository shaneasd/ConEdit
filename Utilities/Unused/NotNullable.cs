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
