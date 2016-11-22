using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Conversation.Serialization;
using System.Xml.Linq;
using Conversation;
using System.IO;
using Utilities;
using System.Collections.ObjectModel;

namespace Tests.ConversationSerialization
{
    public static class XmlConversationTest
    {
        class UIRawData
        {
            public string Value;
        }

        class EditorData
        {
            public string Value;
        }

        class UIRawDataSerializer : Utilities.ISerializerDeserializerXml<UIRawData, UIRawData>
        {
            public UIRawData Read(XElement node)
            {
                return new UIRawData { Value = node.Element("UIRawData").Attribute("Value").Value };
            }

            public void Write(UIRawData data, XElement node)
            {
                node.Add(new XElement("UIRawData", new XAttribute("Value", data.Value)));
            }
        }

        class EditorDataSerializer : Utilities.ISerializerDeserializerXml<EditorData, EditorData>
        {
            public EditorData Read(XElement node)
            {
                return new EditorData { Value = node.Element("EditorData").Attribute("Value").Value };
            }

            public void Write(EditorData data, XElement node)
            {
                node.Add(new XElement("EditorData", new XAttribute("Value", data.Value)));
            }
        }

        private static void AssertDataEqual(XmlGraphData<UIRawData, EditorData> expected, XmlGraphData<UIRawData, EditorData> actual, IDataSource source)
        {
            Assert.That(actual.EditorData.Value, Is.EqualTo(expected.EditorData.Value));
            Assert.That(actual.Errors, Is.Empty);

            var actualNodes = actual.Nodes.ToList();
            foreach (var expectedNode in expected.Nodes)
            {
                //Get the corresponding actualNode
                var x = actualNodes.Where(n => n.GraphData.NodeId == expectedNode.GraphData.NodeId);
                Assert.That(x.Count(), Is.EqualTo(1));
                var actualNode = x.First();
                actualNodes.Remove(actualNode);

                Assert.That(actualNode.UIData.Value, Is.EqualTo(expectedNode.UIData.Value));

                var actualConnectors = actualNode.GraphData.Connectors.ToList();
                foreach (var connector in expectedNode.GraphData.Connectors)
                {
                    //Get the corresponding connector
                    var y = actualConnectors.Where(c => c.Id == connector.Id);
                    Assert.That(y.Count(), Is.EqualTo(1));
                    var actualConnector = y.First();
                    actualConnectors.Remove(actualConnector);

                    Assert.That(actualConnector.Connections.Select(c => c.Id), Is.EquivalentTo(connector.Connections.Select(c => c.Id)));
                }
                Assert.That(actualConnectors, Is.Empty);

                var actualParameters = actualNode.GraphData.Parameters.ToList();
                foreach (var parameter in expectedNode.GraphData.Parameters)
                {
                    var y = actualParameters.Where(p => p.Id == parameter.Id);
                    Assert.That(y.Count(), Is.EqualTo(1));
                    var actualParameter = y.First();
                    actualParameters.Remove(actualParameter);

                    Assert.That(actualParameter.Corrupted, Is.False);
                    Assert.That(actualParameter.ValueAsString(), Is.EqualTo(parameter.ValueAsString()));
                }
                Assert.That(actualParameters, Is.Empty);
            }
            Assert.That(actualNodes, Is.Empty);
        }

        class GraphData : IConversationNodeData
        {
            public static Id<NodeTypeTemp> TYPE1 = Id<NodeTypeTemp>.Parse("7055c8ef-3b99-4535-b2b5-1f182cbf59c9");
            public static Id<NodeTypeTemp> TYPE2 = Id<NodeTypeTemp>.Parse("d7cc4188-6f78-4220-a641-0e58dc667ec6");
            public static Id<NodeTypeTemp> TYPE3 = Id<NodeTypeTemp>.Parse("c332430b-fb79-4f6b-8661-c190bbac1a71");

            class DummyRules : IConnectionRules
            {
                public static DummyRules Instance { get; } = new DummyRules();
                public bool CanConnect(Id<TConnectorDefinition> a, Id<TConnectorDefinition> b)
                {
                    return true;
                }
            }

            public class DummyEnumeration : IEnumeration
            {
                public Either<string, Guid> DefaultValue
                {
                    get
                    {
                        return Options.ElementAt(0);
                    }
                }

                public IEnumerable<Guid> Options
                {
                    get
                    {
                        yield return System.Guid.Parse("eedf04dd-6ef4-4a07-b5bb-6a134e3b3bc1");
                        yield return System.Guid.Parse("8df26e00-2649-40da-bfa4-a9a1b7ae67f1");
                    }
                }

                public ParameterType TypeId
                {
                    get
                    {
                        return ParameterType.Basic.Parse("5e85aa7a-069e-4006-abc7-33abd504df28");
                    }
                }

