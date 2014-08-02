using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using System.Windows;

namespace ConversationEditor
{
    public class ConversationDataSource : IDataSource
    {
        TypeSet m_types = BaseTypeSet.Make();

        private Dictionary<ID<NodeTypeTemp>, EditableGenerator> m_nodes = new Dictionary<ID<NodeTypeTemp>, EditableGenerator>();
        private NodeType m_nodeHeirarchy = new NodeType(null, DomainIDs.CATEGORY_NONE);
        private List<IEnumeration> m_enumerations = new List<IEnumeration>();

        /// <summary>
        /// Fill the data source from all the data stored in all domain files
        /// </summary>
        /// <param name="domains">All the domain files used to populate the datasource</param>
        public ConversationDataSource(IEnumerable<DomainData> domains)
        {
            domains = domains.Evaluate();

            //Types must be generated before Nodes and can be generated before NodeTypes
            foreach (var domain in domains)
            {
                foreach (var typeData in domain.DynamicEnumerations)
                {
                    m_types.AddDynamicEnum(typeData.TypeID, (name, guid) => typeData.Make(name, guid));
                }

                foreach (var typeData in domain.Enumerations)
                {
                    IEnumeration enumeration = typeData.Make();
                    m_enumerations.Add(enumeration);
                    m_types.AddEnum(enumeration.TypeId, enumeration.Parameter);
                }

                foreach (var typeData in domain.Decimals)
                {
                    m_types.AddDecimal(typeData.TypeID, typeData.Make);
                }

                foreach (var typeData in domain.Integers)
                {
                    m_types.AddInteger(typeData.TypeID, typeData.Make);
                }
            }

            //NodeTypes must be generated before Nodes and can be generated before Types. NodeTypes may have interdependencies between files
            var nodeTypeData = domains.SelectMany(d => d.NodeTypes).ToList();
            var duplicates = nodeTypeData.GroupBy(a => a.Guid).Where(g => g.Count() > 1);
            if (duplicates.Any())
                throw new Exception("The following node types have duplicate definitions: " + string.Join(", ", duplicates.Select(g => g.Key).ToArray()));
            List<NodeType> nodeTypes = new List<NodeType> { };

            nodeTypes.Add(m_nodeHeirarchy);

            //foreach (var data in nodeTypeData.Where(d => d.Parent == DomainGUIDS.CATEGORY_NONE).ToList())
            //{
            //    var newNodeType = new NodeType(data.Name, data.Guid);
            //    m_nodes.m_childTypes.Add(newNodeType);
            //    nodeTypes.Add(newNodeType);
            //    nodeTypeData.Remove(data);
            //}

            bool gotOne = true;
            while (nodeTypeData.Any() && gotOne)
            {
                gotOne = false;
                for (int i = 0; i < nodeTypes.Count; i++)
                {
                    var parent = nodeTypes[i];
                    foreach (var data in nodeTypeData.Where(d => d.Parent == parent.Guid).ToList())
                    {
                        var newNodeType = new NodeType(data.Name, data.Guid);
                        parent.m_childTypes.Add(newNodeType);
                        nodeTypes.Add(newNodeType);
                        gotOne = true;
                        nodeTypeData.Remove(data);
                    }
                }
            }

            if (!gotOne)
            {
                //TODO: How to express this to the user?
                //Do they even need to know? It may just be autoresolved
                //throw new Exception("The following node types are ancestors of an unknown node type: " + string.Join(", ", nodeTypeData.Select(d => d.Guid).ToArray()));
                MessageBox.Show("The following node types are ancestors of an unknown node type: " + string.Join(", ", nodeTypeData.Select(d => d.Guid).ToArray()));
                foreach (var orphan in nodeTypeData)
                {
                    m_nodeHeirarchy.m_childTypes.Add(new NodeType(orphan.Name, orphan.Guid));
                }
            }

            //Connectors must be generated after Types but before Nodes
            Dictionary<ID<TConnectorDefinition>, ConnectorDefinitionData> connectorDefinitions = new Dictionary<ID<TConnectorDefinition>, ConnectorDefinitionData>()
            {
                {SpecialConnectors.Output.Id, SpecialConnectors.Output },
                {SpecialConnectors.Input.Id, SpecialConnectors.Input },
            };
            foreach (var domain in domains)
            {
                foreach (var connector in domain.Connectors)
                {
                    connectorDefinitions[connector.Id] = connector;
                }
            }

            //Nodes must be generated after NodeTypes, Types and Connectors
            foreach (var domain in domains)
            {
                foreach (var node in domain.Nodes)
                {
                    var name = node.Name;
                    var guid = node.Guid;

                    Func<NodeData.ConnectorData, Func<IEditable, Output>> processConnector = c =>
                        {
                            Func<IEditable, List<Parameter>, Output> a = connectorDefinitions[c.TypeID].Make(c.Id, ConversationConnectionRules.Instance);
                            return data => a(data, c.Parameters);
                        };

                    IEnumerable<Func<IEditable, Output>> connectors = node.Connectors.Select(processConnector).Evaluate();
                    var parameters = node.Parameters.Select<NodeData.ParameterData, Func<Parameter>>(p => () => p.Make(m_types.Make)).Evaluate();
                    var config = node.Config.ToDictionary(b => b.Name, b => b.Value, StringComparer.OrdinalIgnoreCase);

                    var parent = m_nodeHeirarchy.Collapse(x => x.m_childTypes, x => x.Only()).SingleOrDefault(x => x.Guid == node.Type) ?? m_nodeHeirarchy;

                    var nodeGenerator = new GenericEditableGenerator(name, guid, config, (id, ng) => new ExternalFunction(ng, id, connectors, parameters.Select(b => b()).ToArray()));
                    parent.m_nodes.Add(nodeGenerator);
                    m_nodes[guid] = nodeGenerator;
                }
            }
        }

        public IEnumerable<ID<ParameterType>> ParameterTypes
        {
            get { return m_types.AllTypes; }
        }

        public INodeType Nodes
        {
            get { return m_nodeHeirarchy; }
        }

        public EditableGenerator GetNode(ID<NodeTypeTemp> guid)
        {
            if (m_nodes.ContainsKey(guid))
                return m_nodes[guid];
            else
                return null;
        }

        public bool IsInteger(ID<ParameterType> type)
        {
            return m_types.IsInteger(type);
        }

        public bool IsDecimal(ID<ParameterType> type)
        {
            return m_types.IsDecimal(type);
        }

        public bool IsEnum(ID<ParameterType> type)
        {
            return m_types.IsEnum(type);
        }

        public bool IsDynamicEnum(ID<ParameterType> type)
        {
            return m_types.IsDynamicEnum(type);
        }


        public bool IsCategoryDefinition(ID<NodeTypeTemp> id)
        {
            return false; //Not possible for a conversation to contain domain data
        }

        public bool IsTypeDefinition(ID<NodeTypeTemp> id)
        {
            return false; //Not possible for a conversation to contain domain data
        }

        public bool IsConnectorDefinition(ID<NodeTypeTemp> id)
        {
            return false; //Not possible for a conversation to contain domain data
        }

        public bool IsNodeDefinition(ID<NodeTypeTemp> id)
        {
            return false; //Not possible for a conversation to contain domain data
        }
    }
}
