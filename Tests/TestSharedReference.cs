using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Tests
{
    public static class TestSharedReference
    {
        class GenericDisposable : Disposable
        {
            private Action m_dispose;

            public GenericDisposable(Action dispose)
            {
                m_dispose = dispose;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    m_dispose();
            }
        }

        [NUnit.Framework.Test]
        public static void Test()
        {
            int called = 0;
            Action dispose = () => called++;

            //Make sure the mock IDisposable class works the way it's supposed to
            using (new GenericDisposable(dispose))
            {
                Assert.That(called, Is.EqualTo(0));
            }
            Assert.That(called, Is.EqualTo(1));
            called = 0;

            SharedResource<GenericDisposable> s1 = null;
            SharedResource<GenericDisposable> s2 = null;
            try
            {
                s1 = new SharedResource<GenericDisposable>(() => new GenericDisposable(dispose));
                Assert.That(called, Is.EqualTo(0));
                s2 = s1.Copy();
                Assert.That(called, Is.EqualTo(0));
                s1.Dispose();
                s1 = null;
                Assert.That(called, Is.EqualTo(0));
                s2.Dispose();
                s2 = null;
                Assert.That(called, Is.EqualTo(1));
            }
            finally
            {
                if (s1 != null)
                    s1.Dispose();
                if (s2 != null)
                    s2.Dispose();
            }
        }
    }
}