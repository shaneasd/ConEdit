using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using NUnit.Framework;

namespace Tests
{
    public static partial class Program
    {
        [NUnit.Framework.Test]
        public static void TestWeakEvent()
        {
            object a = new object();
            bool w1Executed = false;
            int val = 0;
            WeakCallback<int> w1 = new WeakCallback<object, int>(a, (A, i) => { w1Executed = true; val = i; });
            w1.Execute(1);
            Assert.True(w1Executed);
            Assert.AreEqual(1, val);
            w1Executed = false;
            a = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            w1.Execute(2);
            Assert.AreEqual(1, val);
            Assert.False(w1Executed);

            object b = new object();
            bool w2Executed = false;
            WeakCallback<int> w2 = new WeakCallback<object, int>(b, (A, i) => { w2Executed = true; val = i; });

            WeakEvent<int> e = new WeakEvent<int>();
            e.Register(w1);
            e.Register(w2);
            e.Execute(3);
            Assert.True(w2Executed);
        }
    }
}
