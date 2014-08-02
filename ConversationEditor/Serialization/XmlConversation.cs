using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Conversation;
using Utilities;
using System.Drawing;

namespace ConversationEditor.Serialization
{
    //using ConversationNode = Conversation.ConversationNode<INodeGUI<TransitionNoduleUIInfo>, TransitionNoduleUIInfo>;

    //TODO: Factory should happen outside of this class. This class should just supply the data required

    public static class XMLConversation<TNodeUI, TTransitionUI, TUIRawData> where TNodeUI : INodeUI<TNodeUI, TTransitionUI>
    {
        private class Inner
        {
            const string XML_VERSION = "1.0";
            public const string ROOT = "Root";

            private class NodeAndLinks
            {
                public readonly ConversationNode<TNodeUI, TTransitionUI> Node;
                public readonly IDictionary<Output, IEnumerable<NodeID>> Links;
                public NodeAndLinks(ConversationNode<TNodeUI, TTransitionUI> node, IDictionary<Output, IEnumerable<NodeID>> links)
                {
                    Node = node;
                    Links = links;
                }
            }

            private XDocument m_document;
            private XElement m_root;
            private ISerializerDeserializerXml<TNodeUI, TUIRawData> m_nodeUISerializer;

            public Inner(ISerializerDeserializerXml<TNodeUI, TUIRawData> nodeUISerializer)
            {
                m_document = new XDocument();
                m_root = new XElement(ROOT, new XAttribute("xmlversion", XML_VERSION));
                m_document.Add(m_root);
                m_nodeUISerializer = nodeUISerializer;
            }

            public void Save(Stream stream)
            {
                m_document.Save(stream);
            }

            private void WriteLinks(ConversationNode<TNodeUI, TTransitionUI> ng, XElement node)
            {
                foreach (var n in ng.TransitionsOut)
                {
                    var guid = new XAttribute("guid", n.Output.Guid);
                    var outputNode = new XElement("Output", guid);
                    foreach (var link in n.Connections)
                    {
                        var to = new XAttribute("To", link.Owner.Id.ToString());
                        var linkNode = new XElement("Link", to);
                        outputNode.Add(linkNode);
                    }
                    node.Add(outputNode);
                }
            }

            private IDictionary<Output, IEnumerable<NodeID>> ReadLinks(IEnumerable<ITransitionOutNode<ConversationNode<TNodeUI, TTransitionUI>, TTransitionUI>> outputs, XElement node)
            {
                Dictionary<Output, IEnumerable<NodeID>> result = new Dictionary<Output, IEnumerable<NodeID>>();
                foreach (var output in outputs.Select(a => a.Output))
                {
                    var outputNode = node.Elements("Output").SingleOrDefault(e => Guid.Parse(e.Attribute("guid").Value) == output.Guid);
                    result[output] = outputNode != null ? outputNode.Elements("Link").Select(l => NodeID.Parse(l.Attribute("To").Value)) : Enumerable.Empty<NodeID>();
                }
                return result;
            }

            private NodeAndLinks MakeNodeAndLinks(ConversationNode<TNodeUI, TTransitionUI> node, XElement xml)
            {
                var links = ReadLinks(node.TransitionsOut, xml);
                return new NodeAndLinks(node, links);
            }

