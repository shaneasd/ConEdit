using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

namespace Conversation.Serialization
{
    public static class XMLDomain<TUIRawData, TEditorData>
    {
        public class Serializer : ISerializer<XmlGraphData<TUIRawData, TEditorData>>
        {
            private ISerializerDeserializerXml<TUIRawData, TUIRawData> m_nodeUISerializer;
            private ISerializerXml<TEditorData> m_editorDataSerializer;

            public Serializer(ISerializerDeserializerXml<TUIRawData, TUIRawData> nodeUISerializer, ISerializerXml<TEditorData> editorDataSerializer)
            {
                m_nodeUISerializer = nodeUISerializer;
                m_editorDataSerializer = editorDataSerializer;
            }

            public void Write(XmlGraphData<TUIRawData, TEditorData> data, Stream stream)
            {
                var serializer = new XMLConversation<TUIRawData, TEditorData>.Serializer(m_nodeUISerializer, m_editorDataSerializer);
                serializer.Write(data, stream);
            }
        }

        public class Deserializer : IDeserializer<XmlGraphData<TUIRawData, TEditorData>>
        {
            private IDeserializer<XmlGraphData<TUIRawData, TEditorData>> m_inner;

            public static IDeserializer<XmlGraphData<TUIRawData, TEditorData>> Categories(IDataSource source, IDeserializerXml<TUIRawData> nodeUISerializer)
            {
                Func<ID<NodeTypeTemp>, bool> filter = id => source.IsCategoryDefinition(id);
                var editorDataDeserializer = NullDeserializer<TEditorData>.Instance;
                var inner = new XMLConversation<TUIRawData, TEditorData>.Deserializer(source, nodeUISerializer, editorDataDeserializer, filter);
                return new Deserializer(inner);
            }

            public static IDeserializer<XmlGraphData<TUIRawData, TEditorData>> Types(IDataSource source, IDeserializerXml<TUIRawData> nodeUISerializer)
            {
                Func<ID<NodeTypeTemp>, bool> filter = id => source.IsTypeDefinition(id);
                var editorDataDeserializer = NullDeserializer<TEditorData>.Instance;
                var inner = new XMLConversation<TUIRawData, TEditorData>.Deserializer(source, nodeUISerializer, editorDataDeserializer, filter);
                return new Deserializer(inner);
            }

            public static IDeserializer<XmlGraphData<TUIRawData, TEditorData>> Connectors(IDataSource source, IDeserializerXml<TUIRawData> nodeUISerializer)
            {
                Func<ID<NodeTypeTemp>, bool> filter = id => source.IsConnectorDefinition(id);
                var editorDataDeserializer = NullDeserializer<TEditorData>.Instance;
                var inner = new XMLConversation<TUIRawData, TEditorData>.Deserializer(source, nodeUISerializer, editorDataDeserializer, filter);
                return new Deserializer(inner);
            }

            public static IDeserializer<XmlGraphData<TUIRawData, TEditorData>> Nodes(IDataSource source, IDeserializerXml<TUIRawData> nodeUISerializer)
            {
                Func<ID<NodeTypeTemp>, bool> filter = id => source.IsNodeDefinition(id);
                var editorDataDeserializer = NullDeserializer<TEditorData>.Instance;
                var inner = new XMLConversation<TUIRawData, TEditorData>.Deserializer(source, nodeUISerializer, editorDataDeserializer, filter);
                return new Deserializer(inner);
            }

            class ErrorsOnly : IDeserializer<XmlGraphData<TUIRawData, TEditorData>>
            {
                private IDeserializer<XmlGraphData<TUIRawData, TEditorData>> m_inner;

                public ErrorsOnly(IDeserializer<XmlGraphData<TUIRawData, TEditorData>> inner)
                {
                    m_inner = inner;
                }

                public XmlGraphData<TUIRawData, TEditorData> Read(Stream stream)
                {
                    var data = m_inner.Read(stream);
                    return new XmlGraphData<TUIRawData, TEditorData>(Enumerable.Empty<GraphAndUI<TUIRawData>>(), data.EditorData, data.Errors);
                }
            }

            public static IDeserializer<XmlGraphData<TUIRawData, TEditorData>> Everything(IDataSource source, IDeserializerXml<TUIRawData> nodeUISerializer)
            {
                Func<ID<NodeTypeTemp>, bool> filter = id => true;
                var editorDataDeserializer = NullDeserializer<TEditorData>.Instance;
                var inner = new XMLConversation<TUIRawData, TEditorData>.Deserializer(source, nodeUISerializer, editorDataDeserializer, filter);
                return new Deserializer(inner);
            }

            public static IDeserializer<XmlGraphData<TUIRawData, TEditorData>> UI(IDataSource source, IDeserializerXml<TUIRawData> nodeUISerializer, IDeserializerXml<TEditorData> editorDataDeserializer)
            {
                Func<ID<NodeTypeTemp>, bool> filter = id => false;
                var inner = new XMLConversation<TUIRawData, TEditorData>.Deserializer(source, nodeUISerializer, editorDataDeserializer, filter);
                return new Deserializer(inner);
            }

            private Deserializer(IDeserializer<XmlGraphData<TUIRawData, TEditorData>> inner)
            {
                m_inner = inner;
            }

            public XmlGraphData<TUIRawData, TEditorData> Read(Stream stream)
            {
                var data = m_inner.Read(stream);
                return new XmlGraphData<TUIRawData, TEditorData>(data.Nodes, data.EditorData, data.Errors);
            }
        }
    }
}
