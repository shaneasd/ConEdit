using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public sealed class Null
    {
        private Null()
        {
        }

        public static readonly Func<Null> Func = () => null;
    }

    public static class Either
    {
        public static Either<T, U> Create<T,U>(bool useA, Func<T> aGenerator, Func<U> bGenerator)
        {
            return new Either<T, U>(useA, aGenerator, bGenerator);
        }
    }

    public class Either<T, U>
    {
        bool m_aSpecified;

        public Either(T a)
        {
            m_aSpecified = true;
            A = a;
            B = default(U);
        }
        public Either(U b)
        {
            m_aSpecified = false;
            A = default(T);
            B = b;
        }
        readonly T A;
        readonly U B;

        public Either(bool useA, Func<T> aGenerator, Func<U> bGenerator)
        {
            m_aSpecified = useA;
            if (useA)
            {
                A = aGenerator();
                B = default(U);
            }
            else
            {
                A = default(T);
                B = bGenerator();
            }
        }

        public static implicit operator Either<T, U>(T a)
        {
            return new Either<T, U>(a);
        }

        public static implicit operator Either<T, U>(U b)
        {
            return new Either<T, U>(b);
        }

        public override int GetHashCode()
        {
            return m_aSpecified ? A.GetHashCode() : B.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Either<T, U>)
            {
                var other = (Either<T, U>)obj;
                if (m_aSpecified != other.m_aSpecified)
                    return false;
                else if (m_aSpecified)
                    return object.Equals(other.A, A);
                else
                    return object.Equals(other.B, B);
            }

            if (m_aSpecified)
                return object.Equals(obj, A);
            else
                return object.Equals(obj, B);
        }

        public static bool operator ==(Either<T, U> a, T b)
        {
            return object.Equals(a, b);
        }
        public static bool operator ==(Either<T, U> a, U b)
        {
            return object.Equals(a, b);
        }
        public static bool operator ==(T b, Either<T, U> a)
        {
            return object.Equals(a, b);
        }
        public static bool operator ==(U b, Either<T, U> a)
        {
            return object.Equals(a, b);
        }

        public static bool operator !=(Either<T, U> a, T b)
        {
            return !object.Equals(a, b);
        }
        public static bool operator !=(Either<T, U> a, U b)
        {
            return !object.Equals(a, b);
        }
        public static bool operator !=(T b, Either<T, U> a)
        {
            return !object.Equals(a, b);
        }
        public static bool operator !=(U b, Either<T, U> a)
        {
            return !object.Equals(a, b);
        }

        public V Transformed<V>(Func<T, V> a, Func<U, V> b)
        {
            if (m_aSpecified)
                return a(A);
            else
                return b(B);
        }

        public Either<V,W> TransformedEither<V, W>(Func<T, V> a, Func<U, W> b)
        {
            if (m_aSpecified)
                return a(A);
            else
                return b(B);
        }

        public void Do(Action<T> a, Action<U> b)
        {
            if (m_aSpecified)
                a.Execute(A);
            else
                b.Execute(B);
        }
    }
}