            private NodeAndLinks ReadEditable(XElement node, IDataSource datasource, INodeFactory<ConversationNode<TNodeUI, TTransitionUI>, TTransitionUI, TUIRawData> nodeFactory)
            {
                NodeID id = NodeID.Parse(node.Attribute("Id").Value);
                Guid guid = Guid.Parse(node.Attribute("Guid").Value);

                Func<NodeID, ExternalFunction> nodeGenerator = datasource.GetNode(guid);
                IEditable editable = nodeGenerator(id);

                List<Tuple<Parameter, Func<bool>>> failedParameters = new List<Tuple<Parameter, Func<bool>>>();
                foreach (Parameter p in editable.Parameters)
                {
                    var pp = p;
                    Func<bool> retry = () =>
                    {
                        try
                        {
                            return pp.TryDeserialiseValue(node.Elements("Parameter").Where(n => Guid.Parse(n.Attribute("guid").Value) == pp.Guid).Select(n => n.Attribute("value").Value).Single());
                        }
                        catch
                        {
                            return false;
                        }
                    };
                    if (!retry())
                        failedParameters.Add(Tuple.Create(pp, retry));
                }
                ConversationNode<TNodeUI, TTransitionUI> a = !failedParameters.Any() ? nodeFactory.MakeNode(editable, m_nodeUISerializer.Read(node))
                                                            : nodeFactory.MakeCorruptedNode(editable, m_nodeUISerializer.Read(node), failedParameters);
                return MakeNodeAndLinks(a, node);
            }

            public void Write(ConversationNode<TNodeUI, TTransitionUI> condition)
            {
                var id = new XAttribute("Id", condition.Id.ToString());
                var guid = new XAttribute("Guid", condition.Guid.ToString());
                var node = new XElement("Node", id, guid);
                m_nodeUISerializer.Write(condition.Renderer, node); //TODO: Bad cast
                //WriteArea(condition, node);
                WriteLinks(condition, node);
                foreach (var parameter in condition.Parameters)
                {
                    var p = new XElement("Parameter", new XAttribute("guid", parameter.Guid), new XAttribute("value", parameter.ValueAsString()));
                    node.Add(p);
                }
                m_root.Add(node);
            }

            public void Write(NodeGroup<ConversationNode<TNodeUI, TTransitionUI>> group)
            {
                //TODO: Write groups again when things have stabilised
                //var g = new XElement("Group");
                //foreach (var node in group.Contents)
                //    g.Add(new XElement("Node", new XAttribute("Id", node.Id.ToString())));
                //m_nodeUISerializer.Write(group, g);
                ////WriteArea(group, g);
                //m_root.Add(g);
            }

            public void ReadNodes(out IEnumerable<ConversationNode<TNodeUI, TTransitionUI>> nodes, out IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>> groups, Stream stream, IDataSource datasource, INodeFactory<ConversationNode<TNodeUI, TTransitionUI>, TTransitionUI, TUIRawData> nodeFactory)
            {
                stream.Position = 0;
                var d = XDocument.Load(stream);
                var root = d.Element(ROOT);

                if (root.Attribute("xmlversion") == null || root.Attribute("xmlversion").Value != XML_VERSION)
                    throw new Exception("unrecognised conversation xml version");

                IDictionary<ConversationNode<TNodeUI, TTransitionUI>, IDictionary<Output, IEnumerable<NodeID>>> links = root.Elements("Node").Select(n => ReadEditable(n, datasource, nodeFactory)).ToDictionary(nnl => nnl.Node, nnl => nnl.Links);
                IDictionary<NodeID, ConversationNode<TNodeUI, TTransitionUI>> allnodes = links.Keys.ToDictionary(n => n.Id, n => n);

                foreach (var nodeAndLink in links)
                {
                    var node = nodeAndLink.Key;
                    foreach (var output in node.TransitionsOut)
                    {
                        if (nodeAndLink.Value.ContainsKey(output.Output))
                        {
                            foreach (var linkto in nodeAndLink.Value[output.Output])
                            {
                                if (allnodes.ContainsKey(linkto))
                                    output.ConnectTo(allnodes[linkto].TransitionsIn.First());
                            }
                        }
                    }
                }
                nodes = allnodes.Values;

                var groupsResult = new List<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>>();
                //TODO: Read groups when serialization has stabilized
                //foreach (var g in root.Elements("Group"))
                //{
                //    var contents = g.Elements("Node").Select(n => NodeID.Parse(n.Attribute("Id").Value));
                //    var containedNodes = contents.Where(id => allnodes.ContainsKey(id)).Select(id => allnodes[id]);
                //    m_nodeUISerializer.Read(g);
                //    //System.Drawing.RectangleF area = ReadArea(g);
                //    groupsResult.Add(new NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>(area, containedNodes));
                //}
                groups = groupsResult;
            }

