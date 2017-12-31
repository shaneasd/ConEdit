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
    public static class ConversationNodeTest
    {
        class DummyUI : INodeUI<DummyUI>
        {
            public DummyUI(ConversationNode<DummyUI> node, bool corrupt)
            {
                Node = node;
                Corrupt = corrupt;
            }
            public ConversationNode<DummyUI> Node { get; }
            public bool Corrupt { get; }
        }

        class DummyParameter : IParameter
        {
            public DummyParameter(bool corrupt)
            {
                Corrupted = corrupt;
            }

            public bool Corrupted { get; }

            public Id<global::Conversation.Parameter> Id => throw new NotSupportedException();

            public string Name => throw new NotSupportedException();

            public ParameterType TypeId => throw new NotSupportedException();

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
                throw new NotImplementedException();
            }
        }

        class EverythingConnects : IConnectionRules
        {
            public bool CanConnect(Id<TConnectorDefinition> a, Id<TConnectorDefinition> b)
            {
                return true;
            }
        }

        class DummyConversationNodeData : IConversationNodeData
        {
            public DummyConversationNodeData(IEnumerable<IParameter> parameters, Id<NodeTemp> nodeId)
            {
                Parameters = parameters;

                var conDef = new ConnectorDefinitionData("def", Id<TConnectorDefinition>.Parse("c596853b-18e4-48fd-9960-bfa4c9c9bdd8"), new NodeData.ParameterData[0], ConnectorPosition.Bottom);

                Connectors = new Output[]
                {
                    new Output(Id<TConnector>.Parse("62bfa34b-d0dd-4438-b079-e1c7d1488f12"), conDef, this, new IParameter[0], new EverythingConnects()),
                    new Output(Id<TConnector>.Parse("c087b5e0-51df-4714-9401-0e8ab4500cfd"), conDef, this, new IParameter[0], new EverythingConnects()),
                    new Output(Id<TConnector>.Parse("4b97608f-cf13-4ebf-8c41-da48f407da30"), conDef, this, new IParameter[0], new EverythingConnects()),
                };
                NodeId = nodeId;
            }

            public IReadOnlyList<NodeData.ConfigData> Config => throw new NotSupportedException();

            public IEnumerable<Output> Connectors { get; }

            public string Name => throw new NotSupportedException();

            public string Description => throw new NotSupportedException();

            public Id<NodeTemp> NodeId { get; }

            public Id<NodeTypeTemp> NodeTypeId => throw new NotSupportedException();

            public IEnumerable<IParameter> Parameters { get; }

            public event Action Linked
            {
                add
                {
                    throw new NotSupportedException();
                }
                remove
                {
                    throw new NotSupportedException();
                }
            }

            public void ChangeId(Id<NodeTemp> id) => throw new NotSupportedException();

            public SimpleUndoPair RemoveUnknownParameter(UnknownParameter p) => throw new NotSupportedException();
        }

        [Test]
        public static void TestInitialState()
        {
            IEnumerable<IParameter> noParameters = new IParameter[]
            {
            };
            IEnumerable<IParameter> notCorrupt = new IParameter[]
            {
                new DummyParameter(false),
                new DummyParameter(false),
            };
            IEnumerable<IParameter> someCorrupt = new IParameter[]
            {
                new DummyParameter(false),
                new DummyParameter(true),
            };
            IEnumerable<IParameter> allCorrupt = new IParameter[]
            {
                new DummyParameter(true),
                new DummyParameter(true),
            };
            Func<ConversationNode<DummyUI>, DummyUI> nodeUI = n => new DummyUI(n, false);
            Func<ConversationNode<DummyUI>, DummyUI> corruptedUI = n => new DummyUI(n, true);
            {
                IConversationNodeData data = new DummyConversationNodeData(noParameters, Id<NodeTemp>.Parse("558d4788-7b8c-4fc2-a822-f8ea90a2c84e"));
                ConversationNode<DummyUI> node = new ConversationNode<DummyUI>(data, nodeUI, corruptedUI);
                Assert.That(node.Data, Is.EqualTo(data));
                Assert.That(node.Renderer.Corrupt, Is.False);
            }
            {
                IConversationNodeData data = new DummyConversationNodeData(notCorrupt, Id<NodeTemp>.Parse("558d4788-7b8c-4fc2-a822-f8ea90a2c84e"));
                ConversationNode<DummyUI> node = new ConversationNode<DummyUI>(data, nodeUI, corruptedUI);
                Assert.That(node.Data, Is.EqualTo(data));
                Assert.That(node.Renderer.Corrupt, Is.False);
            }
            {
                IConversationNodeData data = new DummyConversationNodeData(someCorrupt, Id<NodeTemp>.Parse("558d4788-7b8c-4fc2-a822-f8ea90a2c84e"));
                ConversationNode<DummyUI> node = new ConversationNode<DummyUI>(data, nodeUI, corruptedUI);
                Assert.That(node.Data, Is.EqualTo(data));
                Assert.That(node.Renderer.Corrupt, Is.True);
            }
            {
                IConversationNodeData data = new DummyConversationNodeData(allCorrupt, Id<NodeTemp>.Parse("558d4788-7b8c-4fc2-a822-f8ea90a2c84e"));
                ConversationNode<DummyUI> node = new ConversationNode<DummyUI>(data, nodeUI, corruptedUI);
                Assert.That(node.Data, Is.EqualTo(data));
                Assert.That(node.Renderer.Corrupt, Is.True);
            }
        }

        [Test]
        public static void TestUpdateRendererCorruption()
        {
            //ConversationNode<DummyUI> n;
            //TODO: This method probably shouldn't be public. The node should detect when a parameter is corrupted/decorrupted or added/removed and react accordingly automatically
            //n.UpdateRendererCorruption();
            //n.Renderer;
            //n.RendererChanged;
            //n.RendererChanging;
            Assert.Inconclusive();
        }

        [Test]
        public static void TestSetRenderer()
        {
            IEnumerable<IParameter> noCorrupt = new IParameter[]
            {
                new DummyParameter(false),
            };
            IConversationNodeData data = new DummyConversationNodeData(noCorrupt, Id<NodeTemp>.Parse("558d4788-7b8c-4fc2-a822-f8ea90a2c84e"));
            Func<ConversationNode<DummyUI>, DummyUI> nodeUI = n => new DummyUI(n, false);
            Func<ConversationNode<DummyUI>, DummyUI> corruptedUI = n => new DummyUI(n, true);

            ConversationNode<DummyUI> node = new ConversationNode<DummyUI>(data, nodeUI, corruptedUI);

            var originalRenderer = node.Renderer;
            var newRenderer = new DummyUI(node, false);

            int changing = 0;
            int changed = 0;

            node.RendererChanging += () =>
            {
                changing++;
                Assert.That(changing, Is.EqualTo(1));
                Assert.That(changed, Is.EqualTo(0));
                Assert.That(node.Renderer, Is.EqualTo(originalRenderer));
            };

            node.RendererChanged += () =>
            {
                changed++;
                Assert.That(changing, Is.EqualTo(1));
                Assert.That(changed, Is.EqualTo(1));
                Assert.That(node.Renderer, Is.EqualTo(newRenderer));
            };

            node.SetRenderer(n => newRenderer);

            Assert.That(changed, Is.EqualTo(1));
            Assert.That(changing, Is.EqualTo(1));
        }

        [Test]
        public static void TestGetNodeRemoveActions()
        {
            IEnumerable<IParameter> someCorrupt = new IParameter[]
            {
                new DummyParameter(false),
                new DummyParameter(true),
            };
            IConversationNodeData data1 = new DummyConversationNodeData(someCorrupt, Id<NodeTemp>.Parse("558d4788-7b8c-4fc2-a822-f8ea90a2c84e"));
            IConversationNodeData data2 = new DummyConversationNodeData(someCorrupt, Id<NodeTemp>.Parse("af594e64-e788-4873-9d01-0e13ba430c14"));
            IConversationNodeData data3 = new DummyConversationNodeData(someCorrupt, Id<NodeTemp>.Parse("d3bbcc20-f8c5-4e40-8ad4-bee06b6f28a7"));
            Func<ConversationNode<DummyUI>, DummyUI> nodeUI = n => new DummyUI(n, false);
            Func<ConversationNode<DummyUI>, DummyUI> corruptedUI = n => new DummyUI(n, true);
            ConversationNode<DummyUI> node1 = new ConversationNode<DummyUI>(data1, nodeUI, corruptedUI);
            ConversationNode<DummyUI> node2 = new ConversationNode<DummyUI>(data2, nodeUI, corruptedUI);
            ConversationNode<DummyUI> node3 = new ConversationNode<DummyUI>(data3, nodeUI, corruptedUI);
            var nodes = new[] { node1, node2, node3 };

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    for (int a = i + 1; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            nodes[i].Data.Connectors.ElementAt(j).ConnectTo(nodes[a].Data.Connectors.ElementAt(b), false);
                        }
                    }
                }
            }

            List<Output> allConnectors = new List<Output>();
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        allConnectors.Add(nodes[i].Data.Connectors.ElementAt(j));
                    }
                }
            }

            Action CheckAllConnected = () =>
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Assert.That(nodes[i].Data.Connectors.ElementAt(j).Connections, Is.EquivalentTo(allConnectors.Where(x => x.Parent != nodes[i].Data)));
                    }
                }
            };

            CheckAllConnected();

            var actions = node1.GetNodeRemoveActions();

            //TODO: Test that localizations are removed/readded correctly

            CheckAllConnected();

            actions.Redo();
            for (int i = 1; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Assert.That(nodes[i].Data.Connectors.ElementAt(j).Connections, Is.EquivalentTo(allConnectors.Where(x => x.Parent != nodes[i].Data && x.Parent != node1.Data)));
                }
            }

            for (int j = 0; j < 3; j++)
            {
                Assert.That(node1.Data.Connectors.ElementAt(j).Connections, Is.Empty);
            }

            actions.Undo();
            CheckAllConnected();
        }

        [Test]
        public static void TestConfigureAndModified()
        {
            IEnumerable<IParameter> someCorrupt = new IParameter[]
            {
                new DummyParameter(false),
                new DummyParameter(true),
            };
            IConversationNodeData data = new DummyConversationNodeData(someCorrupt, Id<NodeTemp>.Parse("745fa8ce-174e-4af7-82b7-17e939e9a475"));
            Func<ConversationNode<DummyUI>, DummyUI> nodeUI = n => new DummyUI(n, false);
            Func<ConversationNode<DummyUI>, DummyUI> corruptedUI = n => new DummyUI(n, true);
            ConversationNode<DummyUI> node = new ConversationNode<DummyUI>(data, nodeUI, corruptedUI);

            int modifications = 0;
            int undos = 0;
            int redos = 0;
            int expectedRedos = 0; //To ensure Modified is triggered AFTER the modification
            int expectedUndos = 0; //To ensure Modified is triggered AFTER the modification
            node.Modified += () => { modifications++; Assert.That(redos, Is.EqualTo(expectedRedos)); Assert.That(undos, Is.EqualTo(expectedUndos)); };
            Action redo = () => { redos++; };
            Action undo = () => { undos++; };
            ConfigureResult configure = node.Configure(n => new SimpleUndoPair() { Redo = redo, Undo = undo });

            Assert.That(undos, Is.EqualTo(0));
            Assert.That(redos, Is.EqualTo(0));
            Assert.That(modifications, Is.EqualTo(0));

            expectedRedos = 1;
            configure.Do(a => a.Redo(), b => Assert.Fail("Unexpected Cancelled configuration"));

            Assert.That(undos, Is.EqualTo(0));
            Assert.That(redos, Is.EqualTo(1));
            Assert.That(modifications, Is.EqualTo(1));

            expectedUndos = 1;
            configure.Do(a => a.Undo(), b => Assert.Fail("Unexpected Cancelled configuration"));

            Assert.That(undos, Is.EqualTo(1));
            Assert.That(redos, Is.EqualTo(1));
            Assert.That(modifications, Is.EqualTo(2));

            ConfigureResult cancelConfigure = node.Configure(n => ConfigureResultNotOk.Cancel);
            cancelConfigure.Do(a => Assert.Fail("Expected configure result to be Cancel"), b => Assert.That(b, Is.EqualTo(ConfigureResultNotOk.Cancel)));

            ConfigureResult notApplicableConfigure = node.Configure(n => ConfigureResultNotOk.NotApplicable);
            notApplicableConfigure.Do(a => Assert.Fail("Expected configure result to be Cancel"), b => Assert.That(b, Is.EqualTo(ConfigureResultNotOk.NotApplicable)));
        }
    }
}
