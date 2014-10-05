using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Tests
{
    partial class Program
    {
        public static void TestWeakEvent()
        {
            object a = new object();
            bool w1Executed = false;
            WeakCallback w1 = new WeakCallback<object>(a, A => { w1Executed = true; });
            w1.Execute();
            Assert(w1Executed);
            w1Executed = false;
            a = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            w1.Execute();
            Assert(!w1Executed);

            object b = new object();
            bool w2Executed = false;
            WeakCallback w2 = new WeakCallback<object>(b, A => { w2Executed = true; });

            WeakEvent e = new WeakEvent();
            e.Register(w1);
            e.Register(w2);
            e.Execute();
            Assert(w2Executed);
        }
    }
}
