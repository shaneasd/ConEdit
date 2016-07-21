using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation.Serialization;
using Conversation;
using Utilities;
using System.Xml.Linq;
using System.IO;
using System.Globalization;
using System.Drawing;

namespace RuntimeConversation
{
    public struct CustomDeserializerParameter
    {
        public CustomDeserializerParameter(Guid guid, string value)
        {
            Guid = guid;
            Value = value;
        }
        public Guid Guid { get; }
        public string Value { get; }
    }

    public class CustomDeserializer : IDeserializer<RuntimeConversation.Conversation>
    {
        private static readonly string[] XML_VERSION_READ = new[] { "1.0", "1.1" };

        private Func<Id<NodeTypeTemp>, Id<NodeTemp>, IEnumerable<CustomDeserializerParameter>, PointF, Either<RuntimeConversation.NodeBase, LoadError>> m_datasource;

        public CustomDeserializer(Func<Id<NodeTypeTemp>, Id<NodeTemp>, IEnumerable<CustomDeserializerParameter>, PointF, Either<RuntimeConversation.NodeBase, LoadError>> datasource)
        {
            m_datasource = datasource;
        }

        public RuntimeConversation.Conversation Read(Stream stream)
        {
            stream.Position = 0;
            var d = XDocument.Load(stream);
            var root = d.Element(XmlConversation<object, object>.Root);

            if (root.Attribute("xmlversion") == null || !XML_VERSION_READ.Contains(root.Attribute("xmlversion").Value))
                throw new UnknownXmlVersionException("unrecognised conversation xml version");

            IEnumerable<Either<RuntimeConversation.NodeBase, LoadError>> editables = root.Elements("Node").Select(n => ReadEditable(n, m_datasource)).Evaluate();
            var allnodes = new Dictionary<Id<NodeTemp>, RuntimeConversation.NodeBase>();
            var errors = new List<LoadError>();

            foreach (var editable in editables)
            {
                editable.Do(e =>
                {
                    allnodes[e.Id] = e;
                }, a =>
                {
                    errors.Add(a);
                });
            }

            //This makes a lot of assumptions but I'm fairly sure they're assumptions that currently hold
            IEnumerable<Id<NodeTemp>> filteredNodes = allnodes.Keys.Evaluate();

            var links = ReadLinks(filteredNodes, root);

            foreach (var link in links)
            {
                var id1 = link.Item1.Item2;
                var id2 = link.Item2.Item2;
                RuntimeConversation.NodeBase node1 = allnodes[id1];
                RuntimeConversation.NodeBase node2 = allnodes[id2];
                node1.Connect(link.Item1.Item1, node2, link.Item2.Item1);
            }

            return new RuntimeConversation.Conversation(allnodes.Values.Cast<RuntimeConversation.NodeBase>(), errors);
        }

        private static Either<RuntimeConversation.NodeBase, LoadError> ReadEditable(XElement node, Func<Id<NodeTypeTemp>, Id<NodeTemp>, IEnumerable<CustomDeserializerParameter>, PointF, Either<RuntimeConversation.NodeBase, LoadError>> datasource)
        {
            Id<NodeTemp> id = Id<NodeTemp>.Parse(node.Attribute("Id").Value);
            Id<NodeTypeTemp> guid = Id<NodeTypeTemp>.Parse(node.Attribute("Guid").Value);
            var parameters = node.Elements("Parameter").Select(e => new CustomDeserializerParameter(Guid.Parse(e.Attribute("guid").Value), e.Attribute("value").Value));
            node = node.Element("Area");
            float x = float.Parse(node.Attribute("X").Value, CultureInfo.InvariantCulture);
            float y = float.Parse(node.Attribute("Y").Value, CultureInfo.InvariantCulture);
            return datasource(guid, id, parameters, new PointF(x, y));
        }

        private static HashSet<UnorderedTuple2<Tuple<Id<TConnector>, Id<NodeTemp>>>> ReadLinks(IEnumerable<Id<NodeTemp>> filteredNodes, XElement root)
        {
            HashSet<UnorderedTuple2<Tuple<Id<TConnector>, Id<NodeTemp>>>> result = new HashSet<UnorderedTuple2<Tuple<Id<TConnector>, Id<NodeTemp>>>>();

            foreach (var link in root.Elements("Link"))
            {
                Id<NodeTemp> node1 = Id<NodeTemp>.Parse(link.Attribute("node1").Value);
                Id<TConnector> connector1 = Id<TConnector>.Parse(link.Attribute("connector1").Value);
                Id<NodeTemp> node2 = Id<NodeTemp>.Parse(link.Attribute("node2").Value);
                Id<TConnector> connector2 = Id<TConnector>.Parse(link.Attribute("connector2").Value);

                if (filteredNodes.Any(n => n.Equals(node1) || n.Equals(node2)))
                    result.Add(UnorderedTuple.Make(Tuple.Create(connector1, node1), Tuple.Create(connector2, node2)));
            }
            return result;
        }
    }

}
