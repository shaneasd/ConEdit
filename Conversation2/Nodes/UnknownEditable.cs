using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Collections.ObjectModel;

namespace Conversation
{
    public class UnknownEditable : IEditable
    {
        private Id<NodeTemp> m_nodeId;
        private Id<NodeTypeTemp> m_nodeTypeId;
        private List<UnknownParameter> m_parameters;
        public event Action Linked { add { } remove { } }

        public UnknownEditable(Id<NodeTemp> nodeId, Id<NodeTypeTemp> nodeTypeId, IEnumerable<UnknownParameter> parameters)
        {
            m_nodeId = nodeId;
            m_nodeTypeId = nodeTypeId;
            m_parameters = parameters.ToList();
        }

        public Id<NodeTemp> NodeId
        {
            get { return m_nodeId; }
        }

        public Id<NodeTypeTemp> NodeTypeId
        {
            get { return m_nodeTypeId; }
        }

        public string Name
        {
            get { return "Unknown Node"; }
        }

        public ReadOnlyCollection<NodeData.ConfigData> Config
        {
            get { return new ReadOnlyCollection<NodeData.ConfigData>(new NodeData.ConfigData[0]); }
        }

        public IEnumerable<Parameter> Parameters
        {
            get { return m_parameters; }
        }

        List<Output> m_connectors = new List<Output>();
        public IEnumerable<Output> Connectors
        {
            get { return m_connectors; }
        }

        public void ChangeId(Id<NodeTemp> id)
        {
            m_nodeId = id;
        }

        public void TryDecorrupt()
        {
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
            if (!m_connectors.Any(c => c.ID == id))
            {
                ConnectorDefinitionData data = new ConnectorDefinitionData("", Id<TConnectorDefinition>.ConvertFrom(id), new List<NodeData.ParameterData>(), ConnectorPosition.Bottom);
                m_connectors.Add(new Output(id, data, this, new List<Parameter>(), m_rules));
            }
        }

        public void AllowConnection(Output connector1, Output connector2)
        {
            m_rules.Allow(connector1.m_definition.Id, connector2.m_definition.Id);
        }


        public SimpleUndoPair RemoveUnknownParameter(UnknownParameter p)
        {
            throw new NotSupportedException("Editing of unknown editables is not supported. Please recover the definition of the node type or delete the node");
        }
    }
}
