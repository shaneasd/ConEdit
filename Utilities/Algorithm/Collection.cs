﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public static void ForAll<T>(this IEnumerable<T> e, Action<T> a)
        {
            e.ToList().ForEach(a);
        }

        public static IEnumerable<U> Collapse<T, U>(this T root, Func<T, IEnumerable<T>> subNodeSelector, Func<T, IEnumerable<U>> valueSelector)
        {
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

        public static List<T> Merge<T>(this IEnumerable<T> data, Func<T, T, Tuple<bool,T>> tryMerge)
        {
            List<T> d = data.ToList();

            for (int i = 0; i < d.Count; i++)
            {
                for (int j = i+1; j < d.Count; j++)
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
                if (object.Equals(search,test))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }

        public static int IndexOf<T>(this IEnumerable<T> data, Func<T,bool> search)
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
            while (true)
            {
                foreach (var element in elements)
                    yield return element;
            }
        }

        public static IEnumerable<T> Except<T, U, V>(this IEnumerable<T> first, IEnumerable<U> second, Func<T, V> Tkey, Func<U, V> Ukey)
        {
            var secondKeys = second.Select(Ukey).Evaluate();
            foreach (var t in first)
            {
                if (!secondKeys.Contains(Tkey(t)))
                    yield return t;
            }
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> data)
        {
            return new HashSet<T>(data);
        }

        public static void BringToFront<T>(this List<T> list, IEnumerable<T> shifted)
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
    }
}
