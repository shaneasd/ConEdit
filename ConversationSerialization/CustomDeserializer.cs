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

namespace Conversation.Serialization
{
    public class CustomDeserializer : IDeserializer<RuntimeConversation.Conversation>
    {
        private static readonly string[] XML_VERSION_READ = new[] { "1.0", "1.1" };

        private Func<ID<NodeTypeTemp>, ID<NodeTemp>, IEnumerable<Parameter>, PointF, Or<RuntimeConversation.NodeBase, Error>> m_datasource;
        public struct Parameter { public Guid Guid; public string Value; }

        public CustomDeserializer(Func<ID<NodeTypeTemp>, ID<NodeTemp>, IEnumerable<Parameter>, PointF, Or<RuntimeConversation.NodeBase, Error>> datasource)
        {
            m_datasource = datasource;
        }

        public RuntimeConversation.Conversation Read(Stream stream)
        {
            stream.Position = 0;
            var d = XDocument.Load(stream);
            var root = d.Element(XMLConversation<object, object>.ROOT);

            if (root.Attribute("xmlversion") == null || !XML_VERSION_READ.Contains(root.Attribute("xmlversion").Value))
                throw new Exception("unrecognised conversation xml version");

            IEnumerable<Or<RuntimeConversation.NodeBase, Error>> editables = root.Elements("Node").Select(n => ReadEditable(n, m_datasource)).Evaluate();
            var allnodes = new Dictionary<ID<NodeTemp>, RuntimeConversation.NodeBase>();
            var errors = new List<Error>();

            foreach (var editable in editables)
            {
                editable.Do(e =>
                {
                    allnodes[e.ID] = e;
                }, a =>
                {
                    errors.Add(a);
                });
            }

            //This makes a lot of assumptions but I'm fairly sure they're assumptions that currently hold
            IEnumerable<ID<NodeTemp>> filteredNodes = allnodes.Keys.Evaluate();

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

        private Or<RuntimeConversation.NodeBase, Error> ReadEditable(XElement node, Func<ID<NodeTypeTemp>, ID<NodeTemp>, IEnumerable<Parameter>, PointF, Or<RuntimeConversation.NodeBase, Error>> datasource)
        {
            ID<NodeTemp> id = ID<NodeTemp>.Parse(node.Attribute("Id").Value);
            ID<NodeTypeTemp> guid = ID<NodeTypeTemp>.Parse(node.Attribute("Guid").Value);
            var parameters = node.Elements("Parameter").Select(e => new Parameter() { Guid = Guid.Parse(e.Attribute("guid").Value), Value = e.Attribute("value").Value });
            node = node.Element("Area");
            float x = float.Parse(node.Attribute("X").Value, CultureInfo.InvariantCulture);
            float y = float.Parse(node.Attribute("Y").Value, CultureInfo.InvariantCulture);
            return datasource(guid, id, parameters, new PointF(x,y));
        }

        private static HashSet<UnorderedTuple2<Tuple<ID<TConnector>, ID<NodeTemp>>>> ReadLinks(IEnumerable<ID<NodeTemp>> filteredNodes, XElement root)
        {
            HashSet<UnorderedTuple2<Tuple<ID<TConnector>, ID<NodeTemp>>>> result = new HashSet<UnorderedTuple2<Tuple<ID<TConnector>, ID<NodeTemp>>>>();

            foreach (var link in root.Elements("Link"))
            {
                ID<NodeTemp> node1 = ID<NodeTemp>.Parse(link.Attribute("node1").Value);
                ID<TConnector> connector1 = ID<TConnector>.Parse(link.Attribute("connector1").Value);
                ID<NodeTemp> node2 = ID<NodeTemp>.Parse(link.Attribute("node2").Value);
                ID<TConnector> connector2 = ID<TConnector>.Parse(link.Attribute("connector2").Value);

                if (filteredNodes.Any(n => n.Equals(node1) || n.Equals(node2)))
                    result.Add(UnordererTuple.Make(Tuple.Create(connector1, node1), Tuple.Create(connector2, node2)));
            }
            return result;
        }
    }

}
