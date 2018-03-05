using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Utilities;

namespace Tests
{
    class TestPermutation
    {
        static int Encode(int[] data)
        {
            int result = 0;
            int dec = 1;
            for (int i = 0; i < data.Length; i++)
            {
                result += data[i] * dec;
                dec *= 10;
            }
            return result;
        }

        const int SIZE = 7;
        static string ENCODING { get; } = new string(Enumerable.Repeat('0', SIZE).ToArray());

        [Test]
        public static void Test()
        {
            int Factorial(int n)
            {
                if (n == 1)
                    return 1;
                return Factorial(n - 1) * n;
            }

            int[] data = Enumerable.Range(0, SIZE).Reverse().ToArray();

            void Write(int[] x)
            {
                Console.WriteLine(Encode(x).ToString(ENCODING));
            }

            var results = Permutation<int>.Permute(data.ToList()).Select(x => x.ToArray()).ToList();
            foreach (var result in results)
            {
                Write(result);
            }

            Console.Out.WriteLine("Should have: " + Factorial(SIZE));
            Console.Out.WriteLine("Have: " + results.Count);
            Console.Out.WriteLine("Distinct: " + results.Distinct().Count());

            Assert.That(results.Count, Is.EqualTo(Factorial(SIZE)));
        }
    }
}
