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
        public DomainSerializer Serializer;
        public DomainDeserializer CategoriesDeserializer;
        public DomainDeserializer TypesDeserializer;
        public DomainDeserializer ConnectorsDeserializer;
        public DomainDeserializer NodesDeserializer;
        public DomainDeserializer EditorDataDeserializer;
        public DomainDeserializer EverythingDeserializer;

        public DomainSerializerDeserializer(DomainSerializer serializer, DomainDeserializer categoriesDeserializer, DomainDeserializer typesDeserializer, DomainDeserializer connectorsDeserializer, DomainDeserializer nodesDeserializer, DomainDeserializer editorDataDeserializer, DomainDeserializer everythingDeserializer)
        {
            Serializer = serializer;
            CategoriesDeserializer = categoriesDeserializer;
            TypesDeserializer = typesDeserializer;
            ConnectorsDeserializer = connectorsDeserializer;
            NodesDeserializer = nodesDeserializer;
            EditorDataDeserializer = editorDataDeserializer;
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
            var ErrorDeserializer = XmlDomain<NodeUIData, ConversationEditorData>.Deserializer.Everything(d, NodeUIDataSerializerXml.Instance);
            return new DomainSerializerDeserializer(Serializer, CategoriesDeserializer, TypesDeserializer, ConnectorsDeserializer, NodesDeserializer, EditorDataDeserializer, ErrorDeserializer);
        }
    }

    public static class SerializationUtils
    {
        public static ISerializer<XmlGraphData<NodeUIData, ConversationEditorData>> DomainSerializer
        {
            get
            {
                return new XmlDomain<NodeUIData, ConversationEditorData>.Serializer(NodeUIDataSerializerXml.Instance, new ConversationEditorData.Serializer());
            }
        }

        public static XmlGraphData<NodeUIData, ConversationEditorData> MakeDomainData(IEnumerable<ConversationNode<INodeGUI>> nodes, ConversationEditorData data)
        {
            var nodeData = nodes.Select(n => new GraphAndUI<NodeUIData>(n.m_data, NodeUIData.Make(n.Renderer)));
            return MakeDomainData(nodeData, data);
        }

        public static XmlGraphData<NodeUIData, ConversationEditorData> MakeDomainData(IEnumerable<GraphAndUI<NodeUIData>> nodeData, ConversationEditorData data)
        {
            return new XmlGraphData<NodeUIData, ConversationEditorData>(nodeData, data);
        }

        #region Conversation
        public static ISerializer<XmlGraphData<NodeUIData, ConversationEditorData>> ConversationSerializer
        {
            get
            {
                return new XmlConversation<NodeUIData, ConversationEditorData>.Serializer(NodeUIDataSerializerXml.Instance, new ConversationEditorData.Serializer());
            }
        }

        public static ISerializerDeserializer<XmlGraphData<NodeUIData, ConversationEditorData>> ConversationSerializerDeserializer(IDataSource d)
        {
            return new XmlConversation<NodeUIData, ConversationEditorData>.SerializerDeserializer(d, NodeUIDataSerializerXml.Instance, new ConversationEditorData.Serializer(), new ConversationEditorData.Deserializer());
        }

        public static IDeserializer<XmlGraphData<NodeUIData, ConversationEditorData>> ConversationDeserializer(IDataSource d)
        {
            return new XmlConversation<NodeUIData, ConversationEditorData>.Deserializer(d, NodeUIDataSerializerXml.Instance, new ConversationEditorData.Deserializer());
        }

        public static XmlGraphData<NodeUIData, ConversationEditorData> MakeConversationData(IEnumerable<ConversationNode<INodeGUI>> nodes, ConversationEditorData data)
        {
            var nodeData = nodes.Select(n => new GraphAndUI<NodeUIData>(n.m_data, NodeUIData.Make(n.Renderer)));
            return MakeConversationData(nodeData, data);
        }

        public static XmlGraphData<NodeUIData, ConversationEditorData> MakeConversationData(IEnumerable<GraphAndUI<NodeUIData>> nodeData, ConversationEditorData data)
        {
            return new XmlGraphData<NodeUIData, ConversationEditorData>(nodeData, data);
        }
        #endregion
    }
}
