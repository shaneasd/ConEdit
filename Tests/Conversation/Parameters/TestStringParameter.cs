using Conversation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Conversation.Parameters
{
    public static class TestStringParameter
    {
        [Test]
        public static void TestNull()
        {
            string name = "asdasdf";
            Id<Parameter> id = Id<Parameter>.Parse("1940BAB7-3A0C-435E-8581-7E180D37053A");
            StringParameter p = new StringParameter(name, id, null);
            CheckConstruction(name, id, p);

            TestParameterIs(p, null);

            var action = p.SetValueAction("a");
            Assert.That(action, Is.Not.Null);
            action.Value.Redo();
            TestParameterIs(p, "a");

            Assert.Throws<ArgumentException>(() => p.SetValueAction(null));

            //Test what happens if we undo back to an invalid state
            action.Value.Undo();
            TestParameterIs(p, null);
        }

        private static void CheckConstruction(string name, Id<Parameter> id, StringParameter p)
        {
            Assert.That(p.Id, Is.EqualTo(id));
            Assert.That(p.Name, Is.EqualTo(name));
            Assert.That(p.TypeId, Is.EqualTo(StringParameter.ParameterType));
        }

        [Test]
        public static void Test()
        {
            string name = "stringname";
            Id<Parameter> id = Id<Parameter>.Parse("5096B7CD-91A9-4EF3-A0BA-FD64E90D2A8F");
            StringParameter p = new StringParameter(name, id, "asd");
            CheckConstruction(name, id, p);
            TestParameterIs(p, "asd");

            Assert.That(p.SetValueAction("asd"), Is.Null);

            var set = p.SetValueAction("dsa");
            TestParameterIs(p, "asd");
            set.Value.Redo();
            TestParameterIs(p, "dsa");
            set.Value.Undo();
            TestParameterIs(p, "asd");

            p.TryDeserialiseValue("shane");
            TestParameterIs(p, "shane");

            Assert.Throws<ArgumentNullException>(() => p.TryDeserialiseValue(null));
        }

        private static void TestParameterIs(StringParameter p, string val)
        {
            Assert.That(p.Corrupted, Is.EqualTo(val == null));
            Assert.That(p.Value, Is.EqualTo(val));
            Assert.That(p.ValueAsString(), Is.EqualTo(val));
            Assert.That(p.DisplayValue(a => ""), Is.EqualTo(val));
        }
    }
}
