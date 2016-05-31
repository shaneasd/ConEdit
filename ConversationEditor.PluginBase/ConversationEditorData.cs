using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using System.Xml.Linq;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGui>;
    using XmlConversationData = Conversation.Serialization.XmlGraphData<NodeUIData, ConversationEditorData>;
    using Conversation.Serialization;

    public class ConversationEditorData
    {
        public IEnumerable<NodeGroup> Groups = Enumerable.Empty<NodeGroup>();

        public class Serializer : ISerializerXml<ConversationEditorData>
        {
            public void Write(ConversationEditorData data, XElement node)
            {
                foreach (var group in data.Groups)
                    Write(group, node);
            }

            private static void Write(NodeGroup group, XContainer root)
            {
                var g = new XElement("Group");
                foreach (var node in group.Contents)
                    g.Add(new XElement("Node", new XAttribute("Id", node.Serialized())));
                NodeUIDataSerializerXml.WriteArea(group.Renderer.Area, g);
                root.Add(g);
            }
        }

        public class Deserializer : IDeserializerXml<ConversationEditorData>
        {
            public Deserializer()
            {
            }

            public ConversationEditorData Read(XElement node)
            {
                var groupsResult = new List<NodeGroup>();
                foreach (var g in node.Elements("Group"))
                {
                    var contents = g.Elements("Node").Select(n => Id<NodeTemp>.Parse(n.Attribute("Id").Value));
                    System.Drawing.RectangleF area = NodeUIDataSerializerXml.ReadArea(g);
                    groupsResult.Add(new NodeGroup(area, contents));
                }
                return new ConversationEditorData() { Groups = groupsResult };
            }
        }

        public class SerializerDeserializer : ISerializerDeserializerXml<ConversationEditorData, ConversationEditorData>
        {
            private Serializer m_serializer;
            private Deserializer m_deserializer;
            public SerializerDeserializer()
            {
                m_serializer = new Serializer();
                m_deserializer = new Deserializer();
            }

            public void Write(ConversationEditorData data, XElement node)
            {
                m_serializer.Write(data, node);
            }

            public ConversationEditorData Read(XElement node)
            {
                return m_deserializer.Read(node);
            }
        }
    }
}
