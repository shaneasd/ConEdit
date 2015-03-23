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

namespace Conversation.Serialization
{
    public class Error
    {
        public string Message;
        public Error()
        {
            Message = ""; //Will be assigned later presumably. Not critical.
        }
        public Error(string message)
        {
            Message = message;
        }
    }

    public class GraphAndUI<TUIRawData>
    {
        public readonly IEditable GraphData;
        public readonly TUIRawData UIData;

        public GraphAndUI(IEditable graphData, TUIRawData uiData)
        {
            GraphData = graphData;
            UIData = uiData;
        }
    }

    /// <summary>
    /// All data required for serialization/deserialization of a node graph
    /// </summary>
    public class XmlGraphData<TUIRawData, TEditorData>
    {
        public XmlGraphData(IEnumerable<GraphAndUI<TUIRawData>> nodes, TEditorData editorData, List<Error> errors)
        {
            Nodes = nodes;
            EditorData = editorData;
            Errors = errors;
        }

        public XmlGraphData(IEnumerable<GraphAndUI<TUIRawData>> nodes, TEditorData editorData)
        {
            Nodes = nodes;
            EditorData = editorData;
            Errors = new List<Error>();
        }

        public readonly IEnumerable<GraphAndUI<TUIRawData>> Nodes;
        public readonly TEditorData EditorData;
        public readonly List<Error> Errors;
    }

    public static class XMLConversation<TUIRawData, TEditorData>
    {
        private static readonly string[] XML_VERSION_READ = new[] { "1.0", "1.1" };
        const string XML_VERSION_WRITE = "1.1";
        public const string ROOT = "Root";

        public class SerializerDeserializer : ISerializerDeserializer<XmlGraphData<TUIRawData, TEditorData>>
        {
            Deserializer deserializer;
            Serializer serializer;

            public SerializerDeserializer(IDataSource datasource, ISerializerDeserializerXml<TUIRawData, TUIRawData> nodeUISerializer, ISerializerXml<TEditorData> editorDataSerializer, IDeserializerXml<TEditorData> editorDataDeserializer)
            {
                deserializer = new Deserializer(datasource, nodeUISerializer, editorDataDeserializer);
                serializer = new Serializer(nodeUISerializer, editorDataSerializer);
            }

            public void Write(XmlGraphData<TUIRawData, TEditorData> data, Stream stream)
            {
                serializer.Write(data, stream);
            }

            public XmlGraphData<TUIRawData, TEditorData> Read(Stream stream)
            {
                return deserializer.Read(stream);
            }
        }

        public class Deserializer : IDeserializer<XmlGraphData<TUIRawData, TEditorData>>
        {
            private IDataSource m_datasource;
            private IDeserializerXml<TUIRawData> m_nodeUIDeserializer;
            private IDeserializerXml<TEditorData> m_editorDataDeserializer;
            Func<ID<NodeTypeTemp>, bool> m_filter;

            public Deserializer(IDataSource datasource, IDeserializerXml<TUIRawData> nodeUISerializer, IDeserializerXml<TEditorData> editorDataDeserializer, Func<ID<NodeTypeTemp>, bool> filter = null)
            {
                m_datasource = datasource;
                m_nodeUIDeserializer = nodeUISerializer;
                m_editorDataDeserializer = editorDataDeserializer;
                m_filter = filter ?? new Func<ID<NodeTypeTemp>, bool>(i => true);
            }

