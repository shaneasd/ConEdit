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
    public static class TestEnumParameter
    {
        internal class MockEnumeration1 : IEnumeration
        {
            public static IEnumeration Instance { get; } = new MockEnumeration1();
            Dictionary<Guid, string> m_values = new Dictionary<Guid, string>()
            {
               { Guid.Parse("100DDDA7-5FA9-4D2D-BE0B-DB5D911AFF00"), "name0"},
               { Guid.Parse("E4EA8F6C-CAC7-4C21-905C-8165A532443E"), "name1"},
               { Guid.Parse("E2B72062-598A-4C71-9A77-70E4C9CC038F"), "name2"},
               { Guid.Parse("23DB4D33-79D4-4920-A163-32FB515BF5E8"), "name3" },
            };

            public Either<string, Guid> DefaultValue
            {
                get
                {
                    return m_values.Keys.ElementAt(2);
                }
            }

            public IEnumerable<Guid> Options
            {
                get
                {
                    return m_values.Keys.OfType<Guid>();
                }
            }

            public ParameterType TypeId { get; } = ParameterType.Parse("6C3963EB-591E-4054-8238-225BD503F04E");

            public string GetName(Guid value)
            {
                return m_values[value];
            }
        }

        internal class MockEnumeration2 : IEnumeration
        {
            public static IEnumeration Instance { get; } = new MockEnumeration2();
            Dictionary<Guid, string> m_values = new Dictionary<Guid, string>()
            {
               { Guid.Parse("BEDBF1E6-ED93-4843-8036-86AF0CD904D5"), "nameA"},
               { Guid.Parse("447CE89A-43AC-4F36-B53A-86F64C4FEAC5"), "nameB"},
            };

            public Either<string, Guid> DefaultValue
            {
                get
                {
                    return "unknown value";
                }
            }

            public IEnumerable<Guid> Options
            {
                get
                {
                    return m_values.Keys.OfType<Guid>();
                }
            }

            public ParameterType TypeId { get; } = ParameterType.Parse("01B0F81F-6BF6-4CAA-8953-77CE594196DF");

            public string GetName(Guid value)
            {
                return m_values[value];
            }
        }

        [Test]
        public static void TestGuidDefault()
        {
            string name = "enumname";
            Id<Parameter> id = Id<Parameter>.Parse("3CD0C596-F145-4A03-B483-590BB6EEE5C4");
            EnumParameter p = new EnumParameter(name, id, MockEnumeration1.Instance, MockEnumeration1.Instance.Options.ElementAt(1).ToString());

            Assert.That(p.Id, Is.EqualTo(id));
            Assert.That(p.Name, Is.EqualTo(name));
            Assert.That(p.TypeId, Is.EqualTo(MockEnumeration1.Instance.TypeId));
            Assert.That(p.Options, Is.EquivalentTo(MockEnumeration1.Instance.Options));

            CheckNotCorrupt(p, MockEnumeration1.Instance.Options.ElementAt(1), MockEnumeration1.Instance);

            var initialValue = MockEnumeration1.Instance.Options.ElementAt(1);
            var setValue = MockEnumeration1.Instance.Options.ElementAt(0);

            CheckSettingValue(p, initialValue, setValue, MockEnumeration1.Instance);
        }

        [Test]
        public static void TestStringDefault()
        {
            string name = "enumname2";
            Id<Parameter> id = Id<Parameter>.Parse("6DB8D873-9DA3-412F-9164-322AFDE2895E");
            EnumParameter p = new EnumParameter(name, id, MockEnumeration2.Instance, "AStringDefault");

            Assert.That(p.Id, Is.EqualTo(id));
            Assert.That(p.Name, Is.EqualTo(name));
            Assert.That(p.TypeId, Is.EqualTo(MockEnumeration2.Instance.TypeId));
            Assert.That(p.Options, Is.EquivalentTo(MockEnumeration2.Instance.Options));

            CheckCorrupt(p, "AStringDefault");

            var initialValue = MockEnumeration2.Instance.Options.ElementAt(0);
            p.SetValueAction(initialValue).Value.Redo();
            var setValue = MockEnumeration2.Instance.Options.ElementAt(1);
            CheckSettingValue(p, initialValue, setValue, MockEnumeration2.Instance);
        }

        [Test]
        public static void TestNullEnumGuidDefault()
        {
            string name = "enumname2";
            Id<Parameter> id = Id<Parameter>.Parse("6DB8D873-9DA3-412F-9164-322AFDE2895E");
            EnumParameter p = new EnumParameter(name, id, MockEnumeration1.Instance, null);

            var def = MockEnumeration1.Instance.DefaultValue.Transformed(a => { throw new InvalidOperationException(); }, b => b);
            CheckNotCorrupt(p, def, MockEnumeration1.Instance);

            Guid initialValue = def;
            var setValue = MockEnumeration1.Instance.Options.ElementAt(3);
            CheckSettingValue(p, initialValue, setValue, MockEnumeration1.Instance);
        }

        [Test]
        public static void TestNullEnumStringDefault()
        {
            string name = "enumname2";
            Id<Parameter> id = Id<Parameter>.Parse("6DB8D873-9DA3-412F-9164-322AFDE2895E");
            EnumParameter p = new EnumParameter(name, id, MockEnumeration2.Instance, null);

            var def = MockEnumeration2.Instance.DefaultValue.Transformed(b => b, a => { throw new InvalidOperationException(); });
            CheckCorrupt(p, def);

            Guid initialValue = MockEnumeration2.Instance.Options.ElementAt(0);
            p.SetValueAction(initialValue).Value.Redo();
            var setValue = MockEnumeration2.Instance.Options.ElementAt(1);
            CheckSettingValue(p, initialValue, setValue, MockEnumeration2.Instance);
        }

        [Test]
        public static void TestGetName()
        {
            string name = "enumname";
            Id<Parameter> id = Id<Parameter>.Parse("3CD0C596-F145-4A03-B483-590BB6EEE5C4");
            EnumParameter p = new EnumParameter(name, id, MockEnumeration1.Instance, MockEnumeration1.Instance.Options.ElementAt(1).ToString());
            foreach (var option in MockEnumeration1.Instance.Options)
            {
                Assert.That(p.GetName(option), Is.EqualTo(MockEnumeration1.Instance.GetName(option)));
            }
        }

        private static void CheckCorrupt(EnumParameter p, string value)
        {
            Assert.That(p.Corrupted, Is.True);
            Assert.That(p.DisplayValue(a => ""), Is.EqualTo(value));
            Assert.That(p.ValueAsString(), Is.EqualTo(value));
        }

        private static void CheckNotCorrupt(EnumParameter p, Guid value, IEnumeration enumeration)
        {
            Assert.That(p.Corrupted, Is.False);
            Assert.That(p.DisplayValue(a => ""), Is.EqualTo(enumeration.GetName(value)));
            Assert.That(p.Value, Is.EqualTo(value));
            Guid guid;
            Assert.That(Guid.TryParse(p.ValueAsString(), out guid), Is.True);
            Assert.That(guid, Is.EqualTo(p.Value));
        }

        private static void CheckSettingValue(EnumParameter p, Guid initialValue, Guid setValue, IEnumeration enumeration)
        {
            Assert.That(p.Value, Is.EqualTo(initialValue));
            Assert.That(p.SetValueAction(initialValue), Is.Null);
            var action = p.SetValueAction(setValue);
            Assert.That(action, Is.Not.Null);
            CheckNotCorrupt(p, initialValue, enumeration);
            action.Value.Redo();
            CheckNotCorrupt(p, setValue, enumeration);
            action.Value.Undo();
            CheckNotCorrupt(p, initialValue, enumeration);

            p.TryDeserialiseValue("asd");
            CheckCorrupt(p, "asd");

            //Test what happens if we undo back to an invalid state
            var actions = p.SetValueAction(setValue);
            actions.Value.Redo();
            CheckNotCorrupt(p, setValue, enumeration);
            actions.Value.Undo();
            CheckCorrupt(p, "asd");

            p.TryDeserialiseValue(setValue.ToString());
            CheckNotCorrupt(p, setValue, enumeration);
        }
    }
}
