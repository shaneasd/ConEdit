﻿using System;
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
using System.Collections.ObjectModel;

namespace Conversation.Serialization
{
    public class LoadError
    {
        private string m_message;
        public string Message { get { return m_message; } }
        public LoadError()
        {
            m_message = ""; //Will be assigned later presumably. Not critical.
        }
        public LoadError(string message)
        {
            m_message = message;
        }
    }

    public class GraphAndUI<TUIRawData>
    {
        public IEditable GraphData { get { return m_graphData; } }
        public TUIRawData UIData { get { return m_uiData; } }

        private readonly IEditable m_graphData;
        private readonly TUIRawData m_uiData;

        public GraphAndUI(IEditable graphData, TUIRawData uiData)
        {
            m_graphData = graphData;
            m_uiData = uiData;
        }
    }

    /// <summary>
    /// All data required for serialization/deserialization of a node graph
    /// </summary>
    public class XmlGraphData<TUIRawData, TEditorData>
    {
        public XmlGraphData(IEnumerable<GraphAndUI<TUIRawData>> nodes, TEditorData editorData, ReadOnlyCollection<LoadError> errors, object documentId)
        {
            m_nodes = nodes;
            m_editorData = editorData;
            m_errors = errors;
            m_documentId = documentId;
        }

        public XmlGraphData(IEnumerable<GraphAndUI<TUIRawData>> nodes, TEditorData editorData)
        {
            m_nodes = nodes;
            m_editorData = editorData;
            m_errors = new ReadOnlyCollection<LoadError>(new LoadError[0]);
        }

        private readonly IEnumerable<GraphAndUI<TUIRawData>> m_nodes;
        private readonly TEditorData m_editorData;
        private readonly ReadOnlyCollection<LoadError> m_errors;
        private object m_documentId;

        public IEnumerable<GraphAndUI<TUIRawData>> Nodes { get { return m_nodes; } }
        public TEditorData EditorData { get { return m_editorData; } }
        public ReadOnlyCollection<LoadError> Errors { get { return m_errors; } }
        public object DocumentId { get { return m_documentId; } }
    }

    public static class XmlConversation<TUIRawData, TEditorData>
    {
        private static readonly string[] XmlVersionRead = new[] { "1.0", "1.1" };
        const string XmlVersionWrite = "1.1";
        public const string Root = "Root";

        public class SerializerDeserializer : ISerializerDeserializer<XmlGraphData<TUIRawData, TEditorData>>
        {
            Deserializer deserializer;
            Serializer serializer;

            public SerializerDeserializer(IDataSource datasource, ISerializerDeserializerXml<TUIRawData, TUIRawData> nodeUISerializer, ISerializerXml<TEditorData> editorDataSerializer, IDeserializerXml<TEditorData> editorDataDeserializer)
            {
                deserializer = new Deserializer(datasource, nodeUISerializer, editorDataDeserializer, null);
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
            Func<Id<NodeTypeTemp>, bool> m_filter;

            public Deserializer(IDataSource datasource, IDeserializerXml<TUIRawData> nodeUISerializer, IDeserializerXml<TEditorData> editorDataDeserializer, Func<Id<NodeTypeTemp>, bool> filter)
            {
                m_datasource = datasource;
                m_nodeUIDeserializer = nodeUISerializer;
                m_editorDataDeserializer = editorDataDeserializer;
                m_filter = filter ?? new Func<Id<NodeTypeTemp>, bool>(i => true);
            }

            public XmlGraphData<TUIRawData, TEditorData> Read(Stream stream)
            {
                object documentID = new object();

                stream.Position = 0;
                var d = XDocument.Load(stream);
                var root = d.Element(Root);

                //TODO: Should possibly treat this as a missing file rather than crashing the editor
                if (root.Attribute("xmlversion") == null || !XmlVersionRead.Contains(root.Attribute("xmlversion").Value))
                    throw new UnknownXmlVersionException("unrecognised conversation xml version");

                IEnumerable<Either<GraphAndUI<TUIRawData>, LoadError>> editables = root.Elements("Node").Select(n => ReadEditable(n, m_datasource, documentID)).Evaluate();
                var allnodes = new Dictionary<Id<NodeTemp>, GraphAndUI<TUIRawData>>();
                var errors = new List<LoadError>();

                foreach (var editable in editables)
                {
                    editable.Do(e =>
                    {
                        allnodes[e.GraphData.NodeId] = new GraphAndUI<TUIRawData>(e.GraphData, e.UIData);
                    }, a =>
                    {
                        errors.Add(a);
                    });
                }

                //This makes a lot of assumptions but I'm fairly sure they're assumptions that currently hold
                IEnumerable<Id<NodeTemp>> filteredNodes = allnodes.Keys.Where(k => m_filter(allnodes[k].GraphData.NodeTypeId)).Evaluate();

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

                        var unknownNode1 = node1 as UnknownEditable;
                        var unknownNode2 = node2 as UnknownEditable;

                        if (unknownNode1 != null)
                        {
                            unknownNode1.AddConnector(link.Item1.Item1);
                        }
                        if (unknownNode2 != null)
                        {
                            unknownNode2.AddConnector(link.Item2.Item1);
                        }

                        Output connector1 = node1.Connectors.SingleOrDefault(c => c.Id == link.Item1.Item1);
                        Output connector2 = node2.Connectors.SingleOrDefault(c => c.Id == link.Item2.Item1);

                        if (unknownNode1 != null)
                        {
                            unknownNode1.AllowConnection(connector1, connector2);
                        }

                        if (unknownNode2 != null)
                        {
                            unknownNode2.AllowConnection(connector1, connector2);
                        }

                        if (connector1 == null || connector2 == null)
                            errors.Add(new LoadError("Connector does not exist"));
                        else
                        {
                            bool success = connector1.ConnectTo(connector2, false);
                            if (!success)
                            {
                                success = connector1.ConnectTo(connector2, true);
                                if (!success)
                                    errors.Add(new LoadError("Tried to connect two connectors that could not be connected"));
                            }
                        }
                    }
                }

