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
    public static class OutputTest
    {
        class Editable : IConversationNodeData
        {
            public Editable(Id<NodeTemp> id)
            {
                NodeId = id;
            }

            public IReadOnlyList<NodeData.ConfigData> Config
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IEnumerable<Output> Connectors
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public string Name
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public Id<NodeTemp> NodeId { get; }

            public Id<NodeTypeTemp> NodeTypeId
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IEnumerable<IParameter> Parameters
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public event Action Linked { add { } remove { } }

            public void ChangeId(Id<NodeTemp> id)
            {
                throw new NotImplementedException();
            }

            public SimpleUndoPair RemoveUnknownParameter(UnknownParameter p)
            {
                throw new NotImplementedException();
            }
        }

        class GenericConnectionRules : IConnectionRules
        {
            private Func<Id<TConnectorDefinition>, Id<TConnectorDefinition>, bool> m_canConnect;

            public GenericConnectionRules(Func<Id<TConnectorDefinition>, Id<TConnectorDefinition>, bool> canConnect)
            {
                m_canConnect = canConnect;
            }

            public bool CanConnect(Id<TConnectorDefinition> a, Id<TConnectorDefinition> b)
            {
                return m_canConnect(a, b);
            }
        }

        [Test]
        public static void CannotConnectSameNode()
        {
            var id1 = Id<TConnector>.Parse("505c9566-9b6c-481d-b667-106f347a293f");
            var id2 = Id<TConnector>.Parse("249f9ad5-d04f-4f21-aff2-1644ab697bbd");
            var id3 = Id<TConnector>.Parse("fc206ffe-9bd3-4f19-ac72-5c813bcbd84c");
            ConnectorDefinitionData definition = new ConnectorDefinitionData("AsdasD", Id<TConnectorDefinition>.Parse("9fd8b8af-35f2-4d60-8533-45b065d02057"), new List<NodeData.ParameterData>() { }, ConnectorPosition.Bottom, false);
            var parent1 = new Editable(Id<NodeTemp>.Parse("b0bdc6d1-7f0c-4e82-b4c3-c5e6d2f3686d"));
            var parent2 = new Editable(parent1.NodeId);
            var parent3 = new Editable(Id<NodeTemp>.Parse("05abad3e-a026-4ffa-b66e-8bbaabde9d45"));


            var connectionRules = new GenericConnectionRules((a, b) => true);
            Output o1 = new Output(id1, definition, parent1, new List<IParameter>(), connectionRules);
            Output o2 = new Output(id2, definition, parent2, new List<IParameter>(), connectionRules);
            Output o3 = new Output(id3, definition, parent3, new List<IParameter>(), connectionRules);

            Assert.That(o1.CanConnectTo(o2, ConnectionConsiderations.None), Is.False);
            Assert.That(o2.CanConnectTo(o1, ConnectionConsiderations.None), Is.False);
            Assert.That(o1.CanConnectTo(o2, ConnectionConsiderations.RedundantConnection | ConnectionConsiderations.RuleViolation), Is.False);
            Assert.That(o2.CanConnectTo(o1, ConnectionConsiderations.RedundantConnection | ConnectionConsiderations.RuleViolation), Is.False);
            Assert.That(o1.CanConnectTo(o2, ConnectionConsiderations.SameNode | ConnectionConsiderations.RedundantConnection | ConnectionConsiderations.RuleViolation), Is.True);
            Assert.That(o2.CanConnectTo(o1, ConnectionConsiderations.SameNode | ConnectionConsiderations.RedundantConnection | ConnectionConsiderations.RuleViolation), Is.True);
            Assert.That(o1.CanConnectTo(o2, ConnectionConsiderations.SameNode), Is.True);
            Assert.That(o2.CanConnectTo(o1, ConnectionConsiderations.SameNode), Is.True);

            Assert.That(o1.CanConnectTo(o3, ConnectionConsiderations.None), Is.True);
            Assert.That(o3.CanConnectTo(o1, ConnectionConsiderations.None), Is.True);

            Assert.That(o1.ConnectTo(o2, false), Is.False);
            Assert.That(o1.ConnectTo(o2, true), Is.False);
            Assert.That(o1.ConnectTo(o3, false), Is.True);
        }

        [Test]
        public static void CannotConnectRuleViolation()
        {
            var id1 = Id<TConnector>.Parse("505c9566-9b6c-481d-b667-106f347a293f");
            var id2 = Id<TConnector>.Parse("249f9ad5-d04f-4f21-aff2-1644ab697bbd");
            var id3 = Id<TConnector>.Parse("fc206ffe-9bd3-4f19-ac72-5c813bcbd84c");
            ConnectorDefinitionData definition1 = new ConnectorDefinitionData("AsdasD", Id<TConnectorDefinition>.Parse("9fd8b8af-35f2-4d60-8533-45b065d02057"), new List<NodeData.ParameterData>() { }, ConnectorPosition.Bottom, false);
            ConnectorDefinitionData definition2 = new ConnectorDefinitionData("AsdasD", Id<TConnectorDefinition>.Parse("81efd7b0-91c6-4ce1-899e-9a342477a287"), new List<NodeData.ParameterData>() { }, ConnectorPosition.Bottom, false);
            ConnectorDefinitionData definition3 = new ConnectorDefinitionData("AsdasD", Id<TConnectorDefinition>.Parse("04f53945-1e81-4cf7-8ddf-ea8bb6a5bc63"), new List<NodeData.ParameterData>() { }, ConnectorPosition.Bottom, false);
            var parent1 = new Editable(Id<NodeTemp>.Parse("b0bdc6d1-7f0c-4e82-b4c3-c5e6d2f3686d"));
            var parent2 = new Editable(Id<NodeTemp>.Parse("fa322980-9720-4bc5-923f-a543c5f83e0e"));
            var parent3 = new Editable(Id<NodeTemp>.Parse("05abad3e-a026-4ffa-b66e-8bbaabde9d45"));


            var connectionRules = new GenericConnectionRules((a, b) => a != definition2.Id && b != definition2.Id);
            Output o1 = new Output(id1, definition1, parent1, new List<IParameter>(), connectionRules);
            Output o2 = new Output(id2, definition2, parent2, new List<IParameter>(), connectionRules);
            Output o3 = new Output(id3, definition3, parent3, new List<IParameter>(), connectionRules);

            Assert.That(o1.CanConnectTo(o2, ConnectionConsiderations.None), Is.False);
            Assert.That(o2.CanConnectTo(o1, ConnectionConsiderations.None), Is.False);
            Assert.That(o1.CanConnectTo(o2, ConnectionConsiderations.RedundantConnection | ConnectionConsiderations.SameNode), Is.False);
            Assert.That(o2.CanConnectTo(o1, ConnectionConsiderations.RedundantConnection | ConnectionConsiderations.SameNode), Is.False);
            Assert.That(o1.CanConnectTo(o2, ConnectionConsiderations.SameNode | ConnectionConsiderations.RedundantConnection | ConnectionConsiderations.RuleViolation), Is.True);
            Assert.That(o2.CanConnectTo(o1, ConnectionConsiderations.SameNode | ConnectionConsiderations.RedundantConnection | ConnectionConsiderations.RuleViolation), Is.True);
            Assert.That(o1.CanConnectTo(o2, ConnectionConsiderations.RuleViolation), Is.True);
            Assert.That(o2.CanConnectTo(o1, ConnectionConsiderations.RuleViolation), Is.True);

            Assert.That(o1.CanConnectTo(o3, ConnectionConsiderations.None), Is.True);
            Assert.That(o3.CanConnectTo(o1, ConnectionConsiderations.None), Is.True);

            Assert.That(o1.ConnectTo(o2, false), Is.False);
            Assert.That(o1.ConnectTo(o2, true), Is.True);
            Assert.That(o1.ConnectTo(o3, false), Is.True);
        }

        [Test]
        public static void CannotConnectRedundantConnection()
        {
            var id1 = Id<TConnector>.Parse("505c9566-9b6c-481d-b667-106f347a293f");
            var id2 = Id<TConnector>.Parse("249f9ad5-d04f-4f21-aff2-1644ab697bbd");
            var id3 = Id<TConnector>.Parse("fc206ffe-9bd3-4f19-ac72-5c813bcbd84c");
            ConnectorDefinitionData definition1 = new ConnectorDefinitionData("AsdasD", Id<TConnectorDefinition>.Parse("9fd8b8af-35f2-4d60-8533-45b065d02057"), new List<NodeData.ParameterData>() { }, ConnectorPosition.Bottom, false);
            ConnectorDefinitionData definition2 = new ConnectorDefinitionData("AsdasD", Id<TConnectorDefinition>.Parse("81efd7b0-91c6-4ce1-899e-9a342477a287"), new List<NodeData.ParameterData>() { }, ConnectorPosition.Bottom, false);
            ConnectorDefinitionData definition3 = new ConnectorDefinitionData("AsdasD", Id<TConnectorDefinition>.Parse("04f53945-1e81-4cf7-8ddf-ea8bb6a5bc63"), new List<NodeData.ParameterData>() { }, ConnectorPosition.Bottom, false);
            var parent1 = new Editable(Id<NodeTemp>.Parse("b0bdc6d1-7f0c-4e82-b4c3-c5e6d2f3686d"));
            var parent2 = new Editable(Id<NodeTemp>.Parse("fa322980-9720-4bc5-923f-a543c5f83e0e"));
            var parent3 = new Editable(Id<NodeTemp>.Parse("05abad3e-a026-4ffa-b66e-8bbaabde9d45"));


            var connectionRules = new GenericConnectionRules((a, b) => true);
            Output o1 = new Output(id1, definition1, parent1, new List<IParameter>(), connectionRules);
            Output o2 = new Output(id2, definition2, parent2, new List<IParameter>(), connectionRules);
            Output o3 = new Output(id3, definition3, parent3, new List<IParameter>(), connectionRules);

            Assert.That(o1.ConnectTo(o2, false), Is.True); //Initially we can connect them fine

            //o1 and o2 are now connected but o1 and o3 are not
            Assert.That(o1.CanConnectTo(o2, ConnectionConsiderations.None), Is.False);
            Assert.That(o2.CanConnectTo(o1, ConnectionConsiderations.None), Is.False);
            Assert.That(o1.CanConnectTo(o2, ConnectionConsiderations.RuleViolation | ConnectionConsiderations.SameNode), Is.False);
            Assert.That(o2.CanConnectTo(o1, ConnectionConsiderations.RuleViolation | ConnectionConsiderations.SameNode), Is.False);
            Assert.That(o1.CanConnectTo(o2, ConnectionConsiderations.RuleViolation | ConnectionConsiderations.SameNode | ConnectionConsiderations.RedundantConnection), Is.True);
            Assert.That(o2.CanConnectTo(o1, ConnectionConsiderations.RuleViolation | ConnectionConsiderations.SameNode | ConnectionConsiderations.RedundantConnection), Is.True);
            Assert.That(o1.CanConnectTo(o2, ConnectionConsiderations.RedundantConnection), Is.True);
            Assert.That(o2.CanConnectTo(o1, ConnectionConsiderations.RedundantConnection), Is.True);

            Assert.That(o1.CanConnectTo(o3, ConnectionConsiderations.None), Is.True);
            Assert.That(o3.CanConnectTo(o1, ConnectionConsiderations.None), Is.True);

            Assert.That(o1.ConnectTo(o2, false), Is.False);
            Assert.That(o1.ConnectTo(o2, true), Is.False);
            Assert.That(o1.ConnectTo(o3, false), Is.True);
        }

        [Test]
        public static void ConnectionAndDisconnectionEvents()
        {
            var id1 = Id<TConnector>.Parse("505c9566-9b6c-481d-b667-106f347a293f");
            var id2 = Id<TConnector>.Parse("249f9ad5-d04f-4f21-aff2-1644ab697bbd");
            ConnectorDefinitionData definition = new ConnectorDefinitionData("AsdasD", Id<TConnectorDefinition>.Parse("9fd8b8af-35f2-4d60-8533-45b065d02057"), new List<NodeData.ParameterData>() { }, ConnectorPosition.Bottom, false);
            var parent1 = new Editable(Id<NodeTemp>.Parse("b0bdc6d1-7f0c-4e82-b4c3-c5e6d2f3686d"));
            var parent2 = new Editable(Id<NodeTemp>.Parse("fa322980-9720-4bc5-923f-a543c5f83e0e"));

            var connectionRules = new GenericConnectionRules((a, b) => true);
            Output o1 = new Output(id1, definition, parent1, new List<IParameter>(), connectionRules);
            Output o2 = new Output(id2, definition, parent2, new List<IParameter>(), connectionRules);

            List<Output> o1connections = new List<Output>();
            o1.Connected += o => o1connections.Add(o);
            List<Output> o2connections = new List<Output>();
            o2.Connected += o => o2connections.Add(o);
            List<Output> o1disconnections = new List<Output>();
            o1.Disconnected += o => o1disconnections.Add(o);
            List<Output> o2disconnections = new List<Output>();
            o2.Disconnected += o => o2disconnections.Add(o);
            Action ClearAll = () =>
            {
                o1connections.Clear();
                o2connections.Clear();
                o1disconnections.Clear();
                o2disconnections.Clear();
            };

            o1.ConnectTo(o2, false);
            Assert.That(o1connections, Is.EquivalentTo(o2.Only()));
            Assert.That(o2connections, Is.EquivalentTo(o1.Only()));
            Assert.That(o1disconnections, Is.Empty);
            Assert.That(o2disconnections, Is.Empty);
            ClearAll();
            o1.Disconnect(o2);
            Assert.That(o1connections, Is.Empty);
            Assert.That(o2connections, Is.Empty);
            Assert.That(o1disconnections, Is.EquivalentTo(o2.Only()));
            Assert.That(o2disconnections, Is.EquivalentTo(o1.Only()));
            ClearAll();
            o2.ConnectTo(o1, false);
            Assert.That(o1connections, Is.EquivalentTo(o2.Only()));
            Assert.That(o2connections, Is.EquivalentTo(o1.Only()));
            Assert.That(o1disconnections, Is.Empty);
            Assert.That(o2disconnections, Is.Empty);
            ClearAll();

            var actions = o2.DisconnectAllActions();
            Assert.That(o1connections, Is.Empty);
            Assert.That(o2connections, Is.Empty);
            Assert.That(o1disconnections, Is.Empty);
            Assert.That(o2disconnections, Is.Empty);
            ClearAll();

            actions.Redo();
            Assert.That(o1connections, Is.Empty);
            Assert.That(o2connections, Is.Empty);
            Assert.That(o1disconnections, Is.EquivalentTo(o2.Only()));
            Assert.That(o2disconnections, Is.EquivalentTo(o1.Only()));
            ClearAll();

            actions.Undo();
            Assert.That(o1connections, Is.EquivalentTo(o2.Only()));
            Assert.That(o2connections, Is.EquivalentTo(o1.Only()));
            Assert.That(o1disconnections, Is.Empty);
            Assert.That(o2disconnections, Is.Empty);
        }

        [Test]
        public static void Connections()
        {
            var id1 = Id<TConnector>.Parse("505c9566-9b6c-481d-b667-106f347a293f");
            var id2 = Id<TConnector>.Parse("249f9ad5-d04f-4f21-aff2-1644ab697bbd");
            var id3 = Id<TConnector>.Parse("a2bf9ba3-b4c0-4574-81a9-d8ce0477ec58");
            var id4 = Id<TConnector>.Parse("e7888bf4-c4ed-43d9-aeb4-a89b3cb9cb1f");
            var id5 = Id<TConnector>.Parse("8051c57d-e64b-4932-a34b-5e46c064efa9");
            var id6 = Id<TConnector>.Parse("cc4b021c-2c60-436d-8e75-e83dcde2c7e5");
            ConnectorDefinitionData definition = new ConnectorDefinitionData("AsdasD", Id<TConnectorDefinition>.Parse("9fd8b8af-35f2-4d60-8533-45b065d02057"), new List<NodeData.ParameterData>() { }, ConnectorPosition.Bottom, false);
            var parent1 = new Editable(Id<NodeTemp>.Parse("b0bdc6d1-7f0c-4e82-b4c3-c5e6d2f3686d"));
            var parent2 = new Editable(Id<NodeTemp>.Parse("fa322980-9720-4bc5-923f-a543c5f83e0e"));
            var parent3 = new Editable(Id<NodeTemp>.Parse("9f4c81fd-af20-465e-aa98-04d3dc804798"));
            var parent4 = new Editable(Id<NodeTemp>.Parse("fc1bb15e-2f96-4e01-838d-43cdbbe4f77d"));
            var parent5 = new Editable(Id<NodeTemp>.Parse("196ac3c6-6fe1-41ce-b04b-481a91100cba"));
            var parent6 = new Editable(Id<NodeTemp>.Parse("795c74b9-dcb8-4f41-a397-c0f3ae8b65c9"));

            var connectionRules = new GenericConnectionRules((a, b) => true);
            Output o1 = new Output(id1, definition, parent1, new List<IParameter>(), connectionRules);
            Output o2 = new Output(id2, definition, parent2, new List<IParameter>(), connectionRules);
            Output o3 = new Output(id3, definition, parent3, new List<IParameter>(), connectionRules);
            Output o4 = new Output(id4, definition, parent4, new List<IParameter>(), connectionRules);
            Output o5 = new Output(id5, definition, parent5, new List<IParameter>(), connectionRules);
            Output o6 = new Output(id6, definition, parent6, new List<IParameter>(), connectionRules);

            List<Output> connections1 = new List<Output>();
            List<Output> connections2 = new List<Output>();
            List<Output> connections3 = new List<Output>();
            List<Output> connections4 = new List<Output>();
            List<Output> connections5 = new List<Output>();
            List<Output> connections6 = new List<Output>();

            Action CheckConnections = () =>
            {
                Assert.That(o1.Connections, Is.EquivalentTo(connections1));
                Assert.That(o2.Connections, Is.EquivalentTo(connections2));
                Assert.That(o3.Connections, Is.EquivalentTo(connections3));
                Assert.That(o4.Connections, Is.EquivalentTo(connections4));
                Assert.That(o5.Connections, Is.EquivalentTo(connections5));
                Assert.That(o6.Connections, Is.EquivalentTo(connections6));
            };

            o1.ConnectTo(o2, false);
            connections1.Add(o2);
            connections2.Add(o1);
            CheckConnections();

            o1.ConnectTo(o3, false);
            connections1.Add(o3);
            connections3.Add(o1);
            CheckConnections();

            o1.ConnectTo(o4, false);
            connections1.Add(o4);
            connections4.Add(o1);
            CheckConnections();

            o2.ConnectTo(o3, false);
            connections2.Add(o3);
            connections3.Add(o2);
            CheckConnections();

            o1.Disconnect(o3);
            connections1.Remove(o3);
            connections3.Remove(o1);
            CheckConnections();

            var action = o1.DisconnectAllActions();
            CheckConnections();

            action.Redo();
            connections1.Clear();
            connections2.Remove(o1);
            connections4.Remove(o1);
            CheckConnections();
        }

        [Test]
        public static void ConstructionParameters()
        {
            var id1 = Id<TConnector>.Parse("505c9566-9b6c-481d-b667-106f347a293f");
            var id2 = Id<TConnector>.Parse("249f9ad5-d04f-4f21-aff2-1644ab697bbd");
            ConnectorDefinitionData definition1 = new ConnectorDefinitionData("AsdasD", Id<TConnectorDefinition>.Parse("9fd8b8af-35f2-4d60-8533-45b065d02057"), new List<NodeData.ParameterData>() { }, ConnectorPosition.Bottom, false);
            ConnectorDefinitionData definition2 = new ConnectorDefinitionData("asdasd", Id<TConnectorDefinition>.Parse("81efd7b0-91c6-4ce1-899e-9a342477a287"), new List<NodeData.ParameterData>() { }, ConnectorPosition.Bottom, false);
            var parent1 = new Editable(Id<NodeTemp>.Parse("b0bdc6d1-7f0c-4e82-b4c3-c5e6d2f3686d"));
            var parent2 = new Editable(Id<NodeTemp>.Parse("fa322980-9720-4bc5-923f-a543c5f83e0e"));

            var connectionRules1 = new GenericConnectionRules((a, b) => false);
            var connectionRules2 = new GenericConnectionRules((a, b) => true);

            var parameters1 = new IParameter[] { new StringParameter("asdsdad", Id<Parameter>.Parse("cb82f5b9-9ea8-4e05-b851-20392bca923a")) };
            var parameters2 = new IParameter[] { new BooleanParameter("dxhgdh", Id<Parameter>.Parse("945c7cd9-1d0a-4291-a560-ea77d49b4c19"), "false") };

            Output o1 = new Output(id1, definition1, parent1, parameters1, connectionRules1);
            Output o2 = new Output(id2, definition2, parent2, parameters2, connectionRules2);

            Assert.That(o1.Id, Is.EqualTo(id1));
            Assert.That(o2.Id, Is.EqualTo(id2));
            Assert.That(o1.Definition, Is.EqualTo(definition1));
            Assert.That(o2.Definition, Is.EqualTo(definition2));
            Assert.That(o1.Parameters, Is.EquivalentTo(parameters1));
            Assert.That(o2.Parameters, Is.EquivalentTo(parameters2));
            Assert.That(o1.Parent, Is.EqualTo(parent1));
            Assert.That(o2.Parent, Is.EqualTo(parent2));
            Assert.That(o1.Rules, Is.EqualTo(connectionRules1));
            Assert.That(o2.Rules, Is.EqualTo(connectionRules2));
        }

        [Test]
        public static void GetName()
        {
            var id1 = Id<TConnector>.Parse("505c9566-9b6c-481d-b667-106f347a293f");
            var id2 = Id<TConnector>.Parse("249f9ad5-d04f-4f21-aff2-1644ab697bbd");
            var id3 = Id<TConnector>.Parse("8287d64b-ab8b-495e-8331-d371e85121d1");
            ConnectorDefinitionData definition = new ConnectorDefinitionData("AsdasD", Id<TConnectorDefinition>.Parse("9fd8b8af-35f2-4d60-8533-45b065d02057"), new List<NodeData.ParameterData>() { }, ConnectorPosition.Bottom, false);
            var parent = new Editable(Id<NodeTemp>.Parse("b0bdc6d1-7f0c-4e82-b4c3-c5e6d2f3686d"));
            var connectionRules = new GenericConnectionRules((a, b) => false);

            string name1 = "name1";
            string name2 = "name2";

            var parameters1 = new IParameter[] { new StringParameter("p1", ConnectorDefinitionData.OutputName, name1) };
            var parameters2 = new IParameter[] { new StringParameter("p2", Id<Parameter>.Parse("cb82f5b9-9ea8-4e05-b851-20392bca923a"), name2) };
            var parameters3 = new IParameter[] { new BooleanParameter("p3", Id<Parameter>.Parse("945c7cd9-1d0a-4291-a560-ea77d49b4c19"), "false") };

            Output o1 = new Output(id1, definition, parent, parameters1, connectionRules);
            Output o2 = new Output(id2, definition, parent, parameters2, connectionRules);
            Output o3 = new Output(id3, definition, parent, parameters3, connectionRules);

            Assert.That(o1.GetName(), Is.EqualTo(name1));
            Assert.That(o2.GetName(), Is.EqualTo(name2));
            Assert.That(o3.GetName(), Is.EqualTo(""));
        }
    }
}