            public XmlGraphData<TUIRawData, TEditorData> Read(Stream stream)
            {
                stream.Position = 0;
                var d = XDocument.Load(stream);
                var root = d.Element(ROOT);

                if (root.Attribute("xmlversion") == null || !XML_VERSION_READ.Contains(root.Attribute("xmlversion").Value))
                    throw new Exception("unrecognised conversation xml version");

                IEnumerable<Or<GraphAndUI<TUIRawData>, Error>> editables = root.Elements("Node").Select(n => ReadEditable(n, m_datasource)).Evaluate();
                var allnodes = new Dictionary<ID<NodeTemp>, GraphAndUI<TUIRawData>>();
                var errors = new List<Error>();

                foreach (var editable in editables)
                {
                    editable.Do(e =>
                    {
                        allnodes[e.GraphData.NodeID] = new GraphAndUI<TUIRawData>(e.GraphData, e.UIData);
                    }, a =>
                    {
                        errors.Add(a);
                    });
                }

                //This makes a lot of assumptions but I'm fairly sure they're assumptions that currently hold
                IEnumerable<ID<NodeTemp>> filteredNodes = allnodes.Keys.Where(k => m_filter(allnodes[k].GraphData.NodeTypeID)).Evaluate();

                var links = ReadLinks(filteredNodes, root);

                //HashSet<ID<NodeTemp>> filteredNodes = new HashSet<ID<NodeTemp>>(primaryFiltered.Union(links.SelectMany(t => new[] { t.Item1.Item2, t.Item2.Item2 })));

                foreach (var link in links)
                {
                    var id1 = link.Item1.Item2;
                    var id2 = link.Item2.Item2;
                    if (allnodes.ContainsKey(id1) && allnodes.ContainsKey(id2)) //If copy/pasting a piece of graph, linked nodes may not exist
                    {
                        IEditable node1 = allnodes[id1].GraphData;
                        IEditable node2 = allnodes[id2].GraphData;

                        if (node1 is UnknownEditable)
                        {
                            ((UnknownEditable)node1).AddConnector(link.Item1.Item1);
                        }
                        if (node2 is UnknownEditable)
                        {
                            ((UnknownEditable)node2).AddConnector(link.Item2.Item1);
                        }

                        Output connector1 = node1.Connectors.SingleOrDefault(c => c.ID == link.Item1.Item1);
                        Output connector2 = node2.Connectors.SingleOrDefault(c => c.ID == link.Item2.Item1);

                        if (node1 is UnknownEditable)
                        {
                            ((UnknownEditable)node1).AllowConnection(connector1, connector2);
                        }

                        if (node2 is UnknownEditable)
                        {
                            ((UnknownEditable)node2).AllowConnection(connector1, connector2);
                        }

                        if (connector1 == null || connector2 == null)
                            errors.Add(new Error("Connector does not exist"));
                        else
                        {
                            bool success = connector1.ConnectTo(connector2);
                            if (!success)
                            {
                                errors.Add(new Error("Tried to connect two connectors that could not be connected"));
                            }
                        }
                    }
                }

                foreach (var node in root.Elements("Node"))
                {
                    ID<NodeTemp> nodeID = ID<NodeTemp>.Parse(node.Attribute("Id").Value);
                    ID<NodeTypeTemp> nodeType = ID<NodeTypeTemp>.Parse(node.Attribute("Guid").Value);
                    int outputIndex = 0; //no idea where the output guids come from so just assume they're ordered

                    foreach (var output in node.Elements("Output"))
                    {
                        ID<TConnector> outputGuid = ID<TConnector>.Parse(output.Attribute("guid").Value);
                        bool legacyNodeOutput = ID<TConnector>.Parse("1583e20c-c725-48c3-944d-1ba40c3ebdf4") == outputGuid;
                        var outputConnectors = legacyNodeOutput ? allnodes[nodeID].GraphData.Connectors : allnodes[nodeID].GraphData.Connectors.ElementAt(outputIndex).Only();
                        var connectedNodes = output.Elements("Link").Select(l => ID<NodeTemp>.Parse(l.Attribute("To").Value));
                        foreach (var connectedID in connectedNodes)
                        {
                            var connectedNode = allnodes[connectedID].GraphData;
                            var orderedConnectors = connectedNode.Connectors.OrderBy(o => o.m_definition.Id != ConnectorDefinitionData.INPUT_DEFINITION_ID); //Put any inputs at the front of the list
                            bool success = false;
                            foreach (var connector in orderedConnectors)
                            {
                                if (success) break;
                                foreach (var outputConnector in outputConnectors)
                                {
                                    if (success) break;
                                    if (connector.ConnectTo(outputConnector))
                                    {
                                        success = true;
                                    }
                                }
                            }
                            if (!success)
                                errors.Add(new Error() { Message = "Could not link nodes" });
                        }
                        outputIndex++;
                    }
                }

                return new XmlGraphData<TUIRawData, TEditorData>(filteredNodes.Select(a => allnodes[a]), m_editorDataDeserializer.Read(root), errors);
            }

            private Or<GraphAndUI<TUIRawData>, Error> ReadEditable(XElement node, IDataSource datasource)
            {
                ID<NodeTemp> id = ID<NodeTemp>.Parse(node.Attribute("Id").Value);
                ID<NodeTypeTemp> guid = ID<NodeTypeTemp>.Parse(node.Attribute("Guid").Value);

                EditableGenerator editableGenerator = datasource.GetNode(guid);

                Or<GraphAndUI<TUIRawData>, Error> result;

                var parameterNodes = node.Elements("Parameter");
                var parameterData = parameterNodes.Select(p => new EditableGenerator.ParameterData(ID<Parameter>.Parse(p.Attribute("guid").Value), p.Attribute("value").Value)).ToList();

                if (editableGenerator != null)
                {
                    IEditable editable = editableGenerator.Generate(id, parameterData);
                    result = new GraphAndUI<TUIRawData>(editable, m_nodeUIDeserializer.Read(node));
                }
                else
                {
                    var parameters = parameterData.Select(p => new UnknownParameter(p.Guid, p.Value));
                    UnknownEditable editable = new UnknownEditable(id, guid, parameters);
                    result = new GraphAndUI<TUIRawData>(editable, m_nodeUIDeserializer.Read(node));
                }

                return result;
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
                        result.Add(UnorderedTuple.Make(Tuple.Create(connector1, node1), Tuple.Create(connector2, node2)));
                }
                return result;
            }
        }

