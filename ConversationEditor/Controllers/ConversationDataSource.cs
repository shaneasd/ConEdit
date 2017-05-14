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
        ConstantTypeSet m_types;

        private CallbackDictionary<Id<NodeTypeTemp>, Tuple<Guid, NodeDataGenerator>> m_nodes = new CallbackDictionary<Id<NodeTypeTemp>, Tuple<Guid, NodeDataGenerator>>();
        //private NodeCategory m_nodeHeirarchy;

        Dictionary<Id<TConnectorDefinition>, ConnectorDefinitionData> m_connectorDefinitions = new Dictionary<Id<TConnectorDefinition>, ConnectorDefinitionData>()
            {
                {SpecialConnectors.Output.Id, SpecialConnectors.Output },
                {SpecialConnectors.Input.Id, SpecialConnectors.Input },
            };

        /// <summary>
        /// Fill the data source from all the data stored in all domain files
        /// </summary>
        /// <param name="domains">All the domain files used to populate the datasource</param>
        public ConversationDataSource(IEnumerable<IDomainData> domains)
        {
            domains = domains.Evaluate(); //We're going to be iterating over this a lot
            IEnumerable<DynamicEnumerationData> dynamicEnumerations = domains.SelectMany(d => d.DynamicEnumerations);
            IEnumerable<LocalDynamicEnumerationData> localDynamicEnumerations = domains.SelectMany(d => d.LocalDynamicEnumerations);
            IEnumerable<EnumerationData> enumerations = domains.SelectMany(d => d.Enumerations);
            IEnumerable<DecimalData> decimals = domains.SelectMany(d => d.Decimals);
            IEnumerable<IntegerData> integers = domains.SelectMany(d => d.Integers);

            m_types = BaseTypeSet.MakeConstant(dynamicEnumerations, localDynamicEnumerations, enumerations, decimals, integers);

            m_nodes.Removing += m_nodes_Removing;

            //NodeTypes must be generated before Nodes and can be generated before Types. NodeTypes may have interdependencies between files
            m_categories = domains.SelectMany(d => d.NodeTypes).ToList();
            //GenerateCategories(m_categories);

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

        void m_nodes_Removing(Id<NodeTypeTemp> id, Tuple<Guid, NodeDataGenerator> generator)
        {
            generator.Item2.Removed();
        }

        public void AddNodeType(NodeData node)
        {
            var nodeGenerator = new NodeDataGenerator(node, m_types, m_connectorDefinitions, m_connectionRules, null);
            m_nodes[node.Guid] = new Tuple<Guid, NodeDataGenerator>(node.Category.GetValueOrDefault(DomainIDs.CategoryNone), nodeGenerator);
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

        private static NodeCategory GenerateCategories(List<NodeTypeData> nodeTypeData)
        {
            nodeTypeData = nodeTypeData.ToList(); //Copy that shit because we don't want to break m_categories
            var nodeHeirarchy = new NodeCategory(null, DomainIDs.CategoryNone);

            var duplicates = nodeTypeData.GroupBy(a => a.Guid).Where(g => g.Count() > 1);
            if (duplicates.Any())
                throw new CategoryGenerationException("The following node types have duplicate definitions: " + string.Join(", ", duplicates.Select(g => g.Key).ToArray()));
            List<NodeCategory> nodeTypes = new List<NodeCategory> { };

            nodeTypes.Add(nodeHeirarchy);

            //foreach (var data in nodeTypeData.Where(d => d.Parent == DomainIds.CATEGORY_NONE).ToList())
            //{
            //    var newNodeType = new NodeCategory(data.Name, data.Guid);
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
                        var newNodeType = new NodeCategory(data.Name, data.Guid);
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
                    nodeHeirarchy.AddChildType(new NodeCategory(orphan.Name, orphan.Guid));
                }
            }

            return nodeHeirarchy;
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
                var nodeHeirarchy = GenerateCategories(m_categories.ToList());

                foreach (var node in m_nodes.Values)
                {
                    var parent = nodeHeirarchy.Collapse(x => x.ChildTypes, x => x.Only()).SingleOrDefault(x => x.Guid == node.Item1) ?? nodeHeirarchy;
                    parent.AddNode(node.Item2);
                }

                return nodeHeirarchy;
            }
        }

        public INodeDataGenerator GetNode(Id<NodeTypeTemp> guid)
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

        /// <summary>
        /// Threadsafe
        /// </summary>
        /// <param name="type"></param>
        /// <param name="newSourceId"></param>
        /// <returns></returns>
        public DynamicEnumParameter.Source GetSource(ParameterType type, object newSourceId)
        {
            if (IsLocalDynamicEnum(type))
                return m_types.GetLocalDynamicEnumSource(type, newSourceId);
            else
                return m_types.GetDynamicEnumSource(type);
        }
    }
}
