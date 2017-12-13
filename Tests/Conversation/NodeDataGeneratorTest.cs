using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Conversation;
using System.Collections.ObjectModel;
using System.Collections;

namespace Tests.Conversation
{
    public static class NodeDataGeneratorTest
    {
        class Rules : IConnectionRules
        {
            public static Rules Instance { get; } = new Rules();

            private Rules()
            {
            }

            public bool CanConnect(Id<TConnectorDefinition> a, Id<TConnectorDefinition> b)
            {
                throw new NotImplementedException();
            }
        }

        class DummyParameter : IParameter
        {
            public DummyParameter(string name, Id<Parameter> id, string value, ParameterType typeId)
            {
                Name = name;
                Id = id;
                m_value = value;
                TypeId = typeId;
            }

            public Id<Parameter> Id { get; }

            public bool Corrupted
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public string Name { get; }

            public ParameterType TypeId { get; }

            private string m_value;

            public string DisplayValue(Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localize)
            {
                return m_value;
            }

            public void TryDeserialiseValue(string value)
            {
                m_value = value;
            }

            public string ValueAsString()
            {
                return m_value;
            }
        }

        class OutputComparer : IEqualityComparer
        {
            public new bool Equals(object x, object y)
            {
                var output = x as Output;
                if (output != null)
                {
                    return Compare(output, y as Tuple<NodeData.ConnectorData, ConnectorDefinitionData>);
                }
                else
                {
                    return Compare(y as Output, x as Tuple<NodeData.ConnectorData, ConnectorDefinitionData>);
                }
            }

            private static bool Compare(Output output, Tuple<NodeData.ConnectorData, ConnectorDefinitionData> connectorData)
            {
                bool equal = true;
                equal &= Is.EqualTo(connectorData.Item2).Matches(output.Definition);
                equal &= Is.EqualTo(connectorData.Item1.Id).Matches(output.Id);
                equal &= Is.EqualTo(connectorData.Item1.Parameters).Matches(output.Parameters);
                return equal;
            }

            public int GetHashCode(object obj)
            {
                return 0;
            }
        }