        public class Serializer : ISerializer<XmlGraphData<TUIRawData, TEditorData>>
        {
            ISerializerDeserializerXml<TUIRawData, TUIRawData> m_nodeUISerializer;
            private ISerializerXml<TEditorData> m_editorDataSerializer;
            public Serializer(ISerializerDeserializerXml<TUIRawData, TUIRawData> nodeUISerializer, ISerializerXml<TEditorData> editorDataSerializer)
            {
                m_nodeUISerializer = nodeUISerializer;
                m_editorDataSerializer = editorDataSerializer;
            }
            public void Write(XmlGraphData<TUIRawData, TEditorData> conversation, Stream stream)
            {
                var document = new XDocument();
                var root = new XElement(ROOT, new XAttribute("xmlversion", XML_VERSION_WRITE));
                document.Add(root);
                foreach (var node in conversation.Nodes)
                    Write(node, root);

                WriteLinks(conversation, root);

                m_editorDataSerializer.Write(conversation.EditorData, root);
                stream.Position = 0;
                stream.SetLength(0);
                document.Save(stream);
                stream.Flush();
            }

            private static void WriteLinks(XmlGraphData<TUIRawData, TEditorData> conversation, XElement root)
            {
                HashSet<UnorderedTuple2<Tuple<ID<TConnector>, IEditable>>> links = new HashSet<UnorderedTuple2<Tuple<ID<TConnector>, IEditable>>>();
                foreach (var n in conversation.Nodes)
                {
                    foreach (var c in n.GraphData.Connectors)
                    {
                        foreach (var cc in c.Connections)
                        {
                            var pair1 = Tuple.Create(c.ID, n.GraphData);
                            var pair2 = Tuple.Create(cc.ID, cc.Parent);
                            links.Add(UnorderedTuple.Make(pair1, pair2));
                        }
                    }
                }

                foreach (var link in links)
                {
                    var node1 = new XAttribute("node1", link.Item1.Item2.NodeID.Serialized());
                    var connector1 = new XAttribute("connector1", link.Item1.Item1.Serialized());
                    var node2 = new XAttribute("node2", link.Item2.Item2.NodeID.Serialized());
                    var connector2 = new XAttribute("connector2", link.Item2.Item1.Serialized());
                    root.Add(new XElement("Link", node1, connector1, node2, connector2));
                }
            }

            //private void WriteOutputs(IEditable ng, XElement node)
            //{
            //    foreach (var n in ng.Outputs())
            //    {
            //        var guid = new XAttribute("guid", n.ID.Serialized());
            //        var outputNode = new XElement("Output", guid);
            //        foreach (var link in n.Connections)
            //        {
            //            var to = new XAttribute("To", link.Parent.NodeID.Serialized());
            //            var linkNode = new XElement("Link", to);
            //            outputNode.Add(linkNode);
            //        }
            //        node.Add(outputNode);
            //    }
            //}

            //private void WriteConnectors(IEditable a, XElement node)
            //{
            //    foreach (var n in a.Connectors)
            //    {
            //        var guid = new XAttribute("guid", n.ID.Serialized());
            //        var outputNode = new XElement("Connector", guid);
            //        node.Add(outputNode);
            //    }
            //}

            private void Write(GraphAndUI<TUIRawData> con, XElement root)
            {
                var id = new XAttribute("Id", con.GraphData.NodeID.Serialized());
                var guid = new XAttribute("Guid", con.GraphData.NodeTypeID.Serialized());
                var node = new XElement("Node", id, guid);
                m_nodeUISerializer.Write(con.UIData, node);
                //WriteOutputs(con.GraphData, node);
                //WriteConnectors(con.GraphData, node);
                foreach (var parameter in con.GraphData.Parameters)
                {
                    var value = parameter.ValueAsString();
                    if (value != null)
                    {
                        var p = new XElement("Parameter", new XAttribute("guid", parameter.Id.Serialized()), new XAttribute("value", value));
                        node.Add(p);
                    }
                }
                root.Add(node);
            }
        }
    }
}