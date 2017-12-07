using Conversation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Tests.Conversation.Parameters
{
    public static class TestBooleanParameter
    {
        [Test]
        public static void TestNullDefault()
        {
            string name = "name";
            Id<Parameter> id = Id<Parameter>.Parse("7BE05FD5-B8FC-4E07-A1F0-4228678AD055");
            BooleanParameter p = new BooleanParameter(name, id, null);
            Assert.That(p.Corrupted, Is.True);
            p.TryDeserialiseValue("true");
            TestParameterIs(p, true);
        }

        [NUnit.Framework.Test]
        public static void Test()
        {
            string name = "name";
            Id<Parameter> id = Id<Parameter>.Parse("7BE05FD5-B8FC-4E07-A1F0-4228678AD055");
            BooleanParameter p = new BooleanParameter(name, id, "false");

            Assert.That(p.Id, Is.EqualTo(id));
            Assert.That(p.Name, Is.EqualTo(name));
            Assert.That(p.TypeId, Is.EqualTo(BooleanParameter.ParameterType));

            Assert.That(p.Corrupted, Is.False);

            TestParameterIs(p, false);

            p.TryDeserialiseValue("shane");
            TestCorrupt(p, "shane");

            //Test what happens if we undo back to an invalid state
            var actions = p.SetValueAction(true);
            actions.Value.Redo();
            TestParameterIs(p, true);
            actions.Value.Undo();
            TestCorrupt(p, "shane");

            p.TryDeserialiseValue("true");
            TestParameterIs(p, true);

            Assert.Throws<ArgumentNullException>(() => { p.TryDeserialiseValue(null); });
            TestParameterIs(p, true);

            //Get into a known state where Value==true
            if (p.Value == false)
            {
                p.SetValueAction(true).Value.Redo();
            }

            Assert.That(p.SetValueAction(true), Is.Null);

            var set = p.SetValueAction(false);
            TestParameterIs(p, true);
            set.Value.Redo();
            TestParameterIs(p, false);
            set.Value.Undo();
            TestParameterIs(p, true);

        }

        private static void TestCorrupt(BooleanParameter p, string valueAsString)
        {
            Assert.That(p.Corrupted, Is.True);
            Assert.That(p.ValueAsString(), Is.EqualTo(valueAsString));
        }

        private static void TestParameterIs(BooleanParameter p, bool val, string valueAsString = null)
        {
            Assert.That(p.Value, Is.EqualTo(val));
            bool result;
            bool isBool;
            if (valueAsString == null)
            {
                Assert.That(p.Corrupted, Is.False);
                Assert.That(p.ValueAsString(), Is.EqualTo(val.ToString())); //Tempting to simply verify it will parse as the correct value but we need to be consistent with existing files
            }
            else
            {
                Assert.That(p.Corrupted, Is.True);
                Assert.That(p.ValueAsString(), Is.EqualTo(valueAsString));
            }
            using (TemporaryCulture.English())
            {
                isBool = bool.TryParse(p.DisplayValue((a, b) => ""), out result);
                Assert.That(isBool);
                if (isBool)
                    Assert.That(result, Is.EqualTo(val));
            }
            using (TemporaryCulture.European())
            {
                isBool = bool.TryParse(p.DisplayValue((a, b) => ""), out result);
                Assert.That(isBool);
                if (isBool)
                    Assert.That(result, Is.EqualTo(val));
            }
        }
    }
}