        [Test]
        public static void Test()
        {
            Dictionary<Id<TConnectorDefinition>, ConnectorDefinitionData> connectorDefinitions = new Dictionary<Id<TConnectorDefinition>, ConnectorDefinitionData>()
            {
                { Id<TConnectorDefinition>.Parse("cc901520-7d05-4274-87d9-b86cf00e6ee4"), new ConnectorDefinitionData("Basdas", Id<TConnectorDefinition>.Parse("9fd8b8af-35f2-4d60-8533-45b065d02057"), new List<NodeData.ParameterData>() { }, ConnectorPosition.Bottom, false) },
                { Id<TConnectorDefinition>.Parse("37e0f309-9a40-4b43-a700-2c723062f982"), new ConnectorDefinitionData("AsdasD", Id<TConnectorDefinition>.Parse("81efd7b0-91c6-4ce1-899e-9a342477a287"), new List<NodeData.ParameterData>() { }, ConnectorPosition.Bottom, false) },
            };

            NodeData.ConnectorData[] connectors = new NodeData.ConnectorData[]
            {
                new NodeData.ConnectorData(Id<TConnector>.Parse("6ae078da-1aad-426d-a6cc-614372319957"), connectorDefinitions.Keys.ElementAt(0), new IParameter[] { new DummyParameter("",Id<Parameter>.Parse("b8426482-8dd1-4b0b-ac00-69fad7bb66a0"), "asd", ParameterType.Parse("2e781d14-a0c7-4908-97a7-20db93588218")) }),
                new NodeData.ConnectorData(Id<TConnector>.Parse("d435ccd6-95a0-46c6-8867-732f6f405fb8"), connectorDefinitions.Keys.ElementAt(1) , new IParameter[0]),
            };

            List<NodeData.ConfigData> parameter0Config = new List<NodeData.ConfigData>()
            {
                new NodeData.ConfigData(Id<NodeTypeTemp>.Parse("f174adb6-709a-4632-ba95-aa33706a8886"), new IParameter[0]),
            };

            NodeData.ParameterData[] parameters = new NodeData.ParameterData[]
            {
                new NodeData.ParameterData("parameter name 1", Id<Parameter>.Parse("e335ab11-3ef1-49d8-ba8f-82ebba25aa90"), ParameterType.Parse("78c4517b-bf10-4c7b-8ec1-4edf066e4d0a"), new ReadOnlyCollection<NodeData.ConfigData>(parameter0Config), "A"),
                new NodeData.ParameterData("parameter name 2", Id<Parameter>.Parse("5416af30-e832-4330-9a29-57907c403f5a"), ParameterType.Parse("1957bc04-ff26-415a-9a13-7eca6419b20a"), new ReadOnlyCollection<NodeData.ConfigData>(new List<NodeData.ConfigData>()), null),
            };

            NodeData.ConfigData[] config = new NodeData.ConfigData[]
            {
                new NodeData.ConfigData(Id<NodeTypeTemp>.Parse("e00fc031-1c94-4a67-8631-d8460a6db69e"), new IParameter[0]),
                new NodeData.ConfigData(Id<NodeTypeTemp>.Parse("6b0c0410-48cd-4807-9cbf-00d9b6d823ed"), new IParameter[0]),
            };

            NodeData nodeData = new NodeData("node name", Guid.Parse("9c9fea17-13fb-4eb0-be9b-7ad44a580601"), "node decsription", Id<NodeTypeTemp>.Parse("7ddb7ef4-4ba5-49b5-96bc-e540851ddbf0"), connectors, parameters, config);

            TypeSet types = new TypeSet();
            types.AddOther(parameters[0].Type, "Parameter Type 1", (name, id, defaultValue, d) => new DummyParameter(name, id, defaultValue, parameters[0].Type));
            types.AddOther(parameters[1].Type, "Parameter Type 2", (name, id, defaultValue, d) => new DummyParameter(name, id, defaultValue, parameters[1].Type));

            NodeDataGenerator g = new NodeDataGenerator(nodeData, types, connectorDefinitions, Rules.Instance, a => new List<IParameter>());

            Assert.That(g.Config, Is.EquivalentTo(config));
            Assert.That(g.Name, Is.EqualTo(nodeData.Name));
            Assert.That(g.Guid, Is.EqualTo(nodeData.Guid));
            //TODO: Should I test the description here
            Assert.That(g.GetParameterConfig(parameters[0].Id), Is.EqualTo(parameter0Config));
            Assert.That(g.GetParameterConfig(parameters[1].Id), Is.EqualTo(new List<NodeData.ConfigData>()));

            //Generate with no parameter data (as if adding a brand new node)
            {
                object document = new object();
                var nodeId1 = Id<NodeTemp>.Parse("bcc17300-62dc-4625-8245-dafcf9f8ebfa");
                IEnumerable<NodeDataGeneratorParameterData> parameterData = Enumerable.Empty<NodeDataGeneratorParameterData>();
                var node = g.Generate(nodeId1, parameterData, document);
                CheckNode(connectorDefinitions, connectors, parameters, parameterData, g, nodeId1, node);
            }

            //Generate with completely supplied parameter data
            {
                object document = new object();
                var nodeId1 = Id<NodeTemp>.Parse("cd86cc66-0a9a-42b2-b4de-ce3e2410ce69");
                IEnumerable<NodeDataGeneratorParameterData> parameterData = parameters.Select(p => new NodeDataGeneratorParameterData(p.Id, p.Id.Serialized().Substring(0, 3)));
                var node = g.Generate(nodeId1, parameterData, document);
                CheckNode(connectorDefinitions, connectors, parameters, parameterData, g, nodeId1, node);
            }

            //Generate with partially supplied parameter data and some unrecognised supplied parameter data
            {
                object document = new object();
                var nodeId1 = Id<NodeTemp>.Parse("cd86cc66-0a9a-42b2-b4de-ce3e2410ce69");
                //Partially supplied data
                IEnumerable<NodeDataGeneratorParameterData> parameterData = parameters.Take(1).Select(p => new NodeDataGeneratorParameterData(p.Id, p.Id.Serialized().Substring(0, 3)));
                //unrecognised data
                parameterData = parameterData.Concat(new NodeDataGeneratorParameterData[] { new NodeDataGeneratorParameterData(Id<Parameter>.Parse("90660d64-0ea7-4125-8f5c-cb845c127061"), "90660d64"),
                                                                                            new NodeDataGeneratorParameterData(Id<Parameter>.Parse("2e49733d-56f7-4b24-a9e8-8cc07ecfc2aa"), "2e49733d") });
                var node = g.Generate(nodeId1, parameterData, document);
                CheckNode(connectorDefinitions, connectors, parameters, parameterData, g, nodeId1, node);
            }

            g.Removed();
            Assert.That(g.Name, Is.EqualTo("Definition Deleted"));
        }

