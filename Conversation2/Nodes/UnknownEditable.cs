using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Collections.ObjectModel;

namespace Conversation
{
    public class UnknownEditable : IConversationNodeData
    {
        private Id<NodeTemp> m_nodeId;
        private Id<NodeTypeTemp> m_nodeTypeId;
        private List<UnknownParameter> m_parameters;
        public event Action Linked;

        public UnknownEditable(Id<NodeTemp> nodeId, Id<NodeTypeTemp> nodeTypeId, IEnumerable<UnknownParameter> parameters)
        {
            m_nodeId = nodeId;
            m_nodeTypeId = nodeTypeId;
            m_parameters = parameters.ToList();
        }

        public Id<NodeTemp> NodeId => m_nodeId;

        public Id<NodeTypeTemp> NodeTypeId => m_nodeTypeId;

        public string Name => "Unknown Node";

        public string Description => "This node is of a type unknown in the domain. Is a file missing from the domain or has a node definition been deleted?";

        public IReadOnlyList<NodeData.ConfigData> Config => new ReadOnlyCollection<NodeData.ConfigData>(new NodeData.ConfigData[0]);

        public IEnumerable<IParameter> Parameters => m_parameters;

        List<Output> m_connectors = new List<Output>();
        public IEnumerable<Output> Connectors => m_connectors;

        public void ChangeId(Id<NodeTemp> id)
        {
            m_nodeId = id;
        }

        class CustomConnectionRules : IConnectionRules
        {
            HashSet<UnorderedTuple2<Id<TConnectorDefinition>>> m_rules = new HashSet<UnorderedTuple2<Id<TConnectorDefinition>>>();

            public bool CanConnect(Id<TConnectorDefinition> a, Id<TConnectorDefinition> b)
            {
                return m_rules.Contains(UnorderedTuple.Make(a, b));
            }

            public void Allow(Id<TConnectorDefinition> a, Id<TConnectorDefinition> b)
            {
                m_rules.Add(UnorderedTuple.Make(a, b));
            }
        }

        CustomConnectionRules m_rules = new CustomConnectionRules();

        public void AddConnector(Id<TConnector> id)
        {
            if (!m_connectors.Any(c => c.Id == id))
            {
                ConnectorDefinitionData data = new ConnectorDefinitionData("", Id<TConnectorDefinition>.ConvertFrom(id), new List<NodeData.ParameterData>(), ConnectorPosition.Bottom);
                var connector = new Output(id, data, this, new List<Parameter>(), m_rules);
                connector.Connected += (a) => Linked.Execute();
                connector.Disconnected += (a) => Linked.Execute();
                m_connectors.Add(connector);
            }
        }

        //TODO: Couldn't we just say unknown connections are a violation of the connection rules but then allow them from disk anyway?
        /// <summary>
        /// Updates the rules to permit connection of the input connectors
        /// Since the node is of an unknown type we don't know whether the connectors are allowed to be connected so assume they are
        /// </summary>
        public void AllowConnection(Output connector1, Output connector2)
        {
            m_rules.Allow(connector1.Definition.Id, connector2.Definition.Id);
        }

        public SimpleUndoPair RemoveUnknownParameter(UnknownParameter p)
        {
            throw new NotSupportedException("Editing of unknown editables is not supported. Please recover the definition of the node type or delete the node");
        }
    }
}
