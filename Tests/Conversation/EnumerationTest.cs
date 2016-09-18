using Conversation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Tests.Conversation
{
    public static class EnumerationTest
    {
        [Test]
        public static void ParameterEnum()
        {
            var options = new[] { Tuple.Create(Guid.Parse("928aaa46-0d5f-4c07-9cf3-20fb0061f5b9"), "value1"),
                                  Tuple.Create(Guid.Parse("db99b72c-1d07-4956-9d4b-33d202234de7"), "value2"),
                                  Tuple.Create(Guid.Parse("4c555837-24dc-41b7-ba45-20bde741a4c6"), "value3") };
            var immutableType = ParameterType.Parse("4785df45-f775-43d8-91ab-6e26980afa4f");
            var mutableType = ParameterType.Parse("a5417c30-79df-4d5e-9639-b7b088871d47");
            var id = Id<Parameter>.Parse("5b26720a-814b-4973-849d-2bda4645a541");

            ImmutableEnumeration immutable = new ImmutableEnumeration(options, immutableType, "4c555837-24dc-41b7-ba45-20bde741a4c6");
            MutableEnumeration mutable = new MutableEnumeration(options, mutableType, "4c555837-24dc-41b7-ba45-20bde741a4c6");
            CheckEnumParameter(options, immutable, id, "db99b72c-1d07-4956-9d4b-33d202234de7", false);
            CheckEnumParameter(options, mutable, id, "db99b72c-1d07-4956-9d4b-33d202234de7", false);
            CheckSetParameter(options, immutable, id, "db99b72c-1d07-4956-9d4b-33d202234de7", false);
            CheckSetParameter(options, mutable, id, "db99b72c-1d07-4956-9d4b-33d202234de7", false);
            CheckEnumParameter(options, immutable, id, null, false);
            CheckEnumParameter(options, mutable, id, null, false);
            CheckSetParameter(options, immutable, id, null, false);
            CheckSetParameter(options, mutable, id, null, false);

            ImmutableEnumeration immutable2 = new ImmutableEnumeration(options, immutableType, "asd");
            MutableEnumeration mutable2 = new MutableEnumeration(options, mutableType, "asd");
            CheckEnumParameter(options, immutable2, id, null, true);
            CheckEnumParameter(options, mutable2, id, null, true);
            CheckSetParameter(options, immutable2, id, null, true);
            CheckSetParameter(options, mutable2, id, null, true);
        }

        private static void CheckEnumParameter(Tuple<Guid, string>[] options, IEnumeration enumeration, Id<Parameter> id, string def, bool corrupt)
        {
            string name = "testEnumParameter";
            var enumParameter = enumeration.ParameterEnum(name, id, def);
            Assert.That(enumParameter.Corrupted, Is.EqualTo(corrupt));
            Assert.That(enumParameter.ValueAsString(), Is.EqualTo(def ?? enumeration.DefaultValue.Transformed(s => s, g => enumeration.GetName(g))));
            Assert.That(enumParameter.Id, Is.EqualTo(id));
            Assert.That(enumParameter.Name, Is.EqualTo(name));
            Assert.That(enumParameter.TypeId, Is.EqualTo(enumeration.TypeId));
            Assert.That(enumParameter, Is.InstanceOf<IEnumParameter>());
            Assert.That((enumParameter as IEnumParameter).Options, Is.EquivalentTo(options.Select(o => o.Item1)));
        }

        private static void CheckSetParameter(Tuple<Guid, string>[] options, IEnumeration enumeration, Id<Parameter> id, string def, bool corrupt)
        {
            string name = "testSetParameter";
            var setParameter = enumeration.ParameterSet(name, id, def);
            Assert.That(setParameter.Corrupted, Is.EqualTo(corrupt));
            Assert.That(setParameter.ValueAsString(), Is.EqualTo(def ?? enumeration.DefaultValue.Transformed(s => s, g => enumeration.GetName(g))));
            Assert.That(setParameter.Id, Is.EqualTo(id));
            Assert.That(setParameter.Name, Is.EqualTo(name));
            Assert.That(setParameter.TypeId, Is.EqualTo(ParameterType.ValueSetType.Of(enumeration.TypeId)));
            Assert.That(setParameter, Is.InstanceOf<ISetParameter>());
            Assert.That((setParameter as ISetParameter).Options, Is.EquivalentTo(options.Select(o => o.Item1)));
        }

        [Test]
        public static void Mutable()
        {
            var options = new[] { Tuple.Create(Guid.Parse("928aaa46-0d5f-4c07-9cf3-20fb0061f5b9"), "value1"),
                                  Tuple.Create(Guid.Parse("db99b72c-1d07-4956-9d4b-33d202234de7"), "value2"),
                                  Tuple.Create(Guid.Parse("3fa9e8f9-bf6d-4431-97a5-92b417660192"), "value3") };
            var mutableType = ParameterType.Parse("a5417c30-79df-4d5e-9639-b7b088871d47");
            MutableEnumeration mutable = new MutableEnumeration(options, mutableType, "3fa9e8f9-bf6d-4431-97a5-92b417660192");

            var data = mutable.GetData("anme");
            Assert.That(data.TypeId, Is.EqualTo(mutable.TypeId));
            Assert.That(data.Name, Is.EqualTo("anme"));
            Assert.That(data.Elements.Select(e => Tuple.Create(e.Guid, e.Name)), Is.EquivalentTo(options));

            Tuple<Guid, string> add = Tuple.Create(Guid.Parse("de6aae1d-d9c3-4f96-93f1-77e5ee23e9e8"), "extra");
            mutable.Add(add.Item1, add.Item2);
            var options2 = options.Concat(add.Only());
            Assert.That(mutable.Options, Is.EquivalentTo(options2.Select(o => o.Item1)));
            foreach (var option in options2)
            {
                Assert.That(mutable.GetName(option.Item1), Is.EqualTo(option.Item2));
            }

            Tuple<Guid, string> remove = options[0];
            mutable.Remove(remove.Item1);
            var options3 = options2.Except(remove.Only());
            Assert.That(mutable.Options, Is.EquivalentTo(options3.Select(o => o.Item1)));
            foreach (var option in options3)
            {
                Assert.That(mutable.GetName(option.Item1), Is.EqualTo(option.Item2));
            }

            var newOptions = new EnumerationData.Element[]
            {
                new EnumerationData.Element("a", Guid.Parse("22094bc3-ea3a-4585-9624-6376379dafa6")),
                new EnumerationData.Element("b", Guid.Parse("8ff8c0c1-f406-4f69-8cd7-95b90bffc98c"))
            };
            mutable.SetOptions(newOptions);
            Assert.That(mutable.Options, Is.EquivalentTo(newOptions.Select(o => o.Guid)));
            foreach (var option in newOptions)
            {
                Assert.That(mutable.GetName(option.Guid), Is.EqualTo(option.Name));
            }
        }

        [Test]
        public static void TestConstructionUnknownStringDefault()
        {
            var options = new[] { Tuple.Create(Guid.Parse("928aaa46-0d5f-4c07-9cf3-20fb0061f5b9"), "value1"),
                                  Tuple.Create(Guid.Parse("db99b72c-1d07-4956-9d4b-33d202234de7"), "value2"),
                                  Tuple.Create(Guid.Parse("3fa9e8f9-bf6d-4431-97a5-92b417660192"), "value3") };
            var immutableType = ParameterType.Parse("4785df45-f775-43d8-91ab-6e26980afa4f");
            var mutableType = ParameterType.Parse("a5417c30-79df-4d5e-9639-b7b088871d47");
            ImmutableEnumeration immutable = new ImmutableEnumeration(options, immutableType, "asd");
            MutableEnumeration mutable = new MutableEnumeration(options, mutableType, "as");

            Assert.That("asd", Is.EqualTo(immutable.DefaultValue));
            Assert.That("as", Is.EqualTo(mutable.DefaultValue));
            Assert.That(immutable.Options, Is.EquivalentTo(options.Select(a => a.Item1)));
            Assert.That(mutable.Options, Is.EquivalentTo(options.Select(a => a.Item1)));
            Assert.That(immutable.TypeId, Is.EqualTo(immutableType));
            Assert.That(mutable.TypeId, Is.EqualTo(mutableType));

            foreach (var option in options)
            {
                Assert.That(immutable.GetName(option.Item1), Is.EqualTo(option.Item2));
                Assert.That(mutable.GetName(option.Item1), Is.EqualTo(option.Item2));
            }
        }
    }
}
