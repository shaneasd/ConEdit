using Conversation;
using Conversation.Serialization;
using ConversationEditor;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;
using TData = Conversation.Serialization.XmlGraphData<ConversationEditor.NodeUIData, ConversationEditor.ConversationEditorData>;

namespace Tests.ConversationEditor
{
    public static class ConversationFileTest
    {
        static RectangleF TopPosition(RectangleF area)
        {
            float y = area.Top - 10;
            return new RectangleF(area.Left + (int)(area.Width * 0.5f) - 5, y, 10, 10);
        }

        class DummyDisposable : Disposable
        {
            protected override void Dispose(bool disposing)
            {
            }
        }

        class DummyConnectionRules : IConnectionRules
        {
            public bool CanConnect(Id<TConnectorDefinition> a, Id<TConnectorDefinition> b)
            {
                return false;
            }
        }

        class DummyConversationNodeData : IConversationNodeData
        {
            public DummyConversationNodeData(string name, Id<NodeTemp> nodeId, string description, Id<NodeTypeTemp> nodeTypeId, IEnumerable<IParameter> parameters)
            {
                Name = name;
                NodeId = nodeId;
                Description = description;
                NodeTypeId = nodeTypeId;
                Parameters = parameters;

                foreach (var position in new[] { ConnectorPosition.Top, ConnectorPosition.Bottom, ConnectorPosition.Left, ConnectorPosition.Right })
                {
                    ConnectorDefinitionData connectorDefinition = new ConnectorDefinitionData("", Id<TConnectorDefinition>.New(), new NodeData.ParameterData[0], position, false);
                    m_outputs.Add(new Output(Id<TConnector>.New(), connectorDefinition, this, new IParameter[0], new DummyConnectionRules()));
                }
            }

            public IReadOnlyList<NodeData.ConfigData> Config { get { return new List<NodeData.ConfigData>(); } }

            List<Output> m_outputs = new List<Output>();

            public IEnumerable<Output> Connectors
            {
                get { return m_outputs; }
            }

            public string Name { get; }

            public string Description { get; }

            public Id<NodeTemp> NodeId { get; }

            public Id<NodeTypeTemp> NodeTypeId { get; }

            public IEnumerable<IParameter> Parameters { get; }

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

        class DummyAudioLibrary : IAudioLibrary
        {
            public IProjectElementList<IAudioFile> AudioFiles
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public Audio Generate(AudioGenerationParameters parameters)
            {
                throw new NotImplementedException();
            }

            public void Play(Audio value)
            {
                throw new NotImplementedException();
            }

            public void Play(IAudioFile file)
            {
                throw new NotImplementedException();
            }

            public void Rename(string from, string to)
            {
                throw new NotImplementedException();
            }

            public IDisposable SuppressUpdates()
            {
                return new DummyDisposable();
            }

            public void UpdateUsage()
            {
            }

            public void UpdateUsage(Audio audio)
            {
            }

            public void UpdateUsage(ConversationNode<INodeGui> n)
            {
            }

            public IEnumerable<Audio> UsedAudio()
            {
                throw new NotImplementedException();
            }
        }

        private static INodeGui MakeNodeUI(ConversationNode node, Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localize)
        {
            return new EditableUI(node, new PointF(100, 100), localize);
        }

        private static ConversationNode MakeNode()
        {
            var data = new DummyConversationNodeData("test", Id<NodeTemp>.Parse("d3aa34e9-35e5-4aa5-bd7d-d9b98ab5a54e"), "description", Id<NodeTypeTemp>.Parse("1edf950a-088a-432f-aee9-b67cf9b7f3c0"), Enumerable.Empty<IParameter>());

            return new ConversationNode(data, d => MakeNodeUI(d, (a, b) => null), d => null);
        }

        [Test]
        public static void Test()
        {
            IEnumerable<GraphAndUI<NodeUIData>> nodes = Enumerable.Empty<GraphAndUI<NodeUIData>>();
            List<NodeGroup> groups = new List<NodeGroup>();
            MemoryStream rawData = new MemoryStream();
            DocumentPath file = DocumentPath.FromPath("DeleteMe.txt", new DirectoryInfo("."));
            ISerializer<TData> serializer = null;
            ReadOnlyCollection<LoadError> errors = new ReadOnlyCollection<LoadError>(new List<LoadError>());
            INodeFactory nodeFactory = null;
            GenerateAudio generateAudio = null;
            var source = new DynamicEnumParameter.Source();
            Func<IDynamicEnumParameter, object, DynamicEnumParameter.Source> getDocumentSource = (a, b) => source;
            IAudioLibrary audioProvider = new DummyAudioLibrary();

            List<List<ConversationNode>> states = new List<List<ConversationNode<INodeGui>>>();
            states.Add(new List<ConversationNode>());

            Random r = new Random(0);
            UpToDateFile.BackEnd backend = new UpToDateFile.BackEnd();
            var id = Id<FileInProject>.Parse("6a1bd06a-0028-4099-a375-475f1a5320db");
            using (ConversationFile conversationFile = new ConversationFile(id, nodes, groups, rawData, file, serializer, errors, nodeFactory, generateAudio, getDocumentSource, audioProvider, backend))
            {
                for (int i = 0; i < 10; i++)
                {
                    var node = MakeNode();
                    var state = states[i].ToList();
                    state.Add(node);
                    conversationFile.Add(new[] { node }, Enumerable.Empty<NodeGroup>(), null);
                    CheckState(conversationFile, state);
                    states.Add(state);
                }

                Action<ConversationNode> CheckNode = node =>
                {
                    var connector = conversationFile.UIInfo(node.Data.Connectors.First(), false);
                    Assert.That(connector.Area.Value, Is.EqualTo(TopPosition(node.Renderer.Area)));
                };

                for (int n = 0; n < 10000; n++)
                {
                    var node = states.Last().Last();
                    node.Renderer.MoveTo(new PointF((float)r.NextDouble() * 1000, (float)r.NextDouble() * 1000));
                    CheckNode(node);
                }

                for (int i = 9; i >= 0; i--)
                {
                    conversationFile.UndoableFile.UndoQueue.Undo();
                    var state = states[i];
                    CheckState(conversationFile, state);
                }
                for (int i = 1; i <= 10; i++)
                {
                    conversationFile.UndoableFile.UndoQueue.Redo();
                    var state = states[i];
                    CheckState(conversationFile, state);
                }
            }
            Assert.Inconclusive();
        }

        private static void CheckState(ConversationFile conversationFile, List<ConversationNode<INodeGui>> state)
        {
            Assert.That(conversationFile.Nodes, Is.EquivalentTo(state));
        }
    }
}
