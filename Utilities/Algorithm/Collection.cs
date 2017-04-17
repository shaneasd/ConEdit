using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Utilities
{
    public static class Collection
    {
        public static IEnumerable<T> Concat<T>(params IEnumerable<T>[] collections)
        {
            return collections.Aggregate(Enumerable.Empty<T>(), (a, b) => a.Concat(b));
        }

        public static IEnumerable<T> Evaluate<T>(this IEnumerable<T> collection)
        {
            return collection.ToList();
        }

        private class OneElementCollection<T> : ICollection<T>
        {
            private T m_element;
            public OneElementCollection(T element)
            {
                m_element = element;
            }

            public void Add(T item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(T item)
            {
                return object.Equals(m_element, item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                if (array == null)
                    throw new ArgumentNullException(nameof(array));
                array[arrayIndex] = m_element;
            }

            public int Count
            {
                get { return 1; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public bool Remove(T item)
            {
                throw new NotSupportedException();
            }

            public IEnumerator<T> GetEnumerator()
            {
                yield return m_element;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public static ICollection<T> Only<T>(this T a)
        {
            return new OneElementCollection<T>(a);
        }

        public static void ForAll<T>(this IEnumerable<T> data, Action<T> action)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            foreach (var x in data)
                action(x);
        }

        public static IEnumerable<U> Collapse<T, U>(this T root, Func<T, IEnumerable<T>> subNodeSelector, Func<T, IEnumerable<U>> valueSelector)
        {
            if (valueSelector == null)
                throw new ArgumentNullException(nameof(valueSelector));
            return valueSelector(root).Concat(subNodeSelector(root).SelectMany(a => a.Collapse(subNodeSelector, valueSelector)));
        }

        public static U SingleOrDefault<T, U>(this IEnumerable<T> data, Func<T, bool> predicate, Func<T, U> selector, U def)
        {
            if (data.Where(predicate).Any())
                return selector(data.Where(predicate).Single());
            else
                return def;
        }

        public static U FirstOrDefault<T, U>(this IEnumerable<T> data, Func<T, bool> predicate, Func<T, U> selector, U def)
        {
            if (data.Where(predicate).Any())
                return selector(data.Where(predicate).First());
            else
                return def;
        }

        public static IEnumerable<int> Range(int start)
        {
            for (; ; start++)
                yield return start;
        }

        public static IEnumerable<Tuple<T, T>> Pairs<T>(this IEnumerable<T> a)
        {
            var aa = a.ToArray();
            int count = aa.Length;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                {
                    if (i != j)
                        yield return Tuple.Create(aa[i], aa[j]);
                }
            }
        }

        public static IEnumerable<T> Merge<T>(this IEnumerable<T> data, Func<T, T, Tuple<bool, T>> tryMerge)
        {
            List<T> d = data.ToList();

            for (int i = 0; i < d.Count; i++)
            {
                for (int j = i + 1; j < d.Count; j++)
                {
                    var m = tryMerge(d[i], d[j]);
                    if (m.Item1)
                    {
                        d[i] = m.Item2;
                        d.RemoveAt(j);
                        j--;
                    }
                }
            }

            return d;
        }

        public static int IndexOf<T>(this IEnumerable<T> data, T search)
        {
            int i = 0;
            foreach (var test in data)
            {
                if (object.Equals(search, test))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        public static int IndexOf<T>(this IEnumerable<T> data, Func<T, bool> search)
        {
            int i = 0;
            foreach (var test in data)
            {
                if (search(test))
                    return i;
                i++;
            }
            return -1;
        }

        public static IEnumerable<T> InfiniteRepeat<T>(this IEnumerable<T> elements)
        {
            if (!elements.Any())
                throw new ArgumentException("input must contain at least one element to repeat", "elements");
            while (true)
            {
                foreach (var element in elements)
                    yield return element;
            }
        }

        public static IEnumerable<T> Except<T, U, V>(this IEnumerable<T> first, IEnumerable<U> second, Func<T, V> keyFirst, Func<U, V> keySecond)
        {
            var secondKeys = second.Select(keySecond).Evaluate();
            foreach (var t in first)
            {
                if (!secondKeys.Contains(keyFirst(t)))
                    yield return t;
            }
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> data)
        {
            return new HashSet<T>(data);
        }

        public static void BringToFront<T>(this System.Collections.ObjectModel.Collection<T> list, IEnumerable<T> shifted)
        {
            List<T> toInsert = list.Intersect(shifted).Reverse().ToList();
            foreach (var a in toInsert)
                list.Remove(a);
            foreach (var a in toInsert)
                list.Insert(0, a);
        }

        public static T Best<T>(this IEnumerable<T> data, Func<T, T, bool> firstOperandBetter)
        {
            T best = data.First();
            foreach (var d in data.Skip(1))
            {
                if (!firstOperandBetter(best, d))
                    best = d;
            }
            return best;
        }

        public static IEnumerable<T> TakeUpTo<T>(this IEnumerable<T> data, int count)
        {
            var e = data.GetEnumerator();
            for (int i = 0; e.MoveNext() && i < count; i++)
            {
                yield return e.Current;
            }
        }

        public static T LookupOrDefault<U, T>(this IDictionary<U, T> data, U key, T def)
        {
            if (data.ContainsKey(key))
                return data[key];
            else
                return def;
        }

        public static bool CountEquals<T>(this IEnumerable<T> data, int value)
        {
            int count = 0;
            for (var e = data.GetEnumerator(); e.MoveNext() && count < value + 1;)
                count++;
            return count == value;
        }

        private static IEnumerable<T> ReplaceImplementation<T>(IEnumerable<T> data, Func<T, bool> condition, Func<T, T> replacement, bool once)
        {
            bool got = false;
            foreach (var d in data)
            {
                if (got && once)
                {
                    yield return d;
                }
                else if (condition(d))
                {
                    yield return replacement(d);
                    got = true;
                }
                else
                {
                    yield return d;
                }
            }
        }

        public static IEnumerable<T> Replace<T>(this IEnumerable<T> data, Func<T, bool> condition, Func<T, T> replacement)
        {
            return ReplaceImplementation(data, condition, replacement, false);
        }

        public static IEnumerable<T> ReplaceOnce<T>(this IEnumerable<T> data, Func<T, bool> condition, Func<T, T> replacement)
        {
            return ReplaceImplementation(data, condition, replacement, true);
        }

        public static IEnumerable<T> SelectTwo<T, U>(this IEnumerable<U> data, Func<U, T> a, Func<U, T> b)
        {
            foreach (var element in data)
            {
                yield return a(element);
                yield return b(element);
            }
        }

        public static void CopyTo<T>(this IEnumerable<T> data, T[] sink, int start)
        {
            foreach (var element in data)
            {
                sink[start] = element;
                start++;
            }
        }
    }
}
