using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class Permutation<T>
    {
        public static IEnumerable<IList<T>> Permute(IList<T> data)
        {
            yield return data;
            Permutation<T> p = new Permutation<T>(data);
            int k = data.Count;
            if (k > 1)
            {
                if (k % 2 == 0)
                {
                    foreach (var x in p.KEven(k))
                        yield return x;
                }
                else
                {
                    foreach (var x in p.KOdd(k))
                        yield return x;
                }
            }
        }

        public static IEnumerable<IList<T>> Random(IList<T> data, int count)
        {
            var r = new System.Random(0);
            yield return data;
            for (; count > 1; count--)
            {
                Collection.Randomise(data, r);
                yield return data;
            }
        }

        IList<T> m_data;
        private Permutation(IList<T> data)
        {
            m_data = data;
        }

        IList<T> Swap(int i, int j)
        {
            T temp = m_data[i];
            m_data[i] = m_data[j];
            m_data[j] = temp;
            return m_data;
        }

        IEnumerable<IList<T>> KEven(int k)
        {
            if (k == 2)
                yield return Swap(0, 1);
            else
            {
                for (int i = k - 1; i > 0; i--)
                {
                    foreach (var x in KOdd(k - 1))
                        yield return x;
                    yield return Swap(i, i - 1);
                }
                foreach (var x in KOdd(k - 1))
                    yield return x;
            }
        }

        IEnumerable<IList<T>> KOdd(int k)
        {
            for (int i = 0; i < k - 1; i++)
            {
                foreach (var x in KEven(k - 1))
                    yield return x;
                yield return Swap(0, k - 1);
            }
            foreach (var x in KEven(k - 1))
                yield return x;
        }
    }
}