        private static void CheckNode(Dictionary<Id<TConnectorDefinition>, ConnectorDefinitionData> connectorDefinitions, NodeData.ConnectorData[] connectors, NodeData.ParameterData[] parameters, IEnumerable<NodeDataGeneratorParameterData> parameterData, NodeDataGenerator g, Id<NodeTemp> nodeId1, IConversationNodeData node)
        {
            Assert.That(node.Config, Is.EqualTo(g.Config));

            //Check the connectors are sorted alphabetically by name
            Assert.That(String.Compare(node.Connectors.ElementAt(0).GetName(), node.Connectors.ElementAt(1).GetName(), StringComparison.CurrentCultureIgnoreCase), Is.LessThanOrEqualTo(0));

            Assert.That(node.Connectors, Is.EqualTo(connectors.Select(c => Tuple.Create(c, connectorDefinitions[c.TypeId]))).Using(new OutputComparer()));
            foreach (var output in node.Connectors)
            {
                Assert.That(output.Parent, Is.EqualTo(node));
                Assert.That(output.Rules, Is.EqualTo(Rules.Instance));
            }
            Assert.That(node.Name, Is.EqualTo(g.Name));
            Assert.That(node.NodeId, Is.EqualTo(nodeId1));
            Assert.That(node.NodeTypeId, Is.EqualTo(g.Guid));

            var expectedIds = parameters.Select(p => p.Id).Union(parameterData.Select(d => d.Guid));
            Assert.That(node.Parameters.Select(p => p.Id), Is.EquivalentTo(expectedIds));
            foreach (var parameter in node.Parameters)
            {
                var x = parameters.Where(p => p.Id == parameter.Id);
                if (x.Any())
                {
                    var definition = x.Single();
                    Assert.That(parameter.Name, Is.EqualTo(definition.Name));
                    Assert.That(parameter.TypeId, Is.EqualTo(definition.Type));

                    if (!parameterData.Any(d => d.Guid == parameter.Id))
                        Assert.That(parameter.ValueAsString(), Is.EqualTo(definition.Default));
                }

                var y = parameterData.Where(d => d.Guid == parameter.Id);
                if (y.Any())
                {
                    var data = y.Single();
                    Assert.That(parameter.ValueAsString(), Is.EqualTo(data.Value));
                }
            }

            foreach (var data in parameterData)
            {
                var parameter = node.Parameters.Where(p => p.Id == data.Guid).Single();
                if (!parameters.Any(x => x.Id == data.Guid))
                    Assert.That(parameter is UnknownParameter);
                Assert.That(parameter.ValueAsString(), Is.EqualTo(data.Value));
            }
        }
    }
}
