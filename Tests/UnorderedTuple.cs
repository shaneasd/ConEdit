using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Utilities;

namespace Tests
{
    public static class UnorderedTupleTest
    {
        [NUnit.Framework.Test]
        public static void TestUnorderedTuple()
        {
            var a = UnorderedTuple.Make(0, 1);
            var A = UnorderedTuple.Make(1, 0);
            var b = UnorderedTuple.Make(0, 2);

            Assert.AreEqual(a, A);
            Assert.AreEqual(A, a);
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(b, a);
            Assert.AreNotEqual(A, b);
            Assert.AreNotEqual(b, A);
        }
    }
}
