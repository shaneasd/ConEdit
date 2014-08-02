using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using ConversationNode = Conversation.ConversationNode<Conversation.INodeGUI<ConversationEditor.TransitionNoduleUIInfo>, ConversationEditor.TransitionNoduleUIInfo>;

namespace ConversationEditor.Serialization
{
    public static class XMLDomain<TNodeUI, TTransitionUI, TUIRawData> where TNodeUI : INodeUI<TNodeUI, TTransitionUI>
    {
        public class Serializer : ISerializer<Tuple<IEnumerable<ConversationNode<TNodeUI, TTransitionUI>>, IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>>>>
        {
            private ISerializerDeserializerXml<TNodeUI, TUIRawData> m_nodeUISerializer;

            public Serializer(ISerializerDeserializerXml<TNodeUI, TUIRawData> nodeUISerializer)
            {
                m_nodeUISerializer = nodeUISerializer;
            }

            public void Write(Tuple<IEnumerable<ConversationNode<TNodeUI, TTransitionUI>>, IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>>> data, Stream stream)
            {
                var serializer = new XMLConversation<TNodeUI, TTransitionUI, TUIRawData>.Serializer(m_nodeUISerializer);
                serializer.Write(data, stream);
            }
        }

        public class Deserializer : IDeserializer<Tuple<IEnumerable<ConversationNode<TNodeUI, TTransitionUI>>, IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>>>>
        {
            private IDataSource m_source;
            private INodeFactory<ConversationNode<TNodeUI, TTransitionUI>, TTransitionUI, TUIRawData> m_nodeFactory;
            private ISerializerDeserializerXml<TNodeUI, TUIRawData> m_nodeUISerializer;
            public Deserializer(IDataSource source, INodeFactory<ConversationNode<TNodeUI, TTransitionUI>, TTransitionUI, TUIRawData> nodeFactory, ISerializerDeserializerXml<TNodeUI, TUIRawData> nodeUISerializer)
            {
                m_source = source;
                m_nodeFactory = nodeFactory;
                m_nodeUISerializer = nodeUISerializer;
            }
            public Tuple<IEnumerable<ConversationNode<TNodeUI, TTransitionUI>>, IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>>> Read(Stream stream)
            {
                var serializer = new XMLConversation<TNodeUI, TTransitionUI, TUIRawData>.Deserializer(m_source, m_nodeFactory, m_nodeUISerializer);
                var data = serializer.Read(stream);
                return new Tuple<IEnumerable<ConversationNode<TNodeUI, TTransitionUI>>, IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>>>(data.Item1.Evaluate(), data.Item2.Evaluate());
            }
        }

        public class SerializerDeserializer : ISerializerDeserializer<Tuple<IEnumerable<ConversationNode<TNodeUI, TTransitionUI>>, IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>>>>
        {
            Serializer m_serializer;
            Deserializer m_deserializer;

            public SerializerDeserializer(IDataSource source, INodeFactory<ConversationNode<TNodeUI, TTransitionUI>, TTransitionUI, TUIRawData> nodeFactory, ISerializerDeserializerXml<TNodeUI, TUIRawData> nodeUISerializer)
            {
                m_deserializer = new Deserializer(source, nodeFactory, nodeUISerializer);
                m_serializer = new Serializer(nodeUISerializer);
            }

            public void Write(Tuple<IEnumerable<ConversationNode<TNodeUI, TTransitionUI>>, IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>>> data, Stream stream)
            {
                m_serializer.Write(data, stream);
            }

            public Tuple<IEnumerable<ConversationNode<TNodeUI, TTransitionUI>>, IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>>> Read(Stream stream)
            {
                return m_deserializer.Read(stream);
            }
        }
    }
}
