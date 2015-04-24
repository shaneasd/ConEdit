using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public static class UnorderedTuple
    {
        public static UnorderedTuple2<T> Make<T>(T a, T b)
        {
            return new UnorderedTuple2<T>(a, b);
        }
    }

    public class UnorderedTuple2<T> : IEnumerable<T>
    {
        public readonly T Item1;
        public readonly T Item2;
        public UnorderedTuple2(T a, T b)
        {
            Item1 = a;
            Item2 = b;
        }

        public override bool Equals(object obj)
        {
            UnorderedTuple2<T> other = obj as UnorderedTuple2<T>;
            if (other == null)
                return false;
            if (object.Equals(Item1, other.Item1) && object.Equals(Item2, other.Item2))
                return true;
            if (object.Equals(Item1, other.Item2) && object.Equals(Item2, other.Item1))
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return Item1.GetHashCode() ^ Item2.GetHashCode();
        }

        public bool Contains(T val)
        {
            return object.Equals(Item1, val) || object.Equals(Item2, val);
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return Item1;
            yield return Item2;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            yield return Item1;
            yield return Item2;
        }

        public override string ToString()
        {
            return Item1.ToString() + Item2.ToString();
        }
    }
}
