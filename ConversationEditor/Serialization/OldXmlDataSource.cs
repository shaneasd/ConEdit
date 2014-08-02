using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;
using Utilities;

using ConversationNode = Conversation.ConversationNode<Conversation.INodeGUI<ConversationEditor.TransitionNoduleUIInfo>, ConversationEditor.TransitionNoduleUIInfo>;

namespace ConversationEditor
{
    public class OldXmlDataSource
    {
        public static IEnumerable<Tuple<IEnumerable<ConversationNode>, IEnumerable<NodeGroup<ConversationNode>>, FileStream>> Load(List<FileStream> domainStreams, DomainDomain source, NodeFactory nodeFactory)
        {
            IEnumerable<ConversationNode> allNodes = Enumerable.Empty<ConversationNode>();
            foreach (FileStream stream in domainStreams)
            {
                //Read out all the data. Some may be corrupted.
                ConversationDeserializerGUI serializer = new ConversationDeserializerGUI(source, nodeFactory);
                var data = serializer.Read(stream);
                var nodes = data.Item1.Evaluate();
                var groups = data.Item2.Evaluate();
                yield return Tuple.Create(nodes, groups, stream);
            }
        }

        public static void Write(Tuple<IEnumerable<ConversationNode>, IEnumerable<NodeGroup<ConversationNode>>> data, FileStream file)
        {
            var serializer = new ConversationSerializerGUI();
            serializer.Write(data, file);
        }

        private static void ReadNodeTypes(DomainData domain, XElement root)
        {
            foreach (var a in root.Elements("NodeType"))
            {
                var name = a.Attribute("name").Value;
                var guid = Guid.Parse(a.Attribute("guid").Value);
                var parent = Guid.Parse(a.Attribute("parent").Value);
                domain.NodeTypes.Add(new NodeTypeData(name, guid, parent));
            }
        }

        private static void WriteNodeTypes(DomainData domain, XElement root)
        {
            foreach (var d in domain.NodeTypes)
            {
                var element = new XElement("NodeType", new XAttribute("name", d.Name), new XAttribute("guid", d.Guid));
                if (d.Parent != null)
                    element.Add(new XAttribute("parent", d.Parent.ToString()));
                root.Add(element);
            }
        }

        private static Or<string, Guid> GetType(string raw)
        {
            Guid guid;
            if (Guid.TryParse(raw, out guid))
            {
                return guid;
            }
            else
            {
                return raw;
            }
        }

        private static NodeData.ParameterData ReadParameter(XElement node)
        {
            var parameterType = GetType(node.Attribute("type").Value);
            var parameterName = node.Attribute("name").Value;
            var parameterGuid = Guid.Parse(node.Attribute("guid").Value);
            var def = node.Elements("Default").Select(n => n.Attribute("value").Value).SingleOrDefault();
            return new NodeData.ParameterData(parameterName, parameterGuid, parameterType, def);
        }

        private static void WriteParameter(NodeData.ParameterData parameter, XElement parameterNode)
        {
            parameterNode.Add(new XAttribute("type", parameter.Type.Transformed(s => s, g => g.ToString())));
            parameterNode.Add(new XAttribute("name", parameter.Name));
            parameterNode.Add(new XAttribute("guid", parameter.Guid));
            if (parameter.Default != null)
                parameterNode.Add(parameter.Default);
        }

        private static void ReadNodes(DomainData domain, XElement root)
        {
            foreach (var a in root.Elements("Node"))
            {
                var name = a.Attribute("name").Value;
                var guid = Guid.Parse(a.Attribute("guid").Value);
                var category = a.Attributes("type").SingleOrDefault(x => true, x => Guid.Parse(x.Value), (Guid?)null);
                var input = a.Elements("Input").Any();
                List<NodeData.OutputData> outputData = a.Elements("Output").Select(n => new NodeData.OutputData(n.Attributes("name").SingleOrDefault(x => true, x => x.Value, null), Guid.Parse(n.Attribute("guid").Value))).ToList();
                List<NodeData.ParameterData> parameterData = a.Elements("Parameter").Select(ReadParameter).ToList();
                List<NodeData.ConfigData> configData = a.Elements("Config").Select(node => new NodeData.ConfigData(node.Attribute("name").Value, node.Attribute("value").Value)).ToList();

                NodeData nodeData = new NodeData(name, category, guid, input, outputData, parameterData, configData);

                domain.Nodes.Add(nodeData);
            }
        }

        private static void WriteNodes(DomainData domain, XElement root)
        {
            foreach (var a in domain.Nodes)
            {
                var element = new XElement("Node", new XAttribute("name", a.Name), new XAttribute("guid", a.Guid));
                if (a.Type != null)
                    element.Add(new XAttribute("type", a.Type));
                if (a.Input)
                    element.Add(new XElement("Input"));
                foreach (var output in a.Outputs)
                {
                    var outputNode = new XElement("Output", new XAttribute("guid", output.Guid));
                    if (output.Name != null)
                        outputNode.Add(new XAttribute("name", output.Name));
                    element.Add(outputNode);
                }
                foreach (var parameter in a.Parameters)
                {
                    var parameterNode = new XElement("Parameter");
                    WriteParameter(parameter, parameterNode);
                    element.Add(parameterNode);
                }
                foreach (var config in a.Config)
                {
                    element.Add(new XElement("Config", new XAttribute("name", config.Name), new XAttribute("value", config.Value)));
                }
                root.Add(element);
            }
        }

