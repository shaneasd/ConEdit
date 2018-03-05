using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using Conversation.Serialization;

namespace ConversationEditor
{
    using DomainSerializer = ISerializer<XmlGraphData<NodeUIData, ConversationEditorData>>;
    using DomainDeserializer = IDeserializer<XmlGraphData<NodeUIData, ConversationEditorData>>;
    using System.IO;

    public class DomainSerializerDeserializer
    {
        public DomainSerializer Serializer { get; private set; }
        public DomainDeserializer CategoriesDeserializer { get; private set; }
        public DomainDeserializer TypesDeserializer { get; private set; }
        public DomainDeserializer ConnectorsDeserializer { get; private set; }
        public DomainDeserializer NodesDeserializer { get; private set; }
        public DomainDeserializer EditorDataDeserializer { get; private set; }
        public DomainDeserializer AutoCompleteSuggestionsDeserializer { get; private set; }
        public DomainDeserializer EverythingDeserializer { get; private set; }

        public DomainSerializerDeserializer(DomainSerializer serializer, DomainDeserializer categoriesDeserializer, DomainDeserializer typesDeserializer, DomainDeserializer connectorsDeserializer, DomainDeserializer nodesDeserializer, DomainDeserializer editorDataDeserializer, DomainDeserializer autoCompleteSuggestionsDeserializer, DomainDeserializer everythingDeserializer)
        {
            Serializer = serializer;
            CategoriesDeserializer = categoriesDeserializer;
            TypesDeserializer = typesDeserializer;
            ConnectorsDeserializer = connectorsDeserializer;
            NodesDeserializer = nodesDeserializer;
            EditorDataDeserializer = editorDataDeserializer;
            AutoCompleteSuggestionsDeserializer = autoCompleteSuggestionsDeserializer;
            EverythingDeserializer = everythingDeserializer;
        }

        public static DomainSerializerDeserializer Make(IDataSource d)
        {
            var Serializer = SerializationUtils.DomainSerializer;
            var CategoriesDeserializer = XmlDomain<NodeUIData, ConversationEditorData>.Deserializer.Categories(d, NodeUIDataSerializerXml.Instance);
            var TypesDeserializer = XmlDomain<NodeUIData, ConversationEditorData>.Deserializer.Types(d, NodeUIDataSerializerXml.Instance);
            var ConnectorsDeserializer = XmlDomain<NodeUIData, ConversationEditorData>.Deserializer.Connectors(d, NodeUIDataSerializerXml.Instance);
            var NodesDeserializer = XmlDomain<NodeUIData, ConversationEditorData>.Deserializer.Nodes(d, NodeUIDataSerializerXml.Instance);
            var EditorDataDeserializer = XmlDomain<NodeUIData, ConversationEditorData>.Deserializer.UI(d, NodeUIDataSerializerXml.Instance, new ConversationEditorData.Deserializer());
            var AutoCompleteSuggestionsDeserializer = XmlDomain<NodeUIData, ConversationEditorData>.Deserializer.AutoCompleteSuggestions(d, NodeUIDataSerializerXml.Instance);
            var ErrorDeserializer = XmlDomain<NodeUIData, ConversationEditorData>.Deserializer.Everything(d, NodeUIDataSerializerXml.Instance);
            return new DomainSerializerDeserializer(
                serializer: Serializer,
                categoriesDeserializer: CategoriesDeserializer,
                typesDeserializer: TypesDeserializer,
                connectorsDeserializer: ConnectorsDeserializer,
                nodesDeserializer: NodesDeserializer,
                editorDataDeserializer: EditorDataDeserializer,
                autoCompleteSuggestionsDeserializer: AutoCompleteSuggestionsDeserializer,
                everythingDeserializer: ErrorDeserializer);
        }

        public IReadOnlyCollection<Guid> CheckUniqueIds(IEnumerable<MemoryStream> validStreamsAndPaths)
        {
            return SerializationUtils.CheckUniqueIds(validStreamsAndPaths);
        }
    }

    public static class SerializationUtils
    {
        public static ISerializer<XmlGraphData<NodeUIData, ConversationEditorData>> DomainSerializer 
            => new XmlDomain<NodeUIData, ConversationEditorData>.Serializer(NodeUIDataSerializerXml.Instance, new ConversationEditorData.Serializer());

        public static XmlGraphData<NodeUIData, ConversationEditorData> MakeDomainData(IEnumerable<ConversationNode<INodeGui>> nodes, ConversationEditorData data)
        {
            var nodeData = nodes.Select(n => new GraphAndUI<NodeUIData>(n.Data, NodeUIData.Make(n.Renderer)));
            return MakeDomainData(nodeData, data);
        }

        public static XmlGraphData<NodeUIData, ConversationEditorData> MakeDomainData(IEnumerable<GraphAndUI<NodeUIData>> nodeData, ConversationEditorData data)
        {
            return new XmlGraphData<NodeUIData, ConversationEditorData>(nodeData, data);
        }

        #region Conversation
        public static ISerializer<XmlGraphData<NodeUIData, ConversationEditorData>> ConversationSerializer 
            => new XmlConversation<NodeUIData, ConversationEditorData>.Serializer(NodeUIDataSerializerXml.Instance, new ConversationEditorData.Serializer());

        public static ISerializerDeserializer<XmlGraphData<NodeUIData, ConversationEditorData>> ConversationSerializerDeserializer(IDataSource d)
        {
            return new XmlConversation<NodeUIData, ConversationEditorData>.SerializerDeserializer(d, NodeUIDataSerializerXml.Instance, new ConversationEditorData.Serializer(), new ConversationEditorData.Deserializer());
        }

        public static IDeserializer<XmlGraphData<NodeUIData, ConversationEditorData>> ConversationDeserializer(IDataSource d)
        {
            return new XmlConversation<NodeUIData, ConversationEditorData>.Deserializer(d, NodeUIDataSerializerXml.Instance, new ConversationEditorData.Deserializer(), null);
        }

        public static XmlGraphData<NodeUIData, ConversationEditorData> MakeConversationData(IEnumerable<ConversationNode<INodeGui>> nodes, ConversationEditorData data)
        {
            var nodeData = nodes.Select(n => new GraphAndUI<NodeUIData>(n.Data, NodeUIData.Make(n.Renderer)));
            return MakeConversationData(nodeData, data);
        }

        public static XmlGraphData<NodeUIData, ConversationEditorData> MakeConversationData(IEnumerable<GraphAndUI<NodeUIData>> nodeData, ConversationEditorData data)
        {
            return new XmlGraphData<NodeUIData, ConversationEditorData>(nodeData, data);
        }

        internal static IReadOnlyCollection<Guid> CheckUniqueIds(IEnumerable<MemoryStream> validStreamsAndPaths)
        {
           return XmlConversation<NodeUIData, ConversationEditorData>.Deserializer.CheckUniqueIds(validStreamsAndPaths);
        }
        #endregion
    }
}
