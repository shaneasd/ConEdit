using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using System.Windows;
using System.Runtime.Serialization;

namespace ConversationEditor
{
    [Serializable]
    public class CategoryGenerationException : Exception
    {
        public CategoryGenerationException(string message) : base(message) { }

        public CategoryGenerationException() : base() { }

        public CategoryGenerationException(string message, Exception inner) : base(message, inner) { }

        protected CategoryGenerationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    internal class ConversationDataSource : IDataSource
    {
        ConversationConnectionRules m_connectionRules = new ConversationConnectionRules();
        TypeSet m_types;

        private CallbackDictionary<Id<NodeTypeTemp>, Tuple<Guid, GenericEditableGenerator2>> m_nodes = new CallbackDictionary<Id<NodeTypeTemp>, Tuple<Guid, GenericEditableGenerator2>>();
        private NodeType m_nodeHeirarchy;

        Dictionary<Id<TConnectorDefinition>, ConnectorDefinitionData> m_connectorDefinitions = new Dictionary<Id<TConnectorDefinition>, ConnectorDefinitionData>()
            {
                {SpecialConnectors.Output.Id, SpecialConnectors.Output },
                {SpecialConnectors.Input.Id, SpecialConnectors.Input },
            };

        /// <summary>
        /// Fill the data source from all the data stored in all domain files
        /// </summary>
        /// <param name="domains">All the domain files used to populate the datasource</param>
        public ConversationDataSource(TypeSet typeSet, IEnumerable<DomainData> domains)
        {
            m_types = typeSet;

            m_nodes.Removing += m_nodes_Removing;

            domains = domains.Evaluate();

            //Types must be generated before Nodes and can be generated before NodeTypes
            foreach (var typeData in domains.SelectMany(d => d.DynamicEnumerations))
                AddDynamicEnumType(typeData);

            foreach (var typeData in domains.SelectMany(d => d.LocalDynamicEnumerations))
                AddLocalDynamicEnumType(typeData);

            foreach (var typeData in domains.SelectMany(d => d.Enumerations))
                AddEnumType(typeData);

            foreach (var typeData in domains.SelectMany(d => d.Decimals))
                AddDecimalType(typeData);

            foreach (var typeData in domains.SelectMany(d => d.Integers))
                AddIntegerType(typeData);

            //NodeTypes must be generated before Nodes and can be generated before Types. NodeTypes may have interdependencies between files
            m_categories = domains.SelectMany(d => d.NodeTypes).ToList();
            GenerateCategories(m_categories);

            //Connectors must be generated after Types but before Nodes
            foreach (var connector in domains.SelectMany(d => d.Connectors))
            {
                AddConnector(connector);
            }

            //Nodes must be generated after NodeTypes, Types and Connectors
            foreach (var node in domains.SelectMany(d => d.Nodes))
            {
                AddNodeType(node);
            }

            m_connectionRules.SetRules(domains.SelectMany(d => d.Connections));
        }

        void m_nodes_Removing(Id<NodeTypeTemp> id, Tuple<Guid, GenericEditableGenerator2> generator)
        {
            generator.Item2.Removed();
        }

        public void AddNodeType(NodeData node)
        {
            var nodeGenerator = new GenericEditableGenerator2(node, m_types, m_connectorDefinitions, m_connectionRules);
            m_nodes[node.Guid] = new Tuple<Guid, GenericEditableGenerator2>(node.Type.GetValueOrDefault(DomainIDs.CategoryNone), nodeGenerator);
        }

        internal void RemoveNodeType(Id<NodeTypeTemp> id)
        {
            m_nodes.Remove(id);
        }

        public void AddConnector(ConnectorDefinitionData connector)
        {
            m_connectorDefinitions[connector.Id] = connector;
        }

        public void RemoveConnector(Id<TConnectorDefinition> id)
        {
            m_connectorDefinitions.Remove(id);
        }

        private void GenerateCategories(List<NodeTypeData> nodeTypeData)
        {
            nodeTypeData = nodeTypeData.ToList(); //Copy that shit because we don't want to break m_categories
            m_nodeHeirarchy = new NodeType(null, DomainIDs.CategoryNone);

            var duplicates = nodeTypeData.GroupBy(a => a.Guid).Where(g => g.Count() > 1);
            if (duplicates.Any())
                throw new CategoryGenerationException("The following node types have duplicate definitions: " + string.Join(", ", duplicates.Select(g => g.Key).ToArray()));
            List<NodeType> nodeTypes = new List<NodeType> { };

            nodeTypes.Add(m_nodeHeirarchy);

            //foreach (var data in nodeTypeData.Where(d => d.Parent == DomainIds.CATEGORY_NONE).ToList())
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
                        parent.AddChildType(newNodeType);
                        nodeTypes.Add(newNodeType);
                        gotOne = true;
                        nodeTypeData.Remove(data);
                    }
                }
            }