                public string GetName(Guid id)
                {
                    if (id == Options.ElementAt(0))
                        return "First element";
                    else if (id == Options.ElementAt(1))
                        return "Second element";
                    else
                        return null;
                }
            }

            public AudioParameter AudioParameter { get; } = null;
            public BooleanParameter BooleanParameter { get; } = null;
            public DecimalParameter DecimalParameter { get; } = null;
            public DynamicEnumParameter DynamicEnumParameter { get; } = null;
            public EnumParameter EnumParameter { get; } = null;
            public IntegerParameter IntegerParameter { get; } = null;
            public LocalizedStringParameter LocalizedStringParameter { get; } = null;
            public SetParameter SetParameter { get; } = null;
            public StringParameter StringParameter { get; } = null;
            public StringParameter StringParameter2 { get; } = null;

            public GraphData(Id<NodeTemp> nodeId, Id<NodeTypeTemp> nodeTypeId, IEnumerable<NodeDataGeneratorParameterData> parameterData)
            {
                NodeId = nodeId;
                NodeTypeId = nodeTypeId;

                var allConnectorsDefinition = new ConnectorDefinitionData(null, null, null, null, false);

                var outputs = new List<Output>();
                var parameters = new List<IParameter>();

                DecimalParameter.Definition d = new DecimalParameter.Definition(null, null);
                ParameterType decimalType = ParameterType.Basic.Parse("721796b6-a242-4723-82e9-35201097e675");
                ParameterType dynamicEnumType = ParameterType.Basic.Parse("6d2d52c8-5934-4ba8-8d4e-7081fe57f662");
                DynamicEnumParameter.Source source = new DynamicEnumParameter.Source();
                IEnumeration enumeration = new DummyEnumeration();
                ParameterType integerEnumType = ParameterType.Basic.Parse("de108fdb-db50-4cd5-aad5-0ea791f04721");
                IntegerParameter.Definition i = new IntegerParameter.Definition(null, null);

                AudioParameter = new AudioParameter("Audio", Id<Parameter>.Parse("3ac8d0ca-c9f6-4e06-b18c-c1366e1af7d3"));
                BooleanParameter = new BooleanParameter("Boolean", Id<Parameter>.Parse("0e12e8e3-4c95-43a5-a733-d2d1fbbb780c"), "false");
                DecimalParameter = new DecimalParameter("Decimal", Id<Parameter>.Parse("765e616a-f165-4053-a15c-14ed593429af"), decimalType, d, "1.0");
                DynamicEnumParameter = new DynamicEnumParameter("DynamicEnum", Id<Parameter>.Parse("7c5b019c-79d0-4ef0-b848-0a2c68908f34"), source, dynamicEnumType, "shnae", false);
                EnumParameter = new EnumParameter("Enum", Id<Parameter>.Parse("e576713b-5d45-48d0-8a4e-661f1fedcafd"), enumeration, enumeration.DefaultValue.ToString());
                IntegerParameter = new IntegerParameter("Int", Id<Parameter>.Parse("275d75f3-fe4e-42b1-bfaf-e841ba591999"), integerEnumType, i, "1");
                LocalizedStringParameter = new LocalizedStringParameter("Localized stirng", Id<Parameter>.Parse("f332e619-e9a3-421f-9851-d95a00b62da9"));
                SetParameter = new SetParameter("Set", Id<Parameter>.Parse("2d6235ea-c8a1-447a-b9d8-692f6329be33"), enumeration, null);
                StringParameter = new StringParameter("string", Id<Parameter>.Parse("4752d30e-e1ab-47ba-bc15-b2e6ecfa5416"));
                StringParameter2 = new StringParameter("string2", Id<Parameter>.Parse("dcd4a349-b0a8-4fa3-8989-2d10469b1a17"));

                if (nodeTypeId == TYPE1)
                {
                    outputs.Add(new Output(Id<TConnector>.Parse("0956c9d3-c230-49a2-874a-7e3747b58cff"), allConnectorsDefinition, this, null, DummyRules.Instance));
                    parameters.Add(AudioParameter);
                    parameters.Add(BooleanParameter);
                    parameters.Add(DecimalParameter);
                    parameters.Add(DynamicEnumParameter);
                    parameters.Add(EnumParameter);
                }
                else if (nodeTypeId == TYPE2)
                {
                    outputs.Add(new Output(Id<TConnector>.Parse("da2b4ded-378e-4484-89f0-1328a42f00e3"), allConnectorsDefinition, this, null, DummyRules.Instance));
                    outputs.Add(new Output(Id<TConnector>.Parse("2bf2ca93-6b81-4a9a-814f-809a8bef332f"), allConnectorsDefinition, this, null, DummyRules.Instance));

                    parameters.Add(IntegerParameter);
                    parameters.Add(LocalizedStringParameter);
                    parameters.Add(SetParameter);
                    parameters.Add(StringParameter);
                }
                else if (nodeTypeId == TYPE3)
                {
                    outputs.Add(new Output(Id<TConnector>.Parse("c3f67c87-a3fd-428d-90a2-90cb87906eb2"), allConnectorsDefinition, this, null, DummyRules.Instance));
                    parameters.Add(StringParameter);
                    parameters.Add(StringParameter2);
                }
                else
                {
                    Assert.Fail("Unexpected Id");
                }

                if (parameterData != null)
                {
                    foreach (var data in parameterData)
                    {
                        parameters.Where(p => p.Id == data.Guid).Single().TryDeserialiseValue(data.Value);
                    }
                }

                Connectors = outputs;
                Parameters = parameters;
            }

