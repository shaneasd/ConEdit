using Conversation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Conversation
{
    public static class TypeSetTest
    {
        class OtherData
        {
            class DummyParameter : IParameter
            {
                public DummyParameter(ParameterType typeId, string name, Id<Parameter> id, string defaultValue)
                {
                    TypeId = typeId;
                    Name = name;
                    Id = id;
                    DefaultValue = defaultValue;
                }

                private readonly string DefaultValue;

                public bool Corrupted => false;

                public Id<global::Conversation.Parameter> Id { get; }

                public string Name { get; }

                public ParameterType TypeId { get; }

                public string DisplayValue(Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localize)
                {
                    throw new NotImplementedException();
                }

                public void TryDeserialiseValue(string value)
                {
                    throw new NotImplementedException();
                }

                public string ValueAsString()
                {
                    return DefaultValue;
                }
            }

            public ParameterType TypeId { get; }
            public string Name { get; }

            public ParameterGenerator Factory { get; }

            public OtherData(ParameterType typeId, string n)
            {
                TypeId = typeId;
                Name = n;
                Factory = (name, id, defaultValue, document) =>
                {
                    Assert.That(LastGeneratorParameters, Is.EqualTo(null));
                    LastGeneratorParameters = new LastGeneratorParametersType();
                    LastGeneratorParameters.name = name;
                    LastGeneratorParameters.id = id;
                    LastGeneratorParameters.defaultValue = defaultValue;
                    LastGeneratorParameters.document = document;
                    var createdParameter = new DummyParameter(typeId, name, id, defaultValue);
                    LastGeneratorParameters.createdParameter = createdParameter;
                    return createdParameter;
                };
            }

            public class LastGeneratorParametersType
            {
                public string name;
                public Id<Parameter> id;
                public string defaultValue;
                public object document;
                public IParameter createdParameter;
            };
            public LastGeneratorParametersType LastGeneratorParameters = null;
        }

        static void CheckIs(TypeSet typeSet, ParameterType type, bool dec = false, bool integer = false, bool enumeration = false, bool dynamicEnum = false, bool localDynamicEnum = false, bool localizedString = false)
        {
            Assert.That(typeSet.IsDecimal(type), Is.EqualTo(dec));
            Assert.That(typeSet.IsDynamicEnum(type), Is.EqualTo(dynamicEnum));
            Assert.That(typeSet.IsEnum(type), Is.EqualTo(enumeration));
            Assert.That(typeSet.IsInteger(type), Is.EqualTo(integer));
            Assert.That(typeSet.IsLocalDynamicEnum(type), Is.EqualTo(localDynamicEnum));
            Assert.That(typeSet.IsLocalizedString(type), Is.EqualTo(localizedString));
        }

        /// <summary>
        /// Assert that dynamic enums of the same type share the same source and not the reverse.
        /// </summary>
        [Test]
        public static void TestDynamicEnumSource()
        {
            TypeSet t = new TypeSet();
            {
                DynamicEnumerationData d1 = new DynamicEnumerationData("d1", ParameterType.Parse("ed53be0d-c62a-4c0c-9f6e-e92d996bf3df"));
                DynamicEnumerationData d2 = new DynamicEnumerationData("d2", ParameterType.Parse("6980f4d7-4ad8-4bc0-bacb-4907d922d4e9"));
                t.AddDynamicEnum(d1);
                t.AddDynamicEnum(d2);
                var doc1 = new object();
                var doc2 = new object();
                var A1 = t.Make(d1.TypeId, "test1", Id<Parameter>.Parse("f95d62c4-b6c5-4c31-be69-c0cc3a35697e"), "default1", doc1) as IDynamicEnumParameter;
                var A2 = t.Make(d1.TypeId, "test2", Id<Parameter>.Parse("3ff9251e-bd1f-4543-9719-3b8e5992f8ce"), "default2", doc2) as IDynamicEnumParameter;
                var B1 = t.Make(d2.TypeId, "test3", Id<Parameter>.Parse("117a01a5-21df-4252-8120-8a11fe601ec6"), "default3", doc1) as IDynamicEnumParameter;
                A1.SetValueAction("shane").Value.Redo();
                B1.SetValueAction("not").Value.Redo();
                Assert.That(A1.Options, Is.EquivalentTo(new[] { "shane", "default2" }));
                Assert.That(A2.Options, Is.EquivalentTo(new[] { "shane", "default2" }));
                Assert.That(B1.Options, Is.EquivalentTo(new[] { "not" }));
            }
        }

        /// <summary>
        /// Assert that local dynamic enums of the same type and with the same document share the same source and not the reverse
        /// </summary>
        [Test]
        public static void TestLocalDynamicEnumSource()
        {
            TypeSet t = new TypeSet();
            LocalDynamicEnumerationData d1 = new LocalDynamicEnumerationData("d1", ParameterType.Parse("ed53be0d-c62a-4c0c-9f6e-e92d996bf3df"));
            LocalDynamicEnumerationData d2 = new LocalDynamicEnumerationData("d2", ParameterType.Parse("6980f4d7-4ad8-4bc0-bacb-4907d922d4e9"));
            t.AddLocalDynamicEnum(d1);
            t.AddLocalDynamicEnum(d2);
            var doc1 = new object();
            var doc2 = new object();
            var A1 = t.Make(d1.TypeId, "test1", Id<Parameter>.Parse("f95d62c4-b6c5-4c31-be69-c0cc3a35697e"), "default1", doc1) as IDynamicEnumParameter;
            var A2 = t.Make(d1.TypeId, "test2", Id<Parameter>.Parse("3ff9251e-bd1f-4543-9719-3b8e5992f8ce"), "default2", doc1) as IDynamicEnumParameter;
            var B1 = t.Make(d1.TypeId, "test3", Id<Parameter>.Parse("117a01a5-21df-4252-8120-8a11fe601ec6"), "default3", doc2) as IDynamicEnumParameter;
            var C1 = t.Make(d2.TypeId, "test3", Id<Parameter>.Parse("117a01a5-21df-4252-8120-8a11fe601ec6"), "default3", doc1) as IDynamicEnumParameter;
            A1.SetValueAction("shane").Value.Redo();
            B1.SetValueAction("not").Value.Redo();
            C1.SetValueAction("not either").Value.Redo();
            Assert.That(A1.Options, Is.EquivalentTo(new[] { "shane", "default2" }));
            Assert.That(A2.Options, Is.EquivalentTo(new[] { "shane", "default2" }));
            Assert.That(B1.Options, Is.EquivalentTo(new[] { "not" }));
            Assert.That(C1.Options, Is.EquivalentTo(new[] { "not either" }));
        }

        [Test]
        public static void Test()
        {
            Random random = new Random(0);
            Func<string> randomString = () => "test" + random.NextDouble().ToString(CultureInfo.InvariantCulture);
            Func<Id<Parameter>> randomParameterId = () =>
            {
                byte[] bytes = new byte[16];
                random.NextBytes(bytes);
                return Id<Parameter>.FromGuid(new Guid(bytes)); //Not really a guid but should be unique enough for this test
            };


            List<DecimalData> decimals = new List<DecimalData>();
            List<IntegerData> integers = new List<IntegerData>();
            List<EnumerationData> enums = new List<EnumerationData>();
            List<EnumerationData> hiddenEnums = new List<EnumerationData>();
            List<DynamicEnumerationData> dynamicEnums = new List<DynamicEnumerationData>();
            List<LocalDynamicEnumerationData> localDynamicEnums = new List<LocalDynamicEnumerationData>();
            List<LocalizedStringData> localizedStrings = new List<LocalizedStringData>();
            List<OtherData> others = new List<OtherData>();

            TypeSet t = new TypeSet();

            Action CheckContents = () =>
            {
                foreach (var x in decimals)
                {
                    CheckIs(t, x.TypeId, dec: true);
                    Assert.That(t.GetTypeName(x.TypeId), Is.EqualTo(x.Name));

                    IParameter parameter = CheckBasicMake(randomString(), randomParameterId(), randomString(), t, x.TypeId, null, true); //This type shouldn't care about document
                    Assert.That(parameter, Is.InstanceOf<IDecimalParameter>());
                    IDecimalParameter p = parameter as IDecimalParameter;
                    Assert.That(p.Min, Is.EqualTo(x.Min));
                    Assert.That(p.Max, Is.EqualTo(x.Max));
                }
                foreach (var x in integers)
                {
                    CheckIs(t, x.TypeId, integer: true);
                    Assert.That(t.GetTypeName(x.TypeId), Is.EqualTo(x.Name));

                    IParameter parameter = CheckBasicMake(randomString(), randomParameterId(), randomString(), t, x.TypeId, null, true); //This type shouldn't care about document
                    Assert.That(parameter, Is.InstanceOf<IIntegerParameter>());
                    IIntegerParameter p = parameter as IIntegerParameter;
                    Assert.That(p.Min, Is.EqualTo(x.Min));
                    Assert.That(p.Max, Is.EqualTo(x.Max));
                }
                foreach (var x in enums)
                {
                    CheckIs(t, x.TypeId, enumeration: true);
                    Assert.That(t.GetTypeName(x.TypeId), Is.EqualTo(x.Name));

                    {
                        IParameter parameter = CheckBasicMake(randomString(), randomParameterId(), randomString(), t, x.TypeId, null, true); //This type shouldn't care about document
                        Assert.That(parameter, Is.InstanceOf<IEnumParameter>());
                        IEnumParameter p = parameter as IEnumParameter;
                        Assert.That(p.Options, Is.EquivalentTo(x.Elements.Select(a => a.Guid)));
                    }

                    //Check set parameter creation
                    {
                        IParameter parameter = CheckBasicMake(randomString(), randomParameterId(), randomString(), t, ParameterType.ValueSetType.Of(x.TypeId), null, true); //This type shouldn't care about document
                        Assert.That(parameter, Is.InstanceOf<ISetParameter>());
                        ISetParameter p = parameter as ISetParameter;
                        Assert.That(p.Options, Is.EquivalentTo(x.Elements.Select(a => a.Guid)));
                    }
                }
                foreach (var x in dynamicEnums)
                {
                    CheckIs(t, x.TypeId, dynamicEnum: true);
                    Assert.That(t.GetTypeName(x.TypeId), Is.EqualTo(x.Name));
                    IParameter parameter = CheckBasicMake(randomString(), randomParameterId(), randomString(), t, x.TypeId, null, true); //This type shouldn't care about document
                    Assert.That(parameter, Is.InstanceOf<IDynamicEnumParameter>());
                    IDynamicEnumParameter p = parameter as IDynamicEnumParameter;
                    Assert.That(p.Local, Is.False);
                }
                foreach (var x in localDynamicEnums)
                {
                    CheckIs(t, x.TypeId, localDynamicEnum: true);
                    Assert.That(t.GetTypeName(x.TypeId), Is.EqualTo(x.Name));
                    object document = new object();
                    IParameter parameter = CheckBasicMake(randomString(), randomParameterId(), randomString(), t, x.TypeId, document, true);
                    Assert.That(parameter, Is.InstanceOf<IDynamicEnumParameter>());
                    IDynamicEnumParameter p = parameter as IDynamicEnumParameter;
                    Assert.That(p.Local, Is.True);
                }
                foreach (var x in localizedStrings)
                {
                    CheckIs(t, x.TypeId, localizedString: true);
                    Assert.That(t.GetTypeName(x.TypeId), Is.EqualTo(x.Name));
                    object document = new object();
                    IParameter parameter = CheckBasicMake(randomString(), randomParameterId(), randomString(), t, x.TypeId, document, false);
                    Assert.That(parameter, Is.InstanceOf<ILocalizedStringParameter>());
                }
                foreach (var x in others)
                {
                    CheckIs(t, x.TypeId);
                    Assert.That(t.GetTypeName(x.TypeId), Is.EqualTo(x.Name));
                    string name = randomString();
                    var id = randomParameterId();
                    var def = randomString();
                    object document = new object();
                    IParameter parameter = CheckBasicMake(name, id, def, t, x.TypeId, document, true);
                    Assert.That(x.LastGeneratorParameters, Is.Not.Null);
                    Assert.That(x.LastGeneratorParameters.name, Is.EqualTo(name));
                    Assert.That(x.LastGeneratorParameters.id, Is.EqualTo(id));
                    Assert.That(x.LastGeneratorParameters.defaultValue, Is.EqualTo(def));
                    Assert.That(x.LastGeneratorParameters.document, Is.EqualTo(document));
                    Assert.That(x.LastGeneratorParameters.createdParameter, Is.EqualTo(parameter));
                    x.LastGeneratorParameters = null;
                }

                Assert.That(t.VisibleDecimals, Is.EquivalentTo(decimals));
                Assert.That(t.VisibleDynamicEnums, Is.EquivalentTo(dynamicEnums));
                CheckEnumsMatch(t.VisibleEnums, enums);
                Assert.That(t.VisibleIntegers, Is.EquivalentTo(integers));
                Assert.That(t.VisibleLocalDynamicEnums, Is.EquivalentTo(localDynamicEnums));
                Assert.That(t.VisibleLocalizedStrings, Is.EquivalentTo(localizedStrings));

                var expected = decimals.Select(x => x.TypeId).
                                        Concat(integers.Select(x => x.TypeId)).
                                        Concat(enums.Select(x => x.TypeId)).
                                        Concat(dynamicEnums.Select(x => x.TypeId)).
                                        Concat(localDynamicEnums.Select(x => x.TypeId)).
                                        Concat(localizedStrings.Select(x => x.TypeId)).
                                        Concat(others.Select(x => x.TypeId)).
                                        Concat(enums.Select(x => ParameterType.ValueSetType.Of(x.TypeId))).
                                        Concat(hiddenEnums.Select(x => x.TypeId)). //AllTypes includes hidden types
                                        Concat(hiddenEnums.Select(x => ParameterType.ValueSetType.Of(x.TypeId))).
                                        ToList();

                Assert.That(t.AllTypes, Is.EquivalentTo(expected));
            };

            ParameterType modifiedType = null;
            Action<ParameterType> modifiedCallback = x => { Assert.That(modifiedType, Is.Null); modifiedType = x; };
            t.Modified += modifiedCallback;

            CheckDecimals(decimals, t, CheckContents, ref modifiedType);

            CheckIntegers(integers, t, CheckContents, ref modifiedType);

            CheckEnums(enums, t, CheckContents, ref modifiedType, false);

            CheckEnums(hiddenEnums, t, CheckContents, ref modifiedType, true);

            CheckDynamicEnums(dynamicEnums, t, CheckContents, ref modifiedType);

            CheckLocalDynamicEnums(localDynamicEnums, t, CheckContents, ref modifiedType);

            CheckLocalizedStrings(localizedStrings, t, CheckContents, ref modifiedType);

            CheckOthers(others, t, CheckContents, ref modifiedType);
        }

        private static void CheckIntegers(List<IntegerData> integers, TypeSet t, Action CheckContents, ref ParameterType modifiedType)
        {
            IntegerData Integer1 = new IntegerData("int1", ParameterType.Parse("e051bfd8-d9d8-4f14-bd39-0ec1a9e9073b"), 456, 123);
            IntegerData Integer2 = new IntegerData("repeat", ParameterType.Parse("4427eb13-e799-4959-86a8-31a2dbd71b03"), 456, 123);
            IntegerData Integer3 = new IntegerData("repeat", ParameterType.Parse("e8aa3cef-8b60-4b08-bb8d-e0a9b2a2085e"), 1, 0);

            var a = Integer1;
            t.AddInteger(a);
            integers.Add(a);
            Assert.That(modifiedType, Is.EqualTo(a.TypeId));
            modifiedType = null;
            CheckContents();

            a = Integer2;
            t.AddInteger(a);
            integers.Add(a);
            Assert.That(modifiedType, Is.EqualTo(a.TypeId));
            modifiedType = null;
            CheckContents();

            a = Integer1;
            t.Remove(a.TypeId);
            integers.Remove(a);
            Assert.That(modifiedType, Is.EqualTo(a.TypeId));
            modifiedType = null;
            CheckContents();

            a = Integer3;
            t.AddInteger(a);
            integers.Add(a);
            Assert.That(modifiedType, Is.EqualTo(a.TypeId));
            modifiedType = null;
            CheckContents();

            IntegerData Integer2Replacement = new IntegerData("Integer2 replacement", ParameterType.Parse("4427eb13-e799-4959-86a8-31a2dbd71b03"), 234, -234);
            t.ModifyInteger(Integer2Replacement);
            integers[integers.IndexOf(Integer2)] = Integer2Replacement;
            Assert.That(modifiedType, Is.EqualTo(Integer2Replacement.TypeId));
            modifiedType = null;
            CheckContents();

            IntegerData Integer3Replacement = new IntegerData("replacement integer 3", ParameterType.Parse("e8aa3cef-8b60-4b08-bb8d-e0a9b2a2085e"), 1, 0);
            t.RenameType(Integer3.TypeId, Integer3Replacement.Name);
            integers[integers.IndexOf(Integer3)] = Integer3Replacement;
            Assert.That(modifiedType, Is.EqualTo(Integer3Replacement.TypeId));
            modifiedType = null;
            CheckContents();
        }

        private static void CheckLocalizedStrings(List<LocalizedStringData> localizedStrings, TypeSet t, Action CheckContents, ref ParameterType modifiedType)
        {
            LocalizedStringData string1 = new LocalizedStringData("string1", ParameterType.Parse("03d7b9f3-cc7e-4639-b809-20f92860c041"));
            LocalizedStringData string2 = new LocalizedStringData("repeat", ParameterType.Parse("3f0af919-8c61-4863-9039-9d4b59a2b2e6"));
            LocalizedStringData string3 = new LocalizedStringData("repeat", ParameterType.Parse("3771aec9-a2ea-4f06-a426-fb245de56199"));

            var a = string1;
            t.AddLocalizedString(a);
            localizedStrings.Add(a);
            Assert.That(modifiedType, Is.EqualTo(a.TypeId));
            modifiedType = null;
            CheckContents();

            a = string2;
            t.AddLocalizedString(a);
            localizedStrings.Add(a);
            Assert.That(modifiedType, Is.EqualTo(a.TypeId));
            modifiedType = null;
            CheckContents();

            a = string1;
            t.Remove(a.TypeId);
            localizedStrings.Remove(a);
            Assert.That(modifiedType, Is.EqualTo(a.TypeId));
            modifiedType = null;
            CheckContents();

            a = string3;
            t.AddLocalizedString(a);
            localizedStrings.Add(a);
            Assert.That(modifiedType, Is.EqualTo(a.TypeId));
            modifiedType = null;
            CheckContents();

            LocalizedStringData string2Replacement = new LocalizedStringData("string2 replacement", string2.TypeId);
            t.ModifyLocalizedString(string2Replacement);
            localizedStrings[localizedStrings.IndexOf(string2)] = string2Replacement;
            Assert.That(modifiedType, Is.EqualTo(string2Replacement.TypeId));
            modifiedType = null;
            CheckContents();

            LocalizedStringData string3Replacement = new LocalizedStringData("replacement string 3", string3.TypeId);
            t.RenameType(string3.TypeId, string3Replacement.Name);
            localizedStrings[localizedStrings.IndexOf(string3)] = string3Replacement;
            Assert.That(modifiedType, Is.EqualTo(string3Replacement.TypeId));
            modifiedType = null;
            CheckContents();
        }

        private static void CheckDecimals(List<DecimalData> decimals, TypeSet t, Action CheckContents, ref ParameterType modifiedType)
        {
            DecimalData Decimal1 = new DecimalData("decimal1", ParameterType.Parse("feea56b3-d51b-4c60-bb81-96c6e543e096"), 12, -1);
            DecimalData Decimal2 = new DecimalData("decimal2", ParameterType.Parse("1275ba4a-ff58-492c-ae6f-9e915eefb796"), 15, -1);
            DecimalData Decimal3 = new DecimalData("decimal3", ParameterType.Parse("729e3aa4-ce76-4dbd-95b8-549dbdec035a"), -123, -1231);

            var a = Decimal1;
            t.AddDecimal(a);
            decimals.Add(a);
            Assert.That(modifiedType, Is.EqualTo(a.TypeId));
            modifiedType = null;
            CheckContents();

            a = Decimal2;
            t.AddDecimal(a);
            decimals.Add(a);
            Assert.That(modifiedType, Is.EqualTo(a.TypeId));
            modifiedType = null;
            CheckContents();

            a = Decimal1;
            t.Remove(a.TypeId);
            decimals.Remove(a);
            Assert.That(modifiedType, Is.EqualTo(a.TypeId));
            modifiedType = null;
            CheckContents();

            a = Decimal3;
            t.AddDecimal(a);
            decimals.Add(a);
            Assert.That(modifiedType, Is.EqualTo(a.TypeId));
            modifiedType = null;
            CheckContents();

            DecimalData Decimal2Replacement = new DecimalData("decimal 2 replacement", ParameterType.Parse("1275ba4a-ff58-492c-ae6f-9e915eefb796"), 345, -345);
            t.ModifyDecimal(Decimal2Replacement);
            decimals[decimals.IndexOf(Decimal2)] = Decimal2Replacement;
            Assert.That(modifiedType, Is.EqualTo(Decimal2Replacement.TypeId));
            modifiedType = null;
            CheckContents();

            DecimalData Decimal3Replacement = new DecimalData("Decimal3Replaced", ParameterType.Parse("729e3aa4-ce76-4dbd-95b8-549dbdec035a"), -123, -1231);
            t.RenameType(Decimal3.TypeId, Decimal3Replacement.Name);
            decimals[decimals.IndexOf(Decimal3)] = Decimal3Replacement;
            Assert.That(modifiedType, Is.EqualTo(Decimal3Replacement.TypeId));
            modifiedType = null;
            CheckContents();
        }

        private static void CheckEnums(List<EnumerationData> enums, TypeSet t, Action CheckContents, ref ParameterType modifiedType, bool hidden)
        {
            EnumerationData Enumeration1 = new EnumerationData("enum1", ParameterType.Parse(hidden ? "e25f2768-ac00-4320-a558-12b51d85d771" : "f70fb4d1-59f5-4618-9bc4-8da19f943da2"), new[] { new EnumerationData.Element("value1", Guid.Parse("9ee98b1e-fb29-42c5-8a26-903a8aecacc0")),
                                                                                                                                                                                               new EnumerationData.Element("value2", Guid.Parse("a3ed3b47-9985-4f17-b791-4155cc7196f4")) });
            EnumerationData Enumeration2 = new EnumerationData("enum2", ParameterType.Parse(hidden ? "19bf99ff-98f2-4ab3-96d6-1f7e61d98ae7" : "a532c7c0-2806-4def-9fb2-5182b6803cbe"), new[] { new EnumerationData.Element("value1", Guid.Parse("e80fd4c3-eb10-4309-b683-79c3675b6fb8")),
                                                                                                                                                                                               new EnumerationData.Element("value2", Guid.Parse("c0b0a09e-4825-479e-87dc-599bdacf045f")) });
            EnumerationData Enumeration3 = new EnumerationData("enum3", ParameterType.Parse(hidden ? "b78e53cd-717b-446f-9ab2-fbbf5c41b067" : "aa274a73-95c1-4272-bb23-9c9cabeb183e"), new[] { new EnumerationData.Element("asd",    Guid.Parse("27b59086-7e9b-4b7f-8796-a7a23eb727c1")),
                                                                                                                                                                                               new EnumerationData.Element("sdasd",  Guid.Parse("b08cf9e0-9d77-42cf-9f1c-a21def2b79d3")),
                                                                                                                                                                                               new EnumerationData.Element("sdasd2", Guid.Parse("4f73eaad-86fe-4cbb-adbc-016724399ae5")) });

            var a = Enumeration1;
            t.AddEnum(a, hidden);
            enums.Add(a);
            Assert.That(modifiedType, Is.EqualTo(a.TypeId));
            modifiedType = null;
            CheckContents();

            a = Enumeration2;
            t.AddEnum(a, hidden);
            enums.Add(a);
            Assert.That(modifiedType, Is.EqualTo(a.TypeId));
            modifiedType = null;
            CheckContents();

            a = Enumeration1;
            t.Remove(a.TypeId);
            enums.Remove(a);
            Assert.That(modifiedType, Is.EqualTo(a.TypeId));
            modifiedType = null;
            CheckContents();

            a = Enumeration3;
            t.AddEnum(a, hidden);
            enums.Add(a);
            Assert.That(modifiedType, Is.EqualTo(a.TypeId));
            modifiedType = null;
            CheckContents();

            EnumerationData Enum2Replacement = new EnumerationData("repeat", Enumeration2.TypeId, new[] { new EnumerationData.Element("asd", Guid.Parse("24be3852-b34f-4f20-ab72-3e391e939872")) });
            t.ModifyEnum(Enum2Replacement);
            enums[enums.IndexOf(Enumeration2)] = Enum2Replacement;
            Assert.That(modifiedType, Is.EqualTo(Enum2Replacement.TypeId));
            modifiedType = null;
            CheckContents();

            EnumerationData Enumeration3Replacement = new EnumerationData("enum3 replacement", Enumeration3.TypeId, new[] { new EnumerationData.Element("asd",    Guid.Parse("27b59086-7e9b-4b7f-8796-a7a23eb727c1")),
                                                                                                                            new EnumerationData.Element("sdasd",  Guid.Parse("b08cf9e0-9d77-42cf-9f1c-a21def2b79d3")),
                                                                                                                            new EnumerationData.Element("sdasd2", Guid.Parse("4f73eaad-86fe-4cbb-adbc-016724399ae5")) });
            t.RenameType(Enumeration3.TypeId, Enumeration3Replacement.Name);
            enums[enums.IndexOf(Enumeration3)] = Enumeration3Replacement;
            Assert.That(modifiedType, Is.EqualTo(Enumeration3Replacement.TypeId));
            modifiedType = null;
            CheckContents();
        }

        private static void CheckDynamicEnums(List<DynamicEnumerationData> dynamicEnums, TypeSet t, Action CheckContents, ref ParameterType modifiedType)
        {
            DynamicEnumerationData DE1 = new DynamicEnumerationData("DE1", ParameterType.Parse("89e54648-0572-4de2-82a6-3cdf5ea8aebc"));
            DynamicEnumerationData DE2 = new DynamicEnumerationData("DE2", ParameterType.Parse("17b8ce6a-da16-459e-ab58-c77f62c5eba4"));
            DynamicEnumerationData DE3 = new DynamicEnumerationData("DE3", ParameterType.Parse("882d169d-b697-4ff3-8f4d-d0a3b18549c7"));

            t.AddDynamicEnum(DE1);
            dynamicEnums.Add(DE1);
            Assert.That(modifiedType, Is.EqualTo(DE1.TypeId));
            modifiedType = null;
            CheckContents();

            t.Remove(DE1.TypeId);
            dynamicEnums.Remove(DE1);
            Assert.That(modifiedType, Is.EqualTo(DE1.TypeId));
            modifiedType = null;
            CheckContents();

            t.AddDynamicEnum(DE2);
            dynamicEnums.Add(DE2);
            Assert.That(modifiedType, Is.EqualTo(DE2.TypeId));
            modifiedType = null;
            CheckContents();

            t.AddDynamicEnum(DE3);
            dynamicEnums.Add(DE3);
            Assert.That(modifiedType, Is.EqualTo(DE3.TypeId));
            modifiedType = null;
            CheckContents();

            DynamicEnumerationData DE3Modified = new DynamicEnumerationData("DE3Modified", ParameterType.Parse("882d169d-b697-4ff3-8f4d-d0a3b18549c7"));
            t.RenameType(DE3.TypeId, DE3Modified.Name);
            dynamicEnums[dynamicEnums.IndexOf(DE3)] = DE3Modified;
            Assert.That(modifiedType, Is.EqualTo(DE3Modified.TypeId));
            modifiedType = null;
            CheckContents();
        }

        private static void CheckLocalDynamicEnums(List<LocalDynamicEnumerationData> dynamicEnums, TypeSet t, Action CheckContents, ref ParameterType modifiedType)
        {
            LocalDynamicEnumerationData DE1 = new LocalDynamicEnumerationData("DE1", ParameterType.Parse("5200fb71-c373-457b-be1d-e11b45ce9d9a"));
            LocalDynamicEnumerationData DE2 = new LocalDynamicEnumerationData("DE2", ParameterType.Parse("ad9e7fd8-a86b-4059-b28a-697774aa3b63"));
            LocalDynamicEnumerationData DE3 = new LocalDynamicEnumerationData("DE3", ParameterType.Parse("f29f7131-e6b7-439b-b1cd-e09a6e1b9897"));

            t.AddLocalDynamicEnum(DE1);
            dynamicEnums.Add(DE1);
            Assert.That(modifiedType, Is.EqualTo(DE1.TypeId));
            modifiedType = null;
            CheckContents();

            t.Remove(DE1.TypeId);
            dynamicEnums.Remove(DE1);
            Assert.That(modifiedType, Is.EqualTo(DE1.TypeId));
            modifiedType = null;
            CheckContents();

            t.AddLocalDynamicEnum(DE2);
            dynamicEnums.Add(DE2);
            Assert.That(modifiedType, Is.EqualTo(DE2.TypeId));
            modifiedType = null;
            CheckContents();

            t.AddLocalDynamicEnum(DE3);
            dynamicEnums.Add(DE3);
            Assert.That(modifiedType, Is.EqualTo(DE3.TypeId));
            modifiedType = null;
            CheckContents();

            LocalDynamicEnumerationData DE3Modified = new LocalDynamicEnumerationData("DE3Modified", ParameterType.Parse("f29f7131-e6b7-439b-b1cd-e09a6e1b9897"));
            t.RenameType(DE3.TypeId, DE3Modified.Name);
            dynamicEnums[dynamicEnums.IndexOf(DE3)] = DE3Modified;
            Assert.That(modifiedType, Is.EqualTo(DE3Modified.TypeId));
            modifiedType = null;
            CheckContents();
        }

        private static void CheckOthers(List<OtherData> others, TypeSet t, Action CheckContents, ref ParameterType modifiedType)
        {
            OtherData other1 = new OtherData(ParameterType.Parse("786abbeb-02eb-44a6-bcc9-a75817db3750"), "other1");

            t.AddOther(other1.TypeId, other1.Name, other1.Factory);
            others.Add(other1);
            Assert.That(modifiedType, Is.EqualTo(other1.TypeId));
            modifiedType = null;
            CheckContents();

        }

        private static void CheckEnumsMatch(IEnumerable<EnumerationData> actual, List<EnumerationData> expected)
        {
            var ordered1 = expected.OrderBy(a => a.TypeId.Guid);
            var ordered2 = actual.OrderBy(a => a.TypeId.Guid);
            foreach (var pair in ordered1.Zip(ordered2, (a, b) => Tuple.Create(a, b)))
            {
                Assert.That(pair.Item2.TypeId, Is.EqualTo(pair.Item1.TypeId));
                Assert.That(pair.Item2.Name, Is.EqualTo(pair.Item1.Name));
                Assert.That(pair.Item2.Elements, Is.EquivalentTo(pair.Item1.Elements));
            }
        }

        private static IParameter CheckBasicMake(string name, Id<Parameter> id, string def, TypeSet t, ParameterType typeId, object document, bool useDefault)
        {
            IParameter parameter = t.Make(typeId, name, id, def, document);
            Assert.That(parameter.TypeId, Is.EqualTo(typeId));
            Assert.That(parameter.Name, Is.EqualTo(name));
            Assert.That(parameter.Id, Is.EqualTo(id));
            if (useDefault)
                Assert.That(parameter.ValueAsString(), Is.EqualTo(def));
            else
                Assert.That(parameter.ValueAsString(), Is.EqualTo(""));
            return parameter;
        }
    }
}
