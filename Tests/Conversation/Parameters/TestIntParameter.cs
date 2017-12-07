using Conversation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Tests.Conversation.Parameters
{
    public static class TestIntParameter
    {
        [Test]
        public static void TestBasicConstruction()
        {
            string name = "nameasd";
            Id<Parameter> id = Id<Parameter>.Parse("C6AE15FF-0242-4289-AADD-F9F71F4CFEBB");
            ParameterType type = ParameterType.Parse("86EDCBA3-7807-4DD5-B0E8-0769C52BA0EB");
            {
                IntegerParameter.Definition definition = new IntegerParameter.Definition(min: null, max: null);
                IntegerParameter p = new IntegerParameter(name, id, type, definition, null);
                Assert.That(p.Id, Is.EqualTo(id));
                Assert.That(p.Name, Is.EqualTo(name));
                Assert.That(p.TypeId, Is.EqualTo(type));
                Assert.That(p.Min, Is.EqualTo(int.MinValue));
                Assert.That(p.Max, Is.EqualTo(int.MaxValue));
            }
            {
                IntegerParameter.Definition definition = new IntegerParameter.Definition(min: null, max: 45);
                IntegerParameter p = new IntegerParameter(name, id, type, definition, null);
                Assert.That(p.Id, Is.EqualTo(id));
                Assert.That(p.Name, Is.EqualTo(name));
                Assert.That(p.TypeId, Is.EqualTo(type));
                Assert.That(p.Min, Is.EqualTo(int.MinValue));
                Assert.That(p.Max, Is.EqualTo(45));
            }
            {
                IntegerParameter.Definition definition = new IntegerParameter.Definition(min: -1232, max: null);
                IntegerParameter p = new IntegerParameter(name, id, type, definition, null);
                Assert.That(p.Id, Is.EqualTo(id));
                Assert.That(p.Name, Is.EqualTo(name));
                Assert.That(p.TypeId, Is.EqualTo(type));
                Assert.That(p.Min, Is.EqualTo(-1232));
                Assert.That(p.Max, Is.EqualTo(int.MaxValue));
            }
            {
                IntegerParameter.Definition definition = new IntegerParameter.Definition(min: -34985, max: 34986);
                IntegerParameter p = new IntegerParameter(name, id, type, definition, null);
                Assert.That(p.Id, Is.EqualTo(id));
                Assert.That(p.Name, Is.EqualTo(name));
                Assert.That(p.TypeId, Is.EqualTo(type));
                Assert.That(p.Min, Is.EqualTo(-34985));
                Assert.That(p.Max, Is.EqualTo(34986));
            }
        }

        [Test]
        public static void TestNullDefault()
        {
            string name = "nameasd";
            Id<Parameter> id = Id<Parameter>.Parse("C6AE15FF-0242-4289-AADD-F9F71F4CFEBB");
            ParameterType type = ParameterType.Parse("86EDCBA3-7807-4DD5-B0E8-0769C52BA0EB");
            IntegerParameter.Definition definition = new IntegerParameter.Definition(null, null);
            IntegerParameter p = new IntegerParameter(name, id, type, definition, null);
            Assert.That(p.Corrupted, Is.True);
            p.SetValueAction(1).Value.Redo();
            TestParameterIs(p, 1);
        }

        [NUnit.Framework.Test]
        public static void Test()
        {
            string name = "nameasd";
            Id<Parameter> id = Id<Parameter>.Parse("C6AE15FF-0242-4289-AADD-F9F71F4CFEBB");
            ParameterType type = ParameterType.Parse("86EDCBA3-7807-4DD5-B0E8-0769C52BA0EB");
            IntegerParameter.Definition definition = new IntegerParameter.Definition(-100, 100);
            IntegerParameter p = new IntegerParameter(name, id, type, definition, "2");
            TestParameterIs(p, 2);

            //Test range checking
            Assert.That(() => p.SetValueAction(-101), Throws.ArgumentException);
            TestParameterIs(p, 2);
            Assert.That(() => p.SetValueAction(101), Throws.ArgumentException);
            TestParameterIs(p, 2);

            //Test undo/redo
            var actions = p.SetValueAction(12);
            Assert.That(actions, Is.Not.Null);
            TestParameterIs(p, 2);
            actions.Value.Redo();
            TestParameterIs(p, 12);
            actions.Value.Undo();
            TestParameterIs(p, 2);

            p.TryDeserialiseValue("shane");
            TestCorruptParameter(p, "shane");

            //Test what happens if we undo back to an invalid state
            var actions2 = p.SetValueAction(1);
            actions2.Value.Redo();
            TestParameterIs(p, 1);
            actions2.Value.Undo();
            TestCorruptParameter(p, "shane");

            p.TryDeserialiseValue("54");
            TestParameterIs(p, 54);
        }

        private static void TestCorruptParameter(IntegerParameter p, string valueAsString)
        {
            Assert.That(p.Corrupted, Is.True);
            Assert.That(p.ValueAsString(), Is.EqualTo(valueAsString));
        }

        private static void TestParameterIs(IntegerParameter p, int val)
        {
            Assert.That(p.Value, Is.EqualTo(val));
            int result;
            bool isInt;
            Assert.That(p.Corrupted, Is.False);
            Assert.That(p.ValueAsString(), Is.EqualTo(val.ToString(CultureInfo.InvariantCulture))); //Tempting to simply verify it will parse as the correct value but we need to be consistent with existing files
            using (TemporaryCulture.English())
            {
                isInt = int.TryParse(p.DisplayValue((a, b) => ""), out result);
                Assert.That(isInt);
                if (isInt)
                    Assert.That(result, Is.EqualTo(val));
            }
            using (TemporaryCulture.European())
            {
                isInt = int.TryParse(p.DisplayValue((a, b) => ""), out result);
                Assert.That(isInt);
                if (isInt)
                    Assert.That(result, Is.EqualTo(val));
            }
        }
    }
}
