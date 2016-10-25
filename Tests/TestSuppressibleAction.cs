using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using NUnit.Framework;

namespace Tests
{
    public static class TestSuppressibleAction
    {
        [NUnit.Framework.Test]
        public static void TestZeroSuppressions()
        {
            bool triggered = false;
            SuppressibleAction action = new SuppressibleAction(() => { triggered = true; });
            {
                Assert.False(action.Suppressed);
                var executed = action.TryExecute();
                Assert.True(executed);
                Assert.True(triggered);
            }
        }

        [NUnit.Framework.Test]
        public static void TestOneSuppression()
        {
            bool triggered = false;
            SuppressibleAction action = new SuppressibleAction(() => { triggered = true; });
            {
                using (action.SuppressCallback())
                {
                    Assert.True(action.Suppressed);
                    var executed = action.TryExecute();
                    Assert.False(executed);
                    Assert.False(triggered);
                }
                Assert.True(triggered);
                Assert.False(action.Suppressed);
            }
        }

        [NUnit.Framework.Test]
        public static void TestManySuppression()
        {
            bool triggered = false;
            SuppressibleAction action = new SuppressibleAction(() => { triggered = true; });
            {
                using (action.SuppressCallback())
                {
                    using (action.SuppressCallback())
                    {
                        Assert.True(action.Suppressed);
                        var executed = action.TryExecute();
                        Assert.False(executed);
                        Assert.False(triggered);
                    }
                    Assert.False(triggered);
                    Assert.True(action.Suppressed);
                }
                Assert.True(triggered);
                Assert.False(action.Suppressed);
            }
        }

        //[NUnit.Framework.Test]
        //public static void TestDisposal()
        //{
        //    bool triggered = false;
        //    SuppressibleAction action = new SuppressibleAction(() => { triggered = true; });
        //    try
        //    {
        //        using (action.SuppressCallback())
        //        {
        //            action.TryExecute();
        //            action.Dispose();
        //        }
        //    }
        //    finally
        //    {
        //        action.Dispose();
        //    }
        //    Assert.False(triggered);
        //}
    }
}
