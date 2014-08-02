using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public abstract class ParameterType
    {
    }

    public class ID<T> : IComparable<ID<T>>
    {
        public Guid Guid { get { return m_data; } }

        private readonly Guid m_data;
        public ID()
        {
            m_data = Guid.NewGuid();
        }

        private ID(Guid data)
        {
            m_data = data;
        }

        public static ID<T> ConvertFrom<U>(ID<U> other)
        {
            return new ID<T>(other.Guid);
        }

        public static ID<T> Parse(string p)
        {
            return new ID<T>(Guid.Parse(p));
        }

        public static ID<T> New()
        {
            return new ID<T>(Guid.NewGuid());
        }

        public static ID<T> Dummy = new ID<T>(Guid.Empty);

        public override bool Equals(object obj)
        {
            return (obj as ID<T>) == this;
        }

        public override int GetHashCode()
        {
            return m_data.GetHashCode();
        }

        public static bool operator ==(ID<T> a, ID<T> b)
        {
            bool aNull = object.Equals(null, a);
            bool bNull = object.Equals(null, b);
            if (aNull && bNull)
                return true;
            else if (aNull || bNull)
                return false;
            else
                return object.Equals(a.m_data, b.m_data);
        }

        public static bool operator !=(ID<T> a, ID<T> b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            //We never want to be implicitly converting this to a string
            throw new NotImplementedException("ID<" + typeof(T).FullName + ">.ToString()");
        }

        public string Serialized()
        {
            return m_data.ToString();
        }

        public static ID<T> FromGuid(System.Guid guid)
        {
            return new ID<T>(guid);
        }

        internal static bool TryParse(string value, out ID<T> m_value)
        {
            Guid guid;
            if (Guid.TryParse(value, out guid))
            {
                m_value = new ID<T>(guid);
                return true;
            }
            else
            {
                m_value = null;
                return false;
            }
        }

        public int CompareTo(ID<T> other)
        {
            return Guid.CompareTo(other.Guid);
        }
    }

}