        /// <summary>
        /// Populate a domain's various type data from xml
        /// </summary>
        /// <param name="domain">The domain to fill</param>
        /// <param name="root">The xml root node</param>
        private static void ReadTypes(DomainData domain, XElement root)
        {
            foreach (var a in root.Elements("DynamicEnumeration"))
            {
                DynamicEnumerationData typeData = new DynamicEnumerationData(a.Attribute("name").Value, Guid.Parse(a.Attribute("guid").Value));
                domain.DynamicEnumerations.Add(typeData);
            }
            foreach (var a in root.Elements("Enumeration"))
            {
                var name = a.Attribute("name").Value;
                var guid = Guid.Parse(a.Attribute("guid").Value);
                var values = a.Elements("Value").Select(value => new EnumerationData.Element(value.Attribute("name").Value, Guid.Parse(value.Attribute("guid").Value)));

                EnumerationData typeData;

                if (a.Element("Default") != null)
                {
                    if (a.Element("Default").Attributes("guid").Any())
                    {
                        var def = Guid.Parse(a.Element("Default").Attribute("guid").Value);
                        typeData = new EnumerationData(name, guid, values, def);
                    }
                    else if (a.Element("Default").Attributes("name").Any())
                    {
                        var def = a.Element("Default").Attribute("name").Value;
                        typeData = new EnumerationData(name, guid, values, def);
                    }
                    else
                    {
                        throw new Exception("Enumeration declared with a default node but no recognised default value");
                    }
                }
                else
                {
                    typeData = new EnumerationData(name, guid, values);
                }

                domain.Enumerations.Add(typeData);
            }
            foreach (var a in root.Elements("Decimal"))
            {
                var aName = a.Attribute("name").Value;
                var guid = Guid.Parse(a.Attribute("guid").Value);
                var aMax = a.Attributes("max").Select<XAttribute, decimal?>(b => decimal.Parse(b.Value)).SingleOrDefault();
                var aMin = a.Attributes("min").Select<XAttribute, decimal?>(b => decimal.Parse(b.Value)).SingleOrDefault();
                var aDef = a.Attributes("default").Select<XAttribute, decimal?>(b => decimal.Parse(b.Value)).SingleOrDefault();

                DecimalData typeData = new DecimalData(aName, guid, aMax, aMin, aDef);

                domain.Decimals.Add(typeData);
            }
            foreach (var a in root.Elements("Integer"))
            {
                var aName = a.Attribute("name").Value;
                var guid = Guid.Parse(a.Attribute("guid").Value);
                var aMax = a.Attributes("max").Select<XAttribute, int?>(b => int.Parse(b.Value)).SingleOrDefault();
                var aMin = a.Attributes("min").Select<XAttribute, int?>(b => int.Parse(b.Value)).SingleOrDefault();
                var aDef = a.Attributes("default").Select<XAttribute, int?>(b => int.Parse(b.Value)).SingleOrDefault();

                IntegerData typeData = new IntegerData(aName, guid, aMax, aMin, aDef);

                domain.Integers.Add(typeData);
            }
        }

        private static void WriteTypes(DomainData domain, XElement root)
        {
            foreach (var a in domain.DynamicEnumerations)
            {
                root.Add(new XElement("DynamicEnumeration", new XAttribute("name", a.Name), new XAttribute("guid", a.Guid)));
            }

            foreach (var a in domain.Enumerations)
            {
                var element = new XElement("Enumeration", new XAttribute("name", a.Name), new XAttribute("guid", a.Guid));

                foreach (var value in a.Elements)
                    element.Add(new XElement("Value", new XAttribute("name", value.Name), new XAttribute("guid", value.Guid)));

                if (a.Default != null)
                {
                    var defaultText = a.Default.Transformed(s => s, g => g.ToString());
                    element.Add(new XElement("Default", new XAttribute("guid", defaultText)));
                }
                root.Add(element);
            }

            foreach (var a in domain.Decimals)
            {
                var element = new XElement("Decimal", new XAttribute("name", a.Name), new XAttribute("guid", a.Guid));
                if (a.Max != null)
                    element.Add(new XAttribute("max", a.Max));
                if (a.Min != null)
                    element.Add(new XAttribute("min", a.Min));
                if (a.Default != null)
                    element.Add(new XAttribute("default", a.Default));
                root.Add(element);
            }

            foreach (var a in domain.Integers)
            {
                var element = new XElement("Integer", new XAttribute("name", a.Name), new XAttribute("guid", a.Guid));
                if (a.Max != null)
                    element.Add(new XAttribute("max", a.Max));
                if (a.Min != null)
                    element.Add(new XAttribute("min", a.Min));
                if (a.Default != null)
                    element.Add(new XAttribute("default", a.Default));
                root.Add(element);
            }
        }
    }


 





}
