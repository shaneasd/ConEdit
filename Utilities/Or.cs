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

        public static Func<Null> Func { get; } = () => null;
    }

    public static class Either
    {
        public static Either<T, U> Create<T, U>(bool useA, Func<T> aGenerator, Func<U> bGenerator)
        {
            return new Either<T, U>(useA, aGenerator, bGenerator);
        }

        public static Tuple<IEnumerable<T>, IEnumerable<U>> Split<T, U>(IEnumerable<Either<T, U>> data)
        {
            List<T> l1 = new List<T>();
            List<U> l2 = new List<U>();
            foreach (var d in data)
            {
                d.Do(a => l1.Add(a), b => l2.Add(b));
            }
            return new Tuple<IEnumerable<T>, IEnumerable<U>>(l1, l2);
        }

        public class UpCaster<I>
        {
            public I Cast<T, U>(Either<T, U> either) where T : I where U : I
            {
                return either.Transformed<I>(a => a, a => a);
            }
        }

        public static UpCaster<I> UpCast<I>()
        {
            return new UpCaster<I>();
        }
    }

    public class Either<T, U>
    {
        readonly bool m_aSpecified;
        readonly T A;
        readonly U B;

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
            var other = obj as Either<T, U>;
            if (other != null)
            {
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

        public Either<V, W> TransformedEither<V, W>(Func<T, V> a, Func<U, W> b)
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
