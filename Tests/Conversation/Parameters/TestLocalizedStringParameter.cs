using Conversation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Conversation.Parameters
{
    public static class TestLocalizedStringParameter
    {
        [Test]
        public static void TestConstruction()
        {
            {
                string name = "a";
                Id<Parameter> id = Id<Parameter>.Parse("92499A0E-12FD-414D-9478-C24D3FB7637C");
                LocalizedStringParameter p = new LocalizedStringParameter(name, id);
                Assert.That(p.Name, Is.EqualTo(name));
                Assert.That(p.Id, Is.EqualTo(id));
                Assert.That(p.TypeId, Is.EqualTo(LocalizedStringParameter.ParameterType));
                Assert.That(p.Corrupted, Is.True);
            }
            {
                string name = "b";
                Id<Parameter> id = Id<Parameter>.Parse("2BAFB643-1DD8-42F9-B521-0A50792DB231");
                LocalizedStringParameter p = new LocalizedStringParameter(name, id);
                Assert.That(p.Name, Is.EqualTo(name));
                Assert.That(p.Id, Is.EqualTo(id));
                Assert.That(p.TypeId, Is.EqualTo(LocalizedStringParameter.ParameterType));
                Assert.That(p.Corrupted, Is.True);
            }
        }

        [Test]
        public static void Test()
        {
            string name = "a";
            Id<Parameter> id = Id<Parameter>.Parse("92499A0E-12FD-414D-9478-C24D3FB7637C");
            LocalizedStringParameter p = new LocalizedStringParameter(name, id);

            //Try setting a value
            var value1 = Id<LocalizedText>.Parse("52A20426-339C-4DE2-BEF1-236994EE882A");
            var action1 = p.SetValueAction(value1);
            Assert.That(action1, Is.Not.Null);
            action1.Value.Redo();
            CheckValue(p, value1);

            //Undo to a corrupt state
            action1.Value.Undo();
            Assert.That(p.Corrupted);

            //Attempt to deserialize an invalid string
            p.TryDeserialiseValue("sajgdfskdfbgksf");
            Assert.That(p.ValueAsString(), Is.EqualTo("sajgdfskdfbgksf"));
            Assert.That(p.Corrupted, Is.True);

            var value2 = Id<LocalizedText>.Parse("825560C8-6A97-42C2-B74B-EBC362FC59A1");
            var action2 = p.SetValueAction(value2);
            Assert.That(action2, Is.Not.Null);
            action2.Value.Redo();
            CheckValue(p, value2);

            //Undo to a corrupt state with known string representation
            action2.Value.Undo();
            Assert.That(p.ValueAsString(), Is.EqualTo("sajgdfskdfbgksf"));
            Assert.That(p.Corrupted);

            p.TryDeserialiseValue("08D12849-6B57-4613-9AFF-E528C643FB32");
            CheckValue(p, Id<LocalizedText>.Parse("08D12849-6B57-4613-9AFF-E528C643FB32"));
        }

        private static void CheckValue(LocalizedStringParameter p, Id<LocalizedText> value1)
        {
            Assert.That(p.Value, Is.EqualTo(value1));
            Assert.That(p.Corrupted, Is.False);
            Assert.That(p.SetValueAction(value1), Is.Null);
            Assert.That(p.ValueAsString(), Is.EqualTo(value1.Serialized())); //Tempting to simply verify it will parse as the correct value but we need to be consistent with existing files
            Assert.That(p.DisplayValue(a => a.Equals(value1) ? "equal" : "not equal"), Is.EqualTo("equal"));
        }
    }
}