            public IReadOnlyList<NodeData.ConfigData> Config
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IEnumerable<Output> Connectors { get; }
            public string Name
            {
                get
                {
                    return "GRaphData";
                }
            }

            public Id<NodeTemp> NodeId { get; }

            public Id<NodeTypeTemp> NodeTypeId { get; }

            public IEnumerable<IParameter> Parameters { get; }

            public event Action Linked
            {
                add { }
                remove { }
            }

            public void ChangeId(Id<NodeTemp> id)
            {
                throw new NotImplementedException();
            }

            public SimpleUndoPair RemoveUnknownParameter(UnknownParameter p)
            {
                throw new NotImplementedException();
            }
        }

        public class DataSource : IDataSource
        {
            public INodeType Nodes
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IEnumerable<ParameterType> ParameterTypes
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public Guid GetCategory(Id<NodeTypeTemp> type)
            {
                throw new NotImplementedException();
            }

            private class Generator : INodeDataGenerator
            {

                public Generator(Id<NodeTypeTemp> guid)
                {
                    Guid = guid;
                }

                public IReadOnlyList<NodeData.ConfigData> Config
                {
                    get
                    {
                        throw new NotImplementedException();
                    }
                }

                public Id<NodeTypeTemp> Guid { get; }

                public string Name
                {
                    get
                    {
                        throw new NotImplementedException();
                    }
                }

                public IConversationNodeData Generate(Id<NodeTemp> id, IEnumerable<NodeDataGeneratorParameterData> parameters, object document)
                {
                    return new GraphData(id, Guid, parameters);
                }

                public ReadOnlyCollection<NodeData.ConfigData> GetParameterConfig(Id<Parameter> parameterId)
                {
                    throw new NotImplementedException();
                }
            }

            public INodeDataGenerator GetNode(Id<NodeTypeTemp> guid)
            {
                return new Generator(guid);
            }

            public DynamicEnumParameter.Source GetSource(ParameterType type, object newSourceId)
            {
                throw new NotImplementedException();
            }

            public string GetTypeName(ParameterType type)
            {
                throw new NotImplementedException();
            }

            public bool IsAutoCompleteNode(Id<NodeTypeTemp> id)
            {
                throw new NotImplementedException();
            }

            public bool IsCategoryDefinition(Id<NodeTypeTemp> id)
            {
                throw new NotImplementedException();
            }

            public bool IsConnectorDefinition(Id<NodeTypeTemp> id)
            {
                throw new NotImplementedException();
            }

            public bool IsDecimal(ParameterType type)
            {
                throw new NotImplementedException();
            }

            public bool IsDynamicEnum(ParameterType type)
            {
                throw new NotImplementedException();
            }

            public bool IsEnum(ParameterType type)
            {
                throw new NotImplementedException();
            }

            public bool IsInteger(ParameterType type)
            {
                throw new NotImplementedException();
            }

            public bool IsLocalDynamicEnum(ParameterType type)
            {
                throw new NotImplementedException();
            }

            public bool IsNodeDefinition(Id<NodeTypeTemp> id)
            {
                throw new NotImplementedException();
            }

