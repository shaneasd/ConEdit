using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGui>;

    public abstract class IDomainUsage<TNode, TTransition> where TNode : IRenderable<IGui>
    {
        public abstract IEnumerable<Usage> Usages(IConversationNode node);

        public class Usage
        {
            public readonly TNode Node;
            public readonly IConversationEditorControlData<TNode, TTransition> File;
            public readonly string Description;
            public Usage(TNode node, IConversationEditorControlData<TNode, TTransition> file, string description)
            {
                Node = node;
                File = file;
                Description = description;
            }
        }
    }

    internal class DomainUsage : IDomainUsage<ConversationNode, TransitionNoduleUIInfo>
    {
        private Project m_project;
        public DomainUsage(Project project)
        {
            m_project = project;
        }

        private IEnumerable<Usage> CategoryUsage(Id<NodeTemp> id)
        {
            List<Usage> result = new List<Usage>();
            foreach (var domainFile in m_project.DomainFiles.Evaluate())
            {
                var domainNodes = domainFile.Nodes;
                var categoryNodes = domainNodes.Where(n => n.Data.NodeTypeId == DomainIDs.CategoryGuid);
                foreach (var categoryNode in categoryNodes)
                {
                    var parentParameter = categoryNode.Data.Parameters.Single(p => p.Id == DomainIDs.CategoryParent) as IEnumParameter;
                    if (parentParameter.Value == id.Guid)
                    {
                        result.Add(new Usage(categoryNode, domainFile, "Category definition " + categoryNode.Data.Parameters.Where(x => x.Id == DomainIDs.CategoryName).Single().ValueAsString()));
                    }
                }

                var nodeDefinitionNodes = domainNodes.Where(n => n.Data.NodeTypeId == DomainIDs.NodeGuid);
                foreach (var nodeDefinitionNode in nodeDefinitionNodes)
                {
                    var category = nodeDefinitionNode.Data.Parameters.Single(p => p.Id == DomainIDs.NodeCategory) as IEnumParameter;
                    if (category.Value == id.Guid)
                    {
                        result.Add(new Usage(nodeDefinitionNode, domainFile, "Node definition " + nodeDefinitionNode.Data.Parameters.Where(x => x.Id == DomainIDs.NodeName).Single().ValueAsString()));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Return all usages in the domain of a particular derived primitive type
        /// </summary>
        /// <param name="id">The id of the specific derived type</param>
        /// <param name="type">The type in question</param>
        private List<Usage> DerivedTypeUsage(Id<NodeTemp> id, BaseType type)
        {
            List<Usage> result = new List<Usage>();
            foreach (var domainFile in m_project.DomainFiles.Evaluate())
            {
                var domainNodes = domainFile.Nodes;
                var parameterNodes = domainNodes.Where(n => type.ParameterNodeType == n.Data.NodeTypeId); //All the parameter definitions of this type in the domain
                foreach (var parameterNode in parameterNodes)
                {
                    var typeParameter = parameterNode.Data.Parameters.SingleOrDefault(p => p.Id == DomainIDs.PARAMETER_TYPE) as IEnumParameter; //Identifies what subtype of the base type it is (e.g. what kind of integer)
                    if (typeParameter.Value == id.Guid)
                    {
                        result.Add(new Usage(parameterNode, domainFile, type.Name + " parameter " + parameterNode.Data.Parameters.Where(x => x.Id == DomainIDs.ParameterName).Single().ValueAsString()));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// All usages of enumeration value. Usages can take two forms:
        /// 1. Conversation nodes which have parameters for which the user has selected this value
        /// 2. Domain parameter nodes for which the user has selected this value as the default
        /// </summary>
        /// <param name="node">The node declaring the enumeration value</param>
        private IEnumerable<Usage> EnumerationValueUsage(IConversationNode node)
        {
            List<Usage> result = new List<Usage>();
            //Find all the enum declarations for which this is a value
            var enumDeclarationNodes = node.Data.Connectors.SelectMany(t => t.Connections).Select(t => t.Parent).Where(n => n.NodeTypeId == BaseType.Enumeration.NodeType);

            foreach (var enumDeclarationNode in enumDeclarationNodes)
            {
                var enumTypeID = enumDeclarationNode.NodeId;

                //All the nodes in the conversation that have parameters with this value
                foreach (var conversationFile in m_project.Conversations)
                {
                    foreach (var n in conversationFile.Nodes)
                    {
                        var parameters = n.Data.Parameters;
                        var filteredParameters = parameters.Where(p => p.TypeId == ParameterType.Basic.ConvertFrom(enumTypeID)).Cast<IEnumParameter>()
                                                           .Concat(parameters.Where(p => p.TypeId == ParameterType.ValueSetType.ConvertFrom(enumTypeID)).Cast<IEnumParameter>());

                        var usingParameters = filteredParameters.Where(p => p.Value == node.Data.NodeId.Guid);
                        foreach (var p in usingParameters)
                            result.Add(new Usage(n, conversationFile, "Node " + n.Data.Name + " with Enum parameter " + p.Name));
                    }
                }

                //All parameters in the domain for which this is the default value
                foreach (var domainFile in m_project.DomainFiles)
                {
                    foreach (var n in domainFile.Nodes)
                    {
                        if (n.Data.NodeTypeId == BaseType.Enumeration.ParameterNodeType)
                        {
                            var typeParameter = n.Data.Parameters.Single(p => p.Id == DomainIDs.PARAMETER_TYPE) as IEnumParameter;
                            if (typeParameter.Value == enumTypeID.Guid)
                            {
                                var defaultParameter = n.Data.Parameters.Single(p => p.Id == DomainIDs.ParameterDefault) as IDynamicEnumParameter;
                                var expectedValue = (node.Data.Parameters.Single(p => p.Id == DomainIDs.EnumerationValueParameter) as IStringParameter).Value;
                                if (defaultParameter.Value == expectedValue)
                                    result.Add(new Usage(n, domainFile, "Enum definition " + n.Data.Parameters.Where(x => x.Id == DomainIDs.ParameterName).Single().ValueAsString() + " default value"));
                            }
                        }
                    }
                }
            }

            return result;
        }

        private List<Usage> ConnectorDefinitionsUsages(IEnumerable<Id<TConnectorDefinition>> ids)
        {
            List<Usage> result = new List<Usage>();
            foreach (var domainFile in m_project.DomainFiles)
            {
                foreach (var i in ids)
                {
                    Id<NodeTypeTemp> id = Id<NodeTypeTemp>.ConvertFrom(i);
                    var connectors = domainFile.Nodes.Where(n => n.Data.NodeTypeId == id);
                    result.AddRange(connectors.Select(c => new Usage(c, domainFile, "Connector on node " + c.Data.Name)));

                    foreach (var n in domainFile.Nodes)
                    {
                        if (n.Data.NodeTypeId == DomainIDs.ConnectionDefinitionGuid)
                        {
                            var parameters = n.Data.Parameters.Where(p => p.Id == DomainIDs.ConnectionDefinitionConnector1 || p.Id == DomainIDs.ConnectionDefinitionConnector2);
                            var enumParameters = parameters.OfType<IEnumParameter>();
                            if (enumParameters.Any(p => p.Value == i.Guid))
                                result.Add(new Usage(n, domainFile, "Connection to connector"));
                        }
                    }
                }
            }

            foreach (var conversationFile in m_project.Conversations)
            {
                foreach (var node in conversationFile.Nodes)
                {
                    foreach (var i in ids)
                    {
                        if (node.Data.Connectors.Where(c => c.Definition.Id == i).Any(c => c.Connections.Any()))
                            result.Add(new Usage(node, conversationFile, "Node connection using connector"));
                    }
                }
            }

            return result;
        }

        private IEnumerable<Usage> ConnectedNodesUsages(IConversationNode node)
        {
            HashSet<Id<NodeTypeTemp>> nodeIds = new HashSet<Id<NodeTypeTemp>>();
            nodeIds.UnionWith(node.Data.Connectors.SelectMany(t => t.Connections).Select(t => t.Parent).Where(n => n.NodeTypeId == DomainIDs.NodeGuid).Select(n => Id<NodeTypeTemp>.ConvertFrom(n.NodeId)));
            return NodeDefinitionsUsages(nodeIds);
        }

        private string NodeToString(IConversationNodeData data)
        {
            StringBuilder builder = new StringBuilder();
            foreach (var parameter in data.Parameters)
            {
                builder.Append(" │ ");
                builder.Append(parameter.Name);
                builder.Append(": ");
                builder.Append(parameter.DisplayValue(m_project.Localizer.Localize));
            }
            return builder.ToString();
        }

        /// <summary>
        /// All usages in all conversation files of nodes with the specified types
        /// </summary>
        /// <param name="typeIDs">The IDs identifying the types of nodes to search for usages of</param>
        private List<Usage> NodeDefinitionsUsages(ICollection<Id<NodeTypeTemp>> typeIDs)
        {
            List<Usage> result = new List<Usage>();
            foreach (var conversationFile in m_project.Conversations)
            {
                foreach (var n in conversationFile.Nodes)
                {
                    if (typeIDs.Contains(n.Data.NodeTypeId))
                        result.Add(new Usage(n, conversationFile, "Node " + n.Data.NodeId.Serialized() + NodeToString(n.Data)));
                }
            }
            return result;
        }

        public override IEnumerable<Usage> Usages(IConversationNode node)
        {
            /*
             * Category
             * 
             * Integer
             * Decimal
             * Dynamic Enumeration
             * Enumeration
             * Localized String
             * 
             * Enumeration Value
             * 
             * Connector Definition
             * Connection Definition
             * 
             * Integer Parameter
             * Decimal Parameter
             * String Parameter
             * Localized String Parameter
             * Boolean Parameter
             * Audio Parameter
             * Enumeration Parameter
             * Dynamic Enumeration Parameter
             * 
             * Input
             * Output
             * Custom Connectors*
             * 
             * Node
             */


            Id<NodeTypeTemp> id = node.Data.NodeTypeId;

            BaseType type = BaseType.BaseTypes.SingleOrDefault(t => t.NodeType == id);


            //It's a category
            if (id == DomainIDs.CategoryGuid)
            {
                return CategoryUsage(node.Data.NodeId);
            }
            //It's a type definition
            else if (type != null)
            {
                return DerivedTypeUsage(node.Data.NodeId, type);
            }
            //It's an enum value defintion (i.e. part of a type definition but also usable as a default value
            else if (id == DomainIDs.EnumerationValueDeclaration)
            {
                return EnumerationValueUsage(node);
            }
            //It's a connector definition used to define custom connectors
            else if (id == DomainIDs.ConnectorDefinitionGuid)
            {
                return ConnectorDefinitionsUsages(Id<TConnectorDefinition>.ConvertFrom(node.Data.NodeId).Only());
            }
            else if (id == DomainIDs.ConnectionDefinitionGuid)
            {
                return ConnectionDefinitionUsages(node.Data);
            }
            //It's a parameter
            else if (BaseType.BaseTypes.Any(t => t.ParameterNodeType == id))
            {
                var connectors = ConnectorDefinitionsUsages(node.Data.Connectors.SelectMany(t => t.Connections).Select(t => t.Parent).Where(n => n.NodeTypeId == DomainIDs.ConnectorDefinitionGuid).Select(a => Id<TConnectorDefinition>.ConvertFrom(a.NodeId)));
                var nodes = ConnectedNodesUsages(node);
                return connectors.Concat(nodes);
            }
            //It's a connector part of a node definition
            else if (IsConnector(node))
            {
                return ConnectedNodesUsages(node);
            }
            //It's a plain old node definition
            else if (id == DomainIDs.NodeGuid)
            {
                return NodeDefinitionsUsages(Id<NodeTypeTemp>.ConvertFrom(node.Data.NodeId).Only());
            }
            else
            {
                //This node is of a type unknown to the domain domain (i.e. it's presumably in the conversation domain)
                return Enumerable.Empty<Usage>();
            }
        }

        private IEnumerable<Usage> ConnectionDefinitionUsages(IConversationNodeData data)
        {
            Id<TConnectorDefinition> connector1 = Id<TConnectorDefinition>.FromGuid(data.Parameters.Where(p => p.Id == DomainIDs.ConnectionDefinitionConnector1).OfType<IEnumParameter>().Single().Value);
            Id<TConnectorDefinition> connector2 = Id<TConnectorDefinition>.FromGuid(data.Parameters.Where(p => p.Id == DomainIDs.ConnectionDefinitionConnector2).OfType<IEnumParameter>().Single().Value);
            var reference = UnorderedTuple.Make(connector1, connector2);

            foreach (var conversationFile in m_project.Conversations)
            {
                HashSet<UnorderedTuple2<Output>> connections = new HashSet<UnorderedTuple2<Output>>();
                foreach (var node in conversationFile.Nodes)
                {
                    foreach (var connector in node.Data.Connectors)
                    {
                        if (connector.Definition.Id == connector1 || connector.Definition.Id == connector2)
                        {
                            foreach (var connection in connector.Connections)
                            {
                                var c = UnorderedTuple.Make(connector.Definition.Id, connection.Definition.Id);
                                if (c == reference)
                                    connections.Add(UnorderedTuple.Make(connector, connection));

                                yield return new Usage(node, conversationFile, "Node connection using connector definition");
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Is this node the connector part of a node definition
        /// </summary>
        private bool IsConnector(IConversationNode node)
        {
            return m_project.DomainDataSource.IsConnector(node.Data.NodeTypeId);
        }
    }
}
