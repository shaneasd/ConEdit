using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Tests
{
    public static class TestReplace
    {
        private static List<int> GenerateTestData()
        {
            return new List<int> { 1, 2, 3, 1, 3, 7 };
        }

        [NUnit.Framework.Test]
        public static void TestReplaceOnce()
        {
            var data = GenerateTestData();
            var replace1Once = data.ReplaceOnce(a=> a == 1, a=> 10);
            var replace2Once = data.ReplaceOnce(a => a == 2, a => 20);
            var replace3Once = data.ReplaceOnce(a => a == 3, a => 300);
            var replace7Once = data.ReplaceOnce(a => a == 7, a => 1000);
            var replace5Once = data.ReplaceOnce(a => a == 5, a => 1);

            CollectionAssert.AreEqual(new[] { 10, 2, 3, 1, 3, 7 }, replace1Once);
            CollectionAssert.AreEqual(new[] { 1, 20, 3, 1, 3, 7 }, replace2Once);
            CollectionAssert.AreEqual(new[] { 1, 2, 300, 1, 3, 7 }, replace3Once);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 1, 3, 1000 }, replace7Once);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 1, 3, 7 }, replace5Once);
        }

        [NUnit.Framework.Test]
        public static void TestReplaceMany()
        {
            var data = GenerateTestData();
            var replace1 = data.Replace(a => a == 1, a => 10);
            var replace2 = data.Replace(a => a == 2, a => 20);
            var replace3 = data.Replace(a => a == 3, a => 300);
            var replace7 = data.Replace(a => a == 7, a => 1000);
            var replace5 = data.Replace(a => a == 5, a => 1);

            CollectionAssert.AreEqual(new[] { 10, 2, 3, 10, 3, 7 }, replace1);
            CollectionAssert.AreEqual(new[] { 1, 20, 3, 1, 3, 7 }, replace2);
            CollectionAssert.AreEqual(new[] { 1, 2, 300, 1, 300, 7 }, replace3);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 1, 3, 1000 }, replace7);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 1, 3, 7 }, replace5);
        }
    }
}