            public bool IsTypeDefinition(Id<NodeTypeTemp> id)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Verify that all data is retained when a document with no errors is written and then read
        /// </summary>
        [Test]
        public static void TestConsistency()
        {
            IDataSource source = new DataSource();
            Func<Id<NodeTypeTemp>, bool> filter = a => true;

            XmlConversation<UIRawData, EditorData>.Serializer serializer = new XmlConversation<UIRawData, EditorData>.Serializer(new UIRawDataSerializer(), new EditorDataSerializer());
            XmlConversation<UIRawData, EditorData>.Deserializer deserializer = new XmlConversation<UIRawData, EditorData>.Deserializer(source, new UIRawDataSerializer(), new EditorDataSerializer(), filter);

            object document = new object();

            //Set up nodes
            var nodes = new[]
            {
                source.GetNode(GraphData.TYPE1).Generate(Id<NodeTemp>.Parse("65ea4337-3687-4b0c-a2c5-2c06109b5d86"), new NodeDataGeneratorParameterData[0], document)as GraphData,
                source.GetNode(GraphData.TYPE1).Generate(Id<NodeTemp>.Parse("028405e3-9668-4d3c-bfa7-816a08f62f5e"), new NodeDataGeneratorParameterData[0], document)as GraphData,
                source.GetNode(GraphData.TYPE2).Generate(Id<NodeTemp>.Parse("2362552e-cce6-407b-abc5-005f0450ad25"), new NodeDataGeneratorParameterData[0], document)as GraphData,
                source.GetNode(GraphData.TYPE3).Generate(Id<NodeTemp>.Parse("553daadb-bf55-401c-975c-a069fd54ac12"), new NodeDataGeneratorParameterData[0], document)as GraphData,
                source.GetNode(GraphData.TYPE3).Generate(Id<NodeTemp>.Parse("3a5c9ebb-2aba-4f67-94e1-7e5575b6a369"), new NodeDataGeneratorParameterData[0], document)as GraphData,
            };

            //Set up connections
            nodes[0].Connectors.First().ConnectTo(nodes[1].Connectors.First(), false);
            nodes[0].Connectors.First().ConnectTo(nodes[2].Connectors.First(), false);
            nodes[2].Connectors.ElementAt(1).ConnectTo(nodes[3].Connectors.First(), false);
            nodes[2].Connectors.ElementAt(1).ConnectTo(nodes[3].Connectors.First(), false);

            //Set up parameters
            GraphData.DummyEnumeration referenceEnumeration = new GraphData.DummyEnumeration();
            nodes[0].AudioParameter.SetValueAction(new Audio("anaudiopath")).Value.Redo();
            nodes[0].BooleanParameter.SetValueAction(false)?.Redo(); //It defaults to null so the change action ends up being null
            nodes[0].DecimalParameter.SetValueAction(2345.7342m).Value.Redo();
            nodes[0].DynamicEnumParameter.SetValueAction("djh")?.Redo();
            nodes[0].EnumParameter.SetValueAction(referenceEnumeration.Options.ElementAt(1)).Value.Redo();
            nodes[1].AudioParameter.SetValueAction(new Audio("anaudiopath")).Value.Redo();
            nodes[1].BooleanParameter.SetValueAction(true).Value.Redo();
            nodes[1].DecimalParameter.SetValueAction(986923.24m).Value.Redo();
            nodes[1].DynamicEnumParameter.SetValueAction("fdjngb").Value.Redo();
            nodes[1].EnumParameter.SetValueAction(referenceEnumeration.Options.ElementAt(0)).Value.Redo();
            nodes[2].IntegerParameter.SetValueAction(354).Value.Redo();
            nodes[2].LocalizedStringParameter.SetValueAction(Id<LocalizedText>.Parse("fd9b5426-a296-43bc-84a8-c633b9e88d3c")).Value.Redo();
            nodes[2].SetParameter.SetValueAction(new ReadonlySet<Guid>(referenceEnumeration.Options)).Value.Redo();
            nodes[2].StringParameter.SetValueAction("lhgalkhalsd").Value.Redo();
            nodes[3].StringParameter.SetValueAction("ljlkdskd").Value.Redo();
            nodes[3].StringParameter2.SetValueAction("lkjgaslvgokvagaviua").Value.Redo();
            nodes[4].StringParameter.SetValueAction("ljlkdsdfsdfsdskd").Value.Redo();
            nodes[4].StringParameter2.SetValueAction("lkjgasafafflvgokvagaviua").Value.Redo();

            var nodeData = new GraphAndUI<UIRawData>[]
            {
                new GraphAndUI<UIRawData>(nodes[0], new UIRawData { Value = "test ui raw data 0" }),
                new GraphAndUI<UIRawData>(nodes[1], new UIRawData { Value = "test ui raw data 1" }),
                new GraphAndUI<UIRawData>(nodes[2], new UIRawData { Value = "test ui raw data 2" }),
                new GraphAndUI<UIRawData>(nodes[3], new UIRawData { Value = "test ui raw data 3" }),
                new GraphAndUI<UIRawData>(nodes[4], new UIRawData { Value = "test ui raw data 4" }),
            };
            var data = new XmlGraphData<UIRawData, EditorData>(nodeData, new EditorData { Value = "test editor data" });

            using (var stream = new MemoryStream())
            {
                serializer.Write(data, stream);
                stream.Position = 0;
                var resultData = deserializer.Read(stream);
                AssertDataEqual(data, resultData, source);
            }
        }
    }
}