            public void Write(IEnumerable<ConversationNode<TNodeUI, TTransitionUI>> nodes, IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>> groups)
            {
                Write(nodes);
                Write(groups);
            }

            private void Write(IEnumerable<ConversationNode<TNodeUI, TTransitionUI>> nodes)
            {
                foreach (var node in nodes)
                    Write(node);
            }

            private void Write(IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>> groups)
            {
                foreach (var group in groups)
                    Write(group);
            }
        }

        public class SerializerDeserializer : ISerializerDeserializer<Tuple<IEnumerable<ConversationNode<TNodeUI, TTransitionUI>>, IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>>>>
        {
            Deserializer deserializer;
            Serializer serializer;

            public SerializerDeserializer(IDataSource datasource, INodeFactory<ConversationNode<TNodeUI, TTransitionUI>, TTransitionUI, TUIRawData> nodeFactory, ISerializerDeserializerXml<TNodeUI, TUIRawData> nodeUISerializer)
            {
                deserializer = new Deserializer(datasource, nodeFactory, nodeUISerializer);
                serializer = new Serializer(nodeUISerializer);
            }

            public void Write(Tuple<IEnumerable<ConversationNode<TNodeUI, TTransitionUI>>, IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>>> data, Stream stream)
            {
                serializer.Write(data, stream);
            }

            public Tuple<IEnumerable<ConversationNode<TNodeUI, TTransitionUI>>, IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>>> Read(Stream stream)
            {
                return deserializer.Read(stream);
            }
        }

        public class Deserializer : IDeserializer<Tuple<IEnumerable<ConversationNode<TNodeUI, TTransitionUI>>, IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>>>>
        {
            private IDataSource m_datasource;
            private INodeFactory<ConversationNode<TNodeUI, TTransitionUI>, TTransitionUI, TUIRawData> m_nodeFactory;
            private ISerializerDeserializerXml<TNodeUI, TUIRawData> m_nodeUISerializer;

            public Deserializer(IDataSource datasource, INodeFactory<ConversationNode<TNodeUI, TTransitionUI>, TTransitionUI, TUIRawData> nodeFactory, ISerializerDeserializerXml<TNodeUI, TUIRawData> nodeUISerializer)
            {
                m_datasource = datasource;
                m_nodeFactory = nodeFactory;
                m_nodeUISerializer = nodeUISerializer;
            }

            public Tuple<IEnumerable<ConversationNode<TNodeUI, TTransitionUI>>, IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>>> Read(Stream stream)
            {
                IEnumerable<ConversationNode<TNodeUI, TTransitionUI>> nodes;
                IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>> groups;
                Inner serialiser = new Inner(m_nodeUISerializer);
                serialiser.ReadNodes(out nodes, out groups, stream, m_datasource, m_nodeFactory);
                return Tuple.Create(nodes, groups);
            }
        }

        public class Serializer : ISerializer<Tuple<IEnumerable<ConversationNode<TNodeUI, TTransitionUI>>, IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>>>>
        {
            ISerializerDeserializerXml<TNodeUI, TUIRawData> m_nodeUISerializer;
            public Serializer(ISerializerDeserializerXml<TNodeUI, TUIRawData> nodeUISerializer)
            {
                m_nodeUISerializer = nodeUISerializer;
            }
            public void Write(Tuple<IEnumerable<ConversationNode<TNodeUI, TTransitionUI>>, IEnumerable<NodeGroup<ConversationNode<TNodeUI, TTransitionUI>>>> conversation, Stream stream)
            {
                var serialiser = new Inner(m_nodeUISerializer);
                serialiser.Write(conversation.Item1, conversation.Item2);
                serialiser.Save(stream);
                stream.Flush();
            }
        }
    }
}