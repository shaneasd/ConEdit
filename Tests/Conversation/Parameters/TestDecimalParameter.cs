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
    public static class TestDecimalParameter
    {
        [Test]
        public static void TestBasicConstruction()
        {
            string name = "nameasd";
            Id<Parameter> id = Id<Parameter>.Parse("C6AE15FF-0242-4289-AADD-F9F71F4CFEBB");
            ParameterType type = ParameterType.Parse("86EDCBA3-7807-4DD5-B0E8-0769C52BA0EB");
            {
                DecimalParameter.Definition definition = new DecimalParameter.Definition(min: null, max: null);
                DecimalParameter p = new DecimalParameter(name, id, type, definition, null);
                Assert.That(p.Id, Is.EqualTo(id));
                Assert.That(p.Name, Is.EqualTo(name));
                Assert.That(p.TypeId, Is.EqualTo(type));
                Assert.That(p.Min, Is.EqualTo(decimal.MinValue));
                Assert.That(p.Max, Is.EqualTo(decimal.MaxValue));
            }
            {
                DecimalParameter.Definition definition = new DecimalParameter.Definition(min: null, max: 45.23m);
                DecimalParameter p = new DecimalParameter(name, id, type, definition, null);
                Assert.That(p.Id, Is.EqualTo(id));
                Assert.That(p.Name, Is.EqualTo(name));
                Assert.That(p.TypeId, Is.EqualTo(type));
                Assert.That(p.Min, Is.EqualTo(decimal.MinValue));
                Assert.That(p.Max, Is.EqualTo(45.23m));
            }
            {
                DecimalParameter.Definition definition = new DecimalParameter.Definition(min: -1232.3m, max: null);
                DecimalParameter p = new DecimalParameter(name, id, type, definition, null);
                Assert.That(p.Id, Is.EqualTo(id));
                Assert.That(p.Name, Is.EqualTo(name));
                Assert.That(p.TypeId, Is.EqualTo(type));
                Assert.That(p.Min, Is.EqualTo(-1232.3m));
                Assert.That(p.Max, Is.EqualTo(decimal.MaxValue));
            }
            {
                DecimalParameter.Definition definition = new DecimalParameter.Definition(min: -34985.412m, max: 34986.34m);
                DecimalParameter p = new DecimalParameter(name, id, type, definition, null);
                Assert.That(p.Id, Is.EqualTo(id));
                Assert.That(p.Name, Is.EqualTo(name));
                Assert.That(p.TypeId, Is.EqualTo(type));
                Assert.That(p.Min, Is.EqualTo(-34985.412m));
                Assert.That(p.Max, Is.EqualTo(34986.34m));
            }
        }

        [Test]
        public static void TestNullDefault()
        {
            string name = "nameasd";
            Id<Parameter> id = Id<Parameter>.Parse("C6AE15FF-0242-4289-AADD-F9F71F4CFEBB");
            ParameterType type = ParameterType.Parse("86EDCBA3-7807-4DD5-B0E8-0769C52BA0EB");
            DecimalParameter.Definition definition = new DecimalParameter.Definition(null, null);
            DecimalParameter p = new DecimalParameter(name, id, type, definition, null);
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
            DecimalParameter.Definition definition = new DecimalParameter.Definition(-100, 100);
            DecimalParameter p = new DecimalParameter(name, id, type, definition, "2");
            TestParameterIs(p, 2);

            //Test range checking
            Assert.That(() => p.SetValueAction(-100.1m), Throws.ArgumentException);
            TestParameterIs(p, 2);
            Assert.That(() => p.SetValueAction(100.1m), Throws.ArgumentException);
            TestParameterIs(p, 2);

            using (TemporaryCulture.European()) //To make sure we handle decimal separator properly
            {
                //Test undo/redo
                var actions = p.SetValueAction(12.4m);
                Assert.That(actions, Is.Not.Null);
                TestParameterIs(p, 2);
                actions.Value.Redo();
                TestParameterIs(p, 12.4m);
                actions.Value.Undo();
                TestParameterIs(p, 2);

                p.TryDeserialiseValue("shane");
                TestCorruptParameter(p, "shane");

                //Test what happens if we undo back to an invalid state
                var actions2 = p.SetValueAction(1.0m);
                actions2.Value.Redo();
                actions2.Value.Undo();
                TestCorruptParameter(p, "shane");

                p.TryDeserialiseValue("54.45");
                TestParameterIs(p, 54.45m);
            }
        }

        private static void TestCorruptParameter(DecimalParameter p, string valueAsString)
        {
            Assert.That(p.Corrupted, Is.True);
            Assert.That(p.ValueAsString(), Is.EqualTo(valueAsString));
        }

        private static void TestParameterIs(DecimalParameter p, decimal val)
        {
            Assert.That(p.Value, Is.EqualTo(val));
            decimal result;
            bool isInt;
            Assert.That(p.Corrupted, Is.False);
            Assert.That(p.ValueAsString(), Is.EqualTo(val.ToString(CultureInfo.InvariantCulture))); //Tempting to simply verify it will parse as the correct value but we need to be consistent with existing files
            using (TemporaryCulture.English())
            {
                isInt = decimal.TryParse(p.DisplayValue((a, b) => ""), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
                Assert.That(isInt);
                if (isInt)
                    Assert.That(result, Is.EqualTo(val));
            }
            using (TemporaryCulture.European())
            {
                isInt = decimal.TryParse(p.DisplayValue((a, b) => ""), NumberStyles.Any, CultureInfo.CurrentCulture, out result);
                Assert.That(isInt);
                if (isInt)
                    Assert.That(result, Is.EqualTo(val));
            }
        }
    }
}
