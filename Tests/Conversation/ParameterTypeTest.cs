using Conversation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Conversation
{
    public static class ParameterTypeTest
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Used as a type parameter marker")]
        class MyType { }

        [Test]
        public static void TestBasic()
        {
            var t1 = ParameterType.Basic.New();
            Assert.That(t1.Guid, Is.Not.EqualTo(default(Guid)));
            Assert.That(t1.IsSet, Is.False);
            Assert.That(t1.Serialized(), Is.EqualTo(t1.Guid.ToString()));

            Guid g2 = Guid.Parse("83c63f88-9445-4b31-98fc-dfe1007e195d");
            var t2 = ParameterType.Basic.ConvertFrom(Id<MyType>.FromGuid(g2));
            Assert.That(t2.Guid, Is.EqualTo(g2));
            Assert.That(t2.IsSet, Is.False);
            Assert.That(t2.Serialized(), Is.EqualTo(t2.Guid.ToString()));

            Guid g3 = g2;
            var t3 = ParameterType.Basic.FromGuid(g3);
            Assert.That(t3.Guid, Is.EqualTo(g3));
            Assert.That(t3.IsSet, Is.False);
            Assert.That(t3.Serialized(), Is.EqualTo(t3.Guid.ToString()));

            Assert.That(t2.Equals(t3), Is.True);
            Assert.That(t3.Equals(t2), Is.True);
            Assert.That(t2.GetHashCode(), Is.EqualTo(t3.GetHashCode()));

            Assert.That(t1.Equals(t2), Is.False);
            Assert.That(t2.Equals(t1), Is.False);

            string g4 = "d1633449-6892-49d1-809b-62a3942bd863";
            var t4 = ParameterType.Basic.Parse(g4);
            Assert.That(t4.Guid, Is.EqualTo(Guid.Parse(g4)));
            Assert.That(t4.IsSet, Is.False);
            Assert.That(t4.Serialized(), Is.EqualTo(t4.Guid.ToString()));

            string g5 = "b5756711-193f-4074-affa-c86c4c00ff71";
            var t5 = ParameterType.Parse(g5);
            Assert.That(t5, Is.TypeOf<ParameterType.Basic>());
            Assert.That(t5.Guid, Is.EqualTo(Guid.Parse(g5)));
            Assert.That(t5.IsSet, Is.False);
            Assert.That(t5.Serialized(), Is.EqualTo(t5.Guid.ToString()));
        }

        [Test]
        public static void TestSet()
        {
            var t1 = ParameterType.ValueSetType.New();

            Guid g2 = Guid.Parse("260d3776-6365-481f-a083-64e75c00b2d7");
            var t2 = ParameterType.ValueSetType.ConvertFrom(Id<MyType>.FromGuid(g2));

            Guid g3 = g2;
            var t3 = ParameterType.ValueSetType.FromGuid(g3);

            ParameterType g4 = ParameterType.Parse("3be8f941-214c-4a40-86ce-45b23c21c778");
            var t4 = ParameterType.ValueSetType.Of(g4);

            string g5 = "daf8c625-4907-4a9b-be42-45a918a62575";
            var t5 = ParameterType.ValueSetType.Parse("set:" + g5);

            string g6 = g5;
            var t6 = ParameterType.Parse("set:" + g5);

            Assert.That(t1, Is.TypeOf<ParameterType.ValueSetType>());
            Assert.That(t2, Is.TypeOf<ParameterType.ValueSetType>());
            Assert.That(t3, Is.TypeOf<ParameterType.ValueSetType>());
            Assert.That(t4, Is.TypeOf<ParameterType.ValueSetType>());
            Assert.That(t5, Is.TypeOf<ParameterType.ValueSetType>());
            Assert.That(t6, Is.TypeOf<ParameterType.ValueSetType>());

            Assert.That(t1.IsSet, Is.True);
            Assert.That(t2.IsSet, Is.True);
            Assert.That(t3.IsSet, Is.True);
            Assert.That(t4.IsSet, Is.True);
            Assert.That(t5.IsSet, Is.True);
            Assert.That(t6.IsSet, Is.True);

            Assert.That(t2.Equals(t3), Is.True);
            Assert.That(t3.Equals(t2), Is.True);
            Assert.That(t2.GetHashCode(), Is.EqualTo(t3.GetHashCode()));
            Assert.That(t3.Equals(t4), Is.False);
            Assert.That(t4.Equals(t3), Is.False);

            Assert.That(t1.Guid, Is.Not.EqualTo(default(Guid)));
            Assert.That(t2.Guid, Is.EqualTo(g2));
            Assert.That(t3.Guid, Is.EqualTo(g3));
            Assert.That(t4.Guid, Is.EqualTo(g4.Guid));
            Assert.That(t5.Guid, Is.EqualTo(Guid.Parse(g5)));
            Assert.That(t6.Guid, Is.EqualTo(Guid.Parse(g6)));

            Assert.That(t1.Serialized(), Is.EqualTo("set:" + t1.Guid.ToString()));
            Assert.That(t2.Serialized(), Is.EqualTo("set:" + t2.Guid.ToString()));
            Assert.That(t3.Serialized(), Is.EqualTo("set:" + t3.Guid.ToString()));
            Assert.That(t4.Serialized(), Is.EqualTo("set:" + t4.Guid.ToString()));
            Assert.That(t5.Serialized(), Is.EqualTo("set:" + t5.Guid.ToString()));
            Assert.That(t6.Serialized(), Is.EqualTo("set:" + t6.Guid.ToString()));
        }
    }
}