                foreach (var node in root.Elements("Node"))
                {
                    Id<NodeTemp> nodeID = Id<NodeTemp>.Parse(node.Attribute("Id").Value);
                    //ID<NodeTypeTemp> nodeType = ID<NodeTypeTemp>.Parse(node.Attribute("Guid").Value);
                    int outputIndex = 0; //no idea where the output guids come from so just assume they're ordered

                    //TODO: Remove support for legacy linking
                    foreach (var output in node.Elements("Output"))
                    {
                        Id<TConnector> outputGuid = Id<TConnector>.Parse(output.Attribute("guid").Value);
                        bool legacyNodeOutput = Id<TConnector>.Parse("1583e20c-c725-48c3-944d-1ba40c3ebdf4") == outputGuid;
                        var outputConnectors = legacyNodeOutput ? allnodes[nodeID].GraphData.Connectors : allnodes[nodeID].GraphData.Connectors.ElementAt(outputIndex).Only();
                        var connectedNodes = output.Elements("Link").Select(l => Id<NodeTemp>.Parse(l.Attribute("To").Value));
                        foreach (var connectedID in connectedNodes)
                        {
                            var connectedNode = allnodes[connectedID].GraphData;
                            var orderedConnectors = connectedNode.Connectors.OrderBy(o => o.m_definition.Id != ConnectorDefinitionData.InputDefinitionId); //Put any inputs at the front of the list
                            bool success = false;
                            foreach (var connector in orderedConnectors)
                            {
                                if (success) break;
                                foreach (var outputConnector in outputConnectors)
                                {
                                    if (success) break;
                                    if (connector.ConnectTo(outputConnector, false))
                                    {
                                        success = true;
                                    }
                                    else
                                    {
                                        connector.ConnectTo(outputConnector, true);
                                    }
                                }
                            }
                            if (!success)
                                errors.Add(new LoadError("Could not link nodes"));
                        }
                        outputIndex++;
                    }
                }

                return new XmlGraphData<TUIRawData, TEditorData>(filteredNodes.Select(a => allnodes[a]), m_editorDataDeserializer.Read(root), new ReadOnlyCollection<LoadError>(errors), documentID);
            }

            private Either<GraphAndUI<TUIRawData>, LoadError> ReadEditable(XElement node, IDataSource datasource, object documentID)
            {
                Id<NodeTemp> id = Id<NodeTemp>.Parse(node.Attribute("Id").Value);
                Id<NodeTypeTemp> guid = Id<NodeTypeTemp>.Parse(node.Attribute("Guid").Value);

                EditableGenerator editableGenerator = datasource.GetNode(guid);

                Either<GraphAndUI<TUIRawData>, LoadError> result;

                var parameterNodes = node.Elements("Parameter");
                var parameterData = parameterNodes.Select(p => new EditableGeneratorParameterData(Id<Parameter>.Parse(p.Attribute("guid").Value), p.Attribute("value").Value)).ToList();

                if (editableGenerator != null)
                {
                    IEditable editable = editableGenerator.Generate(id, parameterData, documentID);
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

        public class Serializer : ISerializer<XmlGraphData<TUIRawData, TEditorData>>
        {
            ISerializerDeserializerXml<TUIRawData, TUIRawData> m_nodeUISerializer;
            private ISerializerXml<TEditorData> m_editorDataSerializer;
            public Serializer(ISerializerDeserializerXml<TUIRawData, TUIRawData> nodeUISerializer, ISerializerXml<TEditorData> editorDataSerializer)
            {
                m_nodeUISerializer = nodeUISerializer;
                m_editorDataSerializer = editorDataSerializer;
            }
            public void Write(XmlGraphData<TUIRawData, TEditorData> data, Stream stream)
            {
                var document = new XDocument();
                var root = new XElement(Root, new XAttribute("xmlversion", XmlVersionWrite));
                document.Add(root);
                foreach (var node in data.Nodes)
                    Write(node, root);

                WriteLinks(data, root);

                m_editorDataSerializer.Write(data.EditorData, root);
                stream.Position = 0;
                stream.SetLength(0);
                document.Save(stream);
                stream.Flush();
            }

            private static void WriteLinks(XmlGraphData<TUIRawData, TEditorData> conversation, XElement root)
            {
                HashSet<UnorderedTuple2<Tuple<Id<TConnector>, IEditable>>> links = new HashSet<UnorderedTuple2<Tuple<Id<TConnector>, IEditable>>>();
                foreach (var n in conversation.Nodes)
                {
                    foreach (var c in n.GraphData.Connectors)
                    {
                        foreach (var cc in c.Connections)
                        {
                            var pair1 = Tuple.Create(c.Id, n.GraphData);
                            var pair2 = Tuple.Create(cc.Id, cc.Parent);
                            links.Add(UnorderedTuple.Make(pair1, pair2));
                        }
                    }
                }

                foreach (var link in links)
                {
                    var node1 = new XAttribute("node1", link.Item1.Item2.NodeId.Serialized());
                    var connector1 = new XAttribute("connector1", link.Item1.Item1.Serialized());
                    var node2 = new XAttribute("node2", link.Item2.Item2.NodeId.Serialized());
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
                var id = new XAttribute("Id", con.GraphData.NodeId.Serialized());
                var guid = new XAttribute("Guid", con.GraphData.NodeTypeId.Serialized());
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