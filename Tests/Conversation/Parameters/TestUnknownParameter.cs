using Conversation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Conversation.Parameters
{
    public static class TestUnknownParameter
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Conversation.UnknownParameter", Justification = "The point is to see if constructing the thing causes errors")]
        [Test]
        public static void TestNullValue()
        {
            var id = Id<Parameter>.Parse("46052E44-E83D-4595-B03C-53393DDCCED4");
            Assert.Throws<ArgumentNullException>(() => { new UnknownParameter(id, null); });
        }

        [Test]
        public static void TestAlternateConstruction()
        {
            var id = Id<Parameter>.Parse("5EC4C494-2C9A-451A-91DD-805A00C31E76");
            string value = "TestAlternateConstruction";
            var p = new UnknownParameter(id, value);
            CheckConstruction(id, value, p);
        }

        [Test]
        public static void Test()
        {
            var id = Id<Parameter>.Parse("46052E44-E83D-4595-B03C-53393DDCCED4");
            string value = "as";
            var p = new UnknownParameter(id, value);
            CheckConstruction(id, value, p);

            Assert.Throws<ArgumentNullException>(() => { p.TryDeserialiseValue(null); });
            Assert.Throws<ArgumentException>(() => { p.SetValueAction(null); });
            Assert.Throws<ArgumentException>(() => { p.SetValueAction("test"); });

            string value2 = "test2";
            p.TryDeserialiseValue(value2);
            CheckValue(value2, p);
        }

        private static void CheckConstruction(Id<Parameter> id, string value, UnknownParameter p)
        {
            Assert.That(p.Corrupted, Is.False);
            Assert.That(p.Id, Is.EqualTo(id));
            Assert.That((p as IParameter).TypeId, Is.EqualTo(UnknownParameter.TypeId));
            Assert.That(p.Name, Is.EqualTo("Unknown parameter " + id.Guid.ToString().Substring(0, 8))); //Really not important what the exact value is. This is just a regression test.
            CheckValue(value, p);
        }

        private static void CheckValue(string value, UnknownParameter p)
        {
            Assert.That(p.Value, Is.EqualTo(value));
            Assert.That(p.DisplayValue((a, b) => ""), Is.EqualTo(value));
            Assert.That(p.ValueAsString(), Is.EqualTo(value));
        }
    }
}
