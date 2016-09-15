﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public abstract class ParameterType
    {
        public class Basic : ParameterType
        {
            Guid m_value;
            public Basic(Guid value)
            {
                m_value = value;
            }

            public new static ParameterType Parse(string value)
            {
                Guid g;
                if (Guid.TryParse(value, out g))
                {
                    return new Basic(g);
                }
                return null;
            }

            public override Guid Guid
            {
                get { return m_value; }
            }

            public override string Serialized()
            {
                return Guid.ToString();
            }

            public static ParameterType New()
            {
                return new Basic(Guid.NewGuid());
            }

            public static ParameterType ConvertFrom<T>(Id<T> type)
            {
                return new Basic(type.Guid);
            }

            public static ParameterType FromGuid(Guid g)
            {
                return new Basic(g);
            }

            public override bool Equals(object obj)
            {
                Basic b = obj as Basic;
                if (b == null)
                    return false;
                return b.Guid.Equals(Guid);
            }

            public override int GetHashCode()
            {
                return Guid.GetHashCode();
            }

            public override bool IsSet
            {
                get { return false; }
            }
        }

        /// <summary>
        /// Type representing a set of values from another type
        /// </summary>
        public class ValueSetType : ParameterType
        {
            const string SET_PREFIX = "set:";
            Guid m_of;
            public ValueSetType(Guid of)
            {
                m_of = of;
            }

            public new static ParameterType Parse(string value)
            {
                if (value.StartsWith(SET_PREFIX, StringComparison.OrdinalIgnoreCase))
                {
                    Guid g;
                    if (Guid.TryParse(value.Substring(SET_PREFIX.Length), out g))
                    {
                        return new ValueSetType(g);
                    }
                }
                return null;
            }

            public override Guid Guid
            {
                get { return m_of; }
            }

            public override string Serialized()
            {
                return SET_PREFIX + Guid.ToString();
            }

            public static ParameterType New()
            {
                return new Basic(Guid.NewGuid());
            }

            public static ParameterType ConvertFrom<T>(Id<T> type)
            {
                return new Basic(type.Guid);
            }

            public static ParameterType FromGuid(Guid g)
            {
                return new ValueSetType(g);
            }

            public override bool Equals(object obj)
            {
                ValueSetType b = obj as ValueSetType;
                if (b == null)
                    return false;
                return b.Guid.Equals(Guid);
            }

            public override int GetHashCode()
            {
                return Guid.GetHashCode();
            }

            public static ParameterType Of(ParameterType type)
            {
                if (!(type is Basic))
                    throw new NotImplementedException("Currently only sets of basic types are supported");
                else
                    return new ValueSetType(type.Guid);
            }

            public override bool IsSet
            {
                get { return true; }
            }
        }

        public static ParameterType Parse(string guid)
        {
            return Basic.Parse(guid) 
                ?? ValueSetType.Parse(guid)
                ?? null; //Can add other parsers in a chain of ??
        }

        public abstract string Serialized();

        //public override string ToString()
        //{
        //    //We never want to be implicitly converting this to a string (Except for debugging and tests)
        //    throw new NotImplementedException("ParameterType.ToString()");
        //}

        public abstract Guid Guid { get; }

        public static bool operator ==(ParameterType a, ParameterType b)
        {
            return object.Equals(a, b);
        }

        public static bool operator !=(ParameterType a, ParameterType b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            throw new NotSupportedException("Implementing classes must override ParameterType.Equals");
        }

        public override int GetHashCode()
        {
            throw new NotSupportedException("Implementing classes must override ParameterType.GetHashCode");
        }

        public abstract bool IsSet { get; }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
    public class Id<T> : IComparable<Id<T>>, IComparable
    {
        public Guid Guid { get { return m_data; } }

        private readonly Guid m_data;
        public Id()
        {
            m_data = Guid.NewGuid();
        }

        private Id(Guid data)
        {
            m_data = data;
        }

        public static Id<T> ConvertFrom<U>(Id<U> other)
        {
            return new Id<T>(other.Guid);
        }

        public static Id<T> Parse(string p)
        {
            return new Id<T>(Guid.Parse(p));
        }

        public static Id<T> New()
        {
            return new Id<T>(Guid.NewGuid());
        }

        public override bool Equals(object obj)
        {
            return (obj as Id<T>) == this;
        }

        public override int GetHashCode()
        {
            return m_data.GetHashCode();
        }

        public static bool operator ==(Id<T> a, Id<T> b)
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

        public static bool operator !=(Id<T> a, Id<T> b)
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

        public static Id<T> FromGuid(System.Guid guid)
        {
            return new Id<T>(guid);
        }

        internal static bool TryParse(string value, out Id<T> m_value)
        {
            Guid guid;
            if (Guid.TryParse(value, out guid))
            {
                m_value = new Id<T>(guid);
                return true;
            }
            else
            {
                m_value = null;
                return false;
            }
        }

        public static Id<T> ConvertFrom(ParameterType parameterType)
        {
            return new Id<T>(parameterType.Guid);
        }

        public int CompareTo(Id<T> other)
        {
            if (other == null)
                return -1; //I don't know if this puts nulls at the start or the end but it really doesn't matter
            return m_data.CompareTo(other.m_data);
        }

        public int CompareTo(object obj)
        {
            Id<T> other = obj as Id<T>;
            return m_data.CompareTo(other.m_data);
        }
    }

}