            if (!gotOne)
            {
                //TODO: How to express this to the user?
                //throw new Exception("The following node types are ancestors of an unknown node type: " + string.Join(", ", nodeTypeData.Select(d => d.Guid).ToArray()));
                //    MessageBox.Show("The following node types are ancestors of an unknown node type: " + string.Join(", ", nodeTypeData.Select(d => d.Guid).ToArray()));
                foreach (var orphan in nodeTypeData)
                {
                    m_nodeHeirarchy.AddChildType(new NodeType(orphan.Name, orphan.Guid));
                }
            }
        }

        public void AddEnumType(EnumerationData typeData)
        {
            m_types.AddEnum(typeData, false);
        }

        public void UpdateEnumeration(EnumerationData data)
        {
            m_types.ModifyEnum(data);
        }

        public void RemoveType(ParameterType id)
        {
            m_types.Remove(id);
        }

        public void AddDynamicEnumType(DynamicEnumerationData typeData)
        {
            m_types.AddDynamicEnum(typeData);
        }

        public void AddLocalDynamicEnumType(LocalDynamicEnumerationData typeData)
        {
            m_types.AddLocalDynamicEnum(typeData);
        }

        public void AddDecimalType(DecimalData typeData)
        {
            m_types.AddDecimal(typeData);
        }

        internal void ModifyDecimalType(DecimalData typeData)
        {
            m_types.ModifyDecimal(typeData);
        }

        public void AddIntegerType(IntegerData typeData)
        {
            m_types.AddInteger(typeData);
        }

        internal void ModifyIntegerType(IntegerData typeData)
        {
            m_types.ModifyInteger(typeData);
        }

        public IEnumerable<ParameterType> ParameterTypes
        {
            get { return m_types.AllTypes; }
        }

        public string GetTypeName(ParameterType id)
        {
            return m_types.GetTypeName(id);
        }

        public INodeType Nodes
        {
            get
            {
                GenerateCategories(m_categories.ToList());

                foreach (var node in m_nodes.Values)
                {
                    var parent = m_nodeHeirarchy.Collapse(x => x.m_childTypes, x => x.Only()).SingleOrDefault(x => x.Guid == node.Item1) ?? m_nodeHeirarchy;
                    parent.m_nodes.Add(node.Item2);
                }

                return m_nodeHeirarchy;
            }
        }

        public EditableGenerator GetNode(Id<NodeTypeTemp> guid)
        {
            if (m_nodes.ContainsKey(guid))
                return m_nodes[guid].Item2;
            else
                return null;
        }

        #region GUID Type Queries
        public bool IsInteger(ParameterType type)
        {
            return m_types.IsInteger(type);
        }

        public bool IsDecimal(ParameterType type)
        {
            return m_types.IsDecimal(type);
        }

        public bool IsEnum(ParameterType type)
        {
            return m_types.IsEnum(type);
        }

        public bool IsDynamicEnum(ParameterType type)
        {
            return m_types.IsDynamicEnum(type);
        }

        public bool IsLocalDynamicEnum(ParameterType type)
        {
            return m_types.IsLocalDynamicEnum(type);
        }

        public bool IsCategoryDefinition(Id<NodeTypeTemp> id)
        {
            return false; //Not possible for a conversation to contain domain data
        }

        public bool IsTypeDefinition(Id<NodeTypeTemp> id)
        {
            return false; //Not possible for a conversation to contain domain data
        }

        public bool IsConnectorDefinition(Id<NodeTypeTemp> id)
        {
            return false; //Not possible for a conversation to contain domain data
        }

        public bool IsNodeDefinition(Id<NodeTypeTemp> id)
        {
            return false; //Not possible for a conversation to contain domain data
        }

        public bool IsAutoCompleteNode(Id<NodeTypeTemp> id)
        {
            return false; //Not possible for a conversation to contain domain data
        }

        #endregion

        List<NodeTypeData> m_categories = new List<NodeTypeData>();
        internal void AddCategory(NodeTypeData category)
        {
            m_categories.Add(category);
        }

        internal void RemoveCategory(Guid id)
        {
            m_categories.RemoveAll(c => c.Guid == id);
        }

        internal void RenameCategory(NodeTypeData data)
        {
            var index = m_categories.FindIndex(d => d.Guid == data.Guid);
            m_categories[index] = data;
        }

        public Guid GetCategory(Id<NodeTypeTemp> type)
        {
            foreach (var item in m_nodes.Values)
            {
                if (item.Item2.Guid == type)
                    return item.Item1;
            }
            return Guid.Empty;
        }

        internal DynamicEnumParameter.Source GetSource(IDynamicEnumParameter parameter, object newSourceID)
        {
            return GetSource(parameter.TypeId, newSourceID);
        }

        public DynamicEnumParameter.Source GetSource(ParameterType type, object newSourceId)
        {
            if (IsLocalDynamicEnum(type))
                return m_types.GetLocalDynamicEnumSource(type, newSourceId);
            else
                return m_types.GetDynamicEnumSource(type);
        }
    }
}
