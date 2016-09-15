using Conversation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Conversation.Parameters
{
    public static class TestDynamicEnumParameter
    {
        [Test]
        public static void TestNullDefault()
        {
            string name = "asdg";
            Id<Parameter> id = Id<Parameter>.Parse("F2EA9FA2-61C4-45A0-BFA7-906349445AD8");
            DynamicEnumParameter.Source source = new DynamicEnumParameter.Source();
            ParameterType type = ParameterType.Parse("623014BC-EA1D-4230-9B33-DEA1BD43C0E2");
            DynamicEnumParameter p = new DynamicEnumParameter(name, id, source, type, null, false);

            Assert.That(p.Corrupted, Is.True);
            Assert.That(p.Local, Is.False);
            Assert.That(p.Name, Is.EqualTo(name));
            Assert.That(p.Id, Is.EqualTo(id));
            Assert.That(p.TypeId, Is.EqualTo(type));

            Assert.That(p.Options, Is.EquivalentTo(source.Options));
            Assert.That(source.Options, Is.Empty);

            var actions2 = p.SetValueAction("fdjk");
            actions2.Value.Redo();
            CheckValue(p, "fdjk");
            actions2.Value.Undo();
            Assert.That(p.Corrupted, Is.True);
        }

        [Test]
        public static void Test()
        {
            string name = "asdg";
            Id<Parameter> id = Id<Parameter>.Parse("F2EA9FA2-61C4-45A0-BFA7-906349445AD8");
            DynamicEnumParameter.Source source = new DynamicEnumParameter.Source();
            ParameterType type = ParameterType.Parse("623014BC-EA1D-4230-9B33-DEA1BD43C0E2");
            DynamicEnumParameter p = new DynamicEnumParameter(name, id, source, type, "shane", true);

            Assert.That(p.Local, Is.True);
            Assert.That(p.Name, Is.EqualTo(name));
            Assert.That(p.Id, Is.EqualTo(id));
            Assert.That(p.TypeId, Is.EqualTo(type));

            Assert.That(p.Options, Is.EquivalentTo(source.Options));
            Assert.That(source.Options, Is.EquivalentTo(new[] { "shane" }));

            CheckValue(p, "shane");

            DynamicEnumParameter another = new DynamicEnumParameter("another", Id<Parameter>.Parse("08573582-A196-454A-B911-5F291EFC3C69"), source, ParameterType.Parse("6D14FFB5-3F2A-4865-9B6D-0F215B6F0EFC"), "asd", true);

            Action<string[]> CheckOptions = options =>
            {
                Assert.That(p.Options, Is.EquivalentTo(source.Options));
                Assert.That(another.Options, Is.EquivalentTo(source.Options));
                Assert.That(source.Options, Is.EquivalentTo(options));
            };

            CheckOptions(new[] { "shane", "asd" });

            Assert.That(() => p.SetValueAction(null), Throws.ArgumentException);

            var action = p.SetValueAction("set");
            Assert.That(action, Is.Not.Null);
            CheckValue(p, "shane");
            CheckOptions(new[] { "shane", "asd" });

            action.Value.Redo();
            CheckValue(p, "set");
            CheckOptions(new[] { "set", "asd" });

            action.Value.Undo();
            CheckValue(p, "shane");
            CheckOptions(new[] { "shane", "asd" });

            Assert.Throws<ArgumentNullException>(() => { p.TryDeserialiseValue(null); });
            CheckValue(p, "shane");

            p.TryDeserialiseValue("dasfafd");
            CheckValue(p, "dasfafd");
            CheckOptions(new[] { "dasfafd", "asd" });

            DynamicEnumParameter.Source source2 = new DynamicEnumParameter.Source();
            DynamicEnumParameter third = new DynamicEnumParameter("another", Id<Parameter>.Parse("7DF52211-236E-4848-8117-6B15C51A21EB"), source2, ParameterType.Parse("B117246A-3092-41A5-AEB1-93F7E1F77E32"), "asd2", true);
            p.MergeInto(source2);
            Assert.That(p.Options, Is.EquivalentTo(source2.Options));
            Assert.That(third.Options, Is.EquivalentTo(source2.Options));
            Assert.That(another.Options, Is.EquivalentTo(source.Options));
            Assert.That(source.Options, Is.EquivalentTo(new[] { "asd" }));
            Assert.That(source2.Options, Is.EquivalentTo(new[] { "dasfafd", "asd2" }));
        }

        private static void CheckValue(DynamicEnumParameter p, string value)
        {
            Assert.That(p.Corrupted, Is.False);
            Assert.That(p.Value, Is.EqualTo(value));
            Assert.That(p.ValueAsString(), Is.EqualTo(value));
            Assert.That(p.DisplayValue(a => ""), Is.EqualTo(value));
        }
    }
}
