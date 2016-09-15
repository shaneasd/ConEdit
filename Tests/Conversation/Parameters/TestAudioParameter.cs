using Conversation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Conversation.Parameters
{
    public static class TestAudioParameter
    {
        public static void TestConstruction()
        {
            {
                string name = "bdfgbsgaudio";
                Id<Parameter> id = Id<Parameter>.Parse("BC123324-1BA2-4BCD-83D3-419D854CFE3D");
                ParameterType type = ParameterType.Parse("85C098A7-0CBB-4C23-8C33-8B6596C2A258");
                AudioParameter p = new AudioParameter(name, id, type);
                Assert.That(p.Name, Is.EqualTo(name));
                Assert.That(p.Id, Is.EqualTo(id));
                Assert.That(p.TypeId, Is.EqualTo(type));
                Assert.That(p.Corrupted, Is.True);
                //AudioParameters are always constructed corrupted and the conversation files automatically decorrupt them by generating new values for them
            }

            {
                string name = "bdfgadio";
                Id<Parameter> id = Id<Parameter>.Parse("3F8D6F37-36B8-48A7-BAC6-4F524728D1FE");
                ParameterType type = ParameterType.Parse("12103BA0-7680-44B8-AB80-6F987D885690");
                AudioParameter p = new AudioParameter(name, id, type);
                Assert.That(p.Name, Is.EqualTo(name));
                Assert.That(p.Id, Is.EqualTo(id));
                Assert.That(p.TypeId, Is.EqualTo(type));
                Assert.That(p.Corrupted, Is.True);
                //AudioParameters are always constructed corrupted and the conversation files automatically decorrupt them by generating new values for them
            }
        }

        [Test]
        public static void Test()
        {
            string name = "bdfgbsgaudio";
            Id<Parameter> id = Id<Parameter>.Parse("BC123324-1BA2-4BCD-83D3-419D854CFE3D");
            ParameterType type = ParameterType.Parse("85C098A7-0CBB-4C23-8C33-8B6596C2A258");
            AudioParameter p = new AudioParameter(name, id, type);

            var value1 = new Audio("asdkavds\\asdasd");
            var action = p.SetValueAction(value1);
            Assert.That(action, Is.Not.Null);
            action.Value.Redo();
            CheckValue(p, value1);
            action.Value.Undo();
            Assert.That(p.Corrupted, Is.True);

            p.TryDeserialiseValue("shane");
            CheckValue(p, new Audio("shane"));
            var value2 = new Audio("a\\b\\c");
            var action2 = p.SetValueAction(value2);
            Assert.That(action2, Is.Not.Null);
            action2.Value.Redo();
            CheckValue(p, value2);
            action2.Value.Undo();
            CheckValue(p, new Audio("shane"));
        }

        private static void CheckValue(AudioParameter p, Audio value)
        {
            Assert.That(p.Corrupted, Is.False);
            Assert.That(p.Value, Is.EqualTo(value));
            Assert.That(p.DisplayValue(a => "a"), Is.EqualTo(value.DisplayValue()));
            Assert.That(p.SetValueAction(value), Is.Null);
            Assert.That(p.ValueAsString(), Is.EqualTo(value.Serialize()));
        }
    }
}
