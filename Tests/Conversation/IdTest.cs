using Conversation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Conversation
{
    public static class IdTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Exists to be used as a type parameter")]
        class A { private A() { } }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Exists to be used as a type parameter")]
        class B { private B() { } }

        [Test]
        public static void Test()
        {
            Id<A> a0 = new Id<A>();
            Id<A> a1 = Id<A>.New();
            Id<A> a2 = Id<A>.New();
            Id<A> a3 = Id<A>.FromGuid(Guid.Parse("bfe1e206-6eb6-4d17-b409-92ccd3145ab6"));
            Id<A> a4 = Id<A>.Parse("bfe1e206-6eb6-4d17-b409-92ccd3145ab6");
            Id<A> a5 = Id<A>.ConvertFrom(ParameterType.Parse("bfe1e206-6eb6-4d17-b409-92ccd3145ab6"));
            Id<A> a6 = Id<A>.ConvertFrom(Id<B>.Parse("bfe1e206-6eb6-4d17-b409-92ccd3145ab6"));

            Assert.That(a0.Guid, Is.Not.EqualTo(a1));
            Assert.That(a1.Guid, Is.Not.EqualTo(a2));
            Assert.That(a2.Guid, Is.Not.EqualTo(a0));
            Assert.That(a0, Is.Not.EqualTo(a1));
            Assert.That(a1, Is.Not.EqualTo(a2));
            Assert.That(a2, Is.Not.EqualTo(a0));
            Assert.That(a0 == a1, Is.False);
            Assert.That(a1 == a2, Is.False);
            Assert.That(a2 == a0, Is.False);
            Assert.That(a0 != a1, Is.True);
            Assert.That(a1 != a2, Is.True);
            Assert.That(a2 != a0, Is.True);
            Assert.That(a0.CompareTo(a1), Is.Not.EqualTo(0));
            Assert.That(a1.CompareTo(a2), Is.Not.EqualTo(0));
            Assert.That(a2.CompareTo(a0), Is.Not.EqualTo(0));
            Assert.That(a3.CompareTo(null), Is.Not.EqualTo(0));

            Assert.That(a3.Guid, Is.EqualTo(Guid.Parse("bfe1e206-6eb6-4d17-b409-92ccd3145ab6")));
            Assert.That(a4.Guid, Is.EqualTo(Guid.Parse("bfe1e206-6eb6-4d17-b409-92ccd3145ab6")));
            Assert.That(a5.Guid, Is.EqualTo(Guid.Parse("bfe1e206-6eb6-4d17-b409-92ccd3145ab6")));
            Assert.That(a6.Guid, Is.EqualTo(Guid.Parse("bfe1e206-6eb6-4d17-b409-92ccd3145ab6")));
            Assert.That(a3.GetHashCode(), Is.EqualTo(a4.GetHashCode()));
            Assert.That(a4.GetHashCode(), Is.EqualTo(a5.GetHashCode()));
            Assert.That(a5.GetHashCode(), Is.EqualTo(a6.GetHashCode()));
            Assert.That(a6.GetHashCode(), Is.EqualTo(a3.GetHashCode()));
            Assert.That(a3, Is.EqualTo(a4));
            Assert.That(a4, Is.EqualTo(a5));
            Assert.That(a5, Is.EqualTo(a6));
            Assert.That(a6, Is.EqualTo(a3));
            Assert.That(a3 == a4, Is.True);
            Assert.That(a4 == a5, Is.True);
            Assert.That(a5 == a6, Is.True);
            Assert.That(a6 == a3, Is.True);
            Assert.That(a3 != a4, Is.False);
            Assert.That(a4 != a5, Is.False);
            Assert.That(a5 != a6, Is.False);
            Assert.That(a6 != a3, Is.False);
            Assert.That(a3.CompareTo(a4), Is.EqualTo(0));
            Assert.That(a4.CompareTo(a5), Is.EqualTo(0));
            Assert.That(a5.CompareTo(a6), Is.EqualTo(0));
            Assert.That(a6.CompareTo(a3), Is.EqualTo(0));

            Assert.That(a0.Serialized, Is.EqualTo(a0.Guid.ToString()));
            Assert.That(a1.Serialized, Is.EqualTo(a1.Guid.ToString()));
            Assert.That(a2.Serialized, Is.EqualTo(a2.Guid.ToString()));
            Assert.That(a3.Serialized, Is.EqualTo(a3.Guid.ToString()));
            Assert.That(a4.Serialized, Is.EqualTo(a4.Guid.ToString()));
            Assert.That(a5.Serialized, Is.EqualTo(a5.Guid.ToString()));
            Assert.That(a6.Serialized, Is.EqualTo(a6.Guid.ToString()));
        }
    }
}
