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
    public static class TestSetParameter
    {
        private static void CheckConstruction(IEnumeration enumeration, string name, Id<Parameter> id, SetParameter p)
        {
            Assert.That(p.Id, Is.EqualTo(id));
            Assert.That(p.Name, Is.EqualTo(name));
            Assert.That(p.TypeId, Is.EqualTo(ParameterType.ValueSetType.Of(enumeration.TypeId)));
            Assert.That(p.Options, Is.EquivalentTo(enumeration.Options));

            foreach (var option in enumeration.Options)
            {
                Assert.That(p.GetName(option), Is.EqualTo(enumeration.GetName(option)));
            }
        }

        private static void CheckUsage(SetParameter p)
        {
            var string1 = p.Options.First().ToString() + "87E32D67-330C-499D-BD4B-2AA681AF9D42";
            p.TryDeserialiseValue(string1);
            Assert.That(p.Corrupted, Is.True);
            Assert.That(p.ValueAsString(), Is.EqualTo(string1));

            var value1 = new ReadonlySet<Guid>(p.Options.First());
            var action1 = p.SetValueAction(value1);
            Assert.That(action1, Is.Not.Null);
            action1.Value.Redo();
            Assert.That(p.SetValueAction(value1), Is.Null);
            CheckValue(p, value1);

            var value2 = new ReadonlySet<Guid>(p.Options.First(), p.Options.ElementAt(1));
            var action2 = p.SetValueAction(value2);
            Assert.That(action2, Is.Not.Null);
            action2.Value.Redo();
            CheckValue(p, value2);
            action2.Value.Undo();
            CheckValue(p, value1);
            action1.Value.Undo();
            Assert.That(p.Corrupted, Is.True);
            Assert.That(p.ValueAsString(), Is.EqualTo(string1));

            var string2 = p.Options.ElementAt(0) + "+" + p.Options.ElementAt(1);
            p.TryDeserialiseValue(string2);
            CheckValue(p, new ReadonlySet<Guid>(p.Options.ElementAt(0), p.Options.ElementAt(1)));
        }

        private static void CheckValue(SetParameter p, ReadonlySet<Guid> value)
        {
            Assert.That(p.Corrupted, Is.False);
            Assert.That(p.Value, Is.EqualTo(value));
            Assert.That(p.ValueAsString(), Is.EqualTo(string.Join("+", value.Select(a => a.ToString()))));
            Assert.That(p.DisplayValue((a, b) => null), Is.EqualTo(string.Join(" + ", value.Select(a => p.GetName(a)).OrderBy(a => a)))); //Names are displayed in alphabetical order
        }

        [Test]
        public static void TestNullDefault()
        {
            var enumeration = TestEnumParameter.MockEnumeration1.Instance;
            string name = "a";
            Id<Parameter> id = Id<Parameter>.Parse("0F10DB06-12E8-4FAC-9F76-53BE72D87EC5");
            SetParameter p = new SetParameter(name, id, enumeration, null);
            CheckConstruction(enumeration, name, id, p);

            CheckValue(p, new ReadonlySet<Guid>(enumeration.DefaultValue.Transformed(a => { throw new NotImplementedException(); }, a => a)));

            CheckUsage(p);
        }

        [Test]
        public static void TestInvalidDefaultCannotParse()
        {
            var enumeration = TestEnumParameter.MockEnumeration2.Instance;
            string name = "b";
            Id<Parameter> id = Id<Parameter>.Parse("B6FD1F35-1177-484E-8F42-731E389C57AC");
            string def = "shane";
            SetParameter p = new SetParameter(name, id, enumeration, def);
            CheckConstruction(enumeration, name, id, p);

            Assert.That(p.Corrupted, Is.True);
            Assert.That(p.ValueAsString(), Is.EqualTo(def));
            CheckUsage(p);
        }

        [Test]
        public static void TestInvalidDefaultGuidNotInOptions()
        {
            var enumeration = TestEnumParameter.MockEnumeration2.Instance;
            string name = "b";
            Id<Parameter> id = Id<Parameter>.Parse("B6FD1F35-1177-484E-8F42-731E389C57AC");
            string def = enumeration.Options.ElementAt(0).ToString() + "+" + "2BCD4F8F-73E0-4B57-86C7-8780C833B5C9";
            SetParameter p = new SetParameter(name, id, enumeration, def);
            CheckConstruction(enumeration, name, id, p);

            Assert.That(p.Corrupted, Is.True);
            Assert.That(p.ValueAsString(), Is.EqualTo(def));
            CheckUsage(p);
        }

        [Test]
        public static void TestEmptyDefault()
        {
            var enumeration = TestEnumParameter.MockEnumeration1.Instance;
            string name = "a";
            Id<Parameter> id = Id<Parameter>.Parse("0F10DB06-12E8-4FAC-9F76-53BE72D87EC5");
            SetParameter p = new SetParameter(name, id, enumeration, "");
            CheckConstruction(enumeration, name, id, p);

            Assert.That(p.Corrupted, Is.False);
            CheckValue(p, new ReadonlySet<Guid>());
            CheckUsage(p);
        }

        [Test]
        public static void TestSingleGuidDefault()
        {
            var enumeration = TestEnumParameter.MockEnumeration1.Instance;
            string name = "a";
            Id<Parameter> id = Id<Parameter>.Parse("0F10DB06-12E8-4FAC-9F76-53BE72D87EC5");
            ReadonlySet<Guid> def = new ReadonlySet<Guid>(enumeration.Options.ElementAt(0));
            SetParameter p = new SetParameter(name, id, enumeration, string.Join("+", def.Select(a => a.ToString())));
            CheckConstruction(enumeration, name, id, p);

            Assert.That(p.Corrupted, Is.False);
            CheckValue(p, def);
            CheckUsage(p);
        }

        [Test]
        public static void TestMultiGuidDefault()
        {
            var enumeration = TestEnumParameter.MockEnumeration1.Instance;
            string name = "a";
            Id<Parameter> id = Id<Parameter>.Parse("0F10DB06-12E8-4FAC-9F76-53BE72D87EC5");
            ReadonlySet<Guid> def = new ReadonlySet<Guid>(enumeration.Options.ElementAt(0), enumeration.Options.ElementAt(3), enumeration.Options.ElementAt(1));
            SetParameter p = new SetParameter(name, id, enumeration, string.Join("+", def.Select(a => a.ToString())));
            CheckConstruction(enumeration, name, id, p);

            Assert.That(p.Corrupted, Is.False);
            CheckValue(p, def);
            CheckUsage(p);
        }
    }
}
